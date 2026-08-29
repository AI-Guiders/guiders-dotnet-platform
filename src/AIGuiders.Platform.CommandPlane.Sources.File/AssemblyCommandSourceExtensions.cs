#nullable enable
using System.Reflection;
using AIGuiders.Platform.CommandPlane;

namespace AIGuiders.Platform.CommandPlane.Sources;

/// <summary>Embedded catalog resources in plugin assemblies.</summary>
public static class AssemblyCommandSourceExtensions
{
    /// <summary>
    /// Loads a command catalog from an embedded resource (e.g. <c>commands.toml</c>).
    /// Resource name may be a short suffix — the first manifest name that ends with it wins.
    /// </summary>
    public static ICommandSource FromAssemblyResource(
        this Assembly assembly,
        string resourceName,
        string? sourceId = null)
    {
        ArgumentNullException.ThrowIfNull(assembly);
        ArgumentException.ThrowIfNullOrWhiteSpace(resourceName);

        var manifestName = ResolveManifestResourceName(assembly, resourceName);
        using var stream = assembly.GetManifestResourceStream(manifestName)
                         ?? throw new InvalidOperationException(
                             $"Embedded command catalog '{resourceName}' was not found in assembly '{assembly.GetName().Name}'.");

        using var reader = new StreamReader(stream);
        var content = reader.ReadToEnd();
        var format = CommandSourceFormats.Resolve(resourceName);
        var id = sourceId ?? $"assembly:{assembly.GetName().Name}:{resourceName}";
        return CommandSource.FromText(content, CommandFormatReaders.For(format), id);
    }

    static string ResolveManifestResourceName(Assembly assembly, string resourceName)
    {
        var names = assembly.GetManifestResourceNames();
        var exact = names.FirstOrDefault(name =>
            string.Equals(name, resourceName, StringComparison.OrdinalIgnoreCase));
        if (exact is not null)
        {
            return exact;
        }

        var suffix = names.FirstOrDefault(name =>
            name.EndsWith(resourceName, StringComparison.OrdinalIgnoreCase));
        if (suffix is not null)
        {
            return suffix;
        }

        throw new InvalidOperationException(
            $"Embedded command catalog '{resourceName}' was not found in assembly '{assembly.GetName().Name}'. "
            + $"Available resources: {string.Join(", ", names)}");
    }
}
