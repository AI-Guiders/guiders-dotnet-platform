#nullable enable
using System.Text.Json;
using System.Text.Json.Serialization;
using AIGuiders.Platform.IntermediateRepresentation.Presentation;

namespace AIGuiders.Platform.Notations.Presentation.Topology.Conformance;

public static class TopologySpecConformance
{
    public static TopologySpecDocument Load(string json) =>
        JsonSerializer.Deserialize<TopologySpecDocument>(json, JsonOptions)
        ?? throw new InvalidOperationException("Topology spec JSON deserialized to null.");

    public static IReadOnlyList<string> ValidateDocument(TopologySpecDocument spec)
    {
        var errors = new List<string>();
        foreach (var vector in spec.Vectors)
        {
            if (!TryValidateVector(vector, out var error))
                errors.Add($"[{vector.Id}] {error}");
        }

        return errors;
    }

    public static bool TryValidateVector(TopologySpecVector vector, out string error)
    {
        error = "";
        if (vector.Wire is null)
            return Fail("wire is required.", out error);

        var result = TopologyNotation.Parse(vector.Wire);
        if (!result.IsSuccess || result.Topology is null)
            return Fail(result.Error ?? "parse failed.", out error);

        var actual = result.Topology;
        var expect = vector.Expect;

        if (expect.Arrangement is not null)
        {
            if (!TryParseEnum(expect.Arrangement, out TopologyArrangement arrangement)
                || actual.Arrangement != arrangement)
            {
                error = $"arrangement expected \"{expect.Arrangement}\", got \"{actual.Arrangement}\".";
                return false;
            }
        }

        if (expect.HostCount is not null && actual.HostCount != expect.HostCount)
        {
            error = $"hostCount expected {expect.HostCount}, got {actual.HostCount}.";
            return false;
        }

        if (expect.Hosts is null)
            return true;

        if (actual.Hosts.Count != expect.Hosts.Count)
        {
            error = $"host count expected {expect.Hosts.Count}, got {actual.Hosts.Count}.";
            return false;
        }

        for (var i = 0; i < expect.Hosts.Count; i++)
        {
            if (!HostMatches(expect.Hosts[i], actual.Hosts[i], out error))
                return false;
        }

        return true;
    }

    static bool HostMatches(TopologySpecHost expect, LogicalDisplayHost actual, out string error)
    {
        error = "";
        if (expect.HostIndex is not null && expect.HostIndex != actual.HostIndex)
        {
            error = $"hostIndex expected {expect.HostIndex}, got {actual.HostIndex}.";
            return false;
        }

        if (expect.HostId is not null
            && !string.Equals(expect.HostId, actual.HostId, StringComparison.Ordinal))
        {
            error = $"hostId expected \"{expect.HostId}\", got \"{actual.HostId}\".";
            return false;
        }

        if (expect.Role is not null)
        {
            if (!TryParseEnum(expect.Role, out AttentionDisplayRole role) || actual.Role != role)
            {
                error = $"role expected \"{expect.Role}\", got \"{actual.Role}\".";
                return false;
            }
        }

        if (expect.Compose is not null)
        {
            if (!TryParseEnum(expect.Compose, out ZoneComposeKind compose) || actual.Compose != compose)
            {
                error = $"compose expected \"{expect.Compose}\", got \"{actual.Compose}\".";
                return false;
            }
        }

        if (expect.ChannelStack is not null
            && !actual.ChannelStack.SequenceEqual(expect.ChannelStack, StringComparer.Ordinal))
        {
            error = $"channelStack expected [{string.Join(", ", expect.ChannelStack)}], got [{string.Join(", ", actual.ChannelStack)}].";
            return false;
        }

        return true;
    }

    static bool TryParseEnum<TEnum>(string value, out TEnum parsed) where TEnum : struct, Enum =>
        Enum.TryParse(value, ignoreCase: true, out parsed);

    static bool Fail(string message, out string error)
    {
        error = message;
        return false;
    }

    static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
    };
}

public sealed record TopologySpecDocument(
    [property: JsonPropertyName("kind")] string Kind,
    [property: JsonPropertyName("version")] int Version,
    [property: JsonPropertyName("source")] string? Source,
    [property: JsonPropertyName("vectors")] IReadOnlyList<TopologySpecVector> Vectors);

public sealed record TopologySpecVector(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("wire")] string? Wire,
    [property: JsonPropertyName("expect")] TopologySpecExpectation Expect);

public sealed record TopologySpecExpectation(
    [property: JsonPropertyName("arrangement")] string? Arrangement,
    [property: JsonPropertyName("hostCount")] int? HostCount,
    [property: JsonPropertyName("hosts")] IReadOnlyList<TopologySpecHost>? Hosts);

public sealed record TopologySpecHost(
    [property: JsonPropertyName("hostIndex")] int? HostIndex,
    [property: JsonPropertyName("hostId")] string? HostId,
    [property: JsonPropertyName("role")] string? Role,
    [property: JsonPropertyName("compose")] string? Compose,
    [property: JsonPropertyName("channelStack")] IReadOnlyList<string>? ChannelStack);
