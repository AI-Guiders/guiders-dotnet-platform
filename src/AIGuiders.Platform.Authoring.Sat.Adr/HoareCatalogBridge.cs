using GdlCorrespondence = AIGuiders.Platform.Modeling.Documentation.Correspondence;

namespace AIGuiders.Platform.Authoring.Sat;

/// <summary>
/// Thin C# bridge to F# SSOT Hoare / well-formedness catalogs (SAT-003).
/// </summary>
internal static class HoareCatalogBridge
{
    public static bool IsRegisteredHoare(string obligationId) =>
        GdlCorrespondence.HoareObligationCatalog.isRegistered(obligationId);

    public static bool IsRegisteredWellFormedness(string wfId) =>
        GdlCorrespondence.HoareObligationCatalog.isRegisteredWf(wfId);

    public static bool ValidateObligation(string obligationId) =>
        GdlCorrespondence.HoareObligationCatalog.validateObligation(obligationId);

    public static bool ValidateWellFormedness(string wfId) =>
        GdlCorrespondence.HoareObligationCatalog.validateWellFormedness(wfId);

    public static IReadOnlyList<string> RegisteredHoareIds() =>
        GdlCorrespondence.HoareObligationCatalog.registeredIds();

    public static IReadOnlyList<string> RegisteredWellFormednessIds() =>
        GdlCorrespondence.HoareObligationCatalog.registeredWfIds();
}
