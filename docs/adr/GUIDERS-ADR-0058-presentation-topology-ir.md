# GUIDERS-ADR-0058: Presentation topology IR (typed hosts, display binding)

| | |
|---|---|
| **Status** | **Accepted** (IR + notation v0 — display binding profile TBD) |
| **Date** | 2026-09-02 |
| **Tags** | #guiders #federation #presentation #topology #cds #deck #ir |
| **Related** | [0021](./GUIDERS-ADR-0021-notations-quarry-family.md) · [0048](./GUIDERS-ADR-0048-authoring-quarry-family.md) · [0007](./GUIDERS-ADR-0007-aviation-mental-model.md) · [0055](./GUIDERS-ADR-0055-surface-wpf-guild-deck-authoring.md) |

## Context

`.deck` v0 stored `topology` as a **string** in `AttentionPreset` — technical debt. CDS, modes, and surfaces need **typed** presentation IR. Operators have **1–6 physical monitors**, ultrawide single-surface layouts, or RDS single-screen — topology must separate **logical hosts** from **physical binding**.

## Decision

### 1. Wire vs IR (same split as slash)

| Layer | Owns |
|-------|------|
| `.deck` `topology (MFD)(F)` | declare wire (human text) |
| `Notations.Presentation.Topology` | parse wire → `PresentationTopology` |
| `IntermediateRepresentation.Presentation` | **SSOT types** for CDS, emit, conformance |
| `DisplayBindingProfile` | runtime map `HostIndex` → physical screen (deployment) |
| `Surface.Wpf.*` | project IR → grids / TopLevels |

**Strings do not cross the parse boundary into product code.**

### 2. Core IR types

```text
PresentationTopology
  Arrangement: SingleSurfaceCompositional | SingleHostOneOf | MultiHost
  Hosts[]: LogicalDisplayHost
    HostIndex     — 0..n-1 wire order (NOT OS monitor number)
    HostId        — stable id (forward, mfd, pfd, …)
    Role          — Pfd | Forward | Mfd | PmOneOf | Eicas | …
    Compose       — Split | OneOf
    ChannelStack  — surface/channel ids inside host

DisplayBindingProfile (runtime, separate SSOT)
  Bindings[]: HostIndex → PhysicalScreenSelector
    Primary | Index(n) | DeviceName | UltrawideRegion(l,t,w,h)
```

**Ultrawide / single monitor:** `topology single` + `deck layout { … }` — compositional zones inside one surface; binding profile maps `host-0` → `UltrawideRegion` or `Primary`.

**Three monitors:** `topology (P)(F)(M)` defines **3 logical hosts**; operator `display.toml` binds `0→screen-0`, `1→screen-1`, `2→screen-2` (or any permutation).

### 3. Packages (v0 shipped)

| Package | Role |
|---------|------|
| `AIGuiders.Platform.IntermediateRepresentation.Presentation` | IR types |
| `AIGuiders.Platform.Notations.Presentation.Topology` | `TopologyNotation.Parse` |
| `AIGuiders.Platform.Authoring.Deck` | `AttentionPreset.Topology: PresentationTopology?` |

### 4. Conformance

`docs/conformance/notation/presentation-topology.spec.json` — wire → expected arrangement + hosts.

### 5. Emit

`deck emit` → `PresentationTopology ReportAuthor => TopologyNotation.Parse(ReportAuthorWire).Topology!` (typed accessor, wire kept for audit).

## Consequences

- Closes string debt in deck IR; CDS can migrate `AttentionRoutingInput` from `string?` to `HostIndex` / `AttentionDisplayRole`.
- Glass `PresentationSurfaceWire` should **consume** federation notation (extract peel), not duplicate parsers per planet.

## Non-goals (v0)

- `DisplayBindingProfile` parser / `display.toml` guild
- `Notations.Presentation.Layout` (deck layout board)
- Full 4+ host topology wires
