#nullable enable

using AIGuiders.Platform.Modeling.Documentation.Correspondence;
using AIGuiders.Platform.Modeling.LanguageIntelligence.Relations;
using Microsoft.FSharp.Core;
using Xunit;

namespace AIGuiders.Platform.Tests;

/// <summary>Plan §10 homonym codemod — TypeSystem vs Correspondence "implements" disambiguation.</summary>
public sealed class TypeSystemHomonymTests
{
    [Fact]
    public void TypeSystem_implements_interface_wire_is_not_bare_implements()
    {
        Assert.True(FSharpOption<TypeSystemRelationKind>.get_IsNone(
            TypeSystemRelationKindModule.tryParse("implements")));
        Assert.True(FSharpOption<TypeSystemRelationKind>.get_IsSome(
            TypeSystemRelationKindModule.tryParse("implements-interface")));
    }

    [Fact]
    public void Correspondence_ImplementsObligation_uses_bare_implements_wire()
    {
        var parsed = CorrespondenceRelationKindModule.tryParse(Kind.ImplementsObligation);
        Assert.True(FSharpOption<CorrespondenceRelationKind>.get_IsSome(parsed));
        Assert.Equal(
            Kind.ImplementsObligation,
            CorrespondenceRelationKindModule.toWire(parsed!.Value));
    }
}
