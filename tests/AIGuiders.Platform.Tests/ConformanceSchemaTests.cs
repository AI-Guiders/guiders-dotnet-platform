#nullable enable
using System.Reflection;
using AIGuiders.Platform.Conformance.Schemas;
using Xunit;

namespace AIGuiders.Platform.Tests;

public sealed class ConformanceSchemaTests
{
    [Theory]
    [InlineData("AIGuiders.Platform.Tests.Fixtures.Notation.command-slash.spec.json")]
    [InlineData("AIGuiders.Platform.Tests.Fixtures.Notation.command-console.spec.json")]
    [InlineData("AIGuiders.Platform.Tests.Fixtures.Notation.argument-kv.spec.json")]
    [InlineData("AIGuiders.Platform.Tests.Fixtures.Notation.argument-delimited.spec.json")]
    [InlineData("AIGuiders.Platform.Tests.Fixtures.Notation.argument-positional.spec.json")]
    [InlineData("AIGuiders.Platform.Tests.Fixtures.Notation.argument-cli.spec.json")]
    [InlineData("AIGuiders.Platform.Tests.Fixtures.Notation.invocation-parity.spec.json")]
    [InlineData("AIGuiders.Platform.Tests.Fixtures.Notation.bracket-cdp-square-kv.spec.json")]
    [InlineData("AIGuiders.Platform.Tests.Fixtures.Notation.bracket-doc-symbol.spec.json")]
    [InlineData("AIGuiders.Platform.Tests.Fixtures.Slash.slash-arg-completion.spec.json")]
    [InlineData("AIGuiders.Platform.Tests.Fixtures.Slash.slash-line-resolve.spec.json")]
    [InlineData("AIGuiders.Platform.Tests.Fixtures.Quarry.neovim-kbd.spec.json")]
    [InlineData("AIGuiders.Platform.Tests.Fixtures.Quarry.emacs-kbd.spec.json")]
    [InlineData("AIGuiders.Platform.Tests.Fixtures.MCPlane.pulse-default.spec.json")]
    [InlineData("AIGuiders.Platform.Tests.Fixtures.MCPlane.next-hints.spec.json")]
    public void Embedded_spec_matches_json_schema(string resourceName)
    {
        var json = LoadEmbedded(resourceName);
        var errors = ConformanceSchemaValidator.ValidateJson(json);
        Assert.True(errors.Count == 0, string.Join(Environment.NewLine, errors));
    }

    [Fact]
    public void Docs_conformance_specs_match_json_schema()
    {
        var repoRoot = FindRepoRoot();
        var specDir = Path.Combine(repoRoot, "docs", "conformance");
        var specFiles = Directory.GetFiles(specDir, "*.spec.json", SearchOption.TopDirectoryOnly);
        Assert.NotEmpty(specFiles);

        foreach (var specPath in specFiles)
        {
            var json = File.ReadAllText(specPath);
            var errors = ConformanceSchemaValidator.ValidateJson(json);
            Assert.True(errors.Count == 0, $"{Path.GetFileName(specPath)}:{Environment.NewLine}{string.Join(Environment.NewLine, errors)}");
        }
    }

    [Fact]
    public void Catalog_wire_schema_accepts_descriptor_fields()
    {
        const string json = """
            [
              {
                "commandId": "build.run",
                "path": "build run",
                "tailWireClass": "cli",
                "argParameters": [
                  { "name": "config", "kind": "value", "longOption": "--config" }
                ]
              }
            ]
            """;

        var errors = ConformanceSchemaValidator.ValidateCatalogJson(json);
        Assert.Empty(errors);
    }

    static string LoadEmbedded(string resourceName)
    {
        var asm = Assembly.GetExecutingAssembly();
        using var stream = asm.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException($"Missing embedded resource: {resourceName}");
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

    static string FindRepoRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null)
        {
            if (File.Exists(Path.Combine(dir.FullName, "AIGuiders.Platform.slnx")))
                return dir.FullName;
            dir = dir.Parent;
        }

        throw new InvalidOperationException("Could not locate guiders-platform repo root.");
    }
}
