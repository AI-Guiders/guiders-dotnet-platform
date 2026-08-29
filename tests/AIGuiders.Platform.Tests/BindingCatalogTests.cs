#nullable enable
using AIGuiders.Platform.CommandPlane.Binding;
using AIGuiders.Platform.CommandPlane.Binding.Sources;
using AIGuiders.Platform.InputNotation;
using Xunit;

namespace AIGuiders.Platform.Tests;

public sealed class BindingCatalogTests
{
    [Fact]
    public void Toml_reads_flat_hotkeys_map()
    {
        const string toml = """
            toggle_command_palette = "Ctrl+Q"
            cascade_chord = "Ctrl+K"
            """;

        var catalog = BindingCatalogComposer.Build(BindingSources.FromToml(toml));
        Assert.True(catalog.TryGetByKey("toggle_command_palette", out var palette));
        Assert.Equal(BindingTargetKind.Command, palette.Descriptor.TargetKind);
        Assert.Equal("Ctrl+Q", palette.Descriptor.GestureWire);

        Assert.True(catalog.TryGetChordRoot(out var root));
        Assert.Equal(BindingTargetKind.ChordRoot, root.Descriptor.TargetKind);
        Assert.NotNull(root.NormalizedGesture);
    }

    [Fact]
    public void Composer_overlay_wins_per_binding_key()
    {
        const string ship = """
            toggle_command_palette = "Ctrl+Q"
            """;
        const string user = """
            toggle_command_palette = "Ctrl+Shift+Q"
            """;

        var catalog = BindingCatalogComposer.Build(
            BindingSources.FromToml(ship, "ship"),
            BindingSources.FromToml(user, "user"));

        Assert.True(catalog.TryGetDisplayHint("toggle_command_palette", out var wire));
        Assert.Equal("Ctrl+Shift+Q", wire);
    }

    [Fact]
    public void Json_reads_bindings_object()
    {
        const string json = """
            {
              "bindings": {
                "workspace_go_to_file": "Ctrl+P"
              }
            }
            """;

        var catalog = BindingCatalogComposer.Build(BindingSources.FromJson(json));
        Assert.True(catalog.TryGetByKey("workspace_go_to_file", out var entry));
        Assert.Equal("Ctrl+P", entry.Descriptor.GestureWire);
    }

    [Fact]
    public void Fixture_hotkeys_toml_matches_cide_quarry_subset()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Fixtures", "hotkeys.toml");
        Assert.True(File.Exists(path));

        var catalog = BindingCatalogComposer.Build(BindingSources.FromFile(path));
        Assert.True(catalog.TryGetDisplayHint("toggle_command_palette", out var palette));
        Assert.Equal("Ctrl+Q", palette);
        Assert.True(catalog.TryGetChordRoot(out var root));
        var chord = Assert.IsType<NormalizedChordStep>(root.NormalizedGesture!.Steps[0]);
        Assert.Equal(ChordModifierKeys.Control, chord.Modifiers);
        Assert.Equal("K", chord.KeySymbol);
    }

    [Fact]
    public void Database_source_wraps_delegate()
    {
        var source = DatabaseBindingSources.From(
            () => [BindingDescriptor.FromFlatEntry("build.run", "Ctrl+F5")],
            "db:test");

        var catalog = BindingCatalogComposer.Build(source);
        Assert.True(catalog.TryGetDisplayHint("build.run", out var wire));
        Assert.Equal("Ctrl+F5", wire);
    }
}
