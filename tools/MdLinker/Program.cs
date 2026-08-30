using AIGuiders.Platform.Documentation.LinkCheck;
using AIGuiders.Platform.Language.CSharp.Symbols;

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
    paths.Add(Path.Combine(repoRoot, "docs", "adr"));

var srcRoot = Path.Combine(repoRoot, "src");
var catalog = RoslynDocSymbolCatalog.BuildFromSourceRoot(srcRoot);
var failures = DocAnchorChecker.CheckMarkdownRoots(paths, catalog);

if (failures.Count > 0)
{
    foreach (var failure in failures)
        Console.Error.WriteLine(failure);
    return 1;
}

Console.WriteLine($"mdlinker: ok ({paths.Count} root path(s))");
return 0;

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
        Packages: Documentation.LinkCheck + Language.CSharp.Symbols
        """);
}
