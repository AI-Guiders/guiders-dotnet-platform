#nullable enable

namespace AIGuiders.Platform.CommandPlane;

public sealed class SlashValueConstructorNavigator(
    SlashValueConstructorRegistry registry,
    ISlashConstructorSegmentProvider segmentProvider)
{
    public IReadOnlyList<SlashCompletionItem> GetSuggestions(SlashConstructorDraft draft, string partial)
    {
        if (!TryGetCurrentLeaf(draft, out var leaf, out var segmentIndex))
        {
            return [];
        }

        return segmentProvider.GetSegmentSuggestions(leaf, segmentIndex, draft, partial);
    }

    public bool TryAdvance(SlashConstructorDraft draft, string pickedValue)
    {
        if (!TryGetCurrentLeaf(draft, out var leaf, out var segmentIndex))
        {
            return false;
        }

        var segment = leaf.Segments[segmentIndex];
        draft.ActiveSegments[segment.SegmentId] = pickedValue.Trim();

        if (segmentIndex + 1 < leaf.Segments.Count)
        {
            draft.SegmentIndex = segmentIndex + 1;
            return true;
        }

        CompleteCurrentLeaf(draft, leaf);
        return true;
    }

    public bool TryEmitWire(SlashConstructorDraft draft, out string wireValue, out string? error)
    {
        wireValue = "";
        error = null;

        var root = registry.RequireComposite(draft.RootConstructorId);
        if (draft.CompletedSlots.Count < root.Slots.Count)
        {
            error = "Constructor is incomplete.";
            return false;
        }

        var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var slot in draft.CompletedSlots)
        {
            var slotDef = root.Slots.First(s =>
                s.SlotId.Equals(slot.SlotId, StringComparison.OrdinalIgnoreCase));
            var leaf = registry.RequireLeaf(slotDef.ConstructorId);
            values[slot.SlotId] = FormatWire(leaf, slot.Segments);
        }

        wireValue = ApplyPattern(root.WirePattern, values);
        return true;
    }

    public bool TryApplySegments(
        SlashConstructorDraft draft,
        IReadOnlyDictionary<string, string> segments,
        SlashValueConstructorRegistry registryInstance,
        SlashLocaleInputProfile? profile)
    {
        if (!TryGetCurrentLeaf(draft, out var leaf, out _))
        {
            return false;
        }

        foreach (var (segmentId, value) in segments)
        {
            if (leaf.Segments.All(s => !s.SegmentId.Equals(segmentId, StringComparison.OrdinalIgnoreCase)))
            {
                continue;
            }

            TryAdvance(draft, value);
        }

        if (!string.IsNullOrWhiteSpace(draft.DisplayBuffer))
        {
            return true;
        }

        if (draft.ActiveSegments.Count > 0 && profile is not null)
        {
            draft.DisplayBuffer = SlashLocaleDisplayFormatter.FormatLeaf(leaf, draft.ActiveSegments, profile);
        }

        return true;
    }

    public bool TryApplyLocaleParts(
        SlashConstructorDraft draft,
        SlashLocaleDateParts parts,
        SlashValueConstructorRegistry registryInstance,
        SlashLocaleInputProfile profile) =>
        TryApplySegments(draft, parts.ToWireSegments(), registryInstance, profile);

    void CompleteCurrentLeaf(SlashConstructorDraft draft, SlashLeafConstructorDefinition leaf)
    {
        var composite = registry.RequireComposite(draft.RootConstructorId);
        var slot = composite.Slots[draft.SlotIndex];
        var segments = new Dictionary<string, string>(draft.ActiveSegments, StringComparer.OrdinalIgnoreCase);
        draft.CompletedSlots.Add(new SlashConstructorDraft.CompletedSlot(slot.SlotId, segments));

        var wirePart = FormatWire(leaf, segments);
        var displayPart = FormatDisplay(leaf, segments);
        draft.WireBuffer = string.IsNullOrEmpty(draft.WireBuffer)
            ? wirePart
            : draft.WireBuffer + wirePart;
        draft.DisplayBuffer = string.IsNullOrEmpty(draft.DisplayBuffer)
            ? displayPart
            : draft.DisplayBuffer + displayPart;

        draft.ActiveSegments.Clear();
        draft.SegmentIndex = 0;
        draft.SlotIndex++;

        if (draft.SlotIndex < composite.Slots.Count)
        {
            var nextSlot = composite.Slots[draft.SlotIndex];
            if (!string.IsNullOrWhiteSpace(nextSlot.SeparatorBefore))
            {
                AppendSeparator(draft, nextSlot.SeparatorBefore!, " .. ");
            }
        }
    }

    static string FormatDisplay(SlashLeafConstructorDefinition leaf, IReadOnlyDictionary<string, string> segments)
    {
        var displayValues = segments.ToDictionary(
            pair => pair.Key,
            pair => FormatSegmentValue(leaf, pair.Key, pair.Value, forWire: false),
            StringComparer.OrdinalIgnoreCase);
        return ApplyPattern(leaf.DisplayPattern, displayValues);
    }

    static void AppendSeparator(SlashConstructorDraft draft, string wireSep, string displaySep)
    {
        draft.WireBuffer = string.IsNullOrEmpty(draft.WireBuffer)
            ? wireSep
            : draft.WireBuffer + wireSep;
        draft.DisplayBuffer = string.IsNullOrEmpty(draft.DisplayBuffer)
            ? displaySep.Trim()
            : draft.DisplayBuffer + displaySep;
    }

    public bool TryGetCurrentLeaf(
        SlashConstructorDraft draft,
        out SlashLeafConstructorDefinition leaf,
        out int segmentIndex)
    {
        leaf = null!;
        segmentIndex = draft.SegmentIndex;
        var composite = registry.RequireComposite(draft.RootConstructorId);
        if (draft.SlotIndex >= composite.Slots.Count)
        {
            return false;
        }

        var slot = composite.Slots[draft.SlotIndex];
        leaf = registry.RequireLeaf(slot.ConstructorId);
        return segmentIndex >= 0 && segmentIndex < leaf.Segments.Count;
    }

    public bool TryGetCurrentSlotLabel(SlashConstructorDraft draft, out string label)
    {
        label = "";
        var composite = registry.RequireComposite(draft.RootConstructorId);
        if (draft.SlotIndex >= composite.Slots.Count)
        {
            return false;
        }

        var slot = composite.Slots[draft.SlotIndex];
        label = string.IsNullOrWhiteSpace(slot.Label) ? slot.SlotId : slot.Label;
        return true;
    }

    static string FormatWire(SlashLeafConstructorDefinition leaf, IReadOnlyDictionary<string, string> segments)
    {
        var wireValues = segments.ToDictionary(
            pair => pair.Key,
            pair => FormatSegmentValue(leaf, pair.Key, pair.Value, forWire: true),
            StringComparer.OrdinalIgnoreCase);
        return ApplyPattern(leaf.WirePattern, wireValues);
    }

    static string FormatSegmentValue(
        SlashLeafConstructorDefinition leaf,
        string segmentId,
        string value,
        bool forWire)
    {
        var segment = leaf.Segments.FirstOrDefault(s =>
            s.SegmentId.Equals(segmentId, StringComparison.OrdinalIgnoreCase));
        var minWidth = forWire ? segment?.WireMinWidth : segment?.DisplayMinWidth;
        if (minWidth is > 0 && value.Length < minWidth)
        {
            return value.PadLeft(minWidth.Value, '0');
        }

        return value;
    }

    static string ApplyPattern(string pattern, IReadOnlyDictionary<string, string> values)
    {
        var result = pattern;
        foreach (var (key, value) in values)
        {
            result = result.Replace($"{{{key}}}", value, StringComparison.OrdinalIgnoreCase);
        }

        return result;
    }
}
