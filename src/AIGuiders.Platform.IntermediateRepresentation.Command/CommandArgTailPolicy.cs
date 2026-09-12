#nullable enable

using GdlCommand = AIGuiders.Platform.Modeling.Gdl.Command;

namespace AIGuiders.Platform.IntermediateRepresentation.Command;

/// <summary>
/// GUIDERS-FSHARP-ADR-0003 §4.3 cutover facade — SSOT: Modeling.Gdl.Command.CommandArgTailPolicy.
/// </summary>
public static class CommandArgTailPolicy
{
    public const string ImplicitSelection = GdlCommand.CommandArgTailPolicy.ImplicitSelection;
    public const string ImplicitLineRange = GdlCommand.CommandArgTailPolicy.ImplicitLineRange;

    public static CommandArgTailKind Parse(string? raw) => GdlCommand.CommandArgTailPolicy.parse(raw ?? "");

    public static bool ShouldAutoRunOnCommit(CommandArgTailKind kind, bool isExactPath, bool endsWithSpace, bool hasArgTail) =>
        GdlCommand.CommandArgTailPolicy.shouldAutoRunOnCommit(kind, isExactPath, endsWithSpace, hasArgTail);

    public static bool InsertsTrailingSpaceOnCommit(CommandArgTailKind kind) =>
        GdlCommand.CommandArgTailPolicy.insertsTrailingSpaceOnCommit(kind);

    public static string? ExtractPickerId(string? raw) => FSharpInterop.OptString(GdlCommand.CommandArgTailPolicy.extractPickerId(raw ?? ""));

    public static string? ExtractSuggestionId(string? raw) => FSharpInterop.OptString(GdlCommand.CommandArgTailPolicy.extractSuggestionId(raw ?? ""));

    public static bool IsCompositePickerConstructor(string? raw) =>
        GdlCommand.CommandArgTailPolicy.isCompositePickerConstructor(raw ?? "");

    public static IReadOnlyList<string> ExtractCompositeConstructorIds(string? raw) =>
        GdlCommand.CommandArgTailPolicy.extractCompositeConstructorIds(raw ?? "");

    public static bool IsStaticEnumPicker(string? raw) => GdlCommand.CommandArgTailPolicy.isStaticEnumPicker(raw ?? "");
}
