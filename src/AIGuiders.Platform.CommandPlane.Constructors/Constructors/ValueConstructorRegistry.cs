#nullable enable

namespace AIGuiders.Platform.CommandPlane;

public sealed class ValueConstructorRegistry
{
    readonly Dictionary<string, ConstructorDefinition> _definitions =
        new(StringComparer.OrdinalIgnoreCase);

    public void Register(ConstructorDefinition definition) =>
        _definitions[definition.Id] = definition;

    public void RegisterRange(IEnumerable<ConstructorDefinition> definitions)
    {
        foreach (var definition in definitions)
        {
            Register(definition);
        }
    }

    public bool TryGet(string constructorId, out ConstructorDefinition definition) =>
        _definitions.TryGetValue(constructorId, out definition!);

    public LeafConstructorDefinition RequireLeaf(string constructorId)
    {
        if (!TryGet(constructorId, out var definition)
            || definition is not LeafConstructorDefinition leaf)
        {
            throw new InvalidOperationException($"Leaf constructor '{constructorId}' is not registered.");
        }

        return leaf;
    }

    public CompositeConstructorDefinition RequireComposite(string constructorId)
    {
        if (!TryGet(constructorId, out var definition)
            || definition is not CompositeConstructorDefinition composite)
        {
            throw new InvalidOperationException($"Composite constructor '{constructorId}' is not registered.");
        }

        return composite;
    }
}
