# GUIDERS-ADR-0023: CASE workbench — Vision / Decision Environment (draft)

**Status:** draft (2026-08-30)  
**Tags:** #guiders #platform #case #workbench #fleet #rose #agent-env #aviation #mcplane  
**KB:** `agent-notes/knowledge/worlds/software-case-workbench/` — fundamentals, reading map, transfer matrix

---

## Context

Guiders AI Era product center composes **three pillars** (fleet thesis §3.1–3.2):

| Pillar | Role | Home |
|--------|------|------|
| **Aviation** | **Human + agent pair** — **PM/PF**, cognitive budget, briefing, pressure | ADR-0007 · [essay](../essay/why-aviation-not-pair-programming.md) · `Cockpit.*` |
| **Agent Env** | **Agent half** of the pair — memory, gates, packs, journal | **CDP planet** (`cdp-mcp`); MCPlane = pair wire hyperlane |
| **CASE** (this ADR) | **Vision / Decision Environment** — shared estate model the pair steers | Platform workbench + conformance |

This ADR normates the **CASE pillar** only. It does not subsume Aviation glossary (ADR-0007) or CDP product law.

**Neighbors, not war:** Platform ships hyperlanes; CDP ships Agent Env; CIDE/Glass ship human embassy. Compose on protocol — see Constitution § AI Era stack.

---

## Decision

1. **CASE pillar = desktop Vision/Decision Environment** for protocol estates — architecturally non-optional for full fleet ops (ingress satellites allowed; replacement not).
2. **Three pillars compose:** Aviation governs the **human+agent pair** on **PM/PF**; CASE is the **shared decision surface**; Agent Env is the **agent half's habitat** (CDP + MCPlane pair wire).
3. **Living model** is machine-maintained SSOT under test — not hand-drawn UML as source of truth.
4. **AI + conformance + Agent Env** provide the sync layer the Rose era lacked.

---

## CASE heritage map (what we inherit)

| CASE era concept | Federation primitive | Notes |
|------------------|----------------------|-------|
| Architecture model (multi-view) | Fleet / Planet / Live scopes + MFD drill-down | Same estate, zoom levels |
| Dependency / structure diagram | Hyperlane graph, adoption alliance edges | Generated + drift-gated |
| Requirements traceability | Pain (G-xxx, N-xxx) → ADR → package wave | Human + machine links |
| Model repository | Generated manifests, conformance specs, registry SSOT | Git is store; CI is validator |
| Round-trip engineering | Conformance harness + adoption drift CI | Fail on pin ≠ reality |
| Impact analysis | Briefing before release (pins, alliance row, pains) | Aviation briefing slot |
| Workbench session | CIDE/Glass embassy, hours-in-seat | friction-book ch.24 production channel |
| Forward / reverse engineering | Agent Env maintains vectors; human reviews in workbench | MCPlane + CDP habitat; not autopilot |

**Explicit non-goals (v1):** resurrect UML as primary notation; code-gen from diagrams as center; web-only «fleet dashboard» as workbench substitute.

---

## AI era thesis (why now)

CASE tools failed market-wide when **model sync cost > model value**. Agile correctly rejected lying blueprints.

AI era changes the cost function:

| Before | After (federation direction) |
|--------|------------------------------|
| Human edits model and code | Agents mutate; conformance + drift gates judge |
| Model stale in weeks | Generated tables/specs refreshed in CI |
| One architect bottleneck | High throughput with **mandatory** hygiene |
| Rose DB proprietary | Git + open specs + NuGet hyperlanes |

The federation is already building the Rose **that syncs** — without requiring operators to remember CASE vocabulary day to day.

---

## Attention scopes (CASE object × aviation pair discipline)

| Scope | CASE «view» | Human+agent pair role |
|-------|-------------|----------------------|
| **Fleet** | Formation / portfolio topology | Shared picture; who owns fleet course in this pair |
| **Planet / Ship** | Product / release-train view | Local PFD; pair briefing before ship |
| **Live / Ecosystem** | Infrastructure substrate | Agent triage + human escalation contour |

Scope switch = change **decision root**; pair attention rules unchanged (ADR-0007).

---

## Package / channel direction (quarry)

| Layer | Candidate home |
|-------|----------------|
| Scope IDs + telemetry contracts | `AIGuiders.Platform.Cockpit.*` channels |
| Graph projection (alliance, hyperlane) | Workbench views on CIDE; headless queries for MCPlane |
| Briefing assembly | Correspondence + adoption + conformance bundle |
| Ingress slice | ANPM Human View, MCPlane pulse (not workbench) |

Normative channel IDs and DTOs — follow-on quarry after L1 telemetry stable (roadmap Wave 6).

---

## Consequences

- Roadmap **Wave 6** tracks workbench center explicitly (not only protocol waves).
- Product positioning: **Vision/Decision workbench** for the human+agent pair — aviation pair protocols on CASE estate model; Agent Env as agent half's neighbor habitat.
- Essays and friction-book may cite this ADR; **normative behavior lives here and in fleet thesis**.

---

## Open questions

1. Name in market: «Platform Workbench», «Fleet CASE», or neutral «Attention» desk with CASE docs internal only?  
2. Graph-native views first (alliance) or table-native (pain, registry) — operator default?  
3. How much 4+1 / viewpoints vocabulary to adopt vs invent federation-native terms?

---

## Related

- [PLATFORM-FLEET-THESIS.md](../PLATFORM-FLEET-THESIS.md)  
- [GUIDERS-ADR-0007](GUIDERS-ADR-0007-aviation-mental-model.md)  
- [GUIDERS-ROADMAP.md](../GUIDERS-ROADMAP.md) — Wave 6
