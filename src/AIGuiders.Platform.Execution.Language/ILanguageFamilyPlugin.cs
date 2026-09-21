namespace AIGuiders.Platform.Execution.Language;

/// <summary>
/// Language family — one <see cref="LanguageId"/> for LRC, activation, and surface bundles.
/// Surface hosts (Code Center, CDP) wire families into their registries; not a surface plugin.
/// </summary>
public interface ILanguageFamilyPlugin
{
    /// <summary>Stable family plugin id (e.g. language.fsharp).</summary>
    string PluginId { get; }

    /// <summary>Platform language id (e.g. fsharp).</summary>
    string LanguageId { get; }

    /// <summary>Document profile when opened via this family.</summary>
    string DocumentProfileId { get; }

    /// <summary>File extensions including dot.</summary>
    IReadOnlyList<string> FileExtensions { get; }

    bool MatchesDocument(string documentPathOrId);

    ILanguageBackend CreateLanguageBackend();
}
