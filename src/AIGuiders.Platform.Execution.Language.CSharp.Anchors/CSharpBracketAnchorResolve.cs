#nullable enable

using AIGuiders.Platform.Execution.LanguageIntelligence.Anchors;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AIGuiders.Platform.Execution.Language.CSharp.Anchors;


/// <summary>Resolve S(+K) to line/column range via local C# parse (no MSBuild).</summary>
public static class CSharpBracketAnchorResolve
{
    public sealed record TextRange(int LineStart, int ColumnStart, int LineEnd, int ColumnEnd);

    public sealed record AttachTarget(
        string AbsolutePath,
        SyntaxTree Tree,
        CompilationUnitSyntax Root,
        SyntaxNode Node,
        string Detail);

    public static bool TryResolve(string absoluteFilePath, BracketAnchorSpan span, out TextRange range, out string detail) =>
        TryResolve(absoluteFilePath, sourceText: null, span, out range, out detail);

    /// <param name="sourceText">
    /// Optional buffer text (cdp_buffer). When null, reads <paramref name="absoluteFilePath"/> from disk.
    /// </param>
    public static bool TryResolve(
        string absoluteFilePath,
        string? sourceText,
        BracketAnchorSpan span,
        out TextRange range,
        out string detail)
    {
        if (!TryFindAttachTarget(absoluteFilePath, sourceText, span, out var target, out detail))
        {
            range = default!;
            return false;
        }

        var lineSpan = target.Node.GetLocation().GetLineSpan();
        range = new TextRange(
            lineSpan.StartLinePosition.Line + 1,
            lineSpan.StartLinePosition.Character + 1,
            lineSpan.EndLinePosition.Line + 1,
            Math.Max(1, lineSpan.EndLinePosition.Character + 1));
        detail = target.Detail;

        // T: is parse-only unless we narrow here — silent ignore made place=after
        // insert at end of whole M: member (dogfood: IntentRouter RouteOne wiring).
        if (!string.IsNullOrWhiteSpace(span.TextNeedle))
        {
            if (!TryNarrowRangeToTextNeedle(target.Tree, target.Node, span.TextNeedle, out range, out var narrowDetail))
            {
                detail = narrowDetail;
                return false;
            }

            detail = $"{target.Detail}+T";
        }

        return true;
    }

    /// <summary>Resolve F+(M|L|S[+K]) to a syntax node for annotate/mutate attach.</summary>
    public static bool TryFindAttachTarget(
        string absoluteFilePath,
        BracketAnchorSpan span,
        out AttachTarget target,
        out string detail) =>
        TryFindAttachTarget(absoluteFilePath, sourceText: null, span, out target, out detail);

    /// <param name="sourceText">When set, parse this instead of disk (dirty buffer / in-memory).</param>
    public static bool TryFindAttachTarget(
        string absoluteFilePath,
        string? sourceText,
        BracketAnchorSpan span,
        out AttachTarget target,
        out string detail)
    {
        target = default!;
        detail = "";
        string text;
        if (sourceText is not null)
        {
            text = sourceText;
        }
        else if (File.Exists(absoluteFilePath))
        {
            text = File.ReadAllText(absoluteFilePath);
        }
        else
        {
            detail = "file_missing";
            return false;
        }

        var tree = CSharpSyntaxTree.ParseText(text);
        var root = tree.GetCompilationUnitRoot();

        SyntaxNode searchRoot = root;
        MemberDeclarationSyntax? member = null;
        if (!string.IsNullOrWhiteSpace(span.MemberKey))
        {
            member = root.DescendantNodes()
                .OfType<MemberDeclarationSyntax>()
                .FirstOrDefault(m => MemberName(m).Equals(span.MemberKey, StringComparison.Ordinal));
            if (member is null)
            {
                detail = "member_not_found";
                return false;
            }
            searchRoot = member;
        }

        SyntaxNode? focus;
        string resolveDetail;

        if (!string.IsNullOrWhiteSpace(span.ScopeKind))
        {
            if (!TryResolveScope(searchRoot, span, out focus, out resolveDetail))
                return Fail(resolveDetail, out detail);
        }
        else if (span.LineStart is >= 1)
        {
            focus = FindNodeAtLine(tree, root, searchRoot, span.LineStart.Value);
            if (focus is null)
                return Fail("line_node_not_found", out detail);
            if (!string.IsNullOrWhiteSpace(span.Role))
            {
                if (!TryApplyLineRole(focus, span.Role.Trim(), out focus, out resolveDetail))
                    return Fail(resolveDetail, out detail);
            }
            else
            {
                resolveDetail = "line";
            }
        }
        else if (member is not null)
        {
            focus = member;
            resolveDetail = "member";
            if (!string.IsNullOrWhiteSpace(span.Role))
            {
                if (!TryApplyMemberRole(member, span.Role.Trim(), out focus, out resolveDetail))
                    return Fail(resolveDetail, out detail);
            }
        }
        else if (!string.IsNullOrWhiteSpace(span.TextNeedle))
        {
            // T: alone: search whole compilation unit (M already narrowed searchRoot above).
            focus = searchRoot;
            resolveDetail = "file";
        }
        else
        {
            return Fail("need_M_or_L_or_S", out detail);
        }

        if (focus is null)
            return Fail("node_null", out detail);

        target = new AttachTarget(absoluteFilePath, tree, root, focus, resolveDetail);
        detail = resolveDetail;
        return true;
    }

    /// <summary>
    /// Roles on L: (no S:) — Initializer / Type / Name / Parameter of local or nearby decl.
    /// </summary>
    private static bool TryApplyLineRole(
        SyntaxNode node,
        string role,
        out SyntaxNode? focus,
        out string detail)
    {
        focus = node;
        detail = "";

        var isInit = role.Equals("Initializer", StringComparison.OrdinalIgnoreCase)
                     || role.Equals("Value", StringComparison.OrdinalIgnoreCase)
                     || role.Equals("Rhs", StringComparison.OrdinalIgnoreCase);
        if (!isInit)
        {
            for (var n = node; n is not null; n = n.Parent)
            {
                if (n is MemberDeclarationSyntax or LocalDeclarationStatementSyntax or ParameterSyntax
                    or LocalFunctionStatementSyntax or VariableDeclaratorSyntax)
                {
                    if (TryApplyMemberRole(n, role, out focus, out detail))
                        return true;
                }
            }

            detail = $"unknown_line_role:{role}";
            return false;
        }

        var local = node.AncestorsAndSelf().OfType<LocalDeclarationStatementSyntax>().FirstOrDefault();
        if (local is not null)
        {
            var value = local.Declaration.Variables
                .Select(v => v.Initializer?.Value)
                .FirstOrDefault(v => v is not null);
            if (value is null)
            {
                detail = "no_initializer";
                return false;
            }

            focus = value;
            detail = "line+Initializer";
            return true;
        }

        var field = node.AncestorsAndSelf().OfType<FieldDeclarationSyntax>().FirstOrDefault();
        if (field is not null)
        {
            var value = field.Declaration.Variables
                .Select(v => v.Initializer?.Value)
                .FirstOrDefault(v => v is not null);
            if (value is null)
            {
                detail = "no_initializer";
                return false;
            }

            focus = value;
            detail = "line+Initializer";
            return true;
        }

        var assign = node.AncestorsAndSelf().OfType<AssignmentExpressionSyntax>().FirstOrDefault();
        if (assign is not null)
        {
            focus = assign.Right;
            detail = "line+Rhs";
            return true;
        }

        if (node is EqualsValueClauseSyntax ev)
        {
            focus = ev.Value;
            detail = "line+Initializer";
            return true;
        }

        if (node.Parent is EqualsValueClauseSyntax evParent)
        {
            focus = evParent.Value;
            detail = "line+Initializer";
            return true;
        }

        detail = "initializer_not_found";
        return false;
    }

    private static bool Fail(string why, out string detail)
    {
        detail = why;
        return false;
    }

    /// <summary>
    /// Narrow a resolved syntax node range to the first <c>T:</c> needle match inside it.
    /// Needle is <see cref="BracketAnchorWire.SanitizeTextNeedle"/> (same as wire parse).
    /// </summary>
    public static bool TryNarrowRangeToTextNeedle(
        SyntaxTree tree,
        SyntaxNode scope,
        string needleRaw,
        out TextRange range,
        out string detail)
    {
        range = default!;
        var needle = BracketAnchorWire.SanitizeTextNeedle(needleRaw);
        if (needle.Length == 0)
        {
            detail = "text_needle_empty";
            return false;
        }

        var source = tree.GetText();
        var nodeSpan = scope.Span;
        var haystack = source.ToString(nodeSpan);
        if (!TryFindNeedleOffset(haystack, needle, out var relStart, out var matchLen))
        {
            detail = "text_needle_not_found";
            return false;
        }

        var absStart = nodeSpan.Start + relStart;
        var absEnd = absStart + matchLen;
        // SanitizeTextNeedle strips ';' from the wire value, but source still has
        // statement terminators — extend so place=after does not land between
        // `return "x"` and `;` (would split the statement).
        var fullText = source.ToString();
        if (absEnd < fullText.Length && fullText[absEnd] == ';')
            absEnd++;
        var startPos = source.Lines.GetLinePosition(absStart);
        var endPos = source.Lines.GetLinePosition(absEnd);
        range = new TextRange(
            startPos.Line + 1,
            startPos.Character + 1,
            endPos.Line + 1,
            Math.Max(1, endPos.Character + 1));
        detail = "T";
        return true;
    }

    /// <summary>
    /// Insert edges for block-interior places (type/ns before|after, or method into|end):
    /// inside the braces, not outside the declaration.
    /// Method <c>M:</c>+before|after stays sibling-outside at the DocumentEditPlane layer.
    /// Returns a zero-width <see cref="TextRange"/> at the insert point.
    /// </summary>
    public static bool TryGetBlockInteriorInsertPoint(
        SyntaxNode node,
        bool before,
        out TextRange point,
        out string detail)
    {
        point = default!;
        detail = "";

        SyntaxToken open;
        SyntaxToken close;
        switch (node)
        {
            case BlockSyntax block:
                open = block.OpenBraceToken;
                close = block.CloseBraceToken;
                break;
            case MethodDeclarationSyntax { Body: { } methodBody }:
                open = methodBody.OpenBraceToken;
                close = methodBody.CloseBraceToken;
                break;
            case ConstructorDeclarationSyntax { Body: { } ctorBody }:
                open = ctorBody.OpenBraceToken;
                close = ctorBody.CloseBraceToken;
                break;
            case DestructorDeclarationSyntax { Body: { } dtorBody }:
                open = dtorBody.OpenBraceToken;
                close = dtorBody.CloseBraceToken;
                break;
            case OperatorDeclarationSyntax { Body: { } opBody }:
                open = opBody.OpenBraceToken;
                close = opBody.CloseBraceToken;
                break;
            case LocalFunctionStatementSyntax { Body: { } localBody }:
                open = localBody.OpenBraceToken;
                close = localBody.CloseBraceToken;
                break;
            case AccessorDeclarationSyntax { Body: { } accBody }:
                open = accBody.OpenBraceToken;
                close = accBody.CloseBraceToken;
                break;
            case TypeDeclarationSyntax type
                when !type.OpenBraceToken.IsKind(SyntaxKind.None)
                     && !type.CloseBraceToken.IsKind(SyntaxKind.None):
                open = type.OpenBraceToken;
                close = type.CloseBraceToken;
                break;
            case NamespaceDeclarationSyntax ns:
                open = ns.OpenBraceToken;
                close = ns.CloseBraceToken;
                break;
            default:
                if (TryGetExpressionBodyExpression(node, out var expr))
                    return PointAtNodeEdge(expr, before, "expression_body_edge", out point, out detail);
                detail = "no_block_body";
                return false;
        }

        if (open.IsKind(SyntaxKind.None) || close.IsKind(SyntaxKind.None))
        {
            detail = "no_block_body";
            return false;
        }

        var tree = node.SyntaxTree;
        if (tree is null)
        {
            detail = "no_syntax_tree";
            return false;
        }

        var source = tree.GetText();
        var abs = before ? open.Span.End : close.Span.Start;
        var pos = source.Lines.GetLinePosition(abs);
        point = new TextRange(
            pos.Line + 1,
            pos.Character + 1,
            pos.Line + 1,
            pos.Character + 1);
        detail = before ? "block_body_start" : "block_body_end";
        return true;
    }

    static bool TryGetExpressionBodyExpression(SyntaxNode node, out ExpressionSyntax expr)
    {
        expr = node switch
        {
            MethodDeclarationSyntax { ExpressionBody.Expression: { } e } => e,
            LocalFunctionStatementSyntax { ExpressionBody.Expression: { } e } => e,
            OperatorDeclarationSyntax { ExpressionBody.Expression: { } e } => e,
            AccessorDeclarationSyntax { ExpressionBody.Expression: { } e } => e,
            PropertyDeclarationSyntax { ExpressionBody.Expression: { } e } => e,
            _ => null!
        };
        return expr is not null;
    }

    static bool PointAtNodeEdge(
        SyntaxNode node,
        bool before,
        string detailName,
        out TextRange point,
        out string detail)
    {
        var lineSpan = node.GetLocation().GetLineSpan();
        if (before)
        {
            point = new TextRange(
                lineSpan.StartLinePosition.Line + 1,
                lineSpan.StartLinePosition.Character + 1,
                lineSpan.StartLinePosition.Line + 1,
                lineSpan.StartLinePosition.Character + 1);
        }
        else
        {
            point = new TextRange(
                lineSpan.EndLinePosition.Line + 1,
                Math.Max(1, lineSpan.EndLinePosition.Character + 1),
                lineSpan.EndLinePosition.Line + 1,
                Math.Max(1, lineSpan.EndLinePosition.Character + 1));
        }

        detail = detailName;
        return true;
    }

    /// <summary>
    /// Ordinal match first; if sanitize collapsed whitespace, match collapsed haystack and map back.
    /// </summary>
    static bool TryFindNeedleOffset(string haystack, string needle, out int start, out int length)
    {
        start = haystack.IndexOf(needle, StringComparison.Ordinal);
        if (start >= 0)
        {
            length = needle.Length;
            return true;
        }

        var map = new List<int>(haystack.Length);
        var collapsed = new System.Text.StringBuilder(haystack.Length);
        for (var i = 0; i < haystack.Length; i++)
        {
            var c = haystack[i];
            if (char.IsWhiteSpace(c))
            {
                if (collapsed.Length > 0 && collapsed[^1] != ' ')
                {
                    collapsed.Append(' ');
                    map.Add(i);
                }

                continue;
            }

            collapsed.Append(c);
            map.Add(i);
        }

        var cHay = collapsed.ToString();
        var cIdx = cHay.IndexOf(needle, StringComparison.Ordinal);
        if (cIdx < 0 || cIdx + needle.Length > map.Count)
        {
            length = 0;
            return false;
        }

        start = map[cIdx];
        var endInclusive = map[cIdx + needle.Length - 1];
        length = endInclusive - start + 1;
        return true;
    }

    private static bool TryResolveScope(
        SyntaxNode searchRoot,
        BracketAnchorSpan span,
        out SyntaxNode? focus,
        out string detail)
    {
        focus = null;
        detail = "";
        var index = span.ScopeIndex is > 0 ? span.ScopeIndex.Value : 1;
        SyntaxNode? target = span.ScopeKind switch
        {
            "if" => searchRoot.DescendantNodes().OfType<IfStatementSyntax>().Skip(index - 1).FirstOrDefault(),
            "for" => searchRoot.DescendantNodes().OfType<ForStatementSyntax>().Skip(index - 1).FirstOrDefault(),
            "foreach" => searchRoot.DescendantNodes().OfType<ForEachStatementSyntax>().Skip(index - 1).FirstOrDefault(),
            "while" => searchRoot.DescendantNodes().OfType<WhileStatementSyntax>().Skip(index - 1).FirstOrDefault(),
            _ => null
        };

        if (target is null)
        {
            detail = $"scope_not_found:{span.ScopeKind}:{index}";
            return false;
        }

        focus = target;
        if (!string.IsNullOrWhiteSpace(span.Role))
        {
            var role = span.Role.Trim();
            if (!TryApplyRole(target, role, out focus, out detail))
                return false;
            detail = $"syntax_scope+{role}";
        }
        else
        {
            detail = "syntax_scope";
        }

        return true;
    }

    /// <summary>Roles on member / local / parameter nodes (K:Name, Parameter:x, ReturnType, Body, Type).</summary>
    private static bool TryApplyMemberRole(
        SyntaxNode target,
        string role,
        out SyntaxNode? focus,
        out string detail)
    {
        focus = target;
        detail = "";

        if (role.Equals("Name", StringComparison.OrdinalIgnoreCase))
        {
            focus = target switch
            {
                MethodDeclarationSyntax m => m,
                PropertyDeclarationSyntax p => p,
                TypeDeclarationSyntax t => t,
                ParameterSyntax p => p,
                VariableDeclaratorSyntax v => v,
                LocalDeclarationStatementSyntax loc => loc.Declaration.Variables.FirstOrDefault() ?? (SyntaxNode)loc,
                FieldDeclarationSyntax f => f.Declaration.Variables.FirstOrDefault() ?? (SyntaxNode)f,
                _ => target
            };
            detail = "member+Name";
            return true;
        }

        if (role.Equals("ReturnType", StringComparison.OrdinalIgnoreCase))
        {
            focus = target switch
            {
                MethodDeclarationSyntax m => m.ReturnType,
                PropertyDeclarationSyntax p => p.Type,
                _ => null
            };
            if (focus is null)
            {
                detail = "no_return_type";
                return false;
            }

            detail = "member+ReturnType";
            return true;
        }

        if (role.Equals("Body", StringComparison.OrdinalIgnoreCase))
        {
            focus = target switch
            {
                MethodDeclarationSyntax m => (SyntaxNode?)m.Body ?? m.ExpressionBody,
                AccessorDeclarationSyntax a => (SyntaxNode?)a.Body ?? a.ExpressionBody,
                PropertyDeclarationSyntax p => p.ExpressionBody
                    ?? (SyntaxNode?)p.AccessorList,
                _ => null
            };
            if (focus is null)
            {
                detail = "no_body";
                return false;
            }

            detail = "member+Body";
            return true;
        }

        if (role.Equals("Type", StringComparison.OrdinalIgnoreCase))
        {
            focus = target switch
            {
                ParameterSyntax p => p.Type,
                PropertyDeclarationSyntax p => p.Type,
                VariableDeclaratorSyntax v when v.Parent is VariableDeclarationSyntax vd => vd.Type,
                LocalDeclarationStatementSyntax loc => loc.Declaration.Type,
                FieldDeclarationSyntax f => f.Declaration.Type,
                _ => null
            };
            if (focus is null)
            {
                detail = "no_type";
                return false;
            }

            detail = "member+Type";
            return true;
        }

        var isInit = role.Equals("Initializer", StringComparison.OrdinalIgnoreCase)
                     || role.Equals("Value", StringComparison.OrdinalIgnoreCase)
                     || role.Equals("Rhs", StringComparison.OrdinalIgnoreCase);
        if (isInit)
        {
            focus = target switch
            {
                PropertyDeclarationSyntax p => (SyntaxNode?)p.Initializer?.Value
                    ?? p.ExpressionBody?.Expression,
                FieldDeclarationSyntax f => f.Declaration.Variables.FirstOrDefault()?.Initializer?.Value,
                VariableDeclaratorSyntax v => v.Initializer?.Value,
                LocalDeclarationStatementSyntax loc =>
                    loc.Declaration.Variables.FirstOrDefault()?.Initializer?.Value,
                MethodDeclarationSyntax m => m.ExpressionBody?.Expression,
                _ => null
            };
            if (focus is null)
            {
                detail = "no_initializer";
                return false;
            }

            detail = "member+Initializer";
            return true;
        }

        if (role.StartsWith("Parameter:", StringComparison.OrdinalIgnoreCase))
        {
            var paramName = role["Parameter:".Length..].Trim();
            if (paramName.Length == 0)
            {
                detail = "parameter_name_empty";
                return false;
            }

            SyntaxNode? methodNode = target as MethodDeclarationSyntax
                ?? target as LocalFunctionStatementSyntax
                ?? (SyntaxNode?)target.AncestorsAndSelf().OfType<MethodDeclarationSyntax>().FirstOrDefault()
                ?? target.AncestorsAndSelf().OfType<LocalFunctionStatementSyntax>().FirstOrDefault();

            SeparatedSyntaxList<ParameterSyntax>? parms = methodNode switch
            {
                MethodDeclarationSyntax m => m.ParameterList.Parameters,
                LocalFunctionStatementSyntax lf => lf.ParameterList.Parameters,
                _ => null
            };
            if (parms is null)
            {
                detail = "parameter_needs_method";
                return false;
            }

            var hit = parms.Value.FirstOrDefault(p => p.Identifier.Text.Equals(paramName, StringComparison.Ordinal));
            if (hit is null)
            {
                detail = $"parameter_not_found:{paramName}";
                return false;
            }

            focus = hit;
            detail = "member+Parameter";
            return true;
        }

        // Control-flow roles if target happens to be if/while/…
        if (TryApplyRole(target, role, out focus, out detail))
            return true;

        detail = $"unknown_role:{role}";
        return false;
    }

    private static bool TryApplyRole(
        SyntaxNode target,
        string role,
        out SyntaxNode? focus,
        out string detail)
    {
        focus = target;
        detail = "";

        var isCondition = role.Equals("Condition", StringComparison.OrdinalIgnoreCase);
        var isThen = role.Equals("Branch.True", StringComparison.OrdinalIgnoreCase)
                     || role.Equals("Then", StringComparison.OrdinalIgnoreCase);
        var isElse = role.Equals("Branch.False", StringComparison.OrdinalIgnoreCase)
                     || role.Equals("Else", StringComparison.OrdinalIgnoreCase);
        var isExpression = role.Equals("Expression", StringComparison.OrdinalIgnoreCase)
                           || role.Equals("Collection", StringComparison.OrdinalIgnoreCase);

        switch (target)
        {
            case IfStatementSyntax ifStmt:
                if (isCondition)
                {
                    focus = ifStmt.Condition;
                    return true;
                }

                if (isThen)
                {
                    focus = ifStmt.Statement;
                    return true;
                }

                if (isElse)
                {
                    if (ifStmt.Else is null)
                    {
                        detail = "no_else";
                        return false;
                    }

                    focus = ifStmt.Else.Statement;
                    return true;
                }

                detail = $"unknown_role:{role}";
                return false;

            case WhileStatementSyntax whileStmt:
                if (isCondition)
                {
                    focus = whileStmt.Condition;
                    return true;
                }

                if (isThen)
                {
                    focus = whileStmt.Statement;
                    return true;
                }

                detail = isElse ? "while_no_else" : $"unknown_role:{role}";
                return false;

            case ForStatementSyntax forStmt:
                if (isCondition)
                {
                    if (forStmt.Condition is null)
                    {
                        detail = "for_no_condition";
                        return false;
                    }

                    focus = forStmt.Condition;
                    return true;
                }

                if (isThen)
                {
                    focus = forStmt.Statement;
                    return true;
                }

                detail = isElse ? "for_no_else" : $"unknown_role:{role}";
                return false;

            case ForEachStatementSyntax foreachStmt:
                if (isCondition)
                {
                    detail = "foreach_no_condition_use_Expression";
                    return false;
                }

                if (isExpression)
                {
                    focus = foreachStmt.Expression;
                    return true;
                }

                if (isThen)
                {
                    focus = foreachStmt.Statement;
                    return true;
                }

                detail = isElse ? "foreach_no_else" : $"unknown_role:{role}";
                return false;

            default:
                detail = $"role_unsupported_scope:{target.Kind()}";
                return false;
        }
    }

    private static SyntaxNode? FindNodeAtLine(SyntaxTree tree, CompilationUnitSyntax root, SyntaxNode searchRoot, int line1Based)
    {
        var text = tree.GetText();
        if (line1Based < 1 || line1Based > text.Lines.Count)
            return null;
        var line = text.Lines[line1Based - 1];
        var span = line.Span;
        var lineText = text.ToString(line.Span);
        var trim = lineText.Length - lineText.TrimStart().Length;
        var pos = line.Start + Math.Min(trim, Math.Max(0, line.Span.Length - 1));
        var node = searchRoot.FindNode(new Microsoft.CodeAnalysis.Text.TextSpan(pos, 0), findInsideTrivia: false, getInnermostNodeForTie: true);
        return node == root ? null : node;
    }

    private static string MemberName(MemberDeclarationSyntax m) => m switch
    {
        MethodDeclarationSyntax method => method.Identifier.Text,
        ConstructorDeclarationSyntax ctor => ctor.Identifier.Text,
        PropertyDeclarationSyntax prop => prop.Identifier.Text,
        FieldDeclarationSyntax field => field.Declaration.Variables.FirstOrDefault()?.Identifier.Text ?? "",
        TypeDeclarationSyntax type => type.Identifier.Text,
        _ => ""
    };
}
