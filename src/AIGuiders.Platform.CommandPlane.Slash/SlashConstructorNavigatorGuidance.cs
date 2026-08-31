#nullable enable

namespace AIGuiders.Platform.CommandPlane;

public static class SlashConstructorNavigatorGuidance
{
    public static SlashInputGuidance BuildSlashGuidance(
        this SlashValueConstructorNavigator navigator,
        SlashConstructorDraft draft,
        SlashLocaleInputProfile? profile = null)
    {
        if (navigator.TryGetCurrentSlotLabel(draft, out var slotLabel)
            && navigator.TryGetCurrentLeaf(draft, out var leaf, out var segmentIndex))
        {
            var segment = leaf.Segments[segmentIndex];
            var breadcrumb = BuildBreadcrumb(draft, slotLabel);
            return new SlashInputGuidance(
                SlashInputMode.Constructor,
                breadcrumb,
                segment.Label,
                $"{slotLabel}: {segment.Label}",
                draft.CanonicalPath,
                nameof(SlashInputMode.Constructor),
                DisplayTail: draft.DisplayBuffer);
        }

        if (navigator.TryEmitWire(draft, out var wire, out _))
        {
            return new SlashInputGuidance(
                SlashInputMode.Ready,
                BuildBreadcrumb(draft, null),
                "Press Enter to run",
                draft.DisplayBuffer,
                draft.CanonicalPath,
                nameof(SlashInputMode.Constructor),
                wire,
                draft.DisplayBuffer);
        }

        return new SlashInputGuidance(
            SlashInputMode.Constructor,
            BuildBreadcrumb(draft, null),
            profile?.InputPlaceholder ?? "Value",
            "Choose the next step",
            draft.CanonicalPath,
            nameof(SlashInputMode.Constructor),
            DisplayTail: draft.DisplayBuffer);
    }

    static string BuildBreadcrumb(SlashConstructorDraft draft, string? slotLabel)
    {
        var parts = new List<string> { "/" + draft.CanonicalPath };
        if (!string.IsNullOrWhiteSpace(slotLabel))
        {
            parts.Add(slotLabel);
        }

        if (!string.IsNullOrWhiteSpace(draft.DisplayBuffer))
        {
            parts.Add(draft.DisplayBuffer);
        }

        return string.Join(" › ", parts);
    }
}
