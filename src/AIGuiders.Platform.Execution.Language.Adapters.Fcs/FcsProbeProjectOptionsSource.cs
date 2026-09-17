#nullable disable

using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text.Json;
using AIGuiders.Platform.Modeling.Language.Adapters.Fcs;
using FSharp.Compiler.CodeAnalysis;
using Microsoft.FSharp.Core;

namespace AIGuiders.Platform.Execution.Language.Adapters.Fcs;

/// <summary>
/// Out-of-process F# project options via FcsProjInfoProbe child process (plan §7 File IO @ Execution).
/// </summary>
public sealed class FcsProbeProjectOptionsSource : IFcsProjectOptionsSource
{
    static readonly ConcurrentDictionary<string, (DateTime Mtime, FSharpProjectOptions Options)> Cache =
        new(StringComparer.OrdinalIgnoreCase);

    public FSharpResult<FSharpProjectOptions, FcsProjectOptionsLoadError> TryLoad(string fsprojPath)
    {
        try
        {
            var full = Path.GetFullPath(fsprojPath);
            var mtime = File.GetLastWriteTimeUtc(full);

            if (Cache.TryGetValue(full, out var cached) && cached.Mtime == mtime)
                return FSharpResult<FSharpProjectOptions, FcsProjectOptionsLoadError>.NewOk(cached.Options);

            var probe = RunProbe(full);
            if (probe.IsError)
            {
                Cache.TryRemove(full, out _);
                return FSharpResult<FSharpProjectOptions, FcsProjectOptionsLoadError>.NewError(
                    new FcsProjectOptionsLoadError { Message = probe.ErrorValue });
            }

            var guarded = FcsProbeWireMapping.toFcsOptions(probe.ResultValue);
            if (guarded.IsError)
                return guarded;

            Cache[full] = (mtime, guarded.ResultValue);
            return guarded;
        }
        catch (Exception ex)
        {
            return FSharpResult<FSharpProjectOptions, FcsProjectOptionsLoadError>.NewError(
                new FcsProjectOptionsLoadError { Message = ex.Message });
        }
    }

    public void Warm(string fsprojPath) => TryLoad(fsprojPath);

    public void Invalidate(FSharpOption<string> fsprojPath = default)
    {
        if (FSharpOption<string>.get_IsNone(fsprojPath))
        {
            Cache.Clear();
            return;
        }

        var path = fsprojPath.Value;
        if (!string.IsNullOrWhiteSpace(path))
            Cache.TryRemove(Path.GetFullPath(path), out _);
    }

    static FSharpResult<FcsProbeWire, string> RunProbe(string fsprojPath)
    {
        try
        {
            var dll = ResolveProbeDll(fsprojPath);
            var psi = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = $"exec \"{dll}\" \"{fsprojPath}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };

            using var proc = Process.Start(psi);
            if (proc is null)
                return FSharpResult<FcsProbeWire, string>.NewError(
                    $"FcsProjInfoProbe process failed to start for '{fsprojPath}'.");

            var stdout = proc.StandardOutput.ReadToEnd();
            var stderr = proc.StandardError.ReadToEnd();

            if (!proc.WaitForExit(120_000))
            {
                try { proc.Kill(); } catch { /* best effort */ }
                return FSharpResult<FcsProbeWire, string>.NewError(
                    $"FcsProjInfoProbe timed out for '{fsprojPath}'.");
            }

            if (proc.ExitCode != 0)
                return FSharpResult<FcsProbeWire, string>.NewError(
                    $"FcsProjInfoProbe failed for '{fsprojPath}': {stderr.Trim()}");

            var wire = JsonSerializer.Deserialize<FcsProbeWire>(stdout);
            return wire is null
                ? FSharpResult<FcsProbeWire, string>.NewError(
                    $"FcsProjInfoProbe returned empty wire for '{fsprojPath}'.")
                : FSharpResult<FcsProbeWire, string>.NewOk(wire);
        }
        catch (Exception ex)
        {
            return FSharpResult<FcsProbeWire, string>.NewError(ex.Message);
        }
    }

    static string ResolveProbeDll(string fsprojPath)
    {
        foreach (var candidate in ProbeDllCandidates(fsprojPath))
        {
            if (!string.IsNullOrWhiteSpace(candidate) && File.Exists(candidate))
                return candidate;
        }

        throw new InvalidOperationException(
            $"FcsProjInfoProbe.dll not found for '{fsprojPath}' (set AIGUIDERS_FCS_PROBE)");
    }

    static IEnumerable<string> ProbeDllCandidates(string fsprojPath)
    {
        var env = Environment.GetEnvironmentVariable("AIGUIDERS_FCS_PROBE");
        if (!string.IsNullOrWhiteSpace(env))
            yield return env;

        var baseDir = AppContext.BaseDirectory;
        if (!string.IsNullOrEmpty(baseDir))
        {
            yield return Path.Combine(baseDir, "FcsProjInfoProbe", "FcsProjInfoProbe.dll");
            yield return Path.Combine(baseDir, "FcsProjInfoProbe.dll");
            foreach (var found in WalkUp(baseDir, []))
                yield return found;
        }

        var fsprojDir = Path.GetDirectoryName(Path.GetFullPath(fsprojPath));
        if (!string.IsNullOrEmpty(fsprojDir))
        {
            foreach (var found in WalkUp(fsprojDir, []))
                yield return found;
        }
    }

    static IEnumerable<string> WalkUp(string dir, List<string> acc)
    {
        if (string.IsNullOrEmpty(dir))
            return acc;

        var probeDir = Path.Combine(dir, "tools", "FcsProjInfoProbe");
        if (Directory.Exists(probeDir))
        {
            acc.AddRange(
                Directory.EnumerateFiles(probeDir, "FcsProjInfoProbe.dll", SearchOption.AllDirectories));
        }

        var parent = Directory.GetParent(dir);
        return parent is null ? acc : WalkUp(parent.FullName, acc);
    }
}

/// <summary>Direct probe load for tests/diagnostics — not the federation materialized hot path.</summary>
public static class FcsProbeProjectOptions
{
    static readonly IFcsProjectOptionsSource Loader = new FcsProbeProjectOptionsSource();

    public static FSharpOption<FSharpProjectOptions> TryLoad(string fsprojPath)
    {
        if (string.IsNullOrWhiteSpace(fsprojPath) || !File.Exists(fsprojPath))
            return FSharpOption<FSharpProjectOptions>.None;

        var result = Loader.TryLoad(fsprojPath);
        return result.IsOk
            ? FSharpOption<FSharpProjectOptions>.Some(result.ResultValue)
            : FSharpOption<FSharpProjectOptions>.None;
    }

    /// <summary>F#-friendly probe load for Modeling tests.</summary>
    public static bool TryGet(string fsprojPath, out FSharpProjectOptions options)
    {
        options = null;
        if (string.IsNullOrWhiteSpace(fsprojPath) || !File.Exists(fsprojPath))
            return false;

        var result = Loader.TryLoad(fsprojPath);
        if (!result.IsOk)
            return false;

        options = result.ResultValue;
        return true;
    }
}
