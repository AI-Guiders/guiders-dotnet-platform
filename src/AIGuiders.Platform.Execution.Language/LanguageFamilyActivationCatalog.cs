namespace AIGuiders.Platform.Execution.Language;

/// <summary>Activation catalog built from language family plugins.</summary>
public sealed class LanguageFamilyActivationCatalog : ILanguageActivationCatalog
{
    readonly IReadOnlyList<ILanguageFamilyPlugin> _families;

    public LanguageFamilyActivationCatalog(IEnumerable<ILanguageFamilyPlugin> families)
    {
        ArgumentNullException.ThrowIfNull(families);
        _families = families.ToList();
    }

    public string ResolveLanguageId(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return string.Empty;
        }

        foreach (var family in _families)
        {
            if (family.MatchesDocument(path))
            {
                return family.LanguageId;
            }
        }

        return string.Empty;
    }
}
