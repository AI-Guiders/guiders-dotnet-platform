using AIGuiders.Platform.Modeling.Notations.Keyboard;
#nullable enable
using System.Diagnostics;
using System.Text.Json;
using AIGuiders.Platform.Notations.Keyboard;
using AIGuiders.Platform.Notations.Keyboard.Quarry;

namespace AIGuiders.Platform.Tools.QuarryOracle;

static class OracleJsonParser
{
    static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    public static bool TryParse(string stdout, out NormalizedKeySequence? sequence, out string error)
    {
        sequence = null;
        error = "";
        var line = stdout.Trim();
        if (string.IsNullOrWhiteSpace(line))
        {
            error = "oracle returned empty JSON.";
            return false;
        }

        QuarryOracleWireJson? payload;
        try
        {
            payload = JsonSerializer.Deserialize<QuarryOracleWireJson>(line, JsonOptions);
        }
        catch (Exception ex)
        {
            error = $"oracle JSON parse failed: {ex.Message}";
            return false;
        }

        if (payload?.Steps is null || payload.Steps.Count == 0)
        {
            error = "oracle returned no steps.";
            return false;
        }

        try
        {
            sequence = QuarryOracleIrMapper.ToNormalized(payload.Steps);
        }
        catch (Exception ex)
        {
            error = $"oracle step mapping failed: {ex.Message}";
            return false;
        }

        return true;
    }
}

static class ProcessRunner
{
    public static bool TryRun(
        string file,
        string[] args,
        IReadOnlyDictionary<string, string>? environment,
        out string stdout,
        out string error)
    {
        stdout = "";
        error = "";
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

            if (environment is not null)
            {
                foreach (var (key, value) in environment)
                    startInfo.Environment[key] = value;
            }

            using var process = Process.Start(startInfo);
            if (process is null)
            {
                error = "failed to start process.";
                return false;
            }

            stdout = process.StandardOutput.ReadToEnd();
            var stderr = process.StandardError.ReadToEnd();
            process.WaitForExit();
            if (process.ExitCode != 0)
            {
                error = string.IsNullOrWhiteSpace(stderr) ? $"exit code {process.ExitCode}" : stderr.Trim();
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            error = ex.Message;
            return false;
        }
    }
}
