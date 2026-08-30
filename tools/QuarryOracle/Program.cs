using System.Diagnostics;
using System.Text.Json;
using AIGuiders.Platform.Conformance.Schemas;
using AIGuiders.Platform.Notations.Keyboard;
using AIGuiders.Platform.Notations.Keyboard.Quarry;

namespace AIGuiders.Platform.Tools.QuarryOracle;

static class Program
{
    public static int Main(string[] args)
    {
        if (args.Length == 0 || args is ["-h"] or ["--help"])
        {
            PrintHelp();
            return 0;
        }

        return args[0] switch
        {
            "verify" => VerifySpec(args),
            "audit" => AuditOracle(args),
            _ => Unknown(args[0]),
        };
    }

    static int VerifySpec(string[] args)
    {
        var specPath = RequireOption(args, "--spec");
        var json = File.ReadAllText(specPath);
        if (!ValidateSchema(json, out var schemaExit))
            return schemaExit;

        var spec = QuarrySpecLoader.Load(json);
        var reader = ResolveReader(spec.Surface);
        var errors = QuarrySpecConformance.ValidateDocument(reader, spec);
        if (errors.Count == 0)
        {
            Console.WriteLine($"OK {spec.Surface} ({spec.Vectors.Count} vectors)");
            return 0;
        }

        foreach (var error in errors)
            Console.Error.WriteLine(error);
        return 1;
    }

    static int AuditOracle(string[] args)
    {
        var specPath = RequireOption(args, "--spec");
        var require = args.Contains("--require");
        var json = File.ReadAllText(specPath);
        if (!ValidateSchema(json, out var schemaExit))
            return schemaExit;

        var spec = QuarrySpecLoader.Load(json);
        var reader = ResolveReader(spec.Surface);

        var conformanceErrors = QuarrySpecConformance.ValidateDocument(reader, spec);
        if (conformanceErrors.Count > 0)
        {
            foreach (var error in conformanceErrors)
                Console.Error.WriteLine(error);
            return 1;
        }

        if (!TryResolveOracle(spec.Surface, require, out var resolveError))
        {
            if (require)
            {
                Console.Error.WriteLine(resolveError);
                return 1;
            }

            Console.WriteLine($"OK {spec.Surface} (parser conformance; external oracle skipped — {resolveError})");
            return 0;
        }

        var oracleErrors = new List<string>();
        var matched = 0;
        foreach (var vector in spec.Vectors)
        {
            if (!ExternalOracleClient.TryCompareWireOnce(
                    spec.Surface,
                    reader,
                    vector.Wire,
                    out var platform,
                    out var oracle,
                    out var error))
            {
                oracleErrors.Add(error);
                continue;
            }

            matched++;
            Console.WriteLine($"  match {vector.Wire} -> {QuarryIrComparer.FormatSequence(platform!)}");
        }

        if (oracleErrors.Count > 0)
        {
            foreach (var error in oracleErrors)
                Console.Error.WriteLine(error);
            return 1;
        }

        Console.WriteLine($"OK {spec.Surface} (parser + external oracle IR, {matched}/{spec.Vectors.Count} vectors)");
        return 0;
    }

    static bool ValidateSchema(string json, out int exitCode)
    {
        exitCode = 0;
        var schemaErrors = ConformanceSchemaValidator.ValidateJson(json);
        if (schemaErrors.Count == 0)
            return true;

        foreach (var error in schemaErrors)
            Console.Error.WriteLine(error);
        exitCode = 1;
        return false;
    }

    static bool TryResolveOracle(string surface, bool require, out string error)
    {
        error = "";
        return surface switch
        {
            "neovim-kbd" => ExternalOracleClient.TryResolveNeovim(out _, out error),
            "emacs-kbd" => ExternalOracleClient.TryResolveEmacs(out _, out error),
            _ => true,
        };
    }

    static IKeyboardNotationReader ResolveReader(string surface) => surface switch
    {
        "neovim-kbd" => NeovimNotationReader.Instance,
        "emacs-kbd" => EmacsNotationReader.Instance,
        _ => throw new InvalidOperationException($"Unsupported quarry surface \"{surface}\"."),
    };

    static string RequireOption(string[] args, string name)
    {
        for (var i = 0; i < args.Length - 1; i++)
        {
            if (string.Equals(args[i], name, StringComparison.OrdinalIgnoreCase))
                return args[i + 1];
        }

        throw new InvalidOperationException($"Missing required option {name}.");
    }

    static int Unknown(string command)
    {
        Console.Error.WriteLine($"Unknown command: {command}");
        PrintHelp();
        return 2;
    }

    static void PrintHelp()
    {
        Console.WriteLine("""
            quarry-oracle — keyboard quarry spec + external IR oracle audit

            Commands:
              verify --spec <path>              JSON Schema + parser vs frozen vectors
              audit  --spec <path> [--require] Parser + external neovim/emacs IR compare

            Surfaces: neovim-kbd, emacs-kbd
            """);
    }
}
