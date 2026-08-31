using AIGuiders.Platform.IntermediateRepresentation.Command;
#nullable enable
using System.Text.Json;

namespace AIGuiders.Platform.CommandPlane.Conformance;

public static class SlashSpecLoader
{
    public static SlashSpecDocument Load(string json) =>
        JsonSerializer.Deserialize<SlashSpecDocument>(json, JsonOptions)
        ?? throw new InvalidOperationException("Slash spec JSON deserialized to null.");

    public static CommandCatalogIndex BuildCatalog(IReadOnlyList<SlashSpecCatalogEntry> entries)
    {
        var descriptors = entries.Select(ToDescriptor).ToList();
        return CommandCatalogIndex.FromDescriptors(descriptors);
    }

    public static CommandDescriptor ToDescriptor(SlashSpecCatalogEntry entry) =>
        new()
        {
            Domain = entry.Domain,
            Object = entry.Object,
            Intent = entry.Intent,
            CommandId = entry.CommandId,
            Path = entry.Path,
            PathAliases = entry.PathAliases ?? [],
            Help = entry.Help,
            Group = entry.Group,
            ArgTail = entry.ArgTail ?? "optional",
            ArgHint = entry.ArgHint,
            ArgPickerChoices = (entry.ArgPickerChoices ?? [])
                .Select(c => new CommandPickerChoice
                {
                    Value = c.Value,
                    Label = c.Label,
                    Hint = c.Hint,
                })
                .ToList(),
        };

    static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
    };
}
