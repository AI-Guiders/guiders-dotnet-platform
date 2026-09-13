#nullable enable

using AIGuiders.Platform.Execution.Presentation.Binding;
using AIGuiders.Platform.IntermediateRepresentation.Presentation;
using Xunit;

namespace AIGuiders.Platform.Authoring.Tests;

public sealed class DisplayBindingProfileBuilderTests
{
    [Fact]
    public void TryBuild_from_toml_like_table()
    {
        var table = new Dictionary<string, object?>
        {
            ["profile_id"] = "default",
            ["bindings"] = new List<Dictionary<string, object?>>
            {
                new()
                {
                    ["host"] = 0,
                    ["screen"] = "primary",
                },
                new()
                {
                    ["host_index"] = 1,
                    ["screen"] = new Dictionary<string, object?>
                    {
                        ["kind"] = "index",
                        ["index"] = 2,
                    },
                },
                new()
                {
                    ["host"] = 2,
                    ["screen"] = "ultrawide:0,0.25,1,0.5",
                },
            },
        };

        var result = DisplayBindingProfileBuilder.TryBuild(table);
        Assert.True(result.IsSuccess, result.Error);
        Assert.Equal("default", result.Profile!.ProfileId);
        Assert.Equal(3, result.Profile.Bindings.Count);
        Assert.Equal(PhysicalScreenSelectorKind.Primary, result.Profile.Bindings[0].Screen.Kind);
        Assert.Equal(2, result.Profile.Bindings[1].Screen.ScreenIndex);
        Assert.Equal(0.25, result.Profile.Bindings[2].Screen.RegionTop);
    }
}
