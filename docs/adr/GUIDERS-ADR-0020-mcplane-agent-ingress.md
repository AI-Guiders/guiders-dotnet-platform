# GUIDERS-ADR-0020: MCPlane — federation agent ingress (draft)

| | |
|---|---|
| **Status** | Draft (open questions) |
| **Date** | 2026-08-29 |
| **Tags** | #guiders #platform #mcp #mcplane #agent #pulse #commandplane #conformance |
| **Relates to** | GUIDERS-ADR-0009 · GUIDERS-ADR-0014 · GUIDERS-ADR-0018 · GUIDERS-ADR-0019 · GUIDERS-ADR-0007 · CDP-ADR-0020 · CDP-ADR-0028 · FORGE-ADR-0025 |

## Context

Federation already splits **command semantics** from **invocation surfaces**:

- **CommandPlane** — Catalog · Registry · Command (GoF); slash/melody/binding mechanics; `commandId` SSOT ([GUIDERS-ADR-0009](GUIDERS-ADR-0009-command-surface-pattern.md)).
- **MCP** — product wire (`cdp-mcp`, Forge MCP): `ListTools`, `CallTool`, JSON-RPC, domain tools.

Constitution: hyperlane = «NuGet, schema, **MCP surface**» — surface as **join protocol**, not a monorepo MCP server.

Operators also need **agent context economy** (not JSON walls on every turn):

| Requirement (lived in CDP) | Meaning |
|----------------------------|---------|
| **Pulse default** | Thin response: SA line, seats one-liners, truncation (~240 chars) — [CDP-ADR-0020](https://github.com/AI-Guiders/cdp-mcp/blob/main/docs/adr/CDP-ADR-0020-desk-vs-organ-path.md) |
| **`next[]`** | Suggested follow-ups on pulse (e.g. elevate `go=pressure`) — [CDP-ADR-0018](https://github.com/AI-Guiders/cdp-mcp/blob/main/docs/adr/CDP-ADR-0018-pressure-precompact-desk.md) |
| **On-demand full** | `detail=full`, `go_detail=full`, `pane_full=` — expand **one** organ/seat, not full spray |
| **Refuse thrash** | `seats_detail=full` alone stays pulse; no multi-channel glass spray on default desk path |
| **Why / course** | Sealed operator priority, continuity pointer — habitat product, but **shape** is agent-facing |

Platform already has **seed types** in `AIGuiders.Platform.Abstractions`:

- `IntentOutcome` — product-neutral execute result (aligned with CDP `CitizenRouteHost.Applied`)
- `PulseFormat` — truncation defaults (`DefaultMaxChars = 240`)

These are **not** CommandPlane (no catalog/resolve). They are **agent ingress envelope** — today duplicated across CDP Meta tool docs and Forge `/capabilities`.

**Working name:** **MCPlane** (Model Context / agent **ingress plane**) — sibling hyperlane to CommandPlane, not a package inside it.

## Decision (proposed)

### 1. Two planes, one `commandId`

```text
                    ┌─────────────────────────────────────┐
                    │  Federation platform (headless)      │
                    ├─────────────────┬───────────────────┤
                    │  CommandPlane   │  MCPlane          │
                    │  catalog        │  agent envelope   │
                    │  registry       │  detail tiers     │
                    │  commandId      │  pulse / next[]   │
                    │  slash resolve  │  catalog project  │
                    └────────┬────────┴─────────┬─────────┘
                             │                  │
              slash / palette / hotkey          MCP CallTool
                             │                  │
                    ┌────────┴──────────────────┴─────────┐
                    │  Product: registry host + wire       │
                    │  cdp-mcp · Forge MCP · DashSpec    │
                    └────────────────────────────────────┘
```

| Plane | Question it answers | NuGet (target) | Not |
|-------|---------------------|----------------|-----|
| **CommandPlane** | What command? How resolve path? | `AIGuiders.Platform.CommandPlane.*` | MCP JSON-RPC |
| **MCPlane** | What does agent **see** back? How expand? What's **next**? | `AIGuiders.Platform.MCPlane` (or split `.Abstractions` growth) | Tool handlers |

**Rule:** MCP tool implementation stays in **product**. MCPlane holds **response contract + projection rules** agents and conformance can rely on.

### 2. MCPlane owns (federation)

| Capability | Notes |
|------------|-------|
| **Agent response envelope** | `IntentOutcome` (+ extensions): `ok`, `pulse`, `reason`, structured slots |
| **Detail tiers** | `pulse` (default) · `slim` · `full` — explicit opt-in; default never implies full spray |
| **`next[]` hints** | Ordered suggestions (`go=…`, `commandId`, tool name) — not execution |
| **Pulse truncation** | `PulseFormat` policy; per-surface max chars |
| **Catalog projection** | Agent catalog slice from `ICatalogVisitor` / descriptors — schema for `capabilities.commands[]` |
| **`commandId` parity** | MCP tool name / capabilities entry maps to same id as slash ([FORGE-ADR-0025](https://github.com/AI-Guiders/agent-forge/blob/master/design/FORGE-ADR-0025-human-command-parity.md)) |
| **Cost-path enum** | Desk-thin vs organ-deep vs pane-full (normative names; CDP is reference impl) |

### 3. MCPlane does not own

| Non-goal | Owner |
|----------|--------|
| MCP server, stdio bridge, `ListTools`/`CallTool` | `guiders-core` libs + product host |
| Domain tool bodies (`cdp_buffer`, `forge_issue_*`) | `cdp-mcp`, `agent-forge` |
| Execute / `IPlatformCommand` | CommandPlane registry + product host |
| QRH/ECL **content** | Product + future HelpSurfaces quarry |
| Aviation UI symbology | `guiders-ui-platform` / ASP |

### 4. Detail / folding model (canonical)

Align with lived CDP contract; federation names stay **neutral** in types, aviation in glossary ([GUIDERS-ADR-0007](GUIDERS-ADR-0007-aviation-mental-model.md)):

| Tier | Agent gets | When |
|------|------------|------|
| **Pulse** (default) | Truncated summary, `next[]`, seat one-liners | Every routine call |
| **Slim** | Structured fields without full organ dump | Optional middle tier (tools may alias pulse) |
| **Full** | One pane/organ expanded (`pane_full`, `go_detail=full`) | Explicit request only |
| **Wide** (discouraged) | Multi-channel / full desk spray | Separate slow path; never default |

**Folding:** default response is **folded** (pulse). Unfold is **on-demand** per pane/organ, not global `detail=full` on desk.

### 5. Conformance (with ADR-0019)

| Spec (future) | Proves |
|---------------|--------|
| `mcplane/pulse-default-v1` | Default call returns pulse tier; no full payload |
| `mcplane/detail-expand-v1` | `pane_full` / `go_detail=full` expands only named target |
| `mcplane/next-hints-v1` | `next[]` ordering and shape |
| `agent-catalog-projection-v1` | Descriptor → capabilities JSON round-trip |

Harness: reference types in platform; vectors in `aiguiders-conformance` when repo splits.

### 6. Package map (open — do not ship until quarry slice)

| Today | Target |
|-------|--------|
| `Abstractions` (`IntentOutcome`, `PulseFormat`) | Grow into **MCPlane.Abstractions** or rename package when first conformance ships |
| — | `MCPlane` core: `DetailTier`, `NextHint`, `AgentResponseEnvelope`, projection helpers |
| CommandPlane `ICatalogVisitor` | Stays; MCPlane **consumes** visitor output for agent catalog schema |

**Not chosen yet:** `Platform.Aviation.*` vs `Platform.MCPlane.*` vs fold into `Cockpit.*`. Aviation remains glossary; package prefix TBD ([Desk → Attention](GUIDERS-ADR-0001-platform-boundary.md) same wave).

## Relationship to CommandPlane

| | CommandPlane | MCPlane |
|---|--------------|---------|
| Ingress | slash, melody, binding, palette | MCP (+ future ACP/agent wire) |
| Primary key | `commandId`, slash path | tool name + `commandId` map |
| Output | `CommandOutcome` (effect) | `IntentOutcome` / agent envelope (observation) |
| Conformance | slash-arg-completion, line-resolve | pulse, detail, next, catalog projection |

One registry, two projections: **execute** via CommandPlane; **describe + observe** via MCPlane.

## Open questions

1. **Package name:** `MCPlane` vs `AgentIngress` vs extend `Abstractions` only?
2. **Forge:** `/api/v1/capabilities` schema — normative in MCPlane or Forge-owned with MCPlane conformance?
3. **Citizen wire** ([CDP-ADR-0028](https://github.com/AI-Guiders/cdp-mcp/blob/main/docs/adr/CDP-ADR-0028-citizen-agent-wire.md)): pulse **frames** vs MCP JSON — same envelope, different transport?
4. **CDP-specific knobs** (`go=`, `pane_full`, `course=`): map to neutral MCPlane names or product extensions?
5. **When to quarry:** after CommandPlane registry visitor stable (W2c ✓) + first conformance tag?

## Consequences

- Clear home for pulse/next/folding **without** bloating CommandPlane or cloning MCP in platform.
- CDP Meta tool descriptions become **implementations** of MCPlane tiers — drift detectable via conformance.
- Third-party planets: adopt MCPlane schema + CommandPlane `commandId`; bring own MCP tools.

## References

- [CDP-ADR-0020 desk vs organ path](https://github.com/AI-Guiders/cdp-mcp/blob/main/docs/adr/CDP-ADR-0020-desk-vs-organ-path.md)
- [CDP-ADR-0028 citizen agent wire](https://github.com/AI-Guiders/cdp-mcp/blob/main/docs/adr/CDP-ADR-0028-citizen-agent-wire.md)
- [GUIDERS-ADR-0019 conformance monorepo](GUIDERS-ADR-0019-conformance-hyperlane-monorepo.md)
- `AIGuiders.Platform.Abstractions` — `IntentOutcome.cs`, `PulseFormat.cs`
