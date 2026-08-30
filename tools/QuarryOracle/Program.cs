using System.Diagnostics;
using System.Text.Json;
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
        var spec = QuarrySpecLoader.Load(json);
        var reader = ResolveReader(spec.Surface);

        var conformanceErrors = QuarrySpecConformance.ValidateDocument(reader, spec);
        if (conformanceErrors.Count > 0)
        {
            foreach (var error in conformanceErrors)
                Console.Error.WriteLine(error);
            return 1;
        }

        var oracle = spec.Surface switch
        {
            "neovim-kbd" => TryAuditNeovim(spec, require),
            "emacs-kbd" => TryAuditEmacs(spec, require),
            _ => (Skipped: true, Errors: new List<string>()),
        };

        if (oracle.Errors.Count > 0)
        {
            foreach (var error in oracle.Errors)
                Console.Error.WriteLine(error);
            return 1;
        }

        if (oracle.Skipped)
            Console.WriteLine($"OK {spec.Surface} (parser conformance; oracle skipped — install neovim/emacs for external audit)");

        else
            Console.WriteLine($"OK {spec.Surface} (parser + oracle)");

        return 0;
    }

    static (bool Skipped, List<string> Errors) TryAuditNeovim(QuarrySpecDocument spec, bool require)
    {
        var nvim = FindOnPath("nvim") ?? FindOnPath("vim");
        if (nvim is null)
        {
            if (require)
                return (false, ["neovim/vim not found on PATH (--require)."]);
            return (true, []);
        }

        var errors = new List<string>();
        foreach (var vector in spec.Vectors)
        {
            if (!TryRunNeovimKeytrans(nvim, vector.Wire, out var trans, out var runError))
            {
                errors.Add($"[{vector.Wire}] oracle failed: {runError}");
                continue;
            }

            if (string.IsNullOrWhiteSpace(trans))
                errors.Add($"[{vector.Wire}] oracle returned empty keytrans.");
        }

        return (false, errors);
    }

    static (bool Skipped, List<string> Errors) TryAuditEmacs(QuarrySpecDocument spec, bool require)
    {
        var emacs = FindOnPath("emacs");
        if (emacs is null)
        {
            if (require)
                return (false, ["emacs not found on PATH (--require)."]);
            return (true, []);
        }

        var errors = new List<string>();
        foreach (var vector in spec.Vectors)
        {
            var wire = vector.Wire.Replace("\\", "\\\\").Replace("\"", "\\\"");
            var expr = $"(progn (key-parse \"{wire}\") (print \"OK\"))";
            if (!TryRunProcess(emacs, ["-batch", "--eval", expr], out var stdout, out var stderr, out var exitCode)
                || exitCode != 0)
            {
                errors.Add($"[{vector.Wire}] emacs key-parse failed: {stderr.Trim()} {stdout.Trim()}");
            }
        }

        return (false, errors);
    }

    static bool TryRunNeovimKeytrans(string nvim, string wire, out string trans, out string error)
    {
        trans = "";
        error = "";
        var escaped = JsonSerializer.Serialize(wire);
        var lua = $"print(vim.fn.keytrans({escaped}))";
        if (!TryRunProcess(nvim, ["--headless", "-u", "NONE", "--cmd", $"lua {lua}", "--cmd", "qa!"],
                out var stdout, out var stderr, out var exitCode)
            || exitCode != 0)
        {
            error = stderr.Trim();
            return false;
        }

        trans = stdout.Trim();
        return true;
    }

    static bool TryRunProcess(string file, string[] args, out string stdout, out string stderr, out int exitCode)
    {
        stdout = "";
        stderr = "";
        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = file,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };
            foreach (var arg in args)
                startInfo.ArgumentList.Add(arg);

            using var process = Process.Start(startInfo);
            if (process is null)
            {
                exitCode = -1;
                return false;
            }

            stdout = process.StandardOutput.ReadToEnd();
            stderr = process.StandardError.ReadToEnd();
            process.WaitForExit();
            exitCode = process.ExitCode;
            return true;
        }
        catch (Exception ex)
        {
            stderr = ex.Message;
            exitCode = -1;
            return false;
        }
    }

    static string? FindOnPath(string name)
    {
        var path = Environment.GetEnvironmentVariable("PATH");
        if (string.IsNullOrEmpty(path))
            return null;

        var extensions = OperatingSystem.IsWindows()
            ? new[] { "", ".exe", ".cmd", ".bat" }
            : new[] { "" };

        foreach (var dir in path.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries))
        {
            foreach (var ext in extensions)
            {
                var candidate = Path.Combine(dir, name + ext);
                if (File.Exists(candidate))
                    return candidate;
            }
        }

        return null;
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
            quarry-oracle — keyboard quarry spec + optional external oracle audit

            Commands:
              verify --spec <path>           Parser vs frozen vectors (QuarrySpecConformance)
              audit  --spec <path> [--require] Parser + neovim/emacs key-parse/keytrans when on PATH

            Surfaces: neovim-kbd, emacs-kbd
            """);
    }
}
