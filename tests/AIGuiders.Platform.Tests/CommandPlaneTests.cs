using AIGuiders.Platform.IntermediateRepresentation.Command;
#nullable enable
using AIGuiders.Platform.CommandPlane;
using AIGuiders.Platform.CommandPlane.ArgSuggestions;
using Xunit;

namespace AIGuiders.Platform.Tests;

public sealed class CommandPlaneTests
{
    [Theory]
    [InlineData("none", CommandArgTailKind.None)]
    [InlineData("required", CommandArgTailKind.Required)]
    [InlineData("picker:repo", CommandArgTailKind.Picker)]
    public void ArgTailPolicy_parse(string raw, CommandArgTailKind expected) =>
        Assert.Equal(expected, CommandArgTailPolicy.Parse(raw));

    [Fact]
    public void ArgTailPolicy_extract_suggestion_id()
    {
        Assert.Equal("repo", CommandArgTailPolicy.ExtractSuggestionId("picker:repo"));
        Assert.Equal("repo", CommandArgTailPolicy.ExtractSuggestionId("suggest:repo"));
        Assert.Equal("enum:text_mode", CommandArgTailPolicy.ExtractSuggestionId("picker:enum:text_mode"));
        Assert.Null(CommandArgTailPolicy.ExtractSuggestionId("required"));
    }

    [Fact]
    public void CatalogIndex_longest_prefix_and_merge()
    {
        var bundled = CommandCatalogIndex.FromDescriptors([
            new CommandDescriptor
            {
                Domain = "solution", Object = "build", Intent = "run",
                CommandId = "build.run", Path = "build run", Help = "Build solution",
            },
        ]);
        var overlay = CommandCatalogIndex.FromDescriptors([
            new CommandDescriptor
            {
                Domain = "forge", Object = "repo", Intent = "create",
                CommandId = "forge.repo.create", Path = "forge repo create",
                PathAliases = ["/repo create"], ArgTail = "required", Help = "Create repo",
            },
        ]);
        var merged = bundled.Merge(overlay);

        Assert.True(merged.TryGet("build run", out var build));
        Assert.Equal("build.run", build.CommandId);
        Assert.True(merged.TryGet("repo create", out var forge));
        Assert.Equal("forge.repo.create", forge.CommandId);
        Assert.Equal(CommandArgTailKind.Required, forge.ArgTailKind);
    }

    [Fact]
    public void LineResolver_build_run_with_args()
    {
        var catalog = CommandCatalogIndex.FromDescriptors([
            new CommandDescriptor
            {
                Domain = "solution", Object = "build", Intent = "run",
                CommandId = "build.run", Path = "build run", ArgTail = "optional",
            },
        ]);

        Assert.True(SlashLineResolver.TryResolveSlashLine("/build run Release", catalog, out var res));
        Assert.Equal("build run", res.CanonicalPath);
        Assert.Equal("Release", res.ArgTail);
        Assert.True(res.IsRunnable);
    }

    [Fact]
    public void StepCompletion_root_lists_domains_and_elision()
    {
        var catalog = SemanticTestCatalog();
        var items = SlashStepCompletion.GetSuggestions(catalog, "");
        var segments = items.Select(i => i.StepSegment).ToHashSet(StringComparer.OrdinalIgnoreCase);

        Assert.Contains("intercom", segments);
        Assert.Contains("build", segments);
        Assert.Contains("git", segments);
        Assert.DoesNotContain("run", segments);
    }

    [Fact]
    public void StepCompletion_build_space_lists_intents()
    {
        var catalog = SemanticTestCatalog();
        var items = SlashStepCompletion.GetSuggestions(catalog, "build ");
        Assert.Contains(items, i => i.StepSegment == "run");
        Assert.Contains(items, i => i.StepSegment == "ui");
    }

    [Fact]
    public void StepCompletion_flat_path_catalog()
    {
        var catalog = CommandCatalogIndex.FromDescriptors([
            new CommandDescriptor
            {
                Domain = "", Object = "", Intent = "",
                CommandId = "help", Path = "help", Help = "Help",
            },
            new CommandDescriptor
            {
                Domain = "", Object = "", Intent = "",
                CommandId = "file.open", Path = "file open", Help = "Open file", ArgTail = "required",
            },
        ]);

        var root = SlashStepCompletion.GetSuggestions(catalog, "");
        Assert.Contains(root, i => i.StepSegment == "help");
        Assert.Contains(root, i => i.StepSegment == "file");

        var fileStep = SlashStepCompletion.GetSuggestions(catalog, "file ");
        Assert.Contains(fileStep, i => i.StepSegment == "open");
    }

    [Fact]
    public void StepCompletion_static_enum_picker_filters_by_partial()
    {
        var catalog = CommandCatalogIndex.FromDescriptors([
            new CommandDescriptor
            {
                Domain = "editor", Object = "format", Intent = "mode",
                CommandId = "editor.format.mode",
                Path = "format mode",
                ArgTail = "picker:enum:text_mode",
                ArgPickerChoices = CommandPickerChoices.FromLabels(
                    ("md", "Markdown"),
                    ("html", "HTML")),
            },
        ]);

        var items = SlashStepCompletion.GetSuggestions(catalog, "format mode ");
        Assert.Equal(2, items.Count);
        Assert.All(items, item => Assert.Equal(SlashCompletionItemKind.Picker, item.Kind));
        Assert.Contains(items, item => item.PickValue == "md");

        var filtered = SlashStepCompletion.GetSuggestions(catalog, "format mode h");
        Assert.Single(filtered);
        Assert.Equal("html", filtered[0].PickValue);
        Assert.Equal("/format mode html", filtered[0].InsertText);
    }

    [Fact]
    public void StepCompletion_dynamic_picker_uses_choice_source()
    {
        var catalog = CommandCatalogIndex.FromDescriptors([
            new CommandDescriptor
            {
                Domain = "dash", Object = "select", Intent = "app",
                CommandId = "dash.select.app",
                Path = "select app",
                ArgTail = "picker:dash.field.app",
            },
        ]);

        ICommandArgSuggestionBroker broker = new StubArgSuggestionBroker(
            new Dictionary<string, IReadOnlyList<CommandPickerChoice>>
            {
                ["dash.field.app"] =
                [
                    new CommandPickerChoice { Value = "AutoCAD", Label = "AutoCAD" },
                    new CommandPickerChoice { Value = "Revit", Label = "Revit" },
                ],
            });

        var items = SlashStepCompletion.GetSuggestions(catalog, "select app ", broker);
        Assert.Equal(2, items.Count);
        Assert.Contains(items, item => item.PickValue == "AutoCAD");

        var filtered = SlashStepCompletion.GetSuggestions(catalog, "select app rev", broker);
        Assert.Single(filtered);
        Assert.Equal("Revit", filtered[0].PickValue);
    }

    [Fact]
    public void PickerChoices_FromEnum_builds_values()
    {
        var choices = CommandPickerChoices.FromEnum<TextModes>();
        Assert.Equal(["Md", "Html"], choices.Select(choice => choice.Value));
    }

    private enum TextModes
    {
        Md,
        Html,
    }

    [Fact]
    public void CompletionResult_reports_free_text_mode_for_required_arg()
    {
        var catalog = CommandCatalogIndex.FromDescriptors([
            new CommandDescriptor
            {
                Domain = "", Object = "", Intent = "",
                CommandId = "file.open",
                Path = "file open",
                ArgTail = "required",
                ArgHint = "Path relative to repo root",
            },
        ]);

        var result = SlashCompletion.GetResult(catalog, "file open ");
        Assert.Equal(SlashInputMode.FreeText, result.Guidance.Mode);
        Assert.Contains("free text", result.Guidance.Placeholder, StringComparison.OrdinalIgnoreCase);
        Assert.Equal("Path relative to repo root", result.Guidance.Hint);
    }

    [Fact]
    public void CompletionResult_reports_ready_mode_when_runnable()
    {
        var catalog = CommandCatalogIndex.FromDescriptors([
            new CommandDescriptor
            {
                Domain = "", Object = "", Intent = "",
                CommandId = "help", Path = "help", ArgTail = "none",
            },
        ]);

        var result = SlashCompletion.GetResult(catalog, "help");
        Assert.Equal(SlashInputMode.Ready, result.Guidance.Mode);
        Assert.Contains("Enter", result.Guidance.Placeholder, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void CompletionResult_reports_picker_mode_with_breadcrumb()
    {
        var catalog = CommandCatalogIndex.FromDescriptors([
            new CommandDescriptor
            {
                Domain = "editor", Object = "format", Intent = "mode",
                CommandId = "editor.format.mode",
                Path = "format mode",
                ArgTail = "picker:enum:text_mode",
                ArgPickerChoices = CommandPickerChoices.FromLabels(("md", "Markdown")),
            },
        ]);

        var result = SlashCompletion.GetResult(catalog, "format mode ");
        Assert.Equal(SlashInputMode.Picker, result.Guidance.Mode);
        Assert.Contains("format › mode", result.Guidance.Breadcrumb, StringComparison.OrdinalIgnoreCase);
    }

    static CommandCatalogIndex SemanticTestCatalog() =>
        CommandCatalogIndex.FromDescriptors([
            new CommandDescriptor
            {
                Domain = "intercom", Object = "topic", Intent = "list",
                CommandId = "intercom.topic.list", Path = "intercom topic list", Help = "Topics",
            },
            new CommandDescriptor
            {
                Domain = "intercom", Object = "server", Intent = "list",
                CommandId = "intercom.server.list", Path = "intercom server list", Help = "Servers",
            },
            new CommandDescriptor
            {
                Domain = "solution", Object = "build", Intent = "run",
                CommandId = "build.run", Path = "solution build run", PathAliases = ["build run"], Help = "Build run",
            },
            new CommandDescriptor
            {
                Domain = "solution", Object = "build", Intent = "ui",
                CommandId = "build.ui", Path = "solution build ui", PathAliases = ["build ui"], Help = "Build UI",
            },
            new CommandDescriptor
            {
                Domain = "git", Object = "status", Intent = "",
                CommandId = "git.status", Path = "git status", Help = "Git status",
            },
        ]);

    [Fact]
    public void DataBus_IdeHealth_events_exist()
    {
        var b = new AIGuiders.Platform.Cockpit.DataBus.BuildStateChanged(true, 0, true);
        Assert.True(b.IsBuilding);
    }
}
