#nullable enable
using AIGuiders.Platform.Conformance.Policies;
using AIGuiders.Platform.Conformance.Schemas;
using Xunit;

namespace AIGuiders.Platform.Tests;

public sealed class PolicySpecConformanceTests
{
    [Theory]
    [InlineData("slash-ship-first.spec.json")]
    [InlineData("binding-overlay-wins.spec.json")]
    [InlineData("workspace-field-overlay.spec.json")]
    public void Docs_policy_json_specs_pass_schema_and_harness(string fileName)
    {
        var path = Path.Combine(FindConformanceRoot(), "policies", fileName);
        var json = File.ReadAllText(path);
        var schemaErrors = ConformanceSchemaValidator.ValidatePolicyJson(json);
        Assert.True(schemaErrors.Count == 0, string.Join(Environment.NewLine, schemaErrors));

        var spec = PolicySpecLoader.LoadJson(json);
        var errors = PolicySpecConformance.ValidateDocument(spec);
        Assert.True(errors.Count == 0, string.Join(Environment.NewLine, errors));
    }

    [Fact]
    public void Docs_workspace_policy_toml_matches_json_vectors()
    {
        var root = FindConformanceRoot();
        var jsonPath = Path.Combine(root, "policies", "workspace-field-overlay.spec.json");
        var tomlPath = Path.Combine(root, "policies", "workspace-field-overlay.spec.toml");

        var jsonSpec = PolicySpecLoader.LoadJson(File.ReadAllText(jsonPath));
        var tomlSpec = PolicySpecLoader.LoadFile(tomlPath);

        Assert.Equal(jsonSpec.Policy, tomlSpec.Policy);
        Assert.Equal(jsonSpec.Semantics, tomlSpec.Semantics);
        Assert.Equal(jsonSpec.Vectors.Count, tomlSpec.Vectors.Count);

        var tomlSchemaErrors = PolicySpecFormats.ValidateFile(tomlPath);
        Assert.True(tomlSchemaErrors.Count == 0, string.Join(Environment.NewLine, tomlSchemaErrors));

        var tomlErrors = PolicySpecConformance.ValidateDocument(tomlSpec);
        Assert.True(tomlErrors.Count == 0, string.Join(Environment.NewLine, tomlErrors));
    }

    static string FindConformanceRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null)
        {
            var candidate = Path.Combine(dir.FullName, "docs", "conformance");
            if (Directory.Exists(candidate))
                return candidate;
            dir = dir.Parent;
        }

        throw new InvalidOperationException("Could not locate docs/conformance.");
    }
}
