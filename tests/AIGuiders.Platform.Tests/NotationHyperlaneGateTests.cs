#nullable enable
using System.Reflection;
using System.Text.Json;
using AIGuiders.Platform.Conformance.Schemas;
using AIGuiders.Platform.Modeling.Notations.Bracket;
using AIGuiders.Platform.Notations.Bracket.Conformance;
using AIGuiders.Platform.Notations.Conformance;
using AIGuiders.Platform.Notations.Keyboard;
using AIGuiders.Platform.Notations.Keyboard.Quarry;
using AIGuiders.Platform.Notations.Presentation.Topology.Conformance;
using Xunit;

namespace AIGuiders.Platform.Tests;

/// <summary>
/// Discover-all gate for notation hyperlane embedded fixtures (ADR-0021 §9).
/// </summary>
public sealed class NotationHyperlaneGateTests
{
    public const int MinimumEmbeddedSpecCount = 15;

    static readonly string[] FixturePrefixes =
    [
        "AIGuiders.Platform.Tests.Fixtures.Notation.",
        "AIGuiders.Platform.Tests.Fixtures.Quarry.",
    ];

    public static IEnumerable<object[]> DiscoverEmbeddedNotationSpecs()
    {
        var asm = typeof(NotationHyperlaneGateTests).Assembly;
        foreach (var resourceName in asm.GetManifestResourceNames()
                     .Where(name => FixturePrefixes.Any(prefix => name.StartsWith(prefix, StringComparison.Ordinal))
                                    && name.EndsWith(".spec.json", StringComparison.Ordinal))
                     .OrderBy(name => name, StringComparer.Ordinal))
        {
            yield return [resourceName];
        }
    }

    [Fact]
    public void Gate_discovers_minimum_embedded_spec_count()
    {
        var discovered = DiscoverEmbeddedNotationSpecs().Select(row => (string)row[0]).ToList();
        Assert.True(
            discovered.Count >= MinimumEmbeddedSpecCount,
            $"Expected at least {MinimumEmbeddedSpecCount} embedded notation specs, found {discovered.Count}:{Environment.NewLine}{string.Join(Environment.NewLine, discovered)}");
    }

    [Theory]
    [MemberData(nameof(DiscoverEmbeddedNotationSpecs))]
    public void Embedded_notation_spec_passes_schema_and_harness(string resourceName)
    {
        var json = ConformanceFixture.LoadEmbedded(resourceName);
        var schemaErrors = ConformanceSchemaValidator.ValidateNotationHyperlaneJson(json);
        Assert.True(
            schemaErrors.Count == 0,
            $"{ShortName(resourceName)} schema:{Environment.NewLine}{string.Join(Environment.NewLine, schemaErrors)}");

        var harnessErrors = ValidateHarness(json);
        Assert.True(
            harnessErrors.Count == 0,
            $"{ShortName(resourceName)} harness:{Environment.NewLine}{string.Join(Environment.NewLine, harnessErrors)}");
    }

    static IReadOnlyList<string> ValidateHarness(string json)
    {
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;

        if (root.TryGetProperty("kind", out var kindNode)
            && kindNode.ValueKind == JsonValueKind.String
            && string.Equals(kindNode.GetString(), ConformanceSchemaValidator.PresentationTopologyKind, StringComparison.Ordinal))
        {
            return TopologySpecConformance.ValidateDocument(TopologySpecConformance.Load(json));
        }

        if (!root.TryGetProperty("surface", out var surfaceNode)
            || surfaceNode.ValueKind != JsonValueKind.String
            || string.IsNullOrWhiteSpace(surfaceNode.GetString()))
        {
            return ["Missing required string property 'surface'."];
        }

        return RouteBySurface(surfaceNode.GetString()!, json);
    }

    static IReadOnlyList<string> RouteBySurface(string surface, string json) => surface switch
    {
        "command-slash" or "command-console" or "argument-kv" or "argument-delimited"
            or "argument-positional" or "argument-cli" or "invocation-parity"
            => NotationSpecConformance.ValidateDocument(NotationSpecConformance.Load(json)),

        "bracket-cdp-square-kv"
            => BracketSpecConformance.ValidateDocument(BracketSpecConformance.Load(json)),

        "bracket-angle-opaque"
            => BracketSpecConformance.ValidateDocument(
                BracketSpecConformance.Load(json),
                BracketProfiles.AngleOpaque),

        "bracket-forge-frg"
            => BracketSpecConformance.ValidateDocument(
                BracketSpecConformance.Load(json),
                BracketProfiles.ForgeFrg,
                BracketAxisValuePlans.ForgeFrgCompound),

        "bracket-doc-symbol"
            => BracketSpecConformance.ValidateDocument(
                BracketSpecConformance.Load(json),
                BracketProfiles.DocSymbol),

        "neovim-kbd"
            => QuarrySpecConformance.ValidateDocument(
                NeovimNotationReader.Instance,
                QuarrySpecLoader.Load(json)),

        "emacs-kbd"
            => QuarrySpecConformance.ValidateDocument(
                EmacsNotationReader.Instance,
                QuarrySpecLoader.Load(json)),

        "key-gesture"
            => QuarrySpecConformance.ValidateDocument(
                KeyGestureNotationReader.Instance,
                QuarrySpecLoader.Load(json)),

        _ => [$"No notation hyperlane harness registered for surface \"{surface}\"."],
    };

    static string ShortName(string resourceName)
    {
        var parts = resourceName.Split('.');
        return string.Join('.', parts[^3..]);
    }
}
