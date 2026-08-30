# Platform Fleet Development — thesis (draft)

**Status:** draft · **Date:** 2026-08-30  
**Level:** federation hub — *why* fleet-first; not normative ADR detail  
**Relates to:** [Constitution](GUIDERS-FEDERATION-CONSTITUTION.md) · [pain inventory](GUIDERS-pain-inventory.md) · [roadmap](GUIDERS-ROADMAP.md) · ANPM · MCPlane

---

## 1. Throughput changed; discipline did not

Garbage in, garbage out existed before AI. AI removed latency — not judgment.

| Before | After AI |
|--------|----------|
| Bottleneck filtered bad ideas (slow teams, slow CI) | High-throughput vacuum: ADRs, packages, repos, policies — faster than review |
| Platform = quarters | Platform skeleton = days (see federation commit arc) |
| Management implicit in queue depth | Management must be **explicit** or fleet state rots |

This is not an AI problem. It is a **management and invariant** problem. AI is an industrial vacuum; someone must own the filter bag.

---

## 2. Product-centric vs Fleet-centric

| Product-centric (industry default) | Fleet / Platform-centric (this thesis) |
|------------------------------------|----------------------------------------|
| One repo, one backlog, one user surface | Many **sovereign planets**, one **constitution** |
| Ship feature | Ship **hyperlane** + conformance tag + adoption proof |
| CI = our build is green | CI = **drift** (alliance table, registry policy, spec pin) |
| Integration = merge codebases | Integration = **embed protocol**, native port |
| Docs for humans | ADRs + **machine vectors** + agent ingress (MCPlane) |

**Fleet-first** does not mean «one big app». It means **first-class primitives for operating a protocol estate** — the way product tools treat features.

Early primitives in this federation (proof, not product):

- Pain inventory (G-xxx) → ADR → package wave  
- Conformance hyperlane (`*.spec.json`)  
- Adoption alliance (`AdoptionReport`)  
- Registry pains (ANPM N-xxx) — TP, owners, symbols  
- Coding SSOT (`.editorconfig`, `.gitattributes`) per repo  

---

## 3. What is missing in the market

Tools optimize **first publish** (one package, one service), not **fleet ops**:

- nuget.org: TP flat list, no policy CRUD/API  
- GitHub: repos and Actions, not «46 packages × 4 monorepos × drift»  
- Backstage: service catalog, not protocol semver + conformance + adoption  

**Gap:** Platform / Fleet Development Environment — same **attention mechanics** as product cockpit, different **instrumentation** for fleet, planet, and life-support.

---

## 3.1 Platform center — dual heritage (product, not essay)

Guiders Platform is not «IDE + some federation docs». The **center of the product** is a **desktop workbench** for operating a protocol estate — composed from two mature traditions that each accumulated decades of human-factors and tooling R&D, and that **AI finally makes practical together**:

| Heritage | What it contributes | Platform home |
|----------|---------------------|---------------|
| **Aviation** ([ADR-0007](adr/GUIDERS-ADR-0007-aviation-mental-model.md)) | Attention routing, CDS, PFD/MFD/EICAS, briefing, dark cockpit, phases 0–5 | `Cockpit.*`, CIDE/Glass contours, MCPlane tiers |
| **CASE** (Computer-Aided Software Engineering) | Estate-wide model, multi-view workbench, round-trip discipline, review-before-commit | Fleet / Planet / Live scopes, conformance, adoption graph, pain→ADR trace |

Aviation answers **how operator attention flows**. CASE answers **what object you are steering** when the «system» is dozens of repos, packages, and policies — not one app backlog.

This is **platform product center**, not friction-book color. The book names the demo/production split; the **workbench is what we ship**.

### Why CASE waited

Rational Rose and the CASE wave (~1990s–2000s) invested heavily in:

- visual architecture and dependency topology;
- multiple synchronized views of one estate;
- traceability from requirement → design → implementation;
- desktop sessions measured in hours, not minutes.

They stalled because **humans could not keep the model honest** against code at agile speed. The model became wallpaper; text-first IDEs won; web ate the demo channel.

**AI era unlock:** agents + machine vectors + conformance CI are the **sync layer CASE never had**. Federation primitives are already Rose-shaped — without hand-drawn UML:

| CASE promise (era) | Federation primitive (now) |
|--------------------|----------------------------|
| Living architecture model | Adoption alliance + hyperlane pins |
| View consistency | Attention scopes (Fleet / Planet / Live) on one CDS |
| Round-trip to code | `*.spec.json` conformance + drift gates |
| Impact analysis before change | Briefing: pains closed, pins, alliance row |
| Repository of truth | Pain inventory, ADRs, generated manifests |

The hour CASE waited for is **high-throughput fleet ops with enforced model hygiene** — not nostalgia for diagrams.

See draft [GUIDERS-ADR-0023](adr/GUIDERS-ADR-0023-case-workbench-heritage.md) · roadmap Wave 6.

---

## 4. Fleet-first = same Attention model, different fill

CIDE/Glass already solve **how attention is routed** — CDS, displays, course/health, briefing, EICAS-only-when-drift ([ADR-0007](adr/GUIDERS-ADR-0007-aviation-mental-model.md)).  
Product work fills those instruments with **buffers, LSP, Glass projection, slash/melody**.

**Fleet-first does not invent a new UX paradigm.** It reuses the cockpit attention stack and swaps the **telemetry sources** and **contour labels**:

| Mechanism (unchanged) | Product fill (CIDE/Glass today) | Fleet fill (this thesis) |
|------------------------|----------------------------------|---------------------------|
| **CDS / attention routing** | editor leaf, doc, palette | fleet ↔ planet ↔ live scope switch |
| **PFD** — task now | current buffer, ship work | current intervention (release, drift fix, alliance) |
| **MFD** — on-demand depth | docs, wire graph | planet drill-down, package graph, ADR |
| **Briefing** — picture before act | correspondence / KB | adoption + conformance + registry before publish |
| **EICAS** — dark cockpit; alert on drift | canon ≠ code | manifest ≠ nuget.org, alliance stale, spec lag |
| **Course / near-course** | navigation intent | roadmap wave, hyperlane direction |
| **Health** | env ready, build | CI, conformance, pain status per planet |

Aviation supplies the **instrument grammar**. CASE supplies the **estate object** those instruments observe. Ingress (web, MCPlane pulse) projects slices; the **workbench** is desktop-class — see §6.

---

## 5. Three attention scopes (stack)

One operator (human or agent) switches **scope**; inside each scope the same instrument grammar applies.

```text
                    ┌─────────────────────────────────────┐
  Fleet scope       │  Whole confederation in motion       │
  (formation)       │  course · attractions · fleet health │
                    └─────────────────┬───────────────────┘
                                      │ zoom
                    ┌─────────────────▼───────────────────┐
  Planet / Ship     │  One sovereign product / release train │
  scope             │  same instruments, local telemetry     │
                    └─────────────────┬───────────────────┘
                                      │ rests on
                    ┌─────────────────▼───────────────────┐
  Live / Ecosystem  │  Life support — fuel & plumbing        │
  scope             │  DevSecOps · registry · CI · support   │
                    └─────────────────────────────────────┘
```

### 5.1 Fleet — «куда движется весь флот»

**Question:** Is the confederation on course? What pulls planets together or apart?

| Instrument | Fleet meaning | Federation signals (today / near) |
|----------|---------------|-----------------------------------|
| **Course** | Strategic direction | `GUIDERS-ROADMAP`, ADR waves, constitution |
| **Attractions** | Gravity between bodies | Adoption alliance edges, hyperlane pins, shared conformance tags |
| **Health** | Per-planet rollup | CI badge, open G-xxx / N-xxx count, last release age |
| **Near-course** | Drift without emergency | Spec pin behind package semver, stale `ADOPTION-ALLIANCE.generated.md` |
| **Off-course** | EICAS-worthy | TP orphan, symbol push failed, conformance red, «рисованный альянс» |

Fleet scope is **not** a merged product UI — it is a **CDS contour** over federation SSOT.

### 5.2 Planet / Ship — «тот же прибор, один продукт»

**Question:** Is *this* planet (Forge, CIDE, guiders-platform, …) healthy and shipping coherently?

Same PFD/MFD/Briefing/EICAS grammar, scoped telemetry:

| Instrument | Planet meaning |
|------------|----------------|
| **Course** | Planet roadmap, local ADRs, release train |
| **Health** | Repo CI, test/conformance slice, package versions |
| **Ship** | Releasable unit — tag, nupkg set, MCP exe, app build |
| **Briefing** | Pre-release: what pins, what alliance row, what pains closed |

CIDE today is mostly **Planet scope for code work**. Fleet-first adds **explicit zoom out** to formation view without leaving the attention model.

### 5.3 Live / Ecosystem — «жизнеобеспечение»

**Question:** Is there fuel and pressure to move at all?

Lower layer — not «another app», the **substrate** Fleet and Planet depend on:

| Life-support domain | Examples | Pain IDs |
|---------------------|----------|----------|
| **Fuel** | NuGet publish, TP OIDC, symbols snupkg | N-002, N-020, N-029 |
| **Pressure** | CI runners, build agents, disk, SDK pin | coding SSOT, `global.json` |
| **Hull** | Secrets, signing, vuln scan, license | N-006, PackageIntelligence |
| **Ground crew** | Support, incidents, operator runbooks | G-006b, adoption pact |
| **Air traffic** | Registry owners, co-owner drift, deprecate | N-011, N-022 |

DevSecOps / SRE / platform ops **live here** by default. Product devs visit on EICAS or briefing, not as primary workspace.

---

## 6. Architectural invariant: desktop-class workbench

**There is no architectural fork.** Fleet / platform development environment is **desktop-class by physics**, the way mechanical design is CAD-class and 1990s–2000s architecture modeling was **CASE-class**.

| Physical world | Software engineering | Federation fleet ops |
|----------------|----------------------|----------------------|
| CAD (SolidWorks, …) | **CASE** — Computer-Aided Software Engineering | Platform cockpit (CIDE/Glass + attention scopes) |
| Blueprint, constraints, assembly | Visual model, round-trip, multi-view (Rational Rose, Together, …) | Alliance graph, conformance pins, hyperlane topology, pain→ADR trace |
| Cannot «fit the plant» in a phone widget | Cannot «fit the estate» in a dashboard tile | Ingress yes; **cockpit** no |

**CASE** is the term you were reaching for — not «CAD for code» as a product name, but the same **professional workbench** posture: large surface, long session, navigable graph, review before commit.

Rational Rose and kin died in the market — not because the **problem** was wrong, but because **manual model↔code sync** could not keep pace. Models became lying wallpaper; agile shipped text-first IDEs; web ate the demo channel.

**AI era thesis:** this class gets a second breath when **agents maintain the living model**:

| Rose era (failed sync) | Fleet-first + AI era |
|------------------------|----------------------|
| UML diagram hand-edited | Machine vectors: `*.spec.json`, adoption manifest, pain inventory |
| Round-trip promised, rarely true | Conformance CI + drift gates enforce pin ↔ reality |
| Model = documentation | Model = **SSOT under test**; code is one projection |
| One architect, slow updates | High-throughput vacuum **requires** automated model hygiene |

Fleet attention scopes are not «another dashboard». They are **CASE for a protocol confederation** — formation view, drill-down, briefing before release — with MCPlane as the clerk that keeps the blueprint from rotting.

### 6.1 What is not the workbench (ingress only)

Web, mobile, chat, MCPlane pulse — **valid ingress**, not substitutes for the workbench:

| Surface | Role |
|---------|------|
| **ANPM / web** | Live scope triage — TP status, registry glance, alert |
| **MCPlane** | Agent pulse, `next[]`, scoped observe |
| **Forge / planet UIs** | Sovereign product surfaces |

**CIDE / Glass** (or future Avalonia embassy) hosts the **full attention stack** — Fleet, Planet, Live scopes on one desktop-class surface. That is not a product preference; it is the same constraint as reviewing a 400-line diff or a forty-node dependency graph.

Seamless product integration (Forge inside CIDE, etc.) remains **planet choice**. Federation ships hyperlanes and cockpit contracts; it does not mandate merged executables.

---

## 7. «Good luck fitting this in web» — demo channel vs production

*Friction-book [ch.24 — Invisible desktop production](https://github.com/AI-Guiders/friction-book/blob/main/src/24-invisible-desktop-production.md): we ship behind the monitor, demo in the pocket, pretend the first doesn't exist.*

Fleet ops surfaces the same split:

| | Demo channel | Production channel |
|---|--------------|-------------------|
| **Narrative** | «Fleet dashboard in the browser», MAU, one screen | alliance graph, 46 packages × drift, TP orphans, conformance diff |
| **What gets funded** | web vitrine, agent-in-chat | IDE, multi-repo drill-down, hours-in-seat review |
| **Honest reaction** | looks shippable in a sprint | *«ха-ха, удачи поместить **всё это** в web»* |

That skepticism is **not** anti-web. It is anti **pretending production doesn't exist** — collapsing ingress (alert, pulse, registry glance) into cockpit (formation view, blame-width diff, long session).

Fleet-first telemetry is **production-class**:

- adoption alliance is not a KPI tile — it is a **living graph** with stale-row EICAS;
- Live/Ecosystem (TP, symbols, CI) is not «settings» — it is **life support** you debug for hours;
- Fleet scope without Planet zoom is a **screenshot**, not an operator posture.

**Federation stance** (aligned with friction-book environment-first):

1. **Ingress ≠ cockpit** — web/ANPM/MCPlane pulse for triage; desktop-class surface for review and drill-down when scope demands it.  
2. **PAN, not phone OR desktop** — phone for alert/2FA; laptop/cockpit for production session; continuity, not forced device switch.  
3. **Artifact without viewer is garbage** — generated alliance table without diff/history is demo fluff.  
4. **Do not shame hours-in-seat** — fleet operator at three monitors is not legacy; it is where confederation state is **understood**, not merely displayed.

CIDE/Glass as embassy = **the workbench exists** — not shame about desktop, not a stack bet against web.

---

## 8. Recommended build order

```text
L1  Machine telemetry — manifests, conformance CI, adoption drift, pain→ADR
L2  Attention scopes  — Fleet / Planet / Live on Cockpit.* channels (CASE layer)
L3  Desktop workbench — CIDE/Glass embassy; full scope stack
L0  Protocol hyperlanes — planets sovereign
     Ingress satellites — ANPM web, MCPlane (parallel, not replacement)
```

**L1** is non-negotiable. **L2** is the golden vein: reuse CDS, new feeds, **living model**. **L3** is the architectural home — desktop workbench. Ingress ships in parallel for triage, not instead of L3.

---

## 9. Tensions to keep explicit

| Tension | Resolution |
|---------|------------|
| Fast AI throughput vs invariants | Pain inventory + conformance gates + wave roadmap |
| Demo channel vs production posture | Ingress on web/agent; workbench on desktop (ch.24); no architectural «web cockpit» |
| Operator UX vs nuget.org UX | ANPM / local manifest SSOT; nuget.org is upstream, not control plane |
| Agent throughput vs human clarity | MCPlane pulse default; full detail on demand |

---

## 10. Open questions

1. **Scope switch UX:** one Attention desk with Fleet/Planet/Live tabs, or CDS contour keys (like CIDE today)?  
2. **Fleet PFD default:** roadmap wave vs alliance drift vs release train?  
3. **CASE projection:** which views are graph-native (alliance, hyperlane) vs table-native (pain, registry)?  
4. **Agent parity:** MCPlane maintains model hygiene — enough to avoid Rose-era wallpaper drift?

---

## 11. Related

- [GUIDERS-ADR-0007](adr/GUIDERS-ADR-0007-aviation-mental-model.md) — phases, displays, CDS, EICAS  
- [GUIDERS-ADR-0001](adr/GUIDERS-ADR-0001-platform-boundary.md) — Desk → Attention, Cockpit contracts  
- [GUIDERS-FEDERATION-CONSTITUTION.md](GUIDERS-FEDERATION-CONSTITUTION.md) — planets sovereign  
- [GUIDERS-ROADMAP.md](GUIDERS-ROADMAP.md) · [GUIDERS-pain-inventory.md](GUIDERS-pain-inventory.md)  
- [ANPM pain inventory](https://github.com/AI-Guiders/agent-nuget-pm/blob/main/docs/ANPM-pain-inventory.md) — Live/Ecosystem pains (N-xxx)  
- [Friction-book ch.24](https://github.com/AI-Guiders/friction-book/blob/main/src/24-invisible-desktop-production.md) — invisible desktop, demo vs production channel  

- [GUIDERS-ADR-0023](adr/GUIDERS-ADR-0023-case-workbench-heritage.md) — CASE heritage, platform workbench center  
