#nullable enable

namespace AIGuiders.Platform.CommandPlane.Conformance;

public sealed class SlashSpecStubPickerSource : ISlashPickerChoiceSource
{
    readonly IReadOnlyDictionary<string, SlashSpecPickerStub> _stubs;

    public SlashSpecStubPickerSource(IReadOnlyDictionary<string, SlashSpecPickerStub>? stubs) =>
        _stubs = stubs ?? new Dictionary<string, SlashSpecPickerStub>();

    public IReadOnlyList<SlashPickerChoice> GetChoices(string pickerId, string partial)
    {
        if (!_stubs.TryGetValue(pickerId, out var stub))
            return [];

        var choices = stub.Choices
            .Select(c => new SlashPickerChoice
            {
                Value = c.Value,
                Label = c.Label,
                Hint = c.Hint,
            })
            .ToList();

        if (string.IsNullOrWhiteSpace(partial))
            return choices;

        return choices
            .Where(choice => choice.Value.Contains(partial, StringComparison.OrdinalIgnoreCase)
                             || (choice.Label?.Contains(partial, StringComparison.OrdinalIgnoreCase) ?? false))
            .ToList();
    }
}
