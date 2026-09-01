using AIGuiders.Platform.Authoring.Core;

namespace AIGuiders.Platform.Authoring.Command.Catalog.Parsing;

public static class CatalogProfileResolver
{
    public static void Resolve(CatalogParseContext context, ICatalogBundleLibrary? bundleLibrary)
    {
        if (bundleLibrary is null)
        {
            foreach (var profile in context.Profiles.Where(static p => !string.IsNullOrWhiteSpace(p.BundleSource)))
            {
                context.Diagnostics.Add(new(
                    AuthoringDiagnosticCode.UnknownBundle,
                    $"Bundle `{profile.BundleSource}` requires federation bundle library.",
                    1,
                    Section: "profiles"));
            }

            return;
        }

        for (var i = 0; i < context.Profiles.Count; i++)
        {
            var profile = context.Profiles[i];
            if (string.IsNullOrWhiteSpace(profile.BundleSource))
            {
                continue;
            }

            var importPath = ResolveImportPath(context.Imports, profile.BundleSource);
            if (!bundleLibrary.TryResolve(importPath, out var bundleProfiles))
            {
                context.Diagnostics.Add(new(
                    AuthoringDiagnosticCode.UnknownBundle,
                    $"Unknown federation bundle `{profile.BundleSource}` (import `{importPath}`).",
                    1,
                    Section: "profiles"));
                continue;
            }

            var source = bundleProfiles.FirstOrDefault(p =>
                p.Name.Equals(profile.BundleSource, StringComparison.OrdinalIgnoreCase))
                ?? bundleProfiles[0];

            context.Profiles[i] = new CatalogProfile
            {
                Name = profile.Name,
                Entries = source.Entries,
            };
        }
    }

    public static void ValidateCommandProfiles(CatalogParseContext context)
    {
        var names = context.Profiles
            .Select(static p => p.Name)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var command in context.Commands)
        {
            var profile = TableSurface.NullIfEmpty(command.Columns.GetValueOrDefault("profile"));
            if (profile is null || profile is "—" or "-")
            {
                continue;
            }

            if (!names.Contains(profile))
            {
                context.Diagnostics.Add(new(
                    AuthoringDiagnosticCode.UnknownProfile,
                    $"Command `{command.Command}` references unknown profile `{profile}`.",
                    1,
                    Section: "commands"));
            }
        }
    }

    static string ResolveImportPath(IReadOnlyList<CatalogImport> imports, string bundleRef)
    {
        foreach (var import in imports)
        {
            if (import.Path.EndsWith('/' + bundleRef, StringComparison.Ordinal)
                || import.Path.Equals(bundleRef, StringComparison.OrdinalIgnoreCase))
            {
                return import.Path;
            }

            if (string.Equals(import.Alias, bundleRef, StringComparison.OrdinalIgnoreCase))
            {
                return import.Path;
            }
        }

        return imports.FirstOrDefault()?.Path is { Length: > 0 } path
            && path.EndsWith('/' + bundleRef, StringComparison.Ordinal)
            ? path
            : $"grain/{bundleRef}";
    }
}
