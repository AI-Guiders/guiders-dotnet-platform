#nullable enable

using Xunit;

namespace AIGuiders.Platform.Tests;

/// <summary>GUIDERS-ADR-0066 Phase 4 — AvalonEdit substrate + no planet AvalonEdit leak.</summary>
public sealed class FederationPhase4ChecklistTests
{
    [Fact]
    public void TextEngine_uses_federation_text_surface_substrate_not_nuget_avalonedit()
    {
        var csproj = FindGuidersWpfFile("src", "AIGuiders.Surface.Wpf.TextEngine", "AIGuiders.Surface.Wpf.TextEngine.csproj");
        var text = File.ReadAllText(csproj);
        Assert.Contains("AIGuiders.TextSurface.Substrate", text);
        Assert.DoesNotContain("PackageReference Include=\"AvalonEdit\"", text);
    }

    [Fact]
    public void Federation_text_surface_submodule_points_at_mirror_fork()
    {
        var gitmodules = FindFederationTextSurfaceFile(".gitmodules");
        var text = File.ReadAllText(gitmodules);
        Assert.Contains("vendor/AvalonEdit", text);
        Assert.Contains("AI-Guiders/AvalonEdit", text);
    }

    [Fact]
    public void Adr_0066_phase_4_is_shipped()
    {
        var adr = FindRepoFile("docs", "adr", "GUIDERS-ADR-0066-code-center-federation-product.md");
        var text = File.ReadAllText(adr);
        Assert.Contains("ship-avalonedit-phase4", text);
        Assert.Contains("| **4** | Submodule mirror-fork", text);
        Assert.DoesNotContain("| **4** | Submodule mirror-fork + deprecate NuGet/direct AvalonEdit refs in federation samples | Planned", text);
    }

    [Fact]
    public void Living_matrix_records_ship_avalonedit_phase4()
    {
        var matrix = File.ReadAllText(FindGuidersFsharpFile("docs", "federation", "model-extraction-living-matrix.md"));
        Assert.Contains("ship-avalonedit-phase4", matrix);
        Assert.Contains("**shipped**", matrix);
    }

    [Fact]
    public void Guiders_wpf_has_no_avalonedit_nuget_package_references()
    {
        var root = FindGuidersWpfRoot();
        foreach (var csproj in Directory.EnumerateFiles(root, "*.csproj", SearchOption.AllDirectories))
        {
            var text = File.ReadAllText(csproj);
            Assert.DoesNotContain("PackageReference Include=\"AvalonEdit\"", text);
        }
    }

    [Fact]
    public void DashSpec_studio_editor_has_no_direct_icsharpcode_avalonedit_usings()
    {
        var editorDir = FindDashSpecStudioFile("src", "DashSpec.Studio.Wpf", "Editor");
        Assert.True(Directory.Exists(editorDir), editorDir);
        foreach (var file in Directory.EnumerateFiles(editorDir, "*.cs"))
        {
            var text = File.ReadAllText(file);
            Assert.DoesNotContain("using ICSharpCode.AvalonEdit", text);
        }
    }

    [Fact]
    public void CodeCenter_exposes_text_completion_facade_for_planets()
    {
        var presenter = FindGuidersWpfFile("src", "AIGuiders.Surface.Wpf.TextEngine", "TextCompletionPresenter.cs");
        var control = FindGuidersWpfFile("src", "AIGuiders.Surface.Wpf.CodeCenter", "CodeCenterEditorControl.xaml.cs");
        Assert.True(File.Exists(presenter), presenter);
        var controlText = File.ReadAllText(control);
        Assert.Contains("ShowTextCompletion", controlText);
        Assert.Contains("TextCompletionPresenter", controlText);
    }

    static string FindRepoFile(params string[] parts)
    {
        for (var dir = new DirectoryInfo(AppContext.BaseDirectory); dir is not null; dir = dir.Parent)
        {
            var candidate = Path.Combine([dir.FullName, .. parts]);
            if (File.Exists(candidate))
                return candidate;
        }

        throw new InvalidOperationException($"Could not locate {string.Join('/', parts)}.");
    }

    static string FindGuidersWpfRoot()
    {
        for (var dir = new DirectoryInfo(AppContext.BaseDirectory); dir is not null; dir = dir.Parent)
        {
            var candidate = Path.Combine(dir.FullName, "guiders-wpf");
            if (Directory.Exists(candidate))
                return candidate;
        }

        throw new InvalidOperationException("Could not locate guiders-wpf root.");
    }

    static string FindGuidersWpfFile(params string[] parts)
    {
        var candidate = Path.Combine([FindGuidersWpfRoot(), .. parts]);
        if (File.Exists(candidate))
            return candidate;

        throw new InvalidOperationException($"Could not locate guiders-wpf/{string.Join('/', parts)}.");
    }

    static string FindGuidersFsharpFile(params string[] parts)
    {
        for (var dir = new DirectoryInfo(AppContext.BaseDirectory); dir is not null; dir = dir.Parent)
        {
            var sibling = Path.Combine([dir.FullName, "guiders-fsharp", .. parts]);
            if (File.Exists(sibling))
                return sibling;
        }

        throw new InvalidOperationException($"Could not locate guiders-fsharp/{string.Join('/', parts)}.");
    }

    static string FindFederationTextSurfaceFile(params string[] parts)
    {
        for (var dir = new DirectoryInfo(AppContext.BaseDirectory); dir is not null; dir = dir.Parent)
        {
            var sibling = Path.Combine([dir.FullName, "federation-text-surface", .. parts]);
            if (File.Exists(sibling))
                return sibling;
        }

        throw new InvalidOperationException($"Could not locate federation-text-surface/{string.Join('/', parts)}.");
    }

    static string FindDashSpecStudioFile(params string[] parts)
    {
        for (var dir = new DirectoryInfo(AppContext.BaseDirectory); dir is not null; dir = dir.Parent)
        {
            var sibling = Path.Combine([dir.FullName, "dash-spec-studio", .. parts]);
            if (File.Exists(sibling) || Directory.Exists(sibling))
                return sibling;
        }

        throw new InvalidOperationException($"Could not locate dash-spec-studio/{string.Join('/', parts)}.");
    }
}
