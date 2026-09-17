#nullable enable

using System.Text.Json;
using AIGuiders.Platform.Execution.CommandPlane;
using AIGuiders.Platform.Execution.CommandPlane.ArgSuggestions;
using AIGuiders.Platform.Execution.CommandPlane.Catalog;
using AIGuiders.Platform.Execution.Ide.Session;
using AIGuiders.Platform.Execution.LanguageIntelligence.Bundled;
using AIGuiders.Platform.Modeling.CommandPlane;
using AIGuiders.Platform.Modeling.Ide.Session;
using AIGuiders.Platform.Modeling.Notations.Bracket;
using AIGuiders.Platform.Notations.Bracket;
using Xunit;

namespace AIGuiders.Platform.Tests;

public sealed class AttachCommandTests
{
    [Fact]
    public void FederationAttachCatalog_registers_root_and_verbs()
    {
        var catalog = CommandCatalogIndex.FromDescriptors(FederationAttachCatalog.AllDescriptors());
        Assert.True(catalog.TryGet("attach", out var root));
        Assert.Equal(FederationAttachCatalog.AttachCommandId, root.CommandId);
        Assert.Equal(CommandArgTailKind.Picker, root.ArgTailKind);

        Assert.True(catalog.TryGet("attach code", out var code));
        Assert.Equal("federation.attach.code", code.CommandId);
    }

    [Fact]
    public void AttachVerbArgSuggestionProvider_lists_all_verbs()
    {
        var catalog = CommandCatalogIndex.FromDescriptors(FederationAttachCatalog.AllDescriptors());
        Assert.True(catalog.TryGet("attach", out var route));

        var broker = FederationAttachSuggestions.CreateBroker();
        var request = ArgSuggestionRequest.Create(
            AttachSchemaCatalog.VerbSuggestionId,
            "",
            route,
            "attach");

        var choices = broker.GetSuggestions(request);
        Assert.Equal(6, choices.Count);
        Assert.Contains(choices, c => c.Value == "error");
        Assert.Contains(choices, c => c.Value == "manual");
    }

    [Fact]
    public void AttachVerbArgSuggestionProvider_filters_by_partial()
    {
        var catalog = CommandCatalogIndex.FromDescriptors(FederationAttachCatalog.AllDescriptors());
        Assert.True(catalog.TryGet("attach", out var route));

        var broker = FederationAttachSuggestions.CreateBroker();
        var request = ArgSuggestionRequest.Create(
            AttachSchemaCatalog.VerbSuggestionId,
            "doc",
            route,
            "attach");

        var choices = broker.GetSuggestions(request);
        Assert.Single(choices);
        Assert.Equal("document", choices[0].Value);
    }

    [Fact]
    public void FederationBundledCatalog_merges_with_editor_registry()
    {
        var registry = EditorCommandRegistry.CreateBundled();
        var catalog = CommandCatalogComposer.Build(
            FederationBundledCatalog.AttachSource,
            RegistryCatalogBuilder.ToCommandSource(registry));

        Assert.True(catalog.TryGet("attach", out var attach));
        Assert.Equal(FederationAttachCatalog.AttachCommandId, attach.CommandId);
        Assert.True(catalog.TryGet("editor line select", out _));
    }

    [Fact]
    public void AttachStepArgSuggestionProvider_resolves_schema_prompt()
    {
        var catalog = CommandCatalogIndex.FromDescriptors(FederationAttachCatalog.AllDescriptors());
        Assert.True(catalog.TryGet("attach code", out var code));
        Assert.Equal(CommandArgTailKind.Picker, code.ArgTailKind);

        var broker = FederationAttachSuggestions.CreateBroker();
        var request = ArgSuggestionRequest.Create(
            "federation.attach.step.pick_file",
            "",
            code,
            "attach code");

        var choices = broker.GetSuggestions(request);
        Assert.Single(choices);
        Assert.Equal("pick_file", choices[0].Value);
    }

    [Fact]
    public void AttachDiagnosticArgSuggestionProvider_falls_back_without_session()
    {
        var catalog = CommandCatalogIndex.FromDescriptors(FederationAttachCatalog.AllDescriptors());
        Assert.True(catalog.TryGet("attach error", out var errorRoute));

        var broker = FederationAttachSuggestions.CreateBroker(new NullAttachSessionAccessor());
        var request = ArgSuggestionRequest.Create(
            "federation.attach.step.pick_diagnostic",
            "",
            errorRoute,
            "attach error");

        var choices = broker.GetSuggestions(request);
        Assert.Single(choices);
        Assert.Equal("pick_diagnostic", choices[0].Value);
    }

    [Fact]
    public void AttachRegistryPathArgSuggestionProvider_falls_back_without_session()
    {
        var catalog = CommandCatalogIndex.FromDescriptors(FederationAttachCatalog.AllDescriptors());
        Assert.True(catalog.TryGet("attach code", out var codeRoute));

        var broker = FederationAttachSuggestions.CreateBroker(new NullAttachSessionAccessor());
        var request = ArgSuggestionRequest.Create(
            "federation.attach.step.pick_file",
            "",
            codeRoute,
            "attach code");

        var choices = broker.GetSuggestions(request);
        Assert.Single(choices);
        Assert.Equal("pick_file", choices[0].Value);
    }
}

file sealed class NullAttachSessionAccessor : IAttachSessionAccessor
{
    public SessionRuntime? TryGet(string? workspaceAnchor) => null;
}

public sealed class BracketKindCanonSpecTests
{
    sealed record BracketKindCanonVector(string Id, string Wire, bool ExpectRelationSpec);

    sealed record BracketKindCanonSpec(string Kind, IReadOnlyList<BracketKindCanonVector> Vectors);

    static BracketKindCanonSpec LoadSpec()
    {
        var json = ConformanceFixture.LoadEmbedded(
            "AIGuiders.Platform.Tests.Fixtures.LanguageIntelligence.bracket-kind-canon.spec.json");
        return JsonSerializer.Deserialize<BracketKindCanonSpec>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
        })!;
    }

    [Fact]
    public void BracketKindCanon_spec_vectors_match_runtime()
    {
        var spec = LoadSpec();
        Assert.Equal("language-intelligence.bracket-kind-canon", spec.Kind);

        foreach (var vector in spec.Vectors)
        {
            Assert.True(
                BracketReader.Default.TryRead(
                    vector.Wire,
                    BracketProfiles.CdpSquareKeyValue,
                    out var wire,
                    out var error),
                $"{vector.Id}: {error}");

            var specParsed = BracketRelationWire.tryParseRelationSpec(wire!);
            if (vector.ExpectRelationSpec)
                Assert.NotNull(specParsed);
            else
                Assert.Null(specParsed);
        }
    }
}
