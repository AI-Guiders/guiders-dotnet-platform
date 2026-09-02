# GUIDERS-ADR-0006: Confederation charter (sovereign repos, federated contracts)

**Status:** accepted (2026-08-25)  
**Tags:** #guiders #platform #open #federation #architecture #charter  
**Related:** GUIDERS-ADR-0001 · GUIDERS-ADR-0004 · GUIDERS-ADR-0005 · GUIDERS-UI-0003

---

## Context

Open Source often feels like **hard-to-integrate islands** — each project is a self-contained world with its own config, UI kit, CLI, and implicit claim to be the center. Integration becomes archaeology and diplomacy between hostile poleis, not engineering.

AI Guiders already chose **sibling monorepos** and **headless platform** (GUIDERS-ADR-0001/0004/0005): products keep their repos; shared semantics ship as packages and ADR. That is closer to a **confederation of planets** than a single city or empire:

- Each **planet** (Forge, CDP, Glass, guiders-ui-platform, agent-notes, …) is **sovereign** — own domain, own release cadence, own atmosphere.
- The **federation** provides **protocols** — not cultural unification, not annexation.
- **Reference missions** (e.g. Forge consuming `AIGuiders.UI.*`) prove the hyperlanes work; they do not make Forge the capital.

Operator fatigue target: **roads, signage, and transport** between worlds — not merging worlds into one polis.

---

## Decision

### 1. Confederation model (canonical metaphor)

| Concept | Meaning in AI Guiders |
|---------|------------------------|
| **Planet** | Product or sibling monorepo with sovereign boundary |
| **Federation** | Shared charters, contracts, conformance — cross-repo, no single owner-repo |
| **Prime protocol** | Do not break a planet's domain for integrator convenience (non-annexation) |
| **Hyperlane** | Versioned package, MCP tool surface, CI conformance path |
| **Signage** | ADR, Core schemas, Agent AX manifests, stable `testId`s |
| **Embassy / reference consumer** | Product that demonstrates integration without owning shared SSOT |
| **Council** | Platform ADRs + open Core; products opt in via adapter + tests |

**Rejected metaphors:** single city (implies one center), empire (annexation), monolith (one repo to rule them all).

### 2. Sovereignty rules

1. **Products keep repos** — GUIDERS-ADR-0001 stands; confederation does not reverse it.
2. **Domain stays on the planet** — Forge IOP/diff, CDP buffer plane, Glass projection, KB personal layer — not lifted into platform «for convenience».
3. **Shared kit is protocol, not colony** — `AIGuiders.Platform.*`, `AIGuiders.UI.Core`, tokens, routing envelopes — federated contracts, semver, independent CI.
4. **Native per ecosystem** — adapters (Razor, future React/npm, Python, Qt) implement Core locally; **no bindings** that make one planet's runtime the only gateway (GUIDERS-UI-0003).

### 3. Membership (how a planet joins)

Voluntary, testable:

1. Adopt relevant **Core contract** (package or schema).
2. Ship a **native adapter** in the planet's stack (or consume a sibling adapter repo).
3. Pass **conformance** — journey/contract tests, token lint, AX id stability where applicable.
4. Document **wiring** in product ADR; platform ADR references, does not duplicate domain.

No requirement to merge repos or adopt a single UI framework.

### 4. Federation services (what we build together)

| Service | Current home | Role |
|---------|--------------|------|
| Platform mechanics | `guiders-platform` | Intent, routing, cockpit contracts |
| Backend cores | `guiders-core` (sibling) | Shared organs, MCP libs |
| Human + Agent UX semantics | `guiders-ui-platform` | Core, Tokens, adapters |
| **Plugin transport** | **`guiders-plugin-host`** | ALC, staging, `host-runtime.manifest.json` ([GUIDERS-ADR-0008](GUIDERS-ADR-0008-plugin-host-hyperlane.md)) |
| Reference modular host | `agent-forge` | `IForgePlugin` embassy |
| Agent habitat | `cdp-mcp` | MCP/CDP tools, buffer plane |
| Knowledge continuity | `agent-notes` | Operator KB, line/incarnation canon |

New planets add rows; federation does not absorb the planet.

### 5. Non-annexation (Prime protocol)

**Forbidden:**

- Moving product-specific domain into platform «because it's used twice» without a Core extraction ADR.
- Forcing one planet's markup/runtime as the only agent path (HTML string from .NET as universal bridge).
- README «easy integrate» without contract, version pin, and a reference route.
- Identity overwrite — one product claiming to be the UI/command SSOT for all (GUIDERS-ADR-0005: Forge is reference, not owner).

**Required:**

- Semver on public contracts; breaking change = major + migration note.
- ADR for new hyperlane or changed boundary.
- Second consumer before declaring a contract «stable» (UI platform vNext).

### 6. Open Core as citizenship rights

Conformance is not paywalled:

- Baseline human a11y (tokens, contrast path) and Agent AX exposure (ids, semantics) — open Core/Tokens (GUIDERS-UI-0003).
- Federation membership ≠ proprietary lock-in; products may fork adapters, not Core semantics silently.

---

## Non-goals

- Political governance body or trademark enforcement beyond existing org/repos.
- Single deployable «Guiders OS» image containing all planets.
- Replacing product ADRs with one mega-charter (this ADR frames; product ADRs remain local law).
- Resolving all OSS fragmentation industry-wide — scope is AI Guiders confederation only.

---

## Consequences

- README and onboarding should describe **planets + protocols**, not «the platform repo is the app».
- New shared extraction → sibling monorepo or package family + charter ADR (pattern: GUIDERS-ADR-0005).
- Integration reviews ask: **hyperlane exists? signage? reference mission? annexation risk?**
- **Federation hub for humans:** [GUIDERS-FEDERATION-CONSTITUTION.md](../GUIDERS-FEDERATION-CONSTITUTION.md) (this ADR remains normative charter).
- Operator/agent KB may use federation vocabulary alongside line/incarnation canon (agent-notes, personal layer).

---

## vNext (ordered)

1. Conformance checklist template in `guiders-platform` (platform + UI slices).
2. Second UI consumer (CDP/cockpit or other) — closes «stable hyperlane» for `AIGuiders.UI.*`.
3. JSON Schema export from `AIGuiders.UI.Core` — signage for non-.NET planets.
4. Cross-repo index doc: planet → packages → ADR entry points (one page, links only).
