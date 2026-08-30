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

Desktop vs web is **host choice**, not the model. Glass/WPF, Avalonia CIDE, ANPM Human View, or agent MCPlane pulse can all project the **same attention scopes** with different renderers.

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

## 6. Delivery hosts (secondary — same model)

How scopes are **rendered** is a product decision; scopes are architectural.

| Host | Fits |
|------|------|
| **CIDE / Glass** (desktop embassy) | Full attention stack; fleet scope as formation display + planet zoom |
| **ANPM / web Attention desk** | Live + Fleet registry slices; lighter MFD |
| **MCPlane** (agents) | Pulse per scope; `next[]` crosses Fleet → Planet → Live |
| **Embed / hyperlane** | Planet products stay sovereign; fleet view is optional contour |

Seamless product integration (Forge inside CIDE, etc.) remains **planet choice** — not federation law. Fleet attention can **observe** integrations without owning product domains.

---

## 7. Recommended build order

```text
L1  Machine telemetry — manifests, conformance CI, adoption drift, pain→ADR
L2  Attention scopes  — Fleet / Planet / Live data model on Cockpit.* channels
L3  Human hosts       — web desk (Live+Fleet), desktop embassy (all scopes)
L0  Protocol hyperlanes — planets sovereign
```

**L1** is non-negotiable. **L2** is the golden vein: **reuse CDS, new feeds**. **L3** is where CIDE/Glass or ANPM View plug in.

---

## 8. Tensions to keep explicit

| Tension | Resolution |
|---------|------------|
| Fast AI throughput vs invariants | Pain inventory + conformance gates + wave roadmap |
| Desktop power vs federation openness | Cockpit = embassy; protocols work without it |
| Operator UX vs nuget.org UX | ANPM / local manifest SSOT; nuget.org is upstream, not control plane |
| Agent throughput vs human clarity | MCPlane pulse default; full detail on demand |

---

## 9. Open questions

1. **Scope switch UX:** one Attention desk with Fleet/Planet/Live tabs, or CDS contour keys (like CIDE today)?  
2. **Fleet PFD default:** roadmap wave vs alliance drift vs release train?  
3. **Live scope owner:** ANPM as primary host, or Cockpit channel in CIDE status bar?  
4. **Agent parity:** MCPlane `next[]` across scopes — one envelope or scoped pulse per contour?

---

## 10. Related

- [GUIDERS-ADR-0007](adr/GUIDERS-ADR-0007-aviation-mental-model.md) — phases, displays, CDS, EICAS  
- [GUIDERS-ADR-0001](adr/GUIDERS-ADR-0001-platform-boundary.md) — Desk → Attention, Cockpit contracts  
- [GUIDERS-FEDERATION-CONSTITUTION.md](GUIDERS-FEDERATION-CONSTITUTION.md) — planets sovereign  
- [GUIDERS-ROADMAP.md](GUIDERS-ROADMAP.md) · [GUIDERS-pain-inventory.md](GUIDERS-pain-inventory.md)  
- [ANPM pain inventory](https://github.com/AI-Guiders/agent-nuget-pm/blob/main/docs/ANPM-pain-inventory.md) — Live/Ecosystem pains (N-xxx)  

**Next:** `GUIDERS-ADR-0023` — normative scope model + Cockpit channel IDs for Fleet/Planet/Live telemetry.
