#nullable enable

using AIGuiders.Platform.Execution.Documentation.Reports;
using AIGuiders.Platform.IntermediateRepresentation.Argument;
using Xunit;

namespace AIGuiders.Platform.Tests;

public sealed class NotationVocabularyTests
{
    [Fact]
    public void Build_includes_all_ArgumentReaders_constants()
    {
        var repoRoot = FindRepoRoot(AppContext.BaseDirectory)
            ?? throw new InvalidOperationException("repo root not found");
        var facts = NotationVocabularyBuilder.Build(Path.Combine(repoRoot, "src"));

        Assert.Contains(facts.ArgumentReaders, r => r.ReaderId == ArgumentReaders.Cli && r.ConstantName == nameof(ArgumentReaders.Cli));
        Assert.Contains(facts.ArgumentReaders, r => r.ReaderId == ArgumentReaders.Kv && r.CatalogField == "tail_wire_class");
        Assert.True(facts.ArgumentReaders.Count >= 6);
    }

    [Fact]
    public void Write_emits_reader_table_and_regenerate_command()
    {
        var facts = new NotationVocabularyFactSet(
            [new ArgumentReaderRow("cli", "Cli", "tail_wire_class", "cli")],
            [new NotationPackageRow("AIGuiders.Platform.Notations.Argument", "Notations.Argument")]);

        var md = MarkdownNotationVocabularyWriter.Write(facts, DateTimeOffset.Parse("2026-08-30T00:00:00Z"));

        Assert.Contains("NotationGlossaryReport", md, StringComparison.Ordinal);
        Assert.Contains("| `cli` | `Cli` | `tail_wire_class` | `cli` |", md, StringComparison.Ordinal);
        Assert.Contains("AIGuiders.Platform.Notations.Argument", md, StringComparison.Ordinal);
    }

    static string? FindRepoRoot(string start)
    {
        var dir = new DirectoryInfo(Path.GetFullPath(start));
        while (dir is not null)
        {
            if (File.Exists(Path.Combine(dir.FullName, "AIGuiders.Platform.slnx"))
                || File.Exists(Path.Combine(dir.FullName, "AIGuiders.Platform.sln")))
                return dir.FullName;
            dir = dir.Parent;
        }

        return null;
    }
}
