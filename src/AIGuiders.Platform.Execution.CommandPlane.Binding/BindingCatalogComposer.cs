#nullable enable

namespace AIGuiders.Platform.Execution.CommandPlane.Binding;

public static class BindingCatalogComposer
{
    public static BindingCatalogIndex Build(params IBindingSource[] sources) =>
        Combinations.Binding.BindingCatalogCombination.Compose(sources);

    public static BindingCatalogIndex Build(IEnumerable<IBindingSource> sources) =>
        Combinations.Binding.BindingCatalogCombination.Compose(sources);
}
