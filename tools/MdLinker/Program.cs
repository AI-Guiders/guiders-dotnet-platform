using AIGuiders.Platform.Documentation.LinkCheck;
using AIGuiders.Platform.Documentation.LinkMutate;
using AIGuiders.Platform.Language.CSharp.Symbols;

var repoRoot = FindRepoRoot(Directory.GetCurrentDirectory())
    ?? FindRepoRoot(AppContext.BaseDirectory)
    ?? Directory.GetCurrentDirectory();

var check = false;
var applyRename = false;
var dryRun = false;
string? oldName = null;
string? newName = null;
var renameKind = DocSymbolRenameKind.Member;
var paths = new List<string>();

for (var i = 0; i < args.Length; i++)
{
    switch (args[i])
    {
        case "--check":
            check = true;
            break;
        case "--apply-rename":
            applyRename = true;
            break;
        case "--dry-run":
            dryRun = true;
            break;
        case "--kind" when i + 1 < args.Length:
            renameKind = args[++i] switch
            {
                "type" or "Type" => DocSymbolRenameKind.Type,
                "member" or "Member" => DocSymbolRenameKind.Member,
                _ => throw new ArgumentException($"unknown --kind {args[i]} (use type|member)")
            };
            break;
        case "--help" or "-h":
            PrintHelp();
            return 0;
        default:
            if (applyRename && oldName is null)
            {
                oldName = args[i];
                break;
            }

            if (applyRename && newName is null)
            {
                newName = args[i];
                break;
            }

            paths.Add(Path.GetFullPath(Path.IsPathRooted(args[i]) ? args[i] : Path.Combine(repoRoot, args[i])));
            break;
    }
}

if (!check && !applyRename)
{
    PrintHelp();
    return 1;
}

if (paths.Count == 0)
    paths.Add(Path.Combine(repoRoot, "docs", "adr"));

if (applyRename)
{
    if (string.IsNullOrWhiteSpace(oldName) || string.IsNullOrWhiteSpace(newName))
    {
        Console.Error.WriteLine("mdlinker: --apply-rename requires <oldName> <newName> [paths...]");
        return 1;
    }

    var result = DocAnchorRenamer.ApplyRename(paths, oldName, newName, renameKind, dryRun);
    Console.WriteLine(
        $"mdlinker: rename {(dryRun ? "dry-run " : "")}{oldName} → {newName} ({renameKind}) wires={result.WiresChanged} files={result.FilesChanged}");
    return 0;
}

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
        if (File.Exists(Path.Combine(dir.FullName, "AIGuiders.Platform.slnx")))
            return dir.FullName;
        dir = dir.Parent;
    }

    return null;
}

static void PrintHelp()
{
    Console.WriteLine("""
        mdlinker — markdown doc anchor check / patch (GUIDERS-ADR-0027)

        Usage:
          dotnet run --project tools/MdLinker -- --check [paths...]
          dotnet run --project tools/MdLinker -- --apply-rename <old> <new> [--kind type|member] [--dry-run] [paths...]

        Options:
          --check              Fail on unresolved Family:doc anchors
          --apply-rename       Patch Type/Member axes in doc bracket wires
          --kind type|member   Rename axis (default: member)
          --dry-run            Preview apply-rename without writing
          --help, -h           Show help

        Default path: docs/adr/
        Packages: Documentation.LinkCheck / LinkMutate + Language.CSharp.Symbols
        """);
}
