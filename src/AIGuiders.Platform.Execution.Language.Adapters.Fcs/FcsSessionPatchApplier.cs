#nullable disable

using System;
using System.Collections.Generic;
using System.IO;
using AIGuiders.Platform.Modeling.Core.Identity;
using AIGuiders.Platform.Modeling.Ide.Session;
using AIGuiders.Platform.Modeling.Ide.Session.Ports.DotNet;
using AIGuiders.Platform.Modeling.Language.Adapters.Fcs;
using AIGuiders.Platform.Modeling.LanguageIntelligence.Relations;
using AIGuiders.Platform.Modeling.Paths;
using Microsoft.FSharp.Collections;
using Microsoft.FSharp.Core;

namespace AIGuiders.Platform.Execution.Language.Adapters.Fcs;

/// <summary>
/// Disk-backed federation patch apply for FCS rename-symbol (plan §7 — File IO @ Execution only).
/// </summary>
public sealed class FcsSessionPatchApplier : IFcsSessionPatchApplier
{
    public FSharpResult<Unit, string> TryApply(
        string anchorPath,
        SessionPatch patch,
        FSharpMap<string, string> sourceOverrides)
    {
        if (patch.FileSystem.Writes.IsEmpty && patch.FileSystem.Replacements.IsEmpty)
            return FSharpResult<Unit, string>.NewOk(default);

        if (string.IsNullOrWhiteSpace(anchorPath) || !File.Exists(anchorPath))
            return FSharpResult<Unit, string>.NewError(
                "apply requires solution_or_project_path for SessionOrchestrator.");

        try
        {
            var ownership = DotNetSlnxGraphPort.loadDocumentOwnership(anchorPath);
            var pathContents = BuildPathContents(ownership, sourceOverrides);
            var session = DotNetSlnxGraphPort.loadSession(anchorPath);
            var runtime = SessionOrchestrator.create(session, pathContents, ownership);
            var gitPin = new GitPin(null);

            return SessionOrchestrator.applyPatch(runtime, patch, gitPin) switch
            {
                PatchApplyResult.PatchApplied applied =>
                    FlushWrites(patch, applied.Item),
                PatchApplyResult.PatchRejected rejected =>
                    FSharpResult<Unit, string>.NewError(string.Join("; ", rejected.reasons)),
                _ => FSharpResult<Unit, string>.NewError("unknown_patch_apply_result"),
            };
        }
        catch (Exception ex)
        {
            return FSharpResult<Unit, string>.NewError(ex.Message);
        }
    }

    static IEnumerable<Tuple<string, string>> BuildPathContents(
        FSharpMap<string, ProjectId> ownership,
        FSharpMap<string, string> sourceOverrides)
    {
        foreach (var pair in ownership)
        {
            var path = pair.Key;
            string text;

            if (sourceOverrides.ContainsKey(path))
                text = sourceOverrides[path];
            else if (File.Exists(path))
                text = File.ReadAllText(path);
            else
                text = string.Empty;

            yield return Tuple.Create(path, text);
        }
    }

    static FSharpResult<Unit, string> FlushWrites(SessionPatch patch, SessionRuntime runtime)
    {
        foreach (var write in patch.FileSystem.Writes)
        {
            var path = write.Item1;
            var docIdOpt = DocumentRegistryOps.resolvePath(LogicalPath.Create(path), runtime.Registry);
            if (!FSharpOption<Identity<Document, NumericId>>.get_IsSome(docIdOpt))
                continue;

            if (!runtime.Contents.ContainsKey(docIdOpt.Value))
                continue;

            var text = runtime.Contents[docIdOpt.Value];
            File.WriteAllText(Path.GetFullPath(path), text.text);
        }

        return FSharpResult<Unit, string>.NewOk(default);
    }
}
