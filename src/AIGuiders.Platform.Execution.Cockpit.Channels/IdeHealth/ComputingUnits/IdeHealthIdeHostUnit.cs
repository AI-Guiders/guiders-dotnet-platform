#nullable enable

using AIGuiders.Platform.Execution.Cockpit.ComputingUnits;
using AIGuiders.Platform.Modeling.Cockpit.DataBus;

namespace AIGuiders.Platform.Execution.Cockpit.Channels.IdeHealth.ComputingUnits;

public sealed class IdeHealthIdeHostUnit : ICockpitComputeUnit
{
    public static IdeHealthIdeHostUnit Default { get; } = new();

    public IdeHealthIdeHostInput Compose(in IdeHostStateChanged state) =>
        Compose(state.CSharpLspProcessActive, state.MarkdownLspProcessActive);

    public IdeHealthIdeHostInput Compose(bool csharpLspActive, bool markdownLspActive)
    {
        var hint = (csharpLspActive, markdownLspActive) switch
        {
            (true, true) => "LSP · C# · MD",
            (true, false) => "LSP · C#",
            (false, true) => "LSP · MD",
            _ => (string?)null
        };
        return new IdeHealthIdeHostInput(LspStatusHint: hint);
    }
}
