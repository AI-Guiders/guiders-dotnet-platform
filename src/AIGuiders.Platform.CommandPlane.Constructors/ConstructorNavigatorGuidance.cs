#nullable enable

namespace AIGuiders.Platform.CommandPlane;

public static class ConstructorNavigatorGuidance
{
    public static ArgInputGuidance BuildArgGuidance(
        this ValueConstructorNavigator navigator,
        ArgConstructorDraft draft,
        LocaleInputProfile? profile = null)
    {
        if (navigator.TryGetCurrentSlotLabel(draft, out var slotLabel)
            && navigator.TryGetCurrentLeaf(draft, out var leaf, out var segmentIndex))
        {
            var segment = leaf.Segments[segmentIndex];
            return new ArgInputGuidance(
                InvocationLinePhase.Arg,
                InvocationArgMechanic.Constructor,
                segment.Label,
                $"{slotLabel}: {segment.Label}",
                draft.CanonicalPath,
                nameof(InvocationArgMechanic.Constructor),
                DisplayTail: draft.DisplayBuffer);
        }

        if (navigator.TryEmitWire(draft, out var wire, out _))
        {
            return new ArgInputGuidance(
                InvocationLinePhase.Ready,
                null,
                "Press Enter to run",
                draft.DisplayBuffer,
                draft.CanonicalPath,
                nameof(InvocationArgMechanic.Constructor),
                wire,
                draft.DisplayBuffer);
        }

        return new ArgInputGuidance(
            InvocationLinePhase.Arg,
            InvocationArgMechanic.Constructor,
            profile?.InputPlaceholder ?? "Value",
            "Choose the next step",
            draft.CanonicalPath,
            nameof(InvocationArgMechanic.Constructor),
            DisplayTail: draft.DisplayBuffer);
    }
}
