#nullable enable

using System.Diagnostics;
using System.Text.RegularExpressions;
using AIGuiders.Platform.Modeling.Paths;

namespace AIGuiders.Platform.Execution.Ide.Session;

/// <summary>
/// Execution build phase producer: subprocess <c>dotnet build</c> → MSBuild diagnostic wires (plan §7 / §10 Build).
/// </summary>
public static class BuildDiagnosticProducer
{
    static readonly Regex DiagnosticWithSpan = new(
        @"^(?<file>.+?)\((?<line>\d+),(?<col>\d+)\)\s*:\s*(?<sev>error|warning)\s+(?<code>[A-Z]+\d+)\s*:\s*(?<msg>.+?)(?:\s*\[(?<proj>.+)\])?\s*$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

    static readonly Regex DiagnosticWithoutSpan = new(
        @"^(?<file>.+?)\s*:\s*(?<sev>error|warning)\s+(?<code>[A-Z]+\d+)\s*:\s*(?<msg>.+?)(?:\s*\[(?<proj>.+)\])?\s*$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

    public sealed record CollectResult(
        IReadOnlyList<BuildDiagnosticIngest.BuildDiagnosticWire> Diagnostics,
        int ExitCode,
        string? FailureReason = null);

    public static CollectResult TryCollectFromDotNetBuild(
        string anchorPath,
        string configuration = "Debug",
        bool noRestore = false,
        int timeoutMs = 300_000)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(anchorPath);

        var fullAnchor = Path.GetFullPath(anchorPath.Trim());
        if (!File.Exists(fullAnchor))
            return new CollectResult([], -1, $"anchor_not_found:{fullAnchor}");

        var restoreFlag = noRestore ? "--no-restore" : string.Empty;
        var psi = new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments =
                $"build \"{fullAnchor}\" -c {configuration} {restoreFlag} -v:q -nologo".Trim(),
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };

        try
        {
            using var proc = Process.Start(psi);
            if (proc is null)
                return new CollectResult([], -1, "dotnet_process_failed_to_start");

            var stdout = proc.StandardOutput.ReadToEnd();
            var stderr = proc.StandardError.ReadToEnd();

            if (!proc.WaitForExit(timeoutMs))
            {
                try { proc.Kill(entireProcessTree: true); } catch { /* best effort */ }
                return new CollectResult([], -1, "dotnet_build_timed_out");
            }

            var workspaceRoot = Path.GetDirectoryName(fullAnchor);
            var diagnostics = ParseMsBuildDiagnostics($"{stdout}{Environment.NewLine}{stderr}", workspaceRoot);
            return new CollectResult(diagnostics, proc.ExitCode);
        }
        catch (Exception ex)
        {
            return new CollectResult([], -1, ex.Message);
        }
    }

    public static IReadOnlyList<BuildDiagnosticIngest.BuildDiagnosticWire> ParseMsBuildDiagnostics(
        string output,
        string? workspaceRoot = null)
    {
        ArgumentNullException.ThrowIfNull(output);

        var results = new List<BuildDiagnosticIngest.BuildDiagnosticWire>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var rawLine in output.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries))
        {
            var line = rawLine.Trim();
            if (line.Length == 0)
                continue;

            if (!TryParseDiagnosticLine(line, out var file, out var lineNo, out var column, out var code, out var message))
                continue;

            var normalizedFile = NormalizeDiagnosticPath(file, workspaceRoot);
            var key = $"{normalizedFile}|{lineNo}|{column}|{code}|{message}";
            if (!seen.Add(key))
                continue;

            results.Add(new BuildDiagnosticIngest.BuildDiagnosticWire(
                normalizedFile,
                lineNo,
                column,
                code,
                message));
        }

        return results;
    }

    static bool TryParseDiagnosticLine(
        string line,
        out string file,
        out int lineNo,
        out int column,
        out string code,
        out string message)
    {
        file = string.Empty;
        lineNo = 1;
        column = 1;
        code = string.Empty;
        message = string.Empty;

        var match = DiagnosticWithSpan.Match(line);
        if (!match.Success)
            match = DiagnosticWithoutSpan.Match(line);

        if (!match.Success)
            return false;

        file = match.Groups["file"].Value.Trim();
        if (match.Groups["line"].Success && int.TryParse(match.Groups["line"].Value, out var parsedLine))
            lineNo = parsedLine;

        if (match.Groups["col"].Success && int.TryParse(match.Groups["col"].Value, out var parsedCol))
            column = parsedCol;

        code = match.Groups["code"].Value.Trim();
        message = match.Groups["msg"].Value.Trim();
        return file.Length > 0 && code.Length > 0;
    }

    static string NormalizeDiagnosticPath(string file, string? workspaceRoot)
    {
        var trimmed = file.Trim().Trim('"');
        if (Path.IsPathRooted(trimmed))
            return LogicalPath.Create(Path.GetFullPath(trimmed)).Value;

        if (!string.IsNullOrWhiteSpace(workspaceRoot))
            return LogicalPath.Create(Path.GetFullPath(Path.Combine(workspaceRoot, trimmed))).Value;

        return LogicalPath.Create(trimmed).Value;
    }
}
