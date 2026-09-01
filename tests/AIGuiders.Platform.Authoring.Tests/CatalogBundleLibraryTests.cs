using AIGuiders.Platform.Authoring.Command.Bundles;
using Xunit;

namespace AIGuiders.Platform.Authoring.Tests;

public sealed class CatalogBundleLibraryTests
{
    [Fact]
    public void Federation_resolves_grain_date_filter()
    {
        Assert.True(CatalogBundleLibrary.Federation.TryResolve("grain/date-filter", out var profiles));
        Assert.Contains(profiles, p => p.Name == "date-filter");
        Assert.Contains(profiles[0].Entries, e => e.Entry == "preset" && e.Ref == "today");
    }
}
