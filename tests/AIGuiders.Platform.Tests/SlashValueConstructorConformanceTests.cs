#nullable enable

using System.Globalization;
using System.Reflection;
using System.Text.Json;
using AIGuiders.Platform.CommandPlane;
using Xunit;

namespace AIGuiders.Platform.Tests;

public sealed class SlashValueConstructorConformanceTests
{
    [Fact]
    public void Locale_vectors_emit_expected_wire()
    {
        var json = LoadEmbedded("AIGuiders.Platform.Tests.Fixtures.Slash.slash-value-constructor.spec.json");
        using var document = JsonDocument.Parse(json);
        var cultureName = document.RootElement.GetProperty("culture").GetString() ?? "ru-RU";
        var profile = SlashLocaleInputProfile.FromCulture(CultureInfo.GetCultureInfo(cultureName));

        foreach (var vector in document.RootElement.GetProperty("vectors").EnumerateArray())
        {
            var input = vector.GetProperty("input").GetString()!;
            var expectedWire = vector.GetProperty("wire").GetString()!;
            Assert.True(SlashLocaleDateParser.TryParse(input, profile, out var parts, out var completeness));
            var wire = completeness switch
            {
                SlashLocaleDateCompleteness.CompleteDate => SlashLocaleDateParser.TryToDayWire(parts, out var day)
                    ? day
                    : "",
                SlashLocaleDateCompleteness.MonthYear => SlashLocaleDateParser.TryToMonthWire(parts, out var month)
                    ? month
                    : "",
                SlashLocaleDateCompleteness.CompleteRange => SlashLocaleDateParser.TryToRangeWire(parts, out var range)
                    ? range
                    : "",
                _ => "",
            };
            Assert.Equal(expectedWire, wire);
        }
    }

    static string LoadEmbedded(string resourceName)
    {
        var asm = Assembly.GetExecutingAssembly();
        using var stream = asm.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException($"Missing embedded resource: {resourceName}");
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
