using System.Runtime.CompilerServices;
using AIGuiders.Platform.Execution.Documentation.Relations;

[assembly: TypeForwardedTo(typeof(DocAnchorWire))]
[assembly: TypeForwardedTo(typeof(DocSymbolAnchorResolver))]
[assembly: TypeForwardedTo(typeof(IDocSymbolCatalog))]

namespace AIGuiders.Platform.Execution.Documentation.Anchors;

/// <summary>Obsolete shim marker — SSOT is <see cref="Relations.DocSymbolAnchorResolver"/>.</summary>
public static class AnchorsTypeForwarders;
