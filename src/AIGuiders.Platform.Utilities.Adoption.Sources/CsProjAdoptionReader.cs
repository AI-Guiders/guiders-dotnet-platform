using System.Xml.Linq;

namespace AIGuiders.Platform.Utilities.Adoption.Sources;

/// <summary>Format reader: MSBuild csproj → <see cref="AdoptionPartialFacts"/>.</summary>
public static class CsProjAdoptionReader
{
    public static AdoptionPartialFacts ReadFile(string path, string packageIdPrefix)
    {
        var pins = new List<AdoptionPin>();
        try
        {
            var doc = XDocument.Load(path, LoadOptions.PreserveWhitespace);
            ReadDocument(doc, packageIdPrefix, pins);
        }
        catch
        {
            return AdoptionPartialFacts.Empty;
        }

        return new AdoptionPartialFacts(pins, []);
    }

    public static void ReadDocument(XDocument doc, string packageIdPrefix, List<AdoptionPin> pins)
    {
        var projectRefs = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var el in doc.Descendants())
        {
            if (!el.Name.LocalName.Equals("ProjectReference", StringComparison.Ordinal))
                continue;

            var include = el.Attribute("Include")?.Value;
            if (string.IsNullOrWhiteSpace(include))
                continue;

            var fileName = Path.GetFileNameWithoutExtension(include);
            if (!fileName.StartsWith(packageIdPrefix, StringComparison.Ordinal))
                continue;

            projectRefs.Add(fileName);
        }

        foreach (var el in doc.Descendants())
        {
            if (!el.Name.LocalName.Equals("PackageReference", StringComparison.Ordinal))
                continue;

            var include = el.Attribute("Include")?.Value ?? el.Attribute("Update")?.Value;
            if (string.IsNullOrWhiteSpace(include) || !include.StartsWith(packageIdPrefix, StringComparison.Ordinal))
                continue;

            var version = el.Attribute("Version")?.Value
                ?? el.Element(el.Name.Namespace + "Version")?.Value;

            var kind = projectRefs.Contains(include) ? AdoptionPortKind.ProjectRef : AdoptionPortKind.NuGetPin;
            pins.Add(new AdoptionPin(include, version, kind));
        }

        foreach (var id in projectRefs)
        {
            if (pins.All(p => !p.PackageId.Equals(id, StringComparison.OrdinalIgnoreCase)))
                pins.Add(new AdoptionPin(id, null, AdoptionPortKind.ProjectRef));
        }
    }
}
