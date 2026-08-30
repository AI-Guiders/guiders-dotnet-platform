#nullable enable
using System.Text.Json;
using Json.Schema;

namespace AIGuiders.Platform.Conformance.Schemas;

public static class ConformanceSchemaValidator
{
    static readonly IReadOnlyDictionary<string, JsonSchema> CatalogSchemas;
    static readonly IReadOnlyDictionary<string, JsonSchema> SchemasBySurface;

    static ConformanceSchemaValidator()
    {
        var files = LoadSchemaTexts();
        var catalog = new Dictionary<string, JsonSchema>(StringComparer.Ordinal);

        if (files.TryGetValue("conformance-common.schema.json", out var commonText))
            RegisterSchema(commonText);

        foreach (var (fileName, schemaText) in files.OrderBy(pair => pair.Key, StringComparer.Ordinal))
        {
            if (fileName == "conformance-common.schema.json")
                continue;

            catalog[fileName] = RegisterSchema(schemaText);
        }

        CatalogSchemas = catalog;

        var bySurface = new Dictionary<string, JsonSchema>(StringComparer.Ordinal);
        MapConstSurface(CatalogSchemas, "slash-arg-completion.schema.json", bySurface);
        MapConstSurface(CatalogSchemas, "slash-line-resolve.schema.json", bySurface);
        MapConstSurface(CatalogSchemas, "mcplane-pulse-default.schema.json", bySurface);
        MapConstSurface(CatalogSchemas, "mcplane-next-hints.schema.json", bySurface);
        MapConstSurface(CatalogSchemas, "bracket-spec.schema.json", bySurface);

        if (CatalogSchemas.TryGetValue("notation-spec.schema.json", out var notationSchema))
        {
            foreach (var surface in NotationSurfaces)
                bySurface[surface] = notationSchema;
        }

        if (CatalogSchemas.TryGetValue("notation-quarry.schema.json", out var quarrySchema))
        {
            bySurface["neovim-kbd"] = quarrySchema;
            bySurface["emacs-kbd"] = quarrySchema;
        }

        SchemasBySurface = bySurface;
    }

    public static IReadOnlyCollection<string> KnownSurfaces => SchemasBySurface.Keys.ToArray();

    public static IReadOnlyList<string> ValidateJson(string json)
    {
        using var document = JsonDocument.Parse(json);
        return ValidateElement(document.RootElement);
    }

    public static IReadOnlyList<string> ValidateElement(JsonElement element)
    {
        if (!element.TryGetProperty("surface", out var surfaceNode)
            || surfaceNode.ValueKind != JsonValueKind.String
            || string.IsNullOrWhiteSpace(surfaceNode.GetString()))
        {
            return ["Missing required string property 'surface'."];
        }

        var surface = surfaceNode.GetString()!;
        if (!SchemasBySurface.TryGetValue(surface, out var schema))
            return [$"No JSON Schema registered for surface \"{surface}\"."];

        var result = schema.Evaluate(
            element,
            new EvaluationOptions { OutputFormat = OutputFormat.List });

        return result.IsValid ? [] : CollectErrors(result);
    }

    public static IReadOnlyList<string> ValidateCatalogJson(string json)
    {
        if (!CatalogSchemas.TryGetValue("command-catalog-wire.schema.json", out var schema))
            return ["Missing embedded schema command-catalog-wire.schema.json."];

        using var document = JsonDocument.Parse(json);
        var result = schema.Evaluate(
            document.RootElement,
            new EvaluationOptions { OutputFormat = OutputFormat.List });

        return result.IsValid ? [] : CollectErrors(result);
    }

    static List<string> CollectErrors(EvaluationResults result)
    {
        var errors = new List<string>();
        CollectErrors(result, errors);
        return errors;
    }

    static void CollectErrors(EvaluationResults result, List<string> errors)
    {
        if (result.Errors is not null)
        {
            foreach (var (_, message) in result.Errors)
                errors.Add(message);
        }

        if (result.Details is null)
            return;

        foreach (var detail in result.Details)
            CollectErrors(detail, errors);
    }

    static IReadOnlyDictionary<string, string> LoadSchemaTexts()
    {
        const string marker = ".schemas.";
        var assembly = typeof(ConformanceSchemaValidator).Assembly;
        var files = new Dictionary<string, string>(StringComparer.Ordinal);

        foreach (var resourceName in assembly.GetManifestResourceNames())
        {
            if (!resourceName.EndsWith(".schema.json", StringComparison.Ordinal))
                continue;

            var markerIndex = resourceName.IndexOf(marker, StringComparison.Ordinal);
            var fileName = markerIndex >= 0
                ? resourceName[(markerIndex + marker.Length)..]
                : resourceName;

            using var stream = assembly.GetManifestResourceStream(resourceName)
                ?? throw new InvalidOperationException($"Missing embedded schema resource: {resourceName}");
            using var reader = new StreamReader(stream);
            files[fileName] = reader.ReadToEnd();
        }

        return files;
    }

    static JsonSchema RegisterSchema(string schemaText)
    {
        using var document = JsonDocument.Parse(schemaText);
        var schema = JsonSchema.FromText(schemaText);
        if (document.RootElement.TryGetProperty("$id", out var idNode)
            && idNode.ValueKind == JsonValueKind.String
            && Uri.TryCreate(idNode.GetString(), UriKind.Absolute, out var id))
        {
            SchemaRegistry.Global.Register(id, schema);
        }

        return schema;
    }

    static void MapConstSurface(
        IReadOnlyDictionary<string, JsonSchema> files,
        string fileName,
        IDictionary<string, JsonSchema> bySurface)
    {
        if (!files.TryGetValue(fileName, out var schema))
            return;

        var surface = fileName switch
        {
            "slash-arg-completion.schema.json" => "slash-arg-completion",
            "slash-line-resolve.schema.json" => "slash-line-resolve",
            "mcplane-pulse-default.schema.json" => "mcplane-pulse-default",
            "mcplane-next-hints.schema.json" => "mcplane-next-hints",
            "bracket-spec.schema.json" => "bracket-cdp-square-kv",
            _ => null,
        };

        if (surface is not null)
            bySurface[surface] = schema;
    }

    static readonly string[] NotationSurfaces =
    [
        "command-slash",
        "command-console",
        "argument-kv",
        "argument-delimited",
        "argument-positional",
        "argument-cli",
        "invocation-parity",
    ];
}
