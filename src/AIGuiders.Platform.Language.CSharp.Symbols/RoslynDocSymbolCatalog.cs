#nullable enable

using AIGuiders.Platform.Documentation.Anchors;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AIGuiders.Platform.Language.CSharp.Symbols;

/// <summary>Syntax-tree symbol index for platform doc anchors (no MSBuild).</summary>
public sealed class RoslynDocSymbolCatalog : IDocSymbolCatalog
{
    readonly Dictionary<string, List<DocTypeEntry>> _types = new(StringComparer.Ordinal);

    sealed record DocTypeEntry(string Namespace, IReadOnlySet<string> Members);

    public static RoslynDocSymbolCatalog BuildFromSourceRoot(string srcRoot)
    {
        var catalog = new RoslynDocSymbolCatalog();
        if (!Directory.Exists(srcRoot))
            return catalog;

        foreach (var file in Directory.EnumerateFiles(srcRoot, "*.cs", SearchOption.AllDirectories))
        {
            if (file.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.Ordinal)
                || file.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.Ordinal))
                continue;

            var text = File.ReadAllText(file);
            var tree = CSharpSyntaxTree.ParseText(text);
            var root = tree.GetCompilationUnitRoot();
            var ns = root.DescendantNodes().OfType<BaseNamespaceDeclarationSyntax>().FirstOrDefault()?.Name.ToString() ?? "";
            foreach (var type in root.DescendantNodes().OfType<TypeDeclarationSyntax>())
            {
                if (string.IsNullOrWhiteSpace(type.Identifier.Text))
                    continue;

                var members = CollectMembers(type);
                catalog.Add(type.Identifier.Text, ns, members);
            }
        }

        return catalog;
    }

    static HashSet<string> CollectMembers(TypeDeclarationSyntax type)
    {
        var members = new HashSet<string>(StringComparer.Ordinal);
        if (type.ParameterList is { Parameters.Count: > 0 })
        {
            foreach (var parameter in type.ParameterList.Parameters)
                members.Add(parameter.Identifier.Text);
        }

        foreach (var member in type.Members)
        {
            switch (member)
            {
                case MethodDeclarationSyntax method:
                    members.Add(method.Identifier.Text);
                    break;
                case PropertyDeclarationSyntax property:
                    members.Add(property.Identifier.Text);
                    break;
                case FieldDeclarationSyntax field:
                    foreach (var v in field.Declaration.Variables)
                        members.Add(v.Identifier.Text);
                    break;
                case EventDeclarationSyntax evt:
                    members.Add(evt.Identifier.Text);
                    break;
            }
        }

        return members;
    }

    void Add(string typeName, string ns, IReadOnlySet<string> members)
    {
        if (!_types.TryGetValue(typeName, out var list))
        {
            list = [];
            _types[typeName] = list;
        }

        list.Add(new DocTypeEntry(ns, members));
    }

    public bool TypeExists(string typeName, string? packageHint)
    {
        if (!_types.TryGetValue(typeName, out var entries))
            return false;

        return packageHint is null
            ? entries.Count > 0
            : entries.Any(e => NamespaceMatches(e.Namespace, packageHint));
    }

    public bool MemberExists(string typeName, string memberName, string? packageHint)
    {
        if (!_types.TryGetValue(typeName, out var entries))
            return false;

        foreach (var entry in entries)
        {
            if (packageHint is not null && !NamespaceMatches(entry.Namespace, packageHint))
                continue;
            if (entry.Members.Contains(memberName))
                return true;
        }

        return false;
    }

    static bool NamespaceMatches(string ns, string packageHint)
    {
        if (string.IsNullOrWhiteSpace(ns))
            return false;

        var normalizedHint = packageHint;
        return ns.Contains(normalizedHint, StringComparison.Ordinal)
               || ns.EndsWith("." + normalizedHint, StringComparison.Ordinal)
               || ns.Equals("AIGuiders.Platform." + normalizedHint, StringComparison.Ordinal);
    }
}
