#nullable enable

namespace AIGuiders.Platform.Navigation.Policy;

/// <summary>Canonical related-kind tokens for navigation scenes (GUIDERS-ADR-0033).</summary>
public static class NavigationRelatedKinds
{
    public const string PartialPeer = "partial_peer";
    public const string ProjectPeer = "project_peer";
    public const string XamlCodeBehindPair = "xaml_codebehind_pair";
    public const string TestCounterpart = "test_counterpart";
    public const string SameNamespace = "same_namespace";
    public const string SameDirectory = "same_directory";

    public static IReadOnlyList<string> All { get; } =
    [
        PartialPeer,
        ProjectPeer,
        XamlCodeBehindPair,
        TestCounterpart,
        SameNamespace,
        SameDirectory,
    ];

    /// <summary>Canonical kind name or <c>null</c> when the token is unknown.</summary>
    public static string? TryCanonicalKind(string? token)
    {
        if (string.IsNullOrWhiteSpace(token))
            return null;

        var s = token.Trim();
        foreach (var k in All)
        {
            if (string.Equals(k, s, StringComparison.OrdinalIgnoreCase))
                return k;
        }

        return null;
    }
}
