# GUIDERS-VISION-0001: Plugin Host hyperlane (discussion draft)

| | |
|---|---|
| **Status** | **Vision · discussion** (not Accepted ADR yet) |
| **Date** | 2026-08-25 |
| **Authors** | operator + agent (Composer session, Forge ldap-org / AIGuiders preload arc) |
| **Supersedes** | chat-only proposals — keep this file when context compacts |
| **Target ADRs** | GUIDERS-ADR-0008 (platform boundary) · FORGE-ADR-0060 (Forge dogfood) |
| **Related** | GUIDERS-ADR-0003 · GUIDERS-ADR-0006 · FORGE-ADR-0019 · FORGE-ADR-0023 · ANPM-ADR-0001 |

> **Clarification (2026-08-25):** PluginHost is **not** `guiders-platform` **code**. Platform = slash/cockpit headless mechanics. PluginHost = **fourth hyperlane** (sibling repo, recommended). Vision **P2** = ADR + conformance **charter** (markdown may live in platform repo) — **not** «implement loader inside `AIGuiders.Platform.*`».

---

## 1. North star

Turn the **Forge plugin model** (already richer than the original plan) into a **confederation hyperlane** — optional federation protocol, not a Forge monopoly.

**One sentence:** shared **load / stage / verify** mechanics for modular .NET hosts; each **planet** keeps product law (Forge IOP/MCP, CDP buffer plane, ANPM feeds).

**Why now:** UI kit (`AIGuiders.UI.*`) and slash (`CommandPlane`) are already ecosystem packages. **Plugin runtime** (ALC, shared deps, staging) still lives only inside `AgentForge` → bugs like «DLL on disk but not in Default ALC» get fixed in Forge-only code until we extract contracts.

**Operator goal:** fix loader/staging class of bugs **once** in PluginHost; Forge, ANPM, future hosts consume semver packages + conformance tests.

---

## 2. What PluginHost IS / IS NOT

| PluginHost hyperlane | Product planets (examples) |
|----------------------|----------------------------|
| Collectible / default ALC policy | `IForgePlugin`, bundles, capabilities |
| `host-runtime.manifest.json` bootstrap | `forge.plugins.toml`, ldap-org |
| MSBuild: runtime assets → manifest | Plugin MCP tools, WitDB migrations |
| `pkg/manifest.toml` layout + schema | Human View, slash execute handlers |
| Load-time: duplicate id, ABI gate | Marketplace UI, billing |
| Conformance test harness | Domain analyzers beyond generic rules |

**Explicit non-goals (GUIDERS-ADR-0003 pattern):**

- Not MEF / not a second plugin discovery framework for *product* extension points
- Not Forge MCP executor
- Not replacing `IForgePlugin` with a single global interface for all products
- Not mandatory for every confederation planet

---

## 3. Where it lives: Platform vs PluginHost vs Core

**Short answer:** PluginHost is **not** `guiders-platform`. It is a **fourth hyperlane**, same class as `guiders-ui-platform`.

| Repo / family | Owns | Does **not** own |
|---------------|------|------------------|
| **`guiders-platform`** | `CommandPlane`, routing, cockpit — **headless mechanics** | ALC, `plugins/`, dll staging, Forge loader |
| **`guiders-ui-platform`** | `UI.Core`, Tokens, HTMX adapter | Module load lifecycle |
| **`guiders-core`** | CDP organs, MCP libs, package intelligence | Forge bundles, generic plugin OS (unless we explicitly choose core as *repo home* — still **not** Platform) |
| **`guiders-plugin-host`** *(recommended)* | `AIGuiders.PluginHost.*` | `IForgePlugin`, MCP, WitDB |
| **`agent-forge`** | `IForgePlugin`, bundles, FORGE analyzers | Shared ALC (after P1 extract) |
| **`agent-nuget-pm`** | Feed, pins, verify MCP | Loading dll in a host process |

GUIDERS-ADR-0003 **Non-goals:** *Forge MCP executor / plugin host in platform* — still law.

### Why not `AIGuiders.Platform.PluginHost`?

1. Different blast radius — Platform patch breaks slash everywhere; PluginHost breaks modular hosts only.
2. ANPM needs pkg schema without Cockpit.DataBus.
3. Non-annexation (ADR-0006) — Platform already large; loader is another domain.
4. NuGet Trusted Publishing: `AIGuiders.Platform.*` vs `AIGuiders.PluginHost.*` scopes stay clean.

### What *does* sit in `guiders-platform` for PluginHost?

**Governance only** (Vision P2): `GUIDERS-ADR-0008` markdown, conformance checklist template, federation table links. **No** runtime packages under `AIGuiders.Platform.*` in v1.

### Repo home (Q1 — sibling vs core)

| Option | Verdict |
|--------|---------|
| **A. Sibling `guiders-plugin-host`** | **Recommended** — mirrors ui-platform |
| **B. Folder in `guiders-core`** | Acceptable if we want fewer repos |
| **C. Inside `guiders-platform`** | **Rejected** — ADR-0003 non-goals |

---

## 4. Package family (target)

Sibling monorepo candidate: **`guiders-plugin-host`** (name TBD) or packages under **`guiders-core`** — decision in GUIDERS-ADR-0008.

| Package | Responsibility |
|---------|----------------|
| `AIGuiders.PluginHost.Abstractions` | Module descriptor, manifest schema, ABI version, `depends_on`, load-context hints |
| `AIGuiders.PluginHost.Runtime` | ALC, shared-with-host resolver, bootstrap preload, topo sort, safe type probe |
| `AIGuiders.PluginHost.Build` | MSBuild targets (evolve `ForgePlugin.Build.targets`) → `host-runtime.manifest.json` |
| `AIGuiders.PluginHost.Analyzers` | Generic rules (refs, routes if shared); product-specific rules stay on planet |

Forge keeps **`AgentForge.Plugin.Sdk`** + **`AgentForge.PluginAnalyzers`** as **Forge citizenship** layer atop generic analyzers (like FORGE00x today).

---

## 5. Confederation fit (GUIDERS-ADR-0006)

| Concept | PluginHost mapping |
|---------|-------------------|
| **Planet** | Forge, ANPM, CDP, … sovereign repos |
| **Hyperlane** | `AIGuiders.PluginHost.*` NuGet, manifest schema, conformance CI |
| **Embassy** | **Forge** = first reference host (dogfood) |
| **Second consumer** | Required before declaring hyperlane **stable** — candidate: **ANPM verify** and/or CDP extension slot |
| **Prime protocol** | Do not move IOP/MCP/WitDB into PluginHost «because Forge needs it» |

Federation services table (extended):

| Service | Home | Role |
|---------|------|------|
| Platform mechanics | `guiders-platform` | Slash, routing, cockpit |
| Human + Agent UX | `guiders-ui-platform` | UI Core, Tokens, adapters |
| **Plugin transport** | **`guiders-plugin-host` (new)** | ALC, staging, pkg layout |
| Offline distribution | `agent-nuget-pm` | Feed, pins, sync — **not** loader |
| Reference modular host | `agent-forge` | `IForgePlugin` product law |

---

## 6. ANPM synergy (distribution, not duplication)

ANPM (ANPM-ADR-0001) stays **consumer-neutral**: flat feed, manifest pins, MCP verify/sync.

PluginHost defines **what a valid plugin package looks like on disk**. ANPM defines **how packages arrive on the feed**.

```text
Inet / nuget.org ──ANPM sync──► *.nupkg on feed
                                    │
                    extract / layout verify (ANPM tool)
                                    │
                    PluginHost.Runtime (on each host at startup)
```

Planned ANPM tools (vision, not committed scope):

| Tool | Role |
|------|------|
| `anpm.plugin.verify` | manifest.toml + lib/ layout + forge_abi / host_runtime refs |
| existing `anpm.restore.verify` | solution restore |
| existing pin manifests | add `pluginhost-*.pins.json` alongside platform pins |

ANPM **M2 «Forge plugin»** (if any) = ops wiring (feed URL in instance settings), **not** a second loader implementation.

---

## 7. Phased roadmap — **our numbering** (do not confuse)

This vision uses **PluginHost phases P0–P4**. They are **not** the same as:

| Other registry | What it means |
|----------------|---------------|
| FORGE-ADR-0015 **Phase 2** | Command bar / slash modal in ViewShell |
| GUIDERS-ADR-0003 **Wave W2** | CommandPlane package quarry |
| ANPM **M1 / M2** | MCP feed tools vs optional Forge mount |
| FORGE-ADR-0016 CI phases | Visual CI designer waves |

### P0 — Dogfood contract in Forge (no new repo yet)

**Ship in `agent-forge` only.**

- FORGE-ADR-0060: Host Runtime Contract
- `host-runtime.manifest.json` generated at publish
- `ForgeHostRuntimeBootstrap` before bundle plugin loop (order-independent)
- Permutation integration test for `ldap-org` (and one showcase bundle)
- Fixes DPC-class bugs without waiting for extract

**Exit:** Forge 0.4.14+ green; manifest + bootstrap documented; tests in Forge CI.

### P1 — Extract Runtime + Build to NuGet

- New sibling repo (or `guiders-core` folder) publishes `AIGuiders.PluginHost.Runtime` + `.Build` 0.1.0
- Forge `ForgePluginLoader` becomes thin adapter over PluginHost.Runtime
- `ForgePlugin.Build.targets` → import PluginHost.Build (Forge-specific items remain Forge csproj)

**Exit:** Forge references NuGet (or sibling) instead of inlined ALC policy; DPC scenario still green.

### P2 — Federation charter + conformance **template** (not «Platform code»)

**This is governance and test SSOT, not product features — and not moving loader into `guiders-platform` packages.**

- GUIDERS-ADR-0008 accepted: boundary, packages, non-goals, membership rules
- Conformance checklist **markdown** in `guiders-platform/docs/` (charter vNext item #1) — **docs only**:
  - manifest schema snapshot tests
  - bundle order permutation fixture
  - «shared dep must be in host runtime manifest» negative test
- Cross-link from GUIDERS-ADR-0006 federation table

**P2 does NOT mean:**

- ❌ ANPM production rollout to all CAD hosts
- ❌ Marketplace install UI
- ❌ Second planet fully shipping plugins in production
- ❌ Deleting Forge loader code without P1 complete
- ❌ New `AIGuiders.Platform.PluginHost` or loader code inside `guiders-platform` src/

**P2 DOES mean:**

- ✅ Written contract other planets can opt into
- ✅ Shared test package or `PluginHost.Conformance` NuGet consumed by Forge CI
- ✅ Clear «how to join the hyperlane» for ANPM/CDP teams

### P3 — Second consumer (ANPM verify path)

- `anpm.plugin.verify` (or equivalent) validates package layout against PluginHost schema
- Pin manifest slice for PluginHost packages on offline feeds
- Optional: stage `AIGuiders.PluginHost.*` + Forge plugin nupkgs on ANPM feed for air-gap

**Exit:** two planets green in CI on same schema (Forge load + ANPM verify).

### P4 — Marketplace alignment (FORGE-ADR-0019)

- Registry `pkg/manifest.toml` = PluginHost schema (zip primary)
- `dependencies[]` / `host_runtime[]` in registry metadata
- Commercial zoo repos publish through same layout (ADR-0020)

---

## 8. «Fix bugs once» — three anchors

1. **Build SSOT:** `host-runtime.manifest.json` aggregated from all modules in a bundle/build — no hand-maintained AIGuiders list in loader.
2. **Runtime SSOT:** bootstrap always runs before any module load; bundle order irrelevant for shared deps.
3. **CI SSOT:** permutation + conformance package; regression fails in PluginHost repo first, Forge/ANPM as consumers.

---

## 9. Open questions (discussion)

| # | Question | Options |
|---|----------|---------|
| Q1 | Repo home: `guiders-plugin-host` vs folder in `guiders-core`? | Sibling (UI pattern) vs core organs |
| Q2 | Is ANPM the right **second consumer**, or CDP extension host first? | ANPM = verify-only is lighter; CDP = real load |
| Q3 | Generic module interface name: `IPluginModule` vs keep Forge-only `IForgePlugin` forever? | Bridge interface in Abstractions vs Forge-specific forever |
| Q4 | Analyzers: move all FORGE00x to PluginHost or split generic/product? | Split recommended |
| Q5 | P2 before or after P1 NuGet publish? | **Recommended:** P0 → P1 → P2 (charter on real packages); **alt:** P0+P2 charter in parallel |
| Q6 | McMaster.NETCore.Plugins inside Runtime vs own ALC code? | Wrap/adopt vs keep Forge ALC (dogfood first) |

---

## 10. Immediate next actions (when vision accepted)

1. Accept or revise this vision (operator sign-off)
2. Draft FORGE-ADR-0060 + implement P0 in Forge
3. Draft GUIDERS-ADR-0008 from sections 2–4 + P2 scope
4. Update GUIDERS-ADR-0006 federation table with PluginHost row

---

## 11. Changelog

| Date | Change |
|------|--------|
| 2026-08-25 | Initial vision draft from Forge 0.4.13 ldap-org / AIGuiders preload arc + confederation discussion |
| 2026-08-25 | §3 Platform vs PluginHost; P2 ≠ Platform packages (operator clarification) |
