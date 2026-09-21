using System.Reflection;
using System.Runtime.Loader;
using AIGuiders.Platform.Execution.Language;
using AIGuiders.Platform.Language.CSharp;
using Xunit;

namespace AIGuiders.Platform.Execution.Language.Tests;

public class LanguageFamilyAssemblyLoaderTests
{
    [Fact]
    public void Csharp_family_activates_directly() =>
        Assert.Equal("language.csharp", new CsharpLanguageFamily().PluginId);

    [Fact]
    public void Load_csharp_plugin_from_output_directory()
    {
        var path = Path.Combine(
            AppContext.BaseDirectory,
            "AIGuiders.Platform.Language.CSharp.dll");
        Assert.True(File.Exists(path));

        var assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(path);
        var types = assembly.GetExportedTypes().Select(t => t.FullName).ToArray();
        Assert.Contains("AIGuiders.Platform.Language.CSharp.CsharpLanguageFamily", types);

        var family = LanguageFamilyAssemblyLoader.Load(
            "AIGuiders.Platform.Language.CSharp",
            "AIGuiders.Platform.Language.CSharp.CsharpLanguageFamily",
            AppContext.BaseDirectory);
        Assert.Equal("language.csharp", family.PluginId);
    }

    [Fact]
    public void LoadFederation_from_test_output_resolves_all_shipped_plugins()
    {
        var families = StandardLanguageFamilyManifest.LoadFederation(AppContext.BaseDirectory);
        Assert.Equal(3, families.Count);
        Assert.Contains(families, f => f.PluginId == "language.fsharp");
        Assert.Contains(families, f => f.PluginId == "language.gdl");
        Assert.Contains(families, f => f.PluginId == "language.csharp");
    }
}
