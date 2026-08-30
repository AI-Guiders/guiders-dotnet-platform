using AIGuiders.Platform.Documentation.Reports;

var repoRoot = FindRepoRoot(Directory.GetCurrentDirectory())
    ?? FindRepoRoot(AppContext.BaseDirectory)
    ?? Directory.GetCurrentDirectory();

var writePath = (string?)null;
for (var i = 0; i < args.Length; i++)
{
    if (args[i] is "--write" or "-o" && i + 1 < args.Length)
    {
        writePath = Path.GetFullPath(Path.Combine(repoRoot, args[++i]));
        continue;
    }

    if (args[i] is "--help" or "-h")
    {
        PrintHelp();
        return 0;
    }
}

var facts = NotationVocabularyBuilder.Build(Path.Combine(repoRoot, "src"));
var markdown = MarkdownNotationVocabularyWriter.Write(facts, DateTimeOffset.UtcNow);

if (writePath is not null)
{
    var dir = Path.GetDirectoryName(writePath);
    if (!string.IsNullOrEmpty(dir))
        Directory.CreateDirectory(dir);
    File.WriteAllText(writePath, markdown);
    Console.WriteLine($"Wrote {writePath} ({facts.ArgumentReaders.Count} readers, {facts.NotationPackages.Count} packages)");
}
else
{
    Console.Write(markdown);
}

return 0;

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

static void PrintHelp()
{
    Console.WriteLine("""
        notation-glossary-report — federation notation vocabulary table generator

        Usage:
          dotnet run --project tools/NotationGlossaryReport -- [options]

        Options:
          --write, -o <path>   Write markdown (relative to repo root)
          --help, -h           Show help
        """);
}
