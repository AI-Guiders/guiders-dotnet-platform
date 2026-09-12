#nullable enable

using AIGuiders.Platform.Authoring.Core;
using GdlAuthoring = AIGuiders.Platform.Modeling.Gdl.Authoring;

namespace AIGuiders.Platform.Authoring.Command.Catalog;

internal static class CatalogInterop
{
    internal static AuthoringDiagnostic FromDiagnostic(GdlAuthoring.AuthoringDiagnostic diagnostic) =>
        new(
            Enum.Parse<AuthoringDiagnosticCode>(diagnostic.Code.ToString()),
            diagnostic.Message,
            diagnostic.Line,
            diagnostic.Column,
            FSharpInterop.OptString(diagnostic.Section));
}
