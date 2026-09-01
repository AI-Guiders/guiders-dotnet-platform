#nullable enable

using AIGuiders.Platform.Documentation.Anchors;
using AIGuiders.Platform.IntermediateRepresentation.Bracket;

namespace AIGuiders.Platform.Documentation.LinkMutate;

public enum DocSymbolRenameKind
{
    Type,
    Member
}

public sealed record DocAnchorRenameResult(
    int FilesChanged,
    int WiresChanged,
    IReadOnlyList<string> ChangedFiles);

/// <summary>Structured patch of <c>Family:doc</c> Type/Member axes inside bracket envelopes only.</summary>
public static class DocAnchorRenamer
{
    public static DocAnchorRenameResult ApplyRename(
        IEnumerable<string> rootPaths,
        string oldName,
        string newName,
        DocSymbolRenameKind kind,
        bool dryRun = false)
    {
        if (string.IsNullOrWhiteSpace(oldName))
            throw new ArgumentException("oldName is required.", nameof(oldName));
        if (string.IsNullOrWhiteSpace(newName))
            throw new ArgumentException("newName is required.", nameof(newName));

        var profile = BracketProfiles.DocSymbol;
        var filesChanged = 0;
        var wiresChanged = 0;
        var changedFiles = new List<string>();

        foreach (var path in ExpandMarkdownPaths(rootPaths))
        {
            var text = File.ReadAllText(path);
            var updated = PatchText(text, profile, oldName, newName, kind, ref wiresChanged);
            if (updated == text)
                continue;

            if (!dryRun)
                File.WriteAllText(path, updated);

            filesChanged++;
            changedFiles.Add(path);
        }

        return new DocAnchorRenameResult(filesChanged, wiresChanged, changedFiles);
    }

    public static string PatchText(
        string text,
        BracketNotationProfile profile,
        string oldName,
        string newName,
        DocSymbolRenameKind kind,
        ref int wiresChanged)
    {
        var axisKey = kind == DocSymbolRenameKind.Type ? "Type" : "Member";
        var builder = new System.Text.StringBuilder(text.Length);
        var cursor = 0;

        foreach (var envelope in BracketEnvelopeScan.LocateInText(text))
        {
            builder.Append(text.AsSpan(cursor, envelope.Start - cursor));
            cursor = envelope.End;

            if (!DocAnchorWire.LooksLikeDocEnvelope(envelope.Inner)
                || !BracketReader.Default.TryRead(
                    envelope.Wire,
                    profile,
                    BracketAxisValuePlans.DocSymbol,
                    out var wire,
                    out _)
                || wire is null
                || !DocAnchorWire.HasDocFamily(wire))
            {
                builder.Append(envelope.Wire);
                continue;
            }

            var axes = wire.Axes.ToList();
            var changed = false;
            for (var i = 0; i < axes.Count; i++)
            {
                var axis = axes[i];
                if (!axis.Key.Equals(axisKey, StringComparison.OrdinalIgnoreCase))
                    continue;
                if (!axis.Value.Equals(oldName, StringComparison.Ordinal))
                    continue;

                axes[i] = axis with { Value = newName };
                changed = true;
            }

            if (!changed)
            {
                builder.Append(envelope.Wire);
                continue;
            }

            wiresChanged++;
            var patched = new NormalizedBracketWire(wire.ProfileId, axes, wire.Raw);
            builder.Append(DocAnchorWire.Format(patched, profile));
        }

        builder.Append(text.AsSpan(cursor));
        return builder.ToString();
    }

    static IEnumerable<string> ExpandMarkdownPaths(IEnumerable<string> rootPaths)
    {
        foreach (var path in rootPaths)
        {
            if (File.Exists(path) && path.EndsWith(".md", StringComparison.OrdinalIgnoreCase))
            {
                yield return path;
                continue;
            }

            if (!Directory.Exists(path))
                continue;

            foreach (var file in Directory.EnumerateFiles(path, "*.md", SearchOption.AllDirectories))
                yield return file;
        }
    }
}
