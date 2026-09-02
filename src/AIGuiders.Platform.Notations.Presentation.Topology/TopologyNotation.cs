using System.Text;
using AIGuiders.Platform.IntermediateRepresentation.Presentation;

namespace AIGuiders.Platform.Notations.Presentation.Topology;

/// <summary>
/// Parse topology wires e.g. <c>(MFD)(F)</c>, <c>(F/P/M)</c>, <c>single</c> → <see cref="PresentationTopology"/>.
/// Logical <see cref="LogicalDisplayHost.HostIndex"/> is wire order (0..n-1); physical monitor mapping is <see cref="DisplayBindingProfile"/>.
/// </summary>
public static class TopologyNotation
{
    public static TopologyParseResult Parse(string? wire)
    {
        if (string.IsNullOrWhiteSpace(wire))
        {
            return TopologyParseResult.Fail("Topology wire is empty.");
        }

        var source = wire.Trim();
        if (source.Equals("single", StringComparison.OrdinalIgnoreCase))
        {
            return TopologyParseResult.Ok(new PresentationTopology(
                TopologyArrangement.SingleSurfaceCompositional,
                [],
                source));
        }

        var groups = ParseGroups(source, out var error);
        if (error is not null)
        {
            return TopologyParseResult.Fail(error);
        }

        if (groups.Count == 0)
        {
            return TopologyParseResult.Fail("Topology wire has no host groups.");
        }

        var assign = AssignHosts(groups, source, out error);
        if (error is not null)
        {
            return TopologyParseResult.Fail(error);
        }

        var arrangement = assign.Arrangement;
        var hosts = assign.Hosts
            .Select((host, index) => host with { HostIndex = index, HostId = BuildHostId(host.Role, index, assign.Hosts) })
            .ToArray();

        return TopologyParseResult.Ok(new PresentationTopology(arrangement, hosts, source));
    }

    sealed record AssignResult(TopologyArrangement Arrangement, List<LogicalDisplayHost> Hosts);

    static AssignResult AssignHosts(
        List<(List<string> Stack, ZoneComposeKind Compose)> groups,
        string sourceWire,
        out string? error)
    {
        error = null;
        var hosts = new List<LogicalDisplayHost>();

        if (groups.Count == 1)
        {
            var g = groups[0];
            if (g.Compose != ZoneComposeKind.OneOf)
            {
                error = "Single () group with '+' is spatial split — use multi-host wire or 'single' + layout board.";
                return default!;
            }

            if (g.Stack.Count < 2)
            {
                error = "OneOf group needs at least two channels.";
                return default!;
            }

            hosts.Add(new LogicalDisplayHost(
                0,
                "host-0",
                AttentionDisplayRole.PmOneOf,
                ZoneComposeKind.OneOf,
                g.Stack,
                g.Stack[0]));

            return new(TopologyArrangement.SingleHostOneOf, hosts);
        }

        if (groups.Count == 2)
        {
            var a = groups[0];
            var b = groups[1];
            var aOne = a.Compose == ZoneComposeKind.OneOf;
            var bOne = b.Compose == ZoneComposeKind.OneOf;

            if (!aOne && !bOne && a.Stack.Count == 1 && b.Stack.Count == 1)
            {
                hosts.Add(new LogicalDisplayHost(0, "host-0", InferRole(a.Stack[0]), ZoneComposeKind.Split, a.Stack, a.Stack[0]));
                hosts.Add(new LogicalDisplayHost(1, "host-1", InferRole(b.Stack[0]), ZoneComposeKind.Split, b.Stack, b.Stack[0]));
                return new(TopologyArrangement.MultiHost, hosts);
            }

            if (aOne == bOne)
            {
                error = "Two-window topology needs one dedicated host and one OneOf (/) group, or two dedicated single-channel hosts.";
                return default!;
            }

            for (var w = 0; w < 2; w++)
            {
                var g = groups[w];
                if (g.Compose == ZoneComposeKind.OneOf)
                {
                    if (g.Stack.Count < 2)
                    {
                        error = "OneOf group needs at least two channels.";
                        return default!;
                    }

                    hosts.Add(new LogicalDisplayHost(
                        w,
                        $"host-{w}",
                        AttentionDisplayRole.PmOneOf,
                        ZoneComposeKind.OneOf,
                        g.Stack,
                        g.Stack[0]));
                }
                else
                {
                    if (g.Stack.Count != 1)
                    {
                        error = "Dedicated host group must contain a single channel.";
                        return default!;
                    }

                    hosts.Add(new LogicalDisplayHost(
                        w,
                        $"host-{w}",
                        InferRole(g.Stack[0]),
                        ZoneComposeKind.Split,
                        g.Stack,
                        g.Stack[0]));
                }
            }

            return new(TopologyArrangement.MultiHost, hosts);
        }

        if (groups.Count == 3)
        {
            var remaining = new HashSet<AttentionDisplayRole>
            {
                AttentionDisplayRole.Pfd,
                AttentionDisplayRole.Forward,
                AttentionDisplayRole.Mfd,
            };

            for (var g = 0; g < 3; g++)
            {
                var group = groups[g];
                if (group.Compose == ZoneComposeKind.OneOf && group.Stack.Count < 2)
                {
                    error = "OneOf group needs at least two channels.";
                    return default!;
                }

                var preferred = InferRole(group.Stack[0]);
                var role = remaining.Contains(preferred)
                    ? preferred
                    : remaining.Contains(AttentionDisplayRole.Forward)
                        ? AttentionDisplayRole.Forward
                        : remaining.Contains(AttentionDisplayRole.Pfd)
                            ? AttentionDisplayRole.Pfd
                            : AttentionDisplayRole.Mfd;

                if (!remaining.Remove(role))
                {
                    error = "Could not assign Pfd/Forward/Mfd roles to three hosts.";
                    return default!;
                }

                var scanRole = group.Compose == ZoneComposeKind.OneOf && group.Stack.Count > 1 && role != AttentionDisplayRole.Forward
                    ? AttentionDisplayRole.PmOneOf
                    : role;

                if (group.Compose == ZoneComposeKind.Split || group.Stack.Count == 1)
                {
                    scanRole = role;
                }

                hosts.Add(new LogicalDisplayHost(
                    g,
                    $"host-{g}",
                    scanRole,
                    group.Compose,
                    group.Stack,
                    group.Stack[0]));
            }

            return new(TopologyArrangement.MultiHost, hosts);
        }

        error = "Topology wire supports 1 OneOf group (single TopLevel), 2–3 multi-host groups, or 'single'.";
        return default!;
    }

    static List<(List<string> Stack, ZoneComposeKind Compose)> ParseGroups(string text, out string? error)
    {
        error = null;
        var groups = new List<(List<string> Stack, ZoneComposeKind Compose)>();
        var i = 0;
        SkipWs(text, ref i);

        while (i < text.Length)
        {
            if (text[i] != '(')
            {
                error = $"Expected '(' at position {i}.";
                return groups;
            }

            i++;
            var start = i;
            while (i < text.Length && text[i] != ')')
            {
                i++;
            }

            if (i >= text.Length)
            {
                error = "Missing ')' in topology wire.";
                return groups;
            }

            var inner = CollapseWs(text.AsSpan(start, i - start));
            i++;
            var parsed = ParseGroup(inner);
            if (parsed.Error is not null)
            {
                error = parsed.Error;
                return groups;
            }

            groups.Add((parsed.Stack!, parsed.Compose));
            SkipWs(text, ref i);
        }

        return groups;
    }

    static (List<string>? Stack, ZoneComposeKind Compose, string? Error) ParseGroup(string inner)
    {
        if (inner.Length == 0)
        {
            return (null, ZoneComposeKind.Split, "Empty () group.");
        }

        var stack = new List<string>();
        ZoneComposeKind? compose = null;
        var i = 0;
        while (i < inner.Length)
        {
            if (!TryReadToken(inner, ref i, out var tok))
            {
                return (null, ZoneComposeKind.Split, $"Bad channel token near position {i}.");
            }

            stack.Add(Normalize(tok));

            if (i >= inner.Length)
            {
                break;
            }

            if (inner[i] == '/')
            {
                if (compose is ZoneComposeKind.Split)
                {
                    return (null, default, "Mixed '+' and '/' in one group.");
                }

                compose = ZoneComposeKind.OneOf;
                i++;
                continue;
            }

            if (inner[i] == '+')
            {
                if (compose is ZoneComposeKind.OneOf)
                {
                    return (null, default, "Mixed '+' and '/' in one group.");
                }

                compose = ZoneComposeKind.Split;
                i++;
                continue;
            }

            return (null, default, $"Expected '/' or '+' after '{tok}'.");
        }

        compose ??= ZoneComposeKind.Split;
        if (compose == ZoneComposeKind.OneOf && stack.Count < 2)
        {
            return (null, default, "OneOf needs at least two channels.");
        }

        return (stack, compose.Value, null);
    }

    static AttentionDisplayRole InferRole(string surface)
    {
        return surface switch
        {
            "f" or "forward" or "fwd" or "intercom" or "editor" or "work" => AttentionDisplayRole.Forward,
            "p" or "pfd" or "sit" or "report" or "plan" => AttentionDisplayRole.Pfd,
            "m" or "mfd" or "world" or "probe" or "shell" or "git" or "browser" or "mcp" => AttentionDisplayRole.Mfd,
            "alert" or "ecl" or "eicas" => AttentionDisplayRole.Eicas,
            _ => AttentionDisplayRole.Unknown,
        };
    }

    static string BuildHostId(AttentionDisplayRole role, int index, IReadOnlyList<LogicalDisplayHost> all)
    {
        var baseId = role switch
        {
            AttentionDisplayRole.Forward => "forward",
            AttentionDisplayRole.Pfd => "pfd",
            AttentionDisplayRole.Mfd => "mfd",
            AttentionDisplayRole.PmOneOf => "pm-oneof",
            AttentionDisplayRole.Eicas => "eicas",
            _ => $"host-{index}",
        };

        if (all.Count(x => x.Role == role) <= 1)
        {
            return baseId;
        }

        return $"{baseId}-{index}";
    }

    static bool TryReadToken(string s, ref int i, out string tok)
    {
        var start = i;
        while (i < s.Length && (char.IsLetterOrDigit(s[i]) || s[i] is '_' or '-'))
        {
            i++;
        }

        if (i == start)
        {
            tok = "";
            return false;
        }

        tok = s[start..i];
        return true;
    }

    static string Normalize(string s) => s.Trim().ToLowerInvariant();

    static string CollapseWs(ReadOnlySpan<char> span)
    {
        var sb = new StringBuilder(span.Length);
        foreach (var c in span)
        {
            if (!char.IsWhiteSpace(c))
            {
                sb.Append(c);
            }
        }

        return sb.ToString();
    }

    static void SkipWs(string text, ref int i)
    {
        while (i < text.Length && char.IsWhiteSpace(text[i]))
        {
            i++;
        }
    }
}
