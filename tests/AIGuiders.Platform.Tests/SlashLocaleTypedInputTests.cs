#nullable enable

using System.Globalization;
using AIGuiders.Platform.CommandPlane;
using Xunit;

namespace AIGuiders.Platform.Tests;

public sealed class SlashLocaleDateParserTests
{
    static readonly SlashLocaleInputProfile RuProfile =
        SlashLocaleInputProfile.FromCulture(CultureInfo.GetCultureInfo("ru-RU"));

    [Fact]
    public void TryParse_complete_ru_date_emits_day_wire()
    {
        Assert.True(SlashLocaleDateParser.TryParse("31.08.2026", RuProfile, out var parts, out var completeness));
        Assert.Equal(SlashLocaleDateCompleteness.CompleteDate, completeness);
        Assert.True(SlashLocaleDateParser.TryToDayWire(parts, out var wire));
        Assert.Equal("2026-08-31", wire);
    }

    [Fact]
    public void TryParse_month_year_ru_emits_month_wire()
    {
        Assert.True(SlashLocaleDateParser.TryParse("08.2026", RuProfile, out var parts, out var completeness));
        Assert.Equal(SlashLocaleDateCompleteness.MonthYear, completeness);
        Assert.True(SlashLocaleDateParser.TryToMonthWire(parts, out var wire));
        Assert.Equal("2026-08", wire);
    }

    [Fact]
    public void TryParse_range_ru_emits_range_wire()
    {
        Assert.True(SlashLocaleDateParser.TryParse(
            "31.08.2026 .. 15.09.2026",
            RuProfile,
            out var parts,
            out var completeness));
        Assert.Equal(SlashLocaleDateCompleteness.CompleteRange, completeness);
        Assert.True(SlashLocaleDateParser.TryToRangeWire(parts, out var wire));
        Assert.Equal("2026-08-31..2026-09-15", wire);
    }

    [Fact]
    public void Profile_from_en_us_uses_ambient_pattern()
    {
        var profile = SlashLocaleInputProfile.FromCulture(CultureInfo.GetCultureInfo("en-US"));
        Assert.Contains('/', profile.ShortDatePattern);
    }
}

public sealed class SlashLocaleTypedConstructorCoordinatorTests
{
  static readonly SlashLocaleInputProfile RuProfile =
        SlashLocaleInputProfile.FromCulture(CultureInfo.GetCultureInfo("ru-RU"));

    [Fact]
    public void Coordinator_ready_on_complete_locale_date()
    {
        var registry = BuildDateRegistry();
        var navigator = new SlashValueConstructorNavigator(registry, new StubSegmentProvider());
        var coordinator = new SlashLocaleTypedConstructorCoordinator(navigator, registry);
        var session = new SlashConstructorSession(navigator);
        var catalog = SlashCatalogIndex.FromDescriptors([
            new SlashCommandDescriptor
            {
                Domain = "",
                Object = "",
                Intent = "",
                CommandId = "select.date",
                Path = "select filter usage_date",
                ArgTail = "picker+constructor:+date_month+date_range",
                ArgConstructors =
                [
                    new SlashConstructorBinding("date_month", "Month", "Month grain"),
                    new SlashConstructorBinding("date_range", "Range", "Date range"),
                ],
            },
        ]);

        SlashLineResolver.TryResolveBody(
            "select filter usage_date 31.08.2026",
            catalog,
            out var line);
        catalog.TryGet(line.CanonicalPath, out var route);

        Assert.True(coordinator.TryHandleArgTail(
            line,
            route,
            "31.08.2026",
            session,
            RuProfile,
            out var result));
        Assert.NotNull(result);
        Assert.Equal(SlashInputMode.Ready, result!.Guidance.Mode);
        Assert.Equal("2026-08-31", result.Guidance.ReadyWire);
    }

    [Fact]
    public void Coordinator_ready_on_complete_month_year_locale()
    {
        var registry = BuildDateRegistry();
        var navigator = new SlashValueConstructorNavigator(registry, new StubSegmentProvider());
        var coordinator = new SlashLocaleTypedConstructorCoordinator(navigator, registry);
        var session = new SlashConstructorSession(navigator);
        var catalog = SlashCatalogIndex.FromDescriptors([
            new SlashCommandDescriptor
            {
                Domain = "",
                Object = "",
                Intent = "",
                CommandId = "select.date",
                Path = "select filter usage_date",
                ArgTail = "picker+constructor:+date_month",
                ArgConstructors = [new SlashConstructorBinding("date_month", "Month", "Month grain")],
            },
        ]);

        SlashLineResolver.TryResolveBody("select filter usage_date 08.2026", catalog, out var line);
        catalog.TryGet(line.CanonicalPath, out var route);

        Assert.True(coordinator.TryHandleArgTail(line, route, "08.2026", session, RuProfile, out var result));
        Assert.NotNull(result);
        Assert.Equal(SlashInputMode.Ready, result!.Guidance.Mode);
        Assert.Equal("2026-08", result.Guidance.ReadyWire);
        Assert.False(session.IsActive);
    }

    static SlashValueConstructorRegistry BuildDateRegistry()
    {
        var registry = new SlashValueConstructorRegistry();
        registry.Register(new SlashLeafConstructorDefinition(
            "month_grain",
            "Month",
            [
                new SlashConstructorSegmentDefinition("year", "Year"),
                new SlashConstructorSegmentDefinition("month", "Month", WireMinWidth: 2, DisplayMinWidth: 2),
            ],
            WirePattern: "{year}-{month}",
            DisplayPattern: "{month}.{year}"));

        registry.Register(new SlashCompositeConstructorDefinition(
            "date_month",
            "Month",
            [new SlashConstructorSlotDefinition("value", "month_grain", "Month")],
            WirePattern: "{value}"));

        registry.Register(new SlashLeafConstructorDefinition(
            "date",
            "Date",
            [
                new SlashConstructorSegmentDefinition("year", "Year"),
                new SlashConstructorSegmentDefinition("month", "Month", WireMinWidth: 2, DisplayMinWidth: 2),
                new SlashConstructorSegmentDefinition("day", "Day", WireMinWidth: 2, DisplayMinWidth: 2),
            ],
            WirePattern: "{year}-{month}-{day}",
            DisplayPattern: "{day}.{month}.{year}"));

        registry.Register(new SlashCompositeConstructorDefinition(
            "date_range",
            "Range",
            [
                new SlashConstructorSlotDefinition("from", "date", "From"),
                new SlashConstructorSlotDefinition("to", "date", "To", SeparatorBefore: ".."),
            ],
            WirePattern: "{from}..{to}"));

        return registry;
    }

    sealed class StubSegmentProvider : ISlashConstructorSegmentProvider
    {
        public IReadOnlyList<SlashCompletionItem> GetSegmentSuggestions(
            SlashLeafConstructorDefinition leaf,
            int segmentIndex,
            SlashConstructorDraft draft,
            string partial) => [];
    }
}
