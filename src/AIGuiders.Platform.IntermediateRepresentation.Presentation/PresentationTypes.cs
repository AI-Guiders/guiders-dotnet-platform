#nullable enable

using System.Linq;
using GdlPresentation = AIGuiders.Platform.Modeling.Gdl.Presentation;

namespace AIGuiders.Platform.IntermediateRepresentation.Presentation;

/// <summary>Logical host / scan slot from topology wire — ordered, not tied to OS monitor index.</summary>
public sealed record LogicalDisplayHost(
    int HostIndex,
    string HostId,
    AttentionDisplayRole Role,
    ZoneComposeKind Compose,
    IReadOnlyList<string> ChannelStack,
    string ActiveChannel)
{
    public GdlPresentation.LogicalDisplayHost ToModel() => new(
        HostIndex,
        HostId,
        Role,
        Compose,
        FSharpInterop.ToFSharpList(ChannelStack),
        ActiveChannel);

    public static LogicalDisplayHost FromModel(GdlPresentation.LogicalDisplayHost model) => new(
        model.HostIndex,
        model.HostId,
        model.Role,
        model.Compose,
        FSharpInterop.FromList(model.ChannelStack),
        model.ActiveChannel);
}

/// <summary>Semantic topology from <c>.deck</c> — CDS and surfaces consume this, not raw strings.</summary>
public sealed record PresentationTopology(
    TopologyArrangement Arrangement,
    IReadOnlyList<LogicalDisplayHost> Hosts,
    string SourceWire)
{
    public int HostCount => ToModel().HostCount;

    public GdlPresentation.PresentationTopology ToModel() => new(
        Arrangement,
        FSharpInterop.ToFSharpList(Hosts.Select(static host => host.ToModel())),
        SourceWire);

    public static PresentationTopology FromModel(GdlPresentation.PresentationTopology model) => new(
        model.Arrangement,
        FSharpInterop.FromList(model.Hosts).Select(LogicalDisplayHost.FromModel).ToArray(),
        model.SourceWire);
}

public sealed record PhysicalScreenSelector(
    PhysicalScreenSelectorKind Kind,
    int? ScreenIndex = null,
    string? DeviceName = null,
    double? RegionLeft = null,
    double? RegionTop = null,
    double? RegionWidth = null,
    double? RegionHeight = null)
{
    public GdlPresentation.PhysicalScreenSelector ToModel() => new(
        Kind,
        FSharpInterop.OptInt(ScreenIndex),
        FSharpInterop.OptString(DeviceName),
        FSharpInterop.OptDouble(RegionLeft),
        FSharpInterop.OptDouble(RegionTop),
        FSharpInterop.OptDouble(RegionWidth),
        FSharpInterop.OptDouble(RegionHeight));

    public static PhysicalScreenSelector FromModel(GdlPresentation.PhysicalScreenSelector model) => new(
        model.Kind,
        FSharpInterop.OptInt(model.ScreenIndex),
        FSharpInterop.OptString(model.DeviceName),
        FSharpInterop.OptDouble(model.RegionLeft),
        FSharpInterop.OptDouble(model.RegionTop),
        FSharpInterop.OptDouble(model.RegionWidth),
        FSharpInterop.OptDouble(model.RegionHeight));
}

/// <summary>Runtime binding: logical <see cref="LogicalDisplayHost.HostIndex"/> → physical screen.</summary>
public sealed record DisplayHostBinding(int HostIndex, PhysicalScreenSelector Screen)
{
    public GdlPresentation.DisplayHostBinding ToModel() => new(
        HostIndex,
        Screen.ToModel());

    public static DisplayHostBinding FromModel(GdlPresentation.DisplayHostBinding model) => new(
        model.HostIndex,
        PhysicalScreenSelector.FromModel(model.Screen));
}

/// <summary>Operator / machine display layout — separate from <see cref="PresentationTopology"/>.</summary>
public sealed record DisplayBindingProfile(
    string ProfileId,
    IReadOnlyList<DisplayHostBinding> Bindings)
{
    public GdlPresentation.DisplayBindingProfile ToModel() => new(
        ProfileId,
        Bindings.Select(static binding => binding.ToModel()).ToArray());

    public static DisplayBindingProfile FromModel(GdlPresentation.DisplayBindingProfile model) => new(
        model.ProfileId,
        model.Bindings.Select(DisplayHostBinding.FromModel).ToArray());
}
