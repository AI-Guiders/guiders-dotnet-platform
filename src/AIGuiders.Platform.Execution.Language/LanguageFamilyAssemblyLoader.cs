using System.Reflection;
using System.Runtime.Loader;

namespace AIGuiders.Platform.Execution.Language;

/// <summary>Load language.* family plugins from standard ship layout (DLL + type name).</summary>
public static class LanguageFamilyAssemblyLoader
{
    public static ILanguageFamilyPlugin Load(
        string assemblyName,
        string typeName,
        string appBaseDirectory,
        string? pluginsPath = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(assemblyName);
        ArgumentException.ThrowIfNullOrWhiteSpace(typeName);
        ArgumentException.ThrowIfNullOrWhiteSpace(appBaseDirectory);

        var fileName = assemblyName.EndsWith(".dll", StringComparison.OrdinalIgnoreCase)
            ? assemblyName
            : assemblyName + ".dll";

        var assemblyPath = ResolveAssemblyPath(fileName, appBaseDirectory, pluginsPath)
            ?? throw new InvalidOperationException(
                $"Could not resolve assembly '{assemblyName}' for language family plugin.");

        var assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(assemblyPath);
        var familyType = ResolveFamilyType(assembly, typeName)
            ?? throw new InvalidOperationException(
                $"No {nameof(ILanguageFamilyPlugin)} type '{typeName}' in '{assemblyPath}'.");

        if (Activator.CreateInstance(familyType) is not ILanguageFamilyPlugin activated)
        {
            throw new InvalidOperationException($"Failed to activate language family '{typeName}'.");
        }

        return activated;
    }

    public static IReadOnlyList<ILanguageFamilyPlugin> LoadMany(
        IEnumerable<(string Assembly, string Type)> entries,
        string appBaseDirectory,
        string? pluginsPath = null)
    {
        ArgumentNullException.ThrowIfNull(entries);
        return entries
            .Select(entry => Load(entry.Assembly, entry.Type, appBaseDirectory, pluginsPath))
            .ToArray();
    }

    static string? ResolveAssemblyPath(string fileName, string appBaseDirectory, string? pluginsPath)
    {
        var candidates = new List<string> { Path.Combine(appBaseDirectory, fileName) };
        if (!string.IsNullOrWhiteSpace(pluginsPath))
        {
            candidates.Insert(0, Path.Combine(pluginsPath, fileName));
        }

        return candidates.FirstOrDefault(File.Exists);
    }

    static Type? ResolveFamilyType(Assembly assembly, string typeName)
    {
        if (string.IsNullOrWhiteSpace(typeName))
        {
            return assembly.GetExportedTypes()
                .FirstOrDefault(t =>
                    typeof(ILanguageFamilyPlugin).IsAssignableFrom(t)
                    && t is { IsAbstract: false, IsInterface: false });
        }

        return assembly.GetType(typeName, throwOnError: false, ignoreCase: false)
            ?? assembly.GetExportedTypes()
                .FirstOrDefault(t => string.Equals(t.FullName, typeName, StringComparison.Ordinal));
    }
}
