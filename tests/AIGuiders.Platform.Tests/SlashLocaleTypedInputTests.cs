using AIGuiders.Platform.IntermediateRepresentation.Command;
#nullable enable

using System.Globalization;
using AIGuiders.Platform.CommandPlane;
using Xunit;

namespace AIGuiders.Platform.Tests;

public sealed class LocaleDateParserTests
{
    static readonly LocaleInputProfile RuProfile =
        LocaleInputProfile.FromCulture(CultureInfo.GetCultureInfo("ru-RU"));

    [Fact]
    public void TryParse_complete_ru_date_emits_day_wire()
    {
        Assert.True(LocaleDateParser.TryParse("31.08.2026", RuProfile, out var parts, out var completeness));
        Assert.Equal(LocaleDateCompleteness.CompleteDate, completeness);
        Assert.True(LocaleDateParser.TryToDayWire(parts, out var wire));
        Assert.Equal("2026-08-31", wire);
    }

    [Fact]
    public void TryParse_month_year_ru_emits_month_wire()
    {
        Assert.True(LocaleDateParser.TryParse("08.2026", RuProfile, out var parts, out var completeness));
        Assert.Equal(LocaleDateCompleteness.MonthYear, completeness);
        Assert.True(LocaleDateParser.TryToMonthWire(parts, out var wire));
        Assert.Equal("2026-08", wire);
    }

    [Fact]
    public void TryParse_range_ru_emits_range_wire()
    {
        Assert.True(LocaleDateParser.TryParse(
            "31.08.2026 .. 15.09.2026",
            RuProfile,
            out var parts,
            out var completeness));
        Assert.Equal(LocaleDateCompleteness.CompleteRange, completeness);
        Assert.True(LocaleDateParser.TryToRangeWire(parts, out var wire));
        Assert.Equal("2026-08-31..2026-09-15", wire);
    }

    [Fact]
    public void Profile_from_en_us_uses_ambient_pattern()
    {
        var profile = LocaleInputProfile.FromCulture(CultureInfo.GetCultureInfo("en-US"));
        Assert.Contains('/', profile.ShortDatePattern);
    }
}

public sealed class PrefixArmCoordinatorTests
{
    static readonly LocaleInputProfile RuProfile =
        LocaleInputProfile.FromCulture(CultureInfo.GetCultureInfo("ru-RU"));

    static readonly IPrefixArmProfile DateProfile =
        new LocaleDatePrefixArmProfile(new CultureAmbient(CultureInfo.GetCultureInfo("ru-RU")));

    [Fact]
    public void Coordinator_ready_on_complete_locale_date()
    {
        var registry = BuildDateRegistry();
        var navigator = new ValueConstructorNavigator(registry, new StubSegmentProvider());
        var coordinator = new PrefixArmCoordinator(navigator, registry);
        var session = new ArgConstructorSession(navigator);
        var catalog = CommandCatalogIndex.FromDescriptors([
            new CommandDescriptor
            {
                Domain = "",
                Object = "",
                Intent = "",
                CommandId = "select.date",
                Path = "select filter usage_date",
                ArgTail = "picker+constructor:+date_month+date_range",
                ArgConstructors =
                [
                    new ArgConstructorBinding("date_month", "Month", "Month grain"),
                    new ArgConstructorBinding("date_range", "Range", "Date range"),
                ],
            },
        ]);

        SlashLineResolver.TryResolveBody(
            "select filter usage_date 31.08.2026",
            catalog,
            out var line);
        catalog.TryGet(line.CanonicalPath, out var route);

        Assert.True(coordinator.TryHandle(
            line.CanonicalPath,
            "31.08.2026",
            route.ToPrefixArmSite(),
            session,
            [DateProfile],
            RuProfile,
            out var result));
        Assert.IsType<PrefixArmReadyResult>(result);
        var ready = (PrefixArmReadyResult)result!;
        Assert.Equal("2026-08-31", ready.Wire);
    }

    [Fact]
    public void Coordinator_ready_on_complete_month_year_locale()
    {
        var registry = BuildDateRegistry();
        var navigator = new ValueConstructorNavigator(registry, new StubSegmentProvider());
        var coordinator = new PrefixArmCoordinator(navigator, registry);
        var session = new ArgConstructorSession(navigator);
        var catalog = CommandCatalogIndex.FromDescriptors([
            new CommandDescriptor
            {
                Domain = "",
                Object = "",
                Intent = "",
                CommandId = "select.date",
                Path = "select filter usage_date",
                ArgTail = "picker+constructor:+date_month",
                ArgConstructors = [new ArgConstructorBinding("date_month", "Month", "Month grain")],
            },
        ]);

        SlashLineResolver.TryResolveBody("select filter usage_date 08.2026", catalog, out var line);
        catalog.TryGet(line.CanonicalPath, out var route);

        Assert.True(coordinator.TryHandle(
            line.CanonicalPath,
            "08.2026",
            route.ToPrefixArmSite(),
            session,
            [DateProfile],
            RuProfile,
            out var result));
        Assert.IsType<PrefixArmReadyResult>(result);
        Assert.Equal("2026-08", ((PrefixArmReadyResult)result!).Wire);
        Assert.False(session.IsActive);
    }

    [Fact]
    public void Mock_profile_ready_when_prefix_matches()
    {
        var registry = new ValueConstructorRegistry();
        var navigator = new ValueConstructorNavigator(registry, new StubSegmentProvider());
        var coordinator = new PrefixArmCoordinator(navigator, registry);
        var session = new ArgConstructorSession(navigator);
        var site = PrefixArmSite.FromBindings([], null, "Echo", "required");

        Assert.True(coordinator.TryHandle(
            "demo echo",
            "hello",
            site,
            session,
            [new EchoPrefixProfile()],
            localeProfile: null,
            out var result));
        Assert.IsType<PrefixArmReadyResult>(result);
        Assert.Equal("HELLO", ((PrefixArmReadyResult)result!).Wire);
    }

    sealed class EchoPrefixProfile : IPrefixArmProfile
    {
        public string ProfileId => "echo";

        public bool TryMatch(string partial, PrefixArmSite site, out PrefixArmMatch match)
        {
            if (partial.Equals("hello", StringComparison.OrdinalIgnoreCase))
            {
                match = new PrefixArmMatch(PrefixArmDisposition.Ready, partial.ToUpperInvariant(), partial);
                return true;
            }

            match = PrefixArmMatch.NoMatch;
            return false;
        }
    }

    static ValueConstructorRegistry BuildDateRegistry()
    {
        var registry = new ValueConstructorRegistry();
        registry.Register(new LeafConstructorDefinition(
            "month_grain",
            "Month",
            [
                new ConstructorSegmentDefinition("year", "Year"),
                new ConstructorSegmentDefinition("month", "Month", WireMinWidth: 2, DisplayMinWidth: 2),
            ],
            WirePattern: "{year}-{month}",
            DisplayPattern: "{month}.{year}"));

        registry.Register(new CompositeConstructorDefinition(
            "date_month",
            "Month",
            [new ConstructorSlotDefinition("value", "month_grain", "Month")],
            WirePattern: "{value}"));

        registry.Register(new LeafConstructorDefinition(
            "date",
            "Date",
            [
                new ConstructorSegmentDefinition("year", "Year"),
                new ConstructorSegmentDefinition("month", "Month", WireMinWidth: 2, DisplayMinWidth: 2),
                new ConstructorSegmentDefinition("day", "Day", WireMinWidth: 2, DisplayMinWidth: 2),
            ],
            WirePattern: "{year}-{month}-{day}",
            DisplayPattern: "{day}.{month}.{year}"));

        registry.Register(new CompositeConstructorDefinition(
            "date_range",
            "Range",
            [
                new ConstructorSlotDefinition("from", "date", "From"),
                new ConstructorSlotDefinition("to", "date", "To", SeparatorBefore: ".."),
            ],
            WirePattern: "{from}..{to}"));

        return registry;
    }

    sealed class StubSegmentProvider : IConstructorSegmentProvider
    {
        public IReadOnlyList<ArgCompletionItem> GetSegmentSuggestions(
            LeafConstructorDefinition leaf,
            int segmentIndex,
            ArgConstructorDraft draft,
            string partial) => [];
    }
}
