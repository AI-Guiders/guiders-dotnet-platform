#nullable enable

using AIGuiders.Platform.Modeling.Ide.Session;
using ProjectId = AIGuiders.Platform.Modeling.Core.Identity.ProjectId;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AIGuiders.Platform.Execution.Language.CSharp.Relations;

/// <summary>Roslyn E_dep ingest — emits validated Dependency / TypeSystem edges (plan §2.3).</summary>
public static class RoslynDependencyRelationIngest
{
    public static IReadOnlyList<Relation> IngestFromSource(
        string logicalPath,
        string sourceText,
        ProjectId projectId) =>
        IngestProjectSources([(logicalPath, sourceText)], projectId);

    public static IReadOnlyList<Relation> IngestFromSource(
        string logicalPath,
        string sourceText,
        ProjectId projectId,
        IReadOnlyList<string> compilationSources)
    {
        var sources = compilationSources
            .Select((text, index) => (Path: index == 0 ? logicalPath : $"{logicalPath}#{index}", Text: text))
            .ToArray();
        return IngestProjectSources(sources, projectId);
    }

    public static IReadOnlyList<Relation> IngestProjectSources(
        IReadOnlyList<(string Path, string Text)> sources,
        ProjectId projectId)
    {
        if (sources.Count == 0)
            return Array.Empty<Relation>();

        var trees = sources
            .Select(source => CSharpSyntaxTree.ParseText(source.Text, path: source.Path))
            .ToArray();

        var compilation = CSharpCompilation.Create(
                assemblyName: "RoslynDependencyRelationIngest",
                syntaxTrees: trees,
                references: [MetadataReference.CreateFromFile(typeof(object).Assembly.Location)])
            .WithOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var relations = new List<Relation>();

        for (var i = 0; i < trees.Length; i++)
        {
            var tree = trees[i];
            var logicalPath = sources[i].Path;
            var model = compilation.GetSemanticModel(tree);

            foreach (var classDecl in tree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>())
            {
                if (model.GetDeclaredSymbol(classDecl) is not INamedTypeSymbol classSymbol)
                    continue;

                EmitClassRelations(logicalPath, projectId, classSymbol, classDecl, model, relations);
            }
        }

        return relations;
    }

    public static IReadOnlyList<Relation> IngestUsesFromSource(
        string logicalPath,
        string sourceText,
        ProjectId projectId) =>
        IngestFromSource(logicalPath, sourceText, projectId);

    static void EmitClassRelations(
        string logicalPath,
        ProjectId projectId,
        INamedTypeSymbol classSymbol,
        ClassDeclarationSyntax classDecl,
        SemanticModel model,
        List<Relation> relations)
    {
        var className = classSymbol.Name;

        if (classSymbol.BaseType is { TypeKind: TypeKind.Class } baseType
            && !string.Equals(baseType.Name, "Object", StringComparison.Ordinal)
            && !string.Equals(baseType.Name, className, StringComparison.Ordinal))
        {
            TryAdd(
                relations,
                CorrespondenceMaterialize.buildExtendsFromTypeNames(
                    logicalPath,
                    className,
                    baseType.Name,
                    projectId));
        }

        foreach (var iface in classSymbol.Interfaces)
        {
            if (string.Equals(iface.Name, className, StringComparison.Ordinal))
                continue;

            TryAdd(
                relations,
                CorrespondenceMaterialize.buildImplementsInterfaceFromTypeNames(
                    logicalPath,
                    className,
                    iface.Name,
                    projectId));
        }

        foreach (var field in classDecl.Members.OfType<FieldDeclarationSyntax>())
            TryAddTypeUse(
                relations,
                logicalPath,
                className,
                model.GetTypeInfo(field.Declaration.Type).Type,
                projectId,
                RelationType.Uses);

        foreach (var property in classDecl.Members.OfType<PropertyDeclarationSyntax>())
            TryAddTypeUse(
                relations,
                logicalPath,
                className,
                model.GetTypeInfo(property.Type).Type,
                projectId,
                RelationType.TypeUses);

        foreach (var method in classDecl.Members.OfType<MethodDeclarationSyntax>())
        {
            if (model.GetDeclaredSymbol(method) is not IMethodSymbol methodSymbol)
                continue;

            TryAddTypeUse(
                relations,
                logicalPath,
                className,
                methodSymbol.ReturnType,
                projectId,
                RelationType.TypeUses);

            foreach (var parameter in methodSymbol.Parameters)
                TryAddTypeUse(
                    relations,
                    logicalPath,
                    className,
                    parameter.Type,
                    projectId,
                    RelationType.TypeUses);
        }
    }

    static void TryAddTypeUse(
        List<Relation> relations,
        string logicalPath,
        string className,
        ITypeSymbol? usedType,
        ProjectId projectId,
        RelationType relationType)
    {
        if (usedType is not INamedTypeSymbol named || named.TypeKind != TypeKind.Class)
            return;

        if (string.Equals(named.Name, className, StringComparison.Ordinal))
            return;

        Relation relation =
            relationType == RelationType.Uses
                ? CorrespondenceMaterialize.buildUsesFromTypeNames(logicalPath, className, named.Name, projectId)
                : CorrespondenceMaterialize.buildTypeUsesFromTypeNames(logicalPath, className, named.Name, projectId);

        TryAdd(relations, relation);
    }

    static void TryAdd(List<Relation> relations, Relation relation)
    {
        if (RelationGraph.validateRelation(relation).IsOk)
            relations.Add(relation);
    }
}
