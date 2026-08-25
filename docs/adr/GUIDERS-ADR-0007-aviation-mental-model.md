# GUIDERS-ADR-0007: Aviation mental model — phases, surfaces, CDS, accessibility

**Status:** accepted (2026-08-25)  
**Tags:** #guiders #platform #cockpit #cds #aviation #accessibility #correspondence #asp #charter  
**Related:** GUIDERS-ADR-0002 · GUIDERS-ADR-0006 · GUIDERS-UI-0003 · GUIDERS-UI-0004 · CDP-ADR-0020 · Cascade ADR 0021

**KB:** `agent-notes/knowledge/worlds/aviation-human-factors/kb-aviation-pfd-mfd-efis-eicas-fundamentals-v1.md`

---

## Context

AI Guiders uses an **aviation mental model** for attention, cockpit mechanics, and human factors (Cascade ADR 0021, platform `Cockpit.*`, CDS stack in GUIDERS-ADR-0002).

Recent work added:

- **ASP** (GUIDERS-UI-0004) — symbology for UI surfaces (human AT + Agent AX)
- **Correspondence** (epistemic) — «what is known before action?» — must not be collapsed into ExploreCorr **gate** on `.md` (CDP-ADR-0020 lesson)
- **Confederation** (GUIDERS-ADR-0006) — sovereign repos joined by protocols

**Decision:** one glossary, aviation-only. No parallel metaphors (geology, city, empire) in platform/accessibility ADRs.

---

## Core question (Correspondence essence)

> Before you act — do you understand all available information? Here is what we know; look.

That is **briefing**, not a mutate block. Code/Dev Correspondence is a **consumer** of this model, not its owner.

---

## Phase model (depth — not “layers” from other domains)

Phases describe **depth of the stack**, bottom to top. **Surface** is Phase 4 only.

| Phase | Aviation term | Platform meaning |
|-------|-----------------|------------------|
| **0** | Type certification | Federation charter, identity, semver invariants (GUIDERS-ADR-0006) |
| **1** | Symbology / flight manual | ASP schemas, UI.Core contracts, Correspondence **claims** |
| **2** | Route · waypoints · NOTAM | Cross-repo **wires** (KB↔ADR↔code↔NuGet); federation hyperlanes |
| **3** | Avionics bus · **CDS** | Composition, exposure **profiles**, attention routing (agent-native, IDE-agnostic) |
| **4** | **Surface** (PFD · MFD · HUD) | Rendered snapshot: DOM+ARIA, agent JSON, briefing peek |
| **5** | FDR · post-flight audit | Evidence snapshots, drift reports, conformance (ANUI lineage) |

```
Phase 0  Certification
Phase 1  Symbology + claims
Phase 2  Route / wires
Phase 3  CDS (routing)
Phase 4  Surface  ← what operator/agent touches
Phase 5  FDR / evidence
```

---

## Displays (Phase 4 surfaces)

| Display | Aviation role | AI Guiders use |
|---------|---------------|----------------|
| **PFD** | Primary flight — task now | Forward: editor, current leaf, main ship work |
| **MFD** | Secondary pages on demand | Docs peek, wire graph, CRS/Correspondence surface |
| **HUD** | Overlay on forward view | Inline hints, command palette, light chrome |
| **EICAS** | W/C/A — **Dark Cockpit** when OK | Drift only: canon ≠ reality, a11y/conformance fault |
| **Briefing / FMS** | Pre-departure picture of route | **Correspondence core** — resolve locus → knowledge surface |

**Dark Cockpit:** EICAS quiet in norm; alerting is not the default UI.

---

## Two navigation contours, one CDS

CDS (`ICdsRouter`, attention contour) routes **both** contours to displays. IDE/host is a **glass**, not SSOT.

### UI contour (product surface)

| Phase | Element |
|-------|---------|
| 1 | **Symbology** — ASP, `AIGuiders.UI.Core`, tokens |
| 2 | NuGet consumer wires, Forge reference mission |
| 3 | Razor/React profiles |
| 4 | Rendered human + Agent AX exposure |
| 5 | UI evidence / journey conformance |

### Canon contour (knowledge surface)

| Phase | Element |
|-------|---------|
| 1 | ADR claims, DoD, incarnation canon |
| 2 | KB↔repo wires, version pins |
| 3 | Briefing resolver (agent-native API) |
| 4 | MFD peek / session briefing |
| 5 | Drift: ADR DoD vs `Directory.Build.props` vs nuget.org |

---

## Term glossary (canonical — use these words)

| Term | Meaning | Do **not** use for |
|------|---------|---------------------|
| **Surface** | Phase 4 snapshot on a display | whole stack, “platform” |
| **Symbology** | Phase 1 signs/semantics | ASP package name in prose when meaning concepts |
| **ASP** | Accessibility Surface Protocol — UI symbology + profiles | Correspondence, gate |
| **Briefing** | Correspondence resolve output | pre-mutate block |
| **Correspondence** | Epistemic protocol (briefing + route + drift) | `workspace.toml` gate only |
| **Route / wire** | Phase 2 link between sovereign repos | C# `using` |
| **NOTAM** | Time-bound constraint on route | — |
| **CDS** | Phase 3 attention routing between displays | authorization gate |
| **EICAS** | Drift / warning channel | primary workspace |
| **Checklist** | Optional Dev policy before **code** phase change | ADR/KB authoring |
| **FDR** | Phase 5 recorded evidence | live UI |
| **Type certification** | Phase 0 non-negotiables | product preference |

**Rejected in this charter:** geology layers, “city”, gate-as-Correspondence, owl-on-globe (`workspace.toml` for KB canon).

---

## Consumers (IDE-agnostic)

| Consumer | Uses | Checklist? |
|----------|------|------------|
| **Agent (MCP)** | Briefing API, symbology JSON, CDS hints | policy per host |
| **CDP / CIDE** | Dev Correspondence profile + optional **Checklist** on code | yes, code only |
| **Glass WPF** | Surface snapshots, EICAS channel | no |
| **Forge** | UI symbology + human view | no |
| **CI** | FDR: drift on claims | no |

**Code (Dev) Correspondence** = profile `dev-code` + optional Checklist. **Not** the definition of Correspondence.

---

## CDS routing rules (normative)

1. **PFD** carries the current task; do not flood with briefing walls or EICAS noise.
2. **Briefing** lands on MFD or pre-session attach — agent reads before acting, host does not block `.md` creation.
3. **EICAS** fires on drift severity ≥ caution; norm is Dark Cockpit.
4. **One CDS contract** in `Cockpit.Abstractions`; hosts implement glasses (Cursor, CIDE, opencode).
5. **Symbology** (Phase 1) is stable; **Surface** (Phase 4) may differ per profile (Razor, React, agent-json).

---

## Package map (current + planned)

| Package / ADR home | Phase focus |
|--------------------|-------------|
| `guiders-platform` charter ADRs | 0 |
| `AIGuiders.UI.Core` · `Accessibility.Core` (planned) | 1 |
| Federation ADRs · `kb-wires` (planned) | 2 |
| `AIGuiders.Platform.Cockpit.*` · CDS | 3 |
| `AIGuiders.UI.Web.HTMX` · Glass snapshots | 4 |
| `Anui.Evidence` · CI drift (GUIDERS-UI-0004) | 5 |
| `AIGuiders.Platform.Correspondence` (planned) | 1–3 briefing resolver; not a gate |

---

## Consequences

- New platform/accessibility ADRs **must** use this glossary (aviation-only).
- GUIDERS-UI-0004 ASP is **UI contour symbology**; Correspondence charter is sibling under Phase 1–3 **canon contour** — shared Phase 5 FDR.
- ExploreCorr **Checklist** stays in Dev profile; KB authoring paths remain `explore_corr off` or briefing-only (CDP-ADR-0020).
- CIDE ADR 0021 remains human cockpit SSOT for layout; this ADR is **federation-wide** semantics for agents and multi-repo canon.

**Essay (FAQ for skeptics):** [Why aviation, not Pair Programming](../essay/why-aviation-not-pair-programming.md)

---

## vNext

1. `AIGuiders.Accessibility.Core` — symbology + briefing claim types (Phase 1)
2. `AIGuiders.Platform.Correspondence` — briefing resolve API (Phase 3), no mutate gate
3. `kb-wires.toml` profile for agent-notes (Phase 2)
4. GUIDERS-UI-0005 — ASP symbology catalog mapped to PFD/MFD/EICAS exposure rules
