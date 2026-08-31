#nullable enable

using System.Globalization;

namespace AIGuiders.Platform.CommandPlane;

/// <summary>Ambient culture for slash locale input (GUIDERS-ADR-0037). Host-provided; Platform does not impose locale.</summary>
public interface ISlashCultureAmbient
{
    CultureInfo Culture { get; }
}

/// <summary>Wraps a culture supplied by the host surface.</summary>
public sealed class SlashCultureAmbient(CultureInfo culture) : ISlashCultureAmbient
{
    public static ISlashCultureAmbient Current { get; } = new SlashCultureAmbient(CultureInfo.CurrentCulture);

    public CultureInfo Culture { get; } = culture;
}
