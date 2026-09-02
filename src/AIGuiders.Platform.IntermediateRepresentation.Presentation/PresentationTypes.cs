namespace AIGuiders.Platform.IntermediateRepresentation.Presentation;

/// <summary>Aviation-aligned display role (GUIDERS-ADR-0007).</summary>
public enum AttentionDisplayRole
{
    Unknown = 0,
    Pfd,
    Forward,
    Mfd,
    /// <summary>P/M channel stack on one physical TopLevel (OneOf).</summary>
    PmOneOf,
    Eicas,
    Briefing,
    Hud,
}

/// <summary>How channels share space inside one host window.</summary>
public enum ZoneComposeKind
{
    Split,
    OneOf,
}

/// <summary>How logical hosts relate — independent of physical monitor count.</summary>
public enum TopologyArrangement
{
    /// <summary>One TopLevel; zones composed in-surface (<c>single</c> + layout board).</summary>
    SingleSurfaceCompositional,
    /// <summary>One TopLevel; XOR channel stack (<c>(F/P/M)</c>).</summary>
    SingleHostOneOf,
    /// <summary>2–N logical hosts (scan slots); bind to 1–N physical screens at runtime.</summary>
    MultiHost,
}

/// <summary>Logical host / scan slot from topology wire — ordered, not tied to OS monitor index.</summary>
public sealed record LogicalDisplayHost(
    int HostIndex,
    string HostId,
    AttentionDisplayRole Role,
    ZoneComposeKind Compose,
    IReadOnlyList<string> ChannelStack,
    string ActiveChannel);

/// <summary>Semantic topology from <c>.deck</c> — CDS and surfaces consume this, not raw strings.</summary>
public sealed record PresentationTopology(
    TopologyArrangement Arrangement,
    IReadOnlyList<LogicalDisplayHost> Hosts,
    string SourceWire)
{
    public int HostCount => Hosts.Count;
}

/// <summary>How a logical host maps to a physical screen at runtime (deployment profile).</summary>
public enum PhysicalScreenSelectorKind
{
  Primary,
  Index,
  DeviceName,
  /// <summary>Single ultrawide — host occupies a normalized region (0..1).</summary>
  UltrawideRegion,
}

public sealed record PhysicalScreenSelector(
    PhysicalScreenSelectorKind Kind,
    int? ScreenIndex = null,
    string? DeviceName = null,
    double? RegionLeft = null,
    double? RegionTop = null,
    double? RegionWidth = null,
    double? RegionHeight = null);

/// <summary>Runtime binding: logical <see cref="LogicalDisplayHost.HostIndex"/> → physical screen.</summary>
public sealed record DisplayHostBinding(int HostIndex, PhysicalScreenSelector Screen);

/// <summary>Operator / machine display layout — separate from <see cref="PresentationTopology"/>.</summary>
public sealed record DisplayBindingProfile(
    string ProfileId,
    IReadOnlyList<DisplayHostBinding> Bindings);
