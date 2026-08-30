# GUIDERS-ADR-0020: MCPlane — federation agent ingress (draft)

| | |
|---|---|
| **Status** | Draft (open questions) |
| **Date** | 2026-08-29 |
| **Tags** | #guiders #platform #mcp #mcplane #agent #pulse #commandplane #conformance |
| **Relates to** | GUIDERS-ADR-0009 · GUIDERS-ADR-0014 · GUIDERS-ADR-0018 · GUIDERS-ADR-0019 · GUIDERS-ADR-0021 · [Constitution § Planets are not SSOT](../GUIDERS-FEDERATION-CONSTITUTION.md#planets-are-not-federation-ssot) |

## Context

Federation already splits **command semantics** from **invocation surfaces**:

- **CommandPlane** — Catalog · Registry · Command (GoF); slash/melody/binding mechanics; `commandId` SSOT ([GUIDERS-ADR-0009](GUIDERS-ADR-0009-command-surface-pattern.md)).
- **MCP** — **product** wire (Forge MCP, planet MCP hosts): `ListTools`, `CallTool`, JSON-RPC, domain tools.

Constitution: hyperlane = «NuGet, schema, **MCP surface**» — surface as **join protocol**, not a monorepo MCP server.

Agents need **context economy** (not JSON walls on every turn). Federation defines **neutral** requirements — not imported from any single planet:

| Requirement | Meaning |
|-------------|---------|
| **Pulse default** | Thin response: one-line summary, truncation policy (`PulseFormat`, default ~240 chars) |
| **`next[]`** | Suggested follow-ups on pulse — hints only, not execution |
| **On-demand full** | Expand **one** target to full detail; explicit opt-in |
| **Refuse thrash** | No multi-channel / full spray on default path |
| **Why / course** | Optional structured continuity slots — **shape** in envelope; content is product |

Platform **seed types** in `AIGuiders.Platform.Abstractions`:

- `IntentOutcome` — product-neutral execute/observation result
- `PulseFormat` — truncation defaults (`DefaultMaxChars = 240`)

These are **not** CommandPlane (no catalog/resolve). They are **agent ingress envelope** — today also projected in Forge `/capabilities` and various MCP tool hosts.

**Planet boundary:** experimental habitats (e.g. CDP buffer/Citizen/Meta tools) may dogfood tiers first. Their ADRs and knob names are **informative**, not federation SSOT. See [Constitution — Planets are not federation SSOT](../GUIDERS-FEDERATION-CONSTITUTION.md#planets-are-not-federation-ssot).

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
                    │  Forge MCP · CIDE · DashSpec · …     │
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
| **`next[]` hints** | Ordered suggestions (`commandId`, tool name, neutral hint tokens) — not execution |
| **Pulse truncation** | `PulseFormat` policy; per-surface max chars |
| **Catalog projection** | Agent catalog slice from `ICatalogVisitor` / descriptors — schema for `capabilities.commands[]` |
| **`commandId` parity** | MCP tool name / capabilities entry maps to same id as slash ([FORGE-ADR-0025](https://github.com/AI-Guiders/agent-forge/blob/master/design/FORGE-ADR-0025-human-command-parity.md)) |
| **Cost-path enum** | Neutral: `thin` · `targeted-full` · `wide` (discouraged) — product maps own knobs via extensions |

### 3. MCPlane does not own

| Non-goal | Owner |
|----------|--------|
| MCP server, stdio bridge, `ListTools`/`CallTool` | `guiders-core` libs + **product** host |
| Domain tool bodies (`forge_issue_*`, planet-specific tools) | sovereign planets |
| Execute / `IPlatformCommand` | CommandPlane registry + product host |
| QRH/ECL **content** | Product + future HelpSurfaces quarry |
| Aviation UI symbology | `guiders-ui-platform` / ASP |
| **Planet in-house agent wire** (Citizen frames, `@intent`, buffer Meta grammar) | that planet only — not federation ingress |

### 4. Detail / folding model (canonical)

Federation tier names stay **neutral** in types; aviation terms live in glossary ([GUIDERS-ADR-0007](GUIDERS-ADR-0007-aviation-mental-model.md)). Products may use local knob names as **extensions** mapped to tiers.

| Tier | Agent gets | When |
|------|------------|------|
| **Pulse** (default) | Truncated summary, `next[]`, compact child summaries | Every routine call |
| **Slim** | Structured fields without full dump | Optional middle tier |
| **Full** | One named target expanded | Explicit request only |
| **Wide** (discouraged) | Multi-target / full spray | Separate slow path; never default |

**Folding:** default response is **folded** (pulse). Unfold is **on-demand** per target, not global implicit full.

### 5. Conformance (with ADR-0019)

| Spec (future) | Proves |
|---------------|--------|
| `mcplane/pulse-default` | Default call returns pulse tier; no full payload |
| `mcplane/detail-expand-v1` | explicit target expands only that target |
| `mcplane/next-hints` | `next[]` ordering and shape |
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
| Ingress | slash, melody, binding, palette | MCP `CallTool` result · Forge `/capabilities` |
| Primary key | `commandId`, slash path | tool name + `commandId` map |
| Output | `CommandOutcome` (effect) | `IntentOutcome` / agent envelope (observation) |
| Conformance | slash-arg-completion, line-resolve | pulse, detail, next, catalog projection |

One registry, two projections: **execute** via CommandPlane; **describe + observe** via MCPlane.

## Open questions

1. **Package name:** `MCPlane` vs `AgentIngress` vs extend `Abstractions` only?
2. **Forge:** `/api/v1/capabilities` schema — normative in MCPlane or Forge-owned with MCPlane conformance?
3. **Product extension map:** how planets register local detail knobs → neutral `DetailTier` (registry in product, not CDP names in platform)?
4. **When to quarry:** after CommandPlane registry visitor stable (W2c ✓) + first conformance tag?

## Consequences

- Clear home for pulse/next/folding **without** bloating CommandPlane or cloning MCP in platform.
- Any planet MCP host can implement MCPlane tiers — drift detectable via conformance, not via one product's tool docs.
- Third-party planets: adopt MCPlane schema + CommandPlane `commandId`; bring own MCP tools.
- Root pains: [GUIDERS pain inventory](../GUIDERS-pain-inventory.md) **G-001**–**G-005**.

## References

- [GUIDERS-ADR-0019 conformance monorepo](GUIDERS-ADR-0019-conformance-hyperlane-monorepo.md)
- [FORGE-ADR-0025 human command parity](https://github.com/AI-Guiders/agent-forge/blob/master/design/FORGE-ADR-0025-human-command-parity.md)
- `AIGuiders.Platform.Abstractions` — `IntentOutcome.cs`, `PulseFormat.cs`

### Informative only (planets — not federation SSOT)

- CDP product ADRs (desk/organ, pressure, citizen wire) — early dogfood; neutral names in MCPlane do not require CDP vocabulary
