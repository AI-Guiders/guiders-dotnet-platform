#nullable enable

namespace AIGuiders.Platform.Paths;

static class LogicalPathMatching
{
    public static bool Matches(string candidatePath, string anchorRel, string anchorFileName)
    {
        var c = LogicalPath.Normalize(candidatePath);
        if (c.Equals(anchorRel, StringComparison.OrdinalIgnoreCase))
            return true;
        if (c.EndsWith('/' + anchorRel, StringComparison.OrdinalIgnoreCase))
            return true;

        return string.Equals(Path.GetFileName(c), anchorFileName, StringComparison.OrdinalIgnoreCase)
            && (anchorRel.EndsWith('/' + anchorFileName, StringComparison.OrdinalIgnoreCase)
                || string.Equals(anchorRel, anchorFileName, StringComparison.OrdinalIgnoreCase));
    }
}
