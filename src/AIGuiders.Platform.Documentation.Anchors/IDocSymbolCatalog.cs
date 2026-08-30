#nullable enable

namespace AIGuiders.Platform.Documentation.Anchors;

public interface IDocSymbolCatalog
{
    bool TypeExists(string typeName, string? packageHint);

    bool MemberExists(string typeName, string memberName, string? packageHint);
}
