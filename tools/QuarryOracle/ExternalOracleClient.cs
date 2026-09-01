using AIGuiders.Platform.IntermediateRepresentation.Keyboard;
#nullable enable
using AIGuiders.Platform.Notations.Keyboard;
using AIGuiders.Platform.Notations.Keyboard.Quarry;

namespace AIGuiders.Platform.Tools.QuarryOracle;

public static class ExternalOracleClient
{
    public static bool TryResolveNeovim(out string? executable, out string error)
    {
        executable = FindOnPath("nvim");
        if (executable is null)
        {
            error = "neovim not found on PATH.";
            return false;
        }

        error = "";
        return true;
    }

    public static bool TryResolveEmacs(out string? executable, out string error)
    {
        executable = FindOnPath("emacs");
        if (executable is null)
        {
            error = "emacs not found on PATH.";
            return false;
        }

        error = "";
        return true;
    }

    public static bool TryDecodeNeovim(string wire, out NormalizedKeySequence? sequence, out string error)
    {
        sequence = null;
        if (!TryResolveNeovim(out var nvim, out error))
            return false;

        return TryRunNeovim(nvim!, wire, out sequence, out error);
    }

    public static bool TryDecodeEmacs(string wire, out NormalizedKeySequence? sequence, out string error)
    {
        sequence = null;
        if (!TryResolveEmacs(out var emacs, out error))
            return false;

        return TryRunEmacs(emacs!, wire, out sequence, out error);
    }

    public static bool TryCompareWireOnce(
        string surface,
        IKeyboardNotationReader reader,
        string wire,
        out NormalizedKeySequence? platform,
        out NormalizedKeySequence? oracle,
        out string error)
    {
        platform = null;
        oracle = null;
        error = "";

        if (!reader.TryParseToNormalized(wire, out platform, out var parseError))
        {
            error = $"[{wire}] platform parse failed: {parseError}";
            return false;
        }

        if (platform is null)
        {
            error = $"[{wire}] platform parser returned null sequence.";
            return false;
        }

        var decoded = surface switch
        {
            "neovim-kbd" => TryDecodeNeovim(wire, out oracle, out error),
            "emacs-kbd" => TryDecodeEmacs(wire, out oracle, out error),
            _ => Fail($"unsupported surface \"{surface}\".", out error),
        };

        if (!decoded || oracle is null)
        {
            error = $"[{wire}] oracle decode failed: {error}";
            return false;
        }

        if (!QuarryIrComparer.SequencesEqual(platform, oracle, out var diff))
        {
            error =
                $"[{wire}] IR mismatch: {diff}. platform={QuarryIrComparer.FormatSequence(platform)}; oracle={QuarryIrComparer.FormatSequence(oracle)}";
            return false;
        }

        return true;
    }

    static bool TryRunNeovim(string nvim, string wire, out NormalizedKeySequence? sequence, out string error)
    {
        sequence = null;
        if (!ProcessRunner.TryRun(
                nvim,
                ["--headless", "-u", "NONE", "-l", ScriptPath("neovim-oracle.lua")],
                new Dictionary<string, string> { ["QUARRY_ORACLE_WIRE"] = wire },
                out var stdout,
                out error))
        {
            return false;
        }

        return OracleJsonParser.TryParse(stdout, out sequence, out error);
    }

    static bool TryRunEmacs(string emacs, string wire, out NormalizedKeySequence? sequence, out string error)
    {
        sequence = null;
        if (!ProcessRunner.TryRun(
                emacs,
                ["-batch", "-l", ScriptPath("emacs-oracle.el")],
                new Dictionary<string, string> { ["QUARRY_ORACLE_WIRE"] = wire },
                out var stdout,
                out error))
        {
            return false;
        }

        return OracleJsonParser.TryParse(stdout, out sequence, out error);
    }

    static string ScriptPath(string fileName)
    {
        var baseDir = AppContext.BaseDirectory;
        var candidate = Path.Combine(baseDir, "scripts", fileName);
        if (File.Exists(candidate))
            return candidate;

        var dev = Path.GetFullPath(Path.Combine(baseDir, "..", "..", "..", "scripts", fileName));
        if (File.Exists(dev))
            return dev;

        throw new FileNotFoundException($"Oracle script not found: {fileName}", candidate);
    }

    static string? FindOnPath(string name)
    {
        var path = Environment.GetEnvironmentVariable("PATH");
        if (!string.IsNullOrEmpty(path))
        {
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
        }

        if (OperatingSystem.IsWindows() && string.Equals(name, "emacs", StringComparison.OrdinalIgnoreCase))
        {
            var roots = new[]
            {
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Emacs"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Emacs"),
            };

            foreach (var root in roots)
            {
                if (!Directory.Exists(root))
                    continue;

                foreach (var candidate in Directory.EnumerateFiles(root, "emacs.exe", SearchOption.AllDirectories))
                    return candidate;
            }
        }

        return null;
    }

    static bool Fail(string message, out string error)
    {
        error = message;
        return false;
    }
}
