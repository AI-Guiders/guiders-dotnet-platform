using AIGuiders.Platform.Conformance.Policies;

namespace AIGuiders.Platform.Tools.ContractOracle;

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
        var schemaErrors = PolicySpecFormats.ValidateFile(specPath);
        if (schemaErrors.Count > 0)
        {
            foreach (var error in schemaErrors)
                Console.Error.WriteLine(error);
            return 1;
        }

        var spec = PolicySpecLoader.LoadFile(specPath);
        var errors = PolicySpecConformance.ValidateDocument(spec);
        if (errors.Count == 0)
        {
            Console.WriteLine($"OK {spec.Policy} ({spec.Vectors.Count} vectors)");
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

        var index = ObligationsIndexLoader.Load(File.ReadAllText(indexPath));
        var exitCode = 0;
        foreach (var obligation in index.Obligations)
        {
            if (string.Equals(obligation.Kind, "proof", StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine($"  skip proof {obligation.Id} (tool={obligation.Tool})");
                continue;
            }

            if (!string.Equals(obligation.Kind, "policy", StringComparison.OrdinalIgnoreCase))
            {
                Console.Error.WriteLine($"Unknown obligation kind \"{obligation.Kind}\" for {obligation.Id}.");
                exitCode = 1;
                continue;
            }

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

            var schemaErrors = PolicySpecFormats.ValidateFile(specPath);
            if (schemaErrors.Count > 0)
            {
                foreach (var error in schemaErrors)
                    Console.Error.WriteLine($"{obligation.Id}: {error}");
                exitCode = 1;
                continue;
            }

            var spec = PolicySpecLoader.LoadFile(specPath);
            var errors = PolicySpecConformance.ValidateDocument(spec);
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
            Console.WriteLine($"OK obligations index ({index.Obligations.Count} entries)");

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
            contract-oracle — policy overlay conformance + obligations index

            Commands:
              verify --spec <path>              JSON Schema + combinator vectors
              index  --root <conformance-dir>   obligations.index.yaml + all policy specs
            """);
    }
}
