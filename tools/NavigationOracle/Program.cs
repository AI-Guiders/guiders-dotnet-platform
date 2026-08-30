using AIGuiders.Platform.Conformance.Navigation;
using AIGuiders.Platform.Conformance.Schemas;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace AIGuiders.Platform.Tools.NavigationOracle;

sealed class ObligationsIndexDocument
{
    public int Version { get; set; }
    public List<ObligationEntry> Obligations { get; set; } = [];
}

sealed class ObligationEntry
{
    public string Id { get; set; } = "";
    public string Kind { get; set; } = "";
    public string? Source { get; set; }
}

static class Program
{
    public static int Main(string[] args)
    {
        if (args.Length == 0 || args is ["-h"] or ["--help"])
        {
            PrintHelp();
            return 0;
        }

        return args[0] switch
        {
            "verify" => VerifySpec(args),
            "index" => VerifyIndex(args),
            _ => Unknown(args[0]),
        };
    }

    static int VerifySpec(string[] args)
    {
        var specPath = RequireOption(args, "--spec");
        var json = File.ReadAllText(specPath);
        var schemaErrors = ConformanceSchemaValidator.ValidateNavigationJson(json);
        if (schemaErrors.Count > 0)
        {
            foreach (var error in schemaErrors)
                Console.Error.WriteLine(error);
            return 1;
        }

        var spec = NavigationSpecLoader.LoadFile(specPath);
        var errors = NavigationSpecConformance.ValidateDocument(spec);
        if (errors.Count == 0)
        {
            Console.WriteLine($"OK {spec.Surface} ({spec.Vectors.Count} vectors)");
            return 0;
        }

        foreach (var error in errors)
            Console.Error.WriteLine(error);
        return 1;
    }

    static int VerifyIndex(string[] args)
    {
        var root = RequireOption(args, "--root");
        var indexPath = Path.Combine(root, "obligations.index.yaml");
        if (!File.Exists(indexPath))
        {
            Console.Error.WriteLine($"Missing obligations index: {indexPath}");
            return 1;
        }

        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .IgnoreUnmatchedProperties()
            .Build();
        var index = deserializer.Deserialize<ObligationsIndexDocument>(File.ReadAllText(indexPath))
            ?? throw new InvalidOperationException("Obligations index YAML deserialized to null.");

        var exitCode = 0;
        foreach (var obligation in index.Obligations)
        {
            if (!string.Equals(obligation.Kind, "navigation", StringComparison.OrdinalIgnoreCase))
                continue;

            if (string.IsNullOrWhiteSpace(obligation.Source))
            {
                Console.Error.WriteLine($"Obligation {obligation.Id} missing source path.");
                exitCode = 1;
                continue;
            }

            var specPath = Path.Combine(root, obligation.Source.Replace('/', Path.DirectorySeparatorChar));
            if (!File.Exists(specPath))
            {
                Console.Error.WriteLine($"Missing spec for {obligation.Id}: {specPath}");
                exitCode = 1;
                continue;
            }

            var json = File.ReadAllText(specPath);
            var schemaErrors = ConformanceSchemaValidator.ValidateNavigationJson(json);
            if (schemaErrors.Count > 0)
            {
                foreach (var error in schemaErrors)
                    Console.Error.WriteLine($"{obligation.Id}: {error}");
                exitCode = 1;
                continue;
            }

            var spec = NavigationSpecLoader.LoadFile(specPath);
            var errors = NavigationSpecConformance.ValidateDocument(spec);
            if (errors.Count > 0)
            {
                foreach (var error in errors)
                    Console.Error.WriteLine($"{obligation.Id}: {error}");
                exitCode = 1;
                continue;
            }

            Console.WriteLine($"  OK {obligation.Id} -> {obligation.Source}");
        }

        if (exitCode == 0)
            Console.WriteLine("OK navigation obligations");

        return exitCode;
    }

    static string RequireOption(string[] args, string name)
    {
        for (var i = 0; i < args.Length - 1; i++)
        {
            if (string.Equals(args[i], name, StringComparison.OrdinalIgnoreCase))
                return args[i + 1];
        }

        throw new InvalidOperationException($"Missing required option {name}.");
    }

    static int Unknown(string command)
    {
        Console.Error.WriteLine($"Unknown command: {command}");
        PrintHelp();
        return 2;
    }

    static void PrintHelp()
    {
        Console.WriteLine("""
            navigation-oracle — navigation explore conformance

            Commands:
              verify --spec <path>              JSON Schema + scene vectors
              index  --root <conformance-dir>   navigation obligations from index
            """);
    }
}
