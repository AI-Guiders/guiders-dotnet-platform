#nullable enable

using System.Reflection;
using AIGuiders.Platform.IntermediateRepresentation.Argument;

namespace AIGuiders.Platform.Execution.Documentation.Reports;

public sealed record ArgumentReaderRow(
    string ReaderId,
    string ConstantName,
    string CatalogField,
    string CatalogValue);

public sealed record NotationPackageRow(string PackageId, string HyperlaneHint);

public sealed record NotationVocabularyFactSet(
    IReadOnlyList<ArgumentReaderRow> ArgumentReaders,
    IReadOnlyList<NotationPackageRow> NotationPackages);

public static class NotationVocabularyBuilder
{
    public static NotationVocabularyFactSet Build(string srcRoot)
    {
        var readers = typeof(ArgumentReaders)
            .GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(f => f.FieldType == typeof(string) && f.IsLiteral)
            .Select(f => new ArgumentReaderRow(
                (string)f.GetRawConstantValue()!,
                f.Name,
                "tail_wire_class",
                (string)f.GetRawConstantValue()!))
            .OrderBy(r => r.ReaderId, StringComparer.Ordinal)
            .ToList();

        var packages = Directory.Exists(srcRoot)
            ? Directory.EnumerateFiles(srcRoot, "AIGuiders.Platform.Notations.*.csproj", SearchOption.AllDirectories)
                .Select(path => Path.GetFileNameWithoutExtension(path))
                .Where(id => id is not null)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(id => id, StringComparer.OrdinalIgnoreCase)
                .Select(id => new NotationPackageRow(id!, HyperlaneFromPackageId(id!)))
                .ToList()
            : [];

        return new NotationVocabularyFactSet(readers, packages);
    }

    static string HyperlaneFromPackageId(string packageId)
    {
        if (packageId.Contains(".Notations.Argument", StringComparison.Ordinal))
            return "Notations.Argument";
        if (packageId.Contains(".Notations.Command", StringComparison.Ordinal))
            return "Notations.Command";
        if (packageId.Contains(".Notations.Bracket", StringComparison.Ordinal))
            return "Notations.Bracket";
        if (packageId.Contains(".Notations.Keyboard", StringComparison.Ordinal))
            return "Notations.Keyboard";
        return "Notations";
    }
}
