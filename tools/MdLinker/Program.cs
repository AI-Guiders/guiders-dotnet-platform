using AIGuiders.Platform.LanguageIntelligence.Adapters.Roslyn;
using AIGuiders.Platform.LanguageIntelligence.Anchors;
using AIGuiders.Platform.Notations.Bracket;

var repoRoot = FindRepoRoot(Directory.GetCurrentDirectory())
    ?? FindRepoRoot(AppContext.BaseDirectory)
    ?? Directory.GetCurrentDirectory();

var check = false;
var paths = new List<string>();
for (var i = 0; i < args.Length; i++)
{
    switch (args[i])
    {
        case "--check":
            check = true;
            break;
        case "--help" or "-h":
            PrintHelp();
            return 0;
        default:
            paths.Add(Path.GetFullPath(Path.IsPathRooted(args[i]) ? args[i] : Path.Combine(repoRoot, args[i])));
            break;
    }
}

if (!check)
{
    PrintHelp();
    return 1;
}

if (paths.Count == 0)
{
    paths.Add(Path.Combine(repoRoot, "docs", "adr"));
}

var srcRoot = Path.Combine(repoRoot, "src");
var catalog = RoslynDocSymbolCatalog.BuildFromSourceRoot(srcRoot);
var resolver = new DocSymbolAnchorResolver(catalog);
var profile = BracketProfiles.DocSymbol;
var failures = new List<string>();

foreach (var path in paths)
{
    if (File.Exists(path) && path.EndsWith(".md", StringComparison.OrdinalIgnoreCase))
        CheckFile(path, profile, resolver, failures);
    else if (Directory.Exists(path))
    {
        foreach (var file in Directory.EnumerateFiles(path, "*.md", SearchOption.AllDirectories))
            CheckFile(file, profile, resolver, failures);
    }
    else
        failures.Add($"{path}: not found");
}

if (failures.Count > 0)
{
    foreach (var failure in failures)
        Console.Error.WriteLine(failure);
    return 1;
}

Console.WriteLine($"mdlinker: ok ({paths.Count} root path(s))");
return 0;

static void CheckFile(
    string filePath,
    BracketNotationProfile profile,
    DocSymbolAnchorResolver resolver,
    List<string> failures)
{
    var text = File.ReadAllText(filePath);
    foreach (var envelope in BracketEnvelopeScan.LocateInText(text))
    {
        if (!LooksLikeDocAnchor(envelope.Inner))
            continue;

        if (!BracketReader.Default.TryRead(
                envelope.Wire,
                profile,
                BracketAxisValuePlans.DocSymbol,
                out var wire,
                out var parseError) || wire is null)
        {
            failures.Add($"{filePath}:{text.Take(envelope.Start).Count(c => c == '\n') + 1}: parse:{parseError}");
            continue;
        }

        if (!HasDocFamily(wire))
            continue;

        if (!resolver.TryResolve(wire, out var resolveError))
        {
            var line = text.Take(envelope.Start).Count(c => c == '\n') + 1;
            failures.Add($"{filePath}:{line}: {resolveError} wire={envelope.Wire}");
        }
    }
}

static bool LooksLikeDocAnchor(string inner) =>
    inner.Contains("Family:", StringComparison.OrdinalIgnoreCase)
    && inner.Contains("doc", StringComparison.OrdinalIgnoreCase);

static bool HasDocFamily(NormalizedBracketWire wire)
{
    foreach (var axis in wire.Axes)
    {
        if (axis.Key.Equals("Family", StringComparison.OrdinalIgnoreCase)
            && axis.Value.Equals("doc", StringComparison.OrdinalIgnoreCase))
            return true;
    }

    return false;
}

static string? FindRepoRoot(string start)
{
    var dir = new DirectoryInfo(Path.GetFullPath(start));
    while (dir is not null)
    {
        if (File.Exists(Path.Combine(dir.FullName, "AIGuiders.Platform.sln")))
            return dir.FullName;
        dir = dir.Parent;
    }

    return null;
}

static void PrintHelp()
{
    Console.WriteLine("""
        mdlinker — markdown doc anchor dry-resolve (GUIDERS-ADR-0027)

        Usage:
          dotnet run --project tools/MdLinker -- --check [paths...]

        Options:
          --check          Fail on unresolved Family:doc anchors
          --help, -h       Show help

        Default path: docs/adr/
        """);
}
