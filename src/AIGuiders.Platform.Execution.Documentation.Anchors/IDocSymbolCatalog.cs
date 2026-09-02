#nullable enable

namespace AIGuiders.Platform.Execution.Documentation.Anchors;

public interface IDocSymbolCatalog
{
    bool TypeExists(string typeName, string? packageHint);

    bool MemberExists(string typeName, string memberName, string? packageHint);
}
