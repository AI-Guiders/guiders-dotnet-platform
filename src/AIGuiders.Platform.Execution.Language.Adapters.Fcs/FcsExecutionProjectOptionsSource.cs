#nullable disable

using System.IO;
using AIGuiders.Platform.Modeling.Ide.Session;
using AIGuiders.Platform.Modeling.Language.Adapters.Fcs;
using FSharp.Compiler.CodeAnalysis;
using Microsoft.FSharp.Core;

namespace AIGuiders.Platform.Execution.Language.Adapters.Fcs;

/// <summary>
/// Reads FSharpProjectOptions materialized @ revision via <see cref="FcsExecutionHost"/>.
/// </summary>
public sealed class FcsExecutionProjectOptionsSource : IFcsProjectOptionsSource
{
    static FcsExecutionProjectOptionsSource() => FcsModelingBindings.EnsureInitialized();

    public FSharpResult<FSharpProjectOptions, FcsProjectOptionsLoadError> TryLoad(string fsprojPath)
    {
        var opt = FcsExecutionHost.TryGetOptions(fsprojPath);
        if (FSharpOption<FSharpProjectOptions>.get_IsSome(opt))
            return FSharpResult<FSharpProjectOptions, FcsProjectOptionsLoadError>.NewOk(opt.Value);

        var full = string.IsNullOrWhiteSpace(fsprojPath)
            ? fsprojPath
            : Path.GetFullPath(fsprojPath.Trim());

        return FSharpResult<FSharpProjectOptions, FcsProjectOptionsLoadError>.NewError(new FcsProjectOptionsLoadError
        {
            Message =
                $"F# compiler services are not materialized for '{full}'. "
                + "Ensure federation CompilerServices (FTC → WorkspaceView → materialize) before LRC dispatch.",
        });
    }

    public void Warm(string fsprojPath) => TryLoad(fsprojPath);

    public void Invalidate(FSharpOption<string> fsprojPath = default)
    {
        var path = FSharpOption<string>.get_IsSome(fsprojPath) ? fsprojPath.Value : null;
        FcsExecutionHost.Invalidate(path);
    }
}
