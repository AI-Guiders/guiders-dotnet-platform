using AIGuiders.Platform.Utilities.Adoption;
using AIGuiders.Platform.Utilities.Adoption.Reports.Markdown;
using AIGuiders.Platform.Utilities.Adoption.Sources;

var repoRoot = FindRepoRoot(Directory.GetCurrentDirectory())
    ?? FindRepoRoot(AppContext.BaseDirectory)
    ?? Directory.GetCurrentDirectory();

var planetsPath = Path.Combine(repoRoot, "docs", "adoption", "planets.json");
var hyperlanePath = Path.Combine(repoRoot, "docs", "adoption", "hyperlane-map.json");
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

if (!File.Exists(planetsPath) || !File.Exists(hyperlanePath))
{
    Console.Error.WriteLine($"Missing config under {Path.Combine(repoRoot, "docs", "adoption")}");
    return 1;
}

var config = AdoptionConfigLoader.Load(planetsPath, hyperlanePath);
var sources = new IAdoptionSource[] { AdoptionSources.FromPlanetTree() };
var rows = AdoptionAllianceBuilder.BuildAll(config, Path.GetDirectoryName(planetsPath)!, sources);
var markdown = new MarkdownAllianceReportWriter().Write(rows, DateTimeOffset.UtcNow);

if (writePath is not null)
{
    var dir = Path.GetDirectoryName(writePath);
    if (!string.IsNullOrEmpty(dir))
        Directory.CreateDirectory(dir);
    File.WriteAllText(writePath, markdown);
    Console.WriteLine($"Wrote {writePath} ({rows.Count} rows)");
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
        adoption-report — federation adoption alliance table generator

        Usage:
          dotnet run --project tools/AdoptionReport -- [options]

        Options:
          --write, -o <path>   Write markdown (relative to repo root)
          --help, -h             Show help

        Config:
          docs/adoption/planets.json
          docs/adoption/hyperlane-map.json
        """);
}
