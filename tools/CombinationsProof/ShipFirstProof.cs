#nullable enable
using Microsoft.Z3;

namespace AIGuiders.Platform.Tools.CombinationsProof;

/// <summary>
/// Abstract model of per-key TryAdd merge: baseline wins on collision (ShipFirst).
/// </summary>
public static class ShipFirstProof
{
    public static bool ProveBaselineWinsOnCollision()
    {
        using var ctx = new Context();
        var hasBaseline = ctx.MkBoolConst("hasBaseline");
        var baselineId = ctx.MkIntConst("baselineId");
        var overlayId = ctx.MkIntConst("overlayId");
        var resultId = (ArithExpr)ctx.MkITE(hasBaseline, baselineId, overlayId);

        var theorem = ctx.MkImplies(hasBaseline, ctx.MkEq(resultId, baselineId));
        using var solver = ctx.MkSolver();
        solver.Add(ctx.MkNot(theorem));
        return solver.Check() == Status.UNSATISFIABLE;
    }

    public static bool ProveOverlayFillsMissingKeys()
    {
        using var ctx = new Context();
        var hasBaseline = ctx.MkBoolConst("hasBaseline");
        var overlayId = ctx.MkIntConst("overlayId");
        var resultId = (ArithExpr)ctx.MkITE(hasBaseline, ctx.MkIntConst("baselineId"), overlayId);

        var theorem = ctx.MkImplies(ctx.MkNot(hasBaseline), ctx.MkEq(resultId, overlayId));
        using var solver = ctx.MkSolver();
        solver.Add(ctx.MkNot(theorem));
        return solver.Check() == Status.UNSATISFIABLE;
    }
}
