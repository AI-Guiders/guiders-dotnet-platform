namespace AIGuiders.Platform.Tools.CombinationsProof;

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
            "ship-first" => RunShipFirstProofs(),
            _ => Unknown(args[0]),
        };
    }

    static int RunShipFirstProofs()
    {
        var baselineWins = ShipFirstProof.ProveBaselineWinsOnCollision();
        var overlayFills = ShipFirstProof.ProveOverlayFillsMissingKeys();

        if (!baselineWins || !overlayFills)
        {
            Console.Error.WriteLine(
                $"ShipFirst proof failed (baselineWins={baselineWins}, overlayFills={overlayFills}).");
            return 1;
        }

        Console.WriteLine("OK ShipFirst (baseline wins on collision; overlay fills missing keys)");
        return 0;
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
            combinations-proof — Z3 proofs for Combinations overlay semantics (CI-only)

            Commands:
              ship-first    Prove TryAdd / ShipFirst per-key invariants
            """);
    }
}
