#nullable enable

namespace AIGuiders.Platform.CommandPlane.Melody;

/// <summary>Infer, normalize, and validate melody lines (GUIDERS-ADR-0015 §7).</summary>
public static class MelodyLinePolicy
{
    public static MelodyLineProfile InferProfile(IReadOnlyList<MelodyStep> steps)
    {
        var hasNote = false;
        var hasChord = false;
        foreach (var step in steps)
        {
            if (step.Articulation == MelodyArticulation.ByNote)
                hasNote = true;
            else
                hasChord = true;
        }

        if (hasNote && hasChord)
            return MelodyLineProfile.Mixed;

        return hasChord ? MelodyLineProfile.PureByChord : MelodyLineProfile.PureByNote;
    }

    public static IReadOnlyList<MelodyStep> InferStepsFromSlug(string slug)
    {
        if (string.IsNullOrEmpty(slug))
            return [];

        var steps = new MelodyStep[slug.Length];
        for (var i = 0; i < slug.Length; i++)
        {
            steps[i] = new MelodyStep
            {
                Articulation = MelodyArticulation.ByNote,
                Wire = slug[i].ToString(),
            };
        }

        return steps;
    }

    public static MelodyDescriptor Normalize(MelodyDescriptor descriptor)
    {
        var steps = descriptor.Steps.Count > 0
            ? descriptor.Steps
            : InferStepsFromSlug(descriptor.Slug);

        var profile = descriptor.Steps.Count > 0
            ? descriptor.Profile
            : InferProfile(steps);

        return new MelodyDescriptor
        {
            CommandId = descriptor.CommandId,
            Slug = descriptor.Slug,
            Profile = profile,
            Steps = steps,
            ArgumentNotation = descriptor.ArgumentNotation,
            Help = descriptor.Help,
        };
    }

    public static IReadOnlyList<string> Validate(MelodyDescriptor descriptor)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(descriptor.CommandId))
            errors.Add("CommandId is required.");

        if (string.IsNullOrWhiteSpace(descriptor.Slug))
            errors.Add("Slug is required.");

        var steps = descriptor.Steps.Count > 0
            ? descriptor.Steps
            : InferStepsFromSlug(descriptor.Slug);

        if (steps.Count == 0)
        {
            errors.Add("Melody line requires at least one step or a non-empty slug.");
            return errors;
        }

        for (var i = 0; i < steps.Count; i++)
        {
            if (string.IsNullOrWhiteSpace(steps[i].Wire))
                errors.Add($"Step {i} wire is required.");
        }

        if (descriptor.Steps.Count == 0
            && descriptor.Profile is MelodyLineProfile.PureByChord or MelodyLineProfile.Mixed)
        {
            errors.Add("Explicit steps are required for PureByChord and Mixed profiles.");
            return errors;
        }

        var inferred = InferProfile(steps);
        switch (descriptor.Profile)
        {
            case MelodyLineProfile.PureByNote when inferred != MelodyLineProfile.PureByNote:
                errors.Add("PureByNote profile requires every step to use ByNote articulation.");
                break;
            case MelodyLineProfile.PureByChord when inferred != MelodyLineProfile.PureByChord:
                errors.Add("PureByChord profile requires every step to use ByChord articulation.");
                break;
            case MelodyLineProfile.Mixed when inferred != MelodyLineProfile.Mixed:
                errors.Add("Mixed profile requires at least two different step articulations.");
                break;
        }

        return errors;
    }

    public static bool TryNormalize(
        MelodyDescriptor descriptor,
        out MelodyDescriptor normalized,
        out IReadOnlyList<string> errors)
    {
        normalized = Normalize(descriptor);
        errors = Validate(normalized);
        return errors.Count == 0;
    }
}
