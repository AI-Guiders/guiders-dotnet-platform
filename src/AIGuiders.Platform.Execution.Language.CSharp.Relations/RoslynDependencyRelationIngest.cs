#nullable enable

using AIGuiders.Platform.Modeling.Ide.Session;
using ProjectId = AIGuiders.Platform.Modeling.Core.Identity.ProjectId;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AIGuiders.Platform.Execution.Language.CSharp.Relations;

/// <summary>Roslyn E_dep ingest — emits validated Dependency Uses edges (plan §2.3).</summary>
public static class RoslynDependencyRelationIngest
{
    public static IReadOnlyList<Relation> IngestUsesFromSource(
        string logicalPath,
        string sourceText,
        ProjectId projectId)
    {
        var tree = CSharpSyntaxTree.ParseText(sourceText, path: logicalPath);
        var compilation = CSharpCompilation.Create(
                assemblyName: "RoslynDependencyRelationIngest",
                syntaxTrees: [tree],
                references: [MetadataReference.CreateFromFile(typeof(object).Assembly.Location)])
            .WithOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var model = compilation.GetSemanticModel(tree);
        var relations = new List<Relation>();

        foreach (var classDecl in tree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>())
        {
            var classSymbol = model.GetDeclaredSymbol(classDecl) as INamedTypeSymbol;
            if (classSymbol is null)
                continue;

            foreach (var field in classDecl.Members.OfType<FieldDeclarationSyntax>())
            {
                var usedType = model.GetTypeInfo(field.Declaration.Type).Type as INamedTypeSymbol;
                if (usedType is null || usedType.TypeKind != TypeKind.Class)
                    continue;

                if (string.Equals(usedType.Name, classSymbol.Name, StringComparison.Ordinal))
                    continue;

                var relation = CorrespondenceMaterialize.buildUsesFromTypeNames(
                    logicalPath,
                    classSymbol.Name,
                    usedType.Name,
                    projectId);

                if (RelationGraph.validateRelation(relation).IsOk)
                    relations.Add(relation);
            }
        }

        return relations;
    }
}
