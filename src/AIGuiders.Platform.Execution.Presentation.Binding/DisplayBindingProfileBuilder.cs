#nullable enable

using System.Globalization;
using AIGuiders.Platform.IntermediateRepresentation.Presentation;

namespace AIGuiders.Platform.Execution.Presentation.Binding;

/// <summary>
/// v1 stub: materialize <see cref="DisplayBindingProfile"/> from TOML-like tables
/// (no <c>.display.gdl</c> quarry yet — GUIDERS-ADR-0058 §Non-goals).
/// </summary>
public static class DisplayBindingProfileBuilder
{
    public static DisplayBindingBuildResult TryBuild(IReadOnlyDictionary<string, object?> table)
    {
        if (!TryGetString(table, "profile_id", out var profileId, out var error))
        {
            return DisplayBindingBuildResult.Fail(error);
        }

        if (!table.TryGetValue("bindings", out var bindingsRaw) || bindingsRaw is null)
        {
            return DisplayBindingBuildResult.Fail("bindings is required.");
        }

        if (bindingsRaw is not IEnumerable<object?> bindingRows)
        {
            return DisplayBindingBuildResult.Fail("bindings must be a list of tables.");
        }

        var bindings = new List<DisplayHostBinding>();
        var rowIndex = 0;
        foreach (var row in bindingRows)
        {
            if (row is not IReadOnlyDictionary<string, object?> bindingTable)
            {
                return DisplayBindingBuildResult.Fail($"bindings[{rowIndex}] must be a table.");
            }

            if (!TryGetHostIndex(bindingTable, out var hostIndex, out error))
            {
                return DisplayBindingBuildResult.Fail($"bindings[{rowIndex}]: {error}");
            }

            if (!bindingTable.TryGetValue("screen", out var screenRaw) || screenRaw is null)
            {
                return DisplayBindingBuildResult.Fail($"bindings[{rowIndex}]: screen is required.");
            }

            if (!TryParseScreen(screenRaw, out var screen, out error))
            {
                return DisplayBindingBuildResult.Fail($"bindings[{rowIndex}]: {error}");
            }

            bindings.Add(new DisplayHostBinding(hostIndex, screen));
            rowIndex++;
        }

        if (bindings.Count == 0)
        {
            return DisplayBindingBuildResult.Fail("bindings must contain at least one entry.");
        }

        return DisplayBindingBuildResult.Ok(new DisplayBindingProfile(profileId, bindings));
    }

    static bool TryGetHostIndex(IReadOnlyDictionary<string, object?> table, out int hostIndex, out string error)
    {
        error = "";
        hostIndex = 0;
        if (table.TryGetValue("host", out var host) && host is not null)
        {
            return TryConvertInt(host, out hostIndex, out error);
        }

        if (table.TryGetValue("host_index", out var hostIndexRaw) && hostIndexRaw is not null)
        {
            return TryConvertInt(hostIndexRaw, out hostIndex, out error);
        }

        error = "host or host_index is required.";
        return false;
    }

    static bool TryParseScreen(object screenRaw, out PhysicalScreenSelector screen, out string error)
    {
        error = "";
        screen = PhysicalScreenSelector.Primary();

        if (screenRaw is string wire)
        {
            return TryParseScreenWire(wire.Trim(), out screen, out error);
        }

        if (screenRaw is IReadOnlyDictionary<string, object?> table)
        {
            return TryParseScreenTable(table, out screen, out error);
        }

        error = "screen must be a string wire or table.";
        return false;
    }

    static bool TryParseScreenWire(string wire, out PhysicalScreenSelector screen, out string error)
    {
        error = "";
        screen = PhysicalScreenSelector.Primary();

        if (wire.Equals("primary", StringComparison.OrdinalIgnoreCase))
        {
            screen = PhysicalScreenSelector.Primary();
            return true;
        }

        if (wire.StartsWith("index:", StringComparison.OrdinalIgnoreCase)
            && TryConvertInt(wire["index:".Length..], out var index, out error))
        {
            screen = PhysicalScreenSelector.ByIndex(index);
            return true;
        }

        if (wire.StartsWith("device:", StringComparison.OrdinalIgnoreCase))
        {
            var device = wire["device:".Length..];
            if (string.IsNullOrWhiteSpace(device))
            {
                error = "device: value is empty.";
                return false;
            }

            screen = PhysicalScreenSelector.ByDeviceName(device);
            return true;
        }

        if (wire.StartsWith("ultrawide:", StringComparison.OrdinalIgnoreCase))
        {
            var parts = wire["ultrawide:".Length..].Split(',', StringSplitOptions.TrimEntries);
            if (parts.Length != 4)
            {
                error = "ultrawide wire expects four comma-separated values (left,top,width,height).";
                return false;
            }

            if (!TryParseDouble(parts[0], out var left, out error)
                || !TryParseDouble(parts[1], out var top, out error)
                || !TryParseDouble(parts[2], out var width, out error)
                || !TryParseDouble(parts[3], out var height, out error))
            {
                return false;
            }

            screen = PhysicalScreenSelector.UltrawideRegion(left, top, width, height);
            return true;
        }

        error = $"unknown screen wire \"{wire}\".";
        return false;
    }

    static bool TryParseScreenTable(IReadOnlyDictionary<string, object?> table, out PhysicalScreenSelector screen, out string error)
    {
        error = "";
        screen = PhysicalScreenSelector.Primary();

        if (!TryGetString(table, "kind", out var kind, out error))
        {
            return false;
        }

        switch (kind.ToLowerInvariant())
        {
            case "primary":
                screen = PhysicalScreenSelector.Primary();
                return true;
            case "index":
                if (!table.TryGetValue("index", out var indexRaw) || !TryConvertInt(indexRaw!, out var index, out error))
                {
                    error = "index is required for kind=index.";
                    return false;
                }

                screen = PhysicalScreenSelector.ByIndex(index);
                return true;
            case "device":
            case "devicename":
                if (!TryGetString(table, "device", out var device, out error)
                    && !TryGetString(table, "device_name", out device, out error))
                {
                    error = "device or device_name is required for kind=device.";
                    return false;
                }

                screen = PhysicalScreenSelector.ByDeviceName(device);
                return true;
            case "ultrawide":
            case "ultrawideregion":
                if (!TryGetDouble(table, "left", out var left, out error)
                    || !TryGetDouble(table, "top", out var top, out error)
                    || !TryGetDouble(table, "width", out var width, out error)
                    || !TryGetDouble(table, "height", out var height, out error))
                {
                    return false;
                }

                screen = PhysicalScreenSelector.UltrawideRegion(left, top, width, height);
                return true;
            default:
                error = $"unknown screen kind \"{kind}\".";
                return false;
        }
    }

    static bool TryGetString(IReadOnlyDictionary<string, object?> table, string key, out string value, out string error)
    {
        error = "";
        value = "";
        if (!table.TryGetValue(key, out var raw) || raw is null)
        {
            error = $"{key} is required.";
            return false;
        }

        value = Convert.ToString(raw, CultureInfo.InvariantCulture) ?? "";
        if (string.IsNullOrWhiteSpace(value))
        {
            error = $"{key} is empty.";
            return false;
        }

        return true;
    }

    static bool TryGetDouble(IReadOnlyDictionary<string, object?> table, string key, out double value, out string error)
    {
        error = "";
        value = 0;
        if (!table.TryGetValue(key, out var raw) || raw is null)
        {
            error = $"{key} is required.";
            return false;
        }

        return TryParseDouble(Convert.ToString(raw, CultureInfo.InvariantCulture) ?? "", out value, out error);
    }

    static bool TryConvertInt(object raw, out int value, out string error)
    {
        error = "";
        switch (raw)
        {
            case int i:
                value = i;
                return true;
            case long l:
                value = (int)l;
                return true;
            case string s when int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed):
                value = parsed;
                return true;
            default:
                error = $"expected integer, got \"{raw}\".";
                value = 0;
                return false;
        }
    }

    static bool TryParseDouble(string raw, out double value, out string error)
    {
        if (double.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out value))
        {
            error = "";
            return true;
        }

        error = $"expected number, got \"{raw}\".";
        value = 0;
        return false;
    }
}

public sealed record DisplayBindingBuildResult(bool IsSuccess, DisplayBindingProfile? Profile, string? Error)
{
    public static DisplayBindingBuildResult Ok(DisplayBindingProfile profile) => new(true, profile, null);

    public static DisplayBindingBuildResult Fail(string error) => new(false, null, error);
}
