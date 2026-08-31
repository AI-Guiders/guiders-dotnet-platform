#nullable enable

using System.Globalization;

namespace AIGuiders.Platform.CommandPlane;

/// <summary>Ambient culture for locale arg input (GUIDERS-ADR-0037). Host-provided; Platform does not impose locale.</summary>
public interface ICultureAmbient
{
    CultureInfo Culture { get; }
}

/// <summary>Wraps a culture supplied by the host surface.</summary>
public sealed class CultureAmbient(CultureInfo culture) : ICultureAmbient
{
    public static ICultureAmbient Current { get; } = new CultureAmbient(CultureInfo.CurrentCulture);

    public CultureInfo Culture { get; } = culture;
}
