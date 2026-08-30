namespace AIGuiders.Platform.Utilities.Adoption;

/// <summary>Report sink: IR rows → wire output (markdown, json, …).</summary>
public interface IAdoptionReportWriter
{
    string Write(IReadOnlyList<PlanetAdoptionRow> rows, DateTimeOffset generatedAtUtc);
}
