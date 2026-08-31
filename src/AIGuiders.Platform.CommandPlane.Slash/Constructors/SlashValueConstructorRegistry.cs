#nullable enable

namespace AIGuiders.Platform.CommandPlane;

public sealed class SlashValueConstructorRegistry
{
    readonly Dictionary<string, SlashConstructorDefinition> _definitions =
        new(StringComparer.OrdinalIgnoreCase);

    public void Register(SlashConstructorDefinition definition) =>
        _definitions[definition.Id] = definition;

    public void RegisterRange(IEnumerable<SlashConstructorDefinition> definitions)
    {
        foreach (var definition in definitions)
        {
            Register(definition);
        }
    }

    public bool TryGet(string constructorId, out SlashConstructorDefinition definition) =>
        _definitions.TryGetValue(constructorId, out definition!);

    public SlashLeafConstructorDefinition RequireLeaf(string constructorId)
    {
        if (!TryGet(constructorId, out var definition)
            || definition is not SlashLeafConstructorDefinition leaf)
        {
            throw new InvalidOperationException($"Leaf constructor '{constructorId}' is not registered.");
        }

        return leaf;
    }

    public SlashCompositeConstructorDefinition RequireComposite(string constructorId)
    {
        if (!TryGet(constructorId, out var definition)
            || definition is not SlashCompositeConstructorDefinition composite)
        {
            throw new InvalidOperationException($"Composite constructor '{constructorId}' is not registered.");
        }

        return composite;
    }
}
