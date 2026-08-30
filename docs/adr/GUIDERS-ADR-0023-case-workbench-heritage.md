# GUIDERS-ADR-0023: CASE workbench heritage — platform center (draft)

**Status:** draft (2026-08-30)  
**Tags:** #guiders #platform #case #workbench #fleet #rose #model-driven #mcplane  
**Related:** GUIDERS-ADR-0007 · GUIDERS-ADR-0001 · GUIDERS-ADR-0006 · GUIDERS-ADR-0020 · GUIDERS-ADR-0022 · [PLATFORM-FLEET-THESIS](../PLATFORM-FLEET-THESIS.md)

---

## Context

Guiders Platform product center is a **desktop workbench** for fleet / protocol-estate operations (see fleet thesis §3.1, §6).

Two heritages compose it — **not** competing metaphors:

| Heritage | Question answered | Normative home |
|----------|-------------------|----------------|
| **Aviation** (ADR-0007) | How does operator **attention** route? | CDS, displays, phases, briefing, EICAS |
| **CASE** (this ADR) | What **estate object** is being steered? | Scopes, topology, traceability, workbench posture |

ADR-0007 remains **aviation-only** for attention and accessibility glossary. CASE vocabulary applies to **estate modeling and workbench semantics** — topology, views, sync, trace — not to replace PFD/MFD/EICAS terms.

---

## Decision

1. **Platform center = CASE-class workbench** on desktop, architecturally non-optional for full fleet ops (ingress satellites allowed; replacement not).
2. **Aviation + CASE compose:** CDS routes attention **over** CASE-shaped estate views (Fleet / Planet / Live scopes).
3. **Living model** is machine-maintained SSOT under test — not hand-drawn UML as source of truth.
4. **AI + conformance** provide the round-trip sync layer the Rose era lacked.

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
| Forward / reverse engineering | Agent maintains vectors; human reviews in workbench | MCPlane clerk, not autopilot |

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

## Attention scopes (CASE object × aviation instruments)

| Scope | CASE «view» | Aviation instruments (fill) |
|-------|-------------|----------------------------|
| **Fleet** | Formation / portfolio topology | Course, attractions, fleet health rollup |
| **Planet / Ship** | Product / release-train view | Local course, health, ship briefing |
| **Live / Ecosystem** | Infrastructure / platform substrate | Life-support fuel, pressure, hull |

Scope switch = change **model root**; display grammar unchanged (ADR-0007).

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
- Product positioning: **Platform Workbench** for protocol estates — aviation UX on CASE object model.
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
