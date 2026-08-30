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

**Gap:** Platform / Fleet Development Environment — control plane for builders who ship **roads & signage**, not a single app.

---

## 4. If we reorganize Fleet-first — three delivery shapes

These are **not** mutually exclusive. They are layers; the failure mode is picking only one and calling it «the platform».

### A. Seamless product integration

Products plug into each other: shared shell, shared navigation, shared session, «feels like one app».

| Pros | Cons |
|------|------|
| Best end-user UX in a single org | **Borg integration** — violates confederation if normative |
| Familiar to product management | Hides coupling; breaks when planets diverge |
| | Does not help **third-party** planets |

**Federation stance:** seamless UX is a **planet choice** (e.g. CIDE embedding Glass), not federation law. Federation ships **hyperlanes** (CommandPlane, Notations, MCPlane), not one merged executable.

### B. Platform Cockpit (CIDE / Glass — our cockpit, not «industry cockpit»)

A **fleet operator surface**: registry health, adoption table, conformance status, pain backlog, release trains, multi-repo drill-down — wired to real SSOT (ANPM manifest, `ADOPTION-ALLIANCE.generated.md`, CI badges), not marketing.

| Pros | Cons |
|------|------|
| Matches how platform teams actually work | Risk of building **another monolith** |
| Can dogfood Cockpit.* packages (Channels, DataBus, CCU) | Desktop weight, long install, release cadence |
| CIDE/Glass already host slash, melody, projection | Easy to confuse with **product** IDE |

**Federation stance:** Cockpit is a **reference embassy** for operators and power users — not mandatory citizenship for every adopter. Forge may stay browser-first; DashSpec another shape.

### C. Heavy desktop (+ optional web)

Fleet ops may need deep tools (diff, LSP, multi-buffer, long sessions). Web for **registry / ANPM / alliance**; desktop for **composition and debug** (CIDE/Glass).

| Pros | Cons |
|------|------|
| Serious operator UX | Cost: Avalonia/WPF, updates, OS matrix |
| Aligns with Glass projection, buffer metaphors | Conflicts with «lightweight embed» story for third parties |
| Web slice still possible (ANPM Human View, Forge) | Two surfaces to maintain |

**Federation stance:** **Split by role**, not by dogma:

| Role | Surface |
|------|---------|
| Registry / policy / adoption ops | Web-first (ANPM View, generated tables, future `/policy` manifest diff) |
| Protocol authoring & cross-planet debug | Desktop embassy (CIDE/Glass) optional |
| Agents | MCPlane observe — not full desktop |

---

## 5. Recommended stack (working)

Do **not** choose A OR B OR C. Stack them:

```text
┌─────────────────────────────────────────────────────────┐
│  L3  Human: Platform Cockpit (CIDE/Glass) — optional    │
│      power shell for operators who live in the IDE      │
├─────────────────────────────────────────────────────────┤
│  L2  Human: Web fleet desk (ANPM, alliance, registry)   │
├─────────────────────────────────────────────────────────┤
│  L1  Machine: manifests, conformance CI, adoption drift │
│      pain → ADR, coding SSOT, MCPlane for agents        │
├─────────────────────────────────────────────────────────┤
│  L0  Protocol: NuGet hyperlanes, planets sovereign      │
└─────────────────────────────────────────────────────────┘
```

**L1 is non-negotiable for fleet-first.** L2/L3 are product bets — ship when L1 invariants hurt without UI.

Product integration (A) stays **at planet boundary** via embed + hyperlane pin, not federation merge.

---

## 6. Tensions to keep explicit

| Tension | Resolution |
|---------|------------|
| Fast AI throughput vs invariants | Pain inventory + conformance gates + wave roadmap |
| Desktop power vs federation openness | Cockpit = embassy; protocols work without it |
| Operator UX vs nuget.org UX | ANPM / local manifest SSOT; nuget.org is upstream, not control plane |
| Agent throughput vs human clarity | MCPlane pulse default; full detail on demand |

---

## 7. Open questions

1. **Name:** Platform Cockpit vs **Attention** desk (ADR-0001 wave) — fleet operator home?  
2. **ANPM:** L3 registry only, or also **fleet IDE** host?  
3. **Minimum viable L2:** adoption table + TP manifest diff + conformance pin report — enough?  
4. **Monetization / audience:** internal federation only vs product for other platform teams?

---

## 8. Related

- [GUIDERS-FEDERATION-CONSTITUTION.md](GUIDERS-FEDERATION-CONSTITUTION.md) — planets sovereign; roads not citizenship  
- [GUIDERS-ROADMAP.md](GUIDERS-ROADMAP.md) — implementation waves  
- [GUIDERS-pain-inventory.md](GUIDERS-pain-inventory.md) — management friction log  
- [ANPM pain inventory](https://github.com/AI-Guiders/agent-nuget-pm/blob/main/docs/ANPM-pain-inventory.md) — registry fleet pains (N-xxx)  
- `AIGuiders.Platform.Cockpit.*` — headless contracts; UI lives on planets  

**Next:** promote sections into `GUIDERS-ADR-0023` when L1 primitives (registry manifest, conformance extract) have a tagged slice.
