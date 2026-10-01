# GUIDERS-ADR-0068: Everything-as-Code on GDL rails

| | |
|---|---|
| **Status** | **Accepted** (platform charter; quarries ship incrementally) |
| **Date** | 2026-10-01 |
| **Tags** | #guiders #federation #gdl #eac #dashspec #forge #policy #declare #emit |
| **Related** | [0059](./GUIDERS-ADR-0059-gdl-hyperlane.md) · [0064](./GUIDERS-ADR-0064-config-gdl-quarry-family.md) · [0065](./GUIDERS-ADR-0065-gdl-emit-operational-paths.md) · [0067](./GUIDERS-ADR-0067-language-profile-federation-model.md) · [0031](./GUIDERS-ADR-0031-policy-as-readable-code-overlay-profiles.md) · [0032](./GUIDERS-ADR-0032-conformance-obligations-policy-specs.md) · [0006](./GUIDERS-ADR-0006-confederation-charter.md) · [DASHSPEC-ADR-0032](https://github.com/AI-Guiders/dash-spec/blob/develop/design/DASHSPEC-ADR-0032-extension-blocks-and-plugins.md) · [DASHSPEC-ADR-0071](https://github.com/AI-Guiders/dash-spec/blob/develop/design/DASHSPEC-ADR-0071-block-and-member-grammar.md) · [DASHSPEC-ADR-0051](https://github.com/AI-Guiders/dash-spec/blob/develop/design/DASHSPEC-ADR-0051-language-affinity-modeling-execution.md) |

## Context

Operators want **Everything-as-Code (EaC)** across federation products: DashSpec (dashboards), Forge (repos, CI, rights), planet apps (LUS, …) — versioned declarations, review, diff, automated enforce — not ad-hoc UI state or duplicated constants.

We already have pieces:

| Rail | Role today |
|------|------------|
| **GDL** ([0059](./GUIDERS-ADR-0059-gdl-hyperlane.md)) | Declare-time language: `*.catalog.gdl`, config quarries, `import`, **emit → generated artifacts**, drift gates |
| **DashSpec surface** | `.dashspec` / `.dashlayout` — human authoring; F# **Modeling.Parse** spine ([0071](https://github.com/AI-Guiders/dash-spec/blob/develop/design/DASHSPEC-ADR-0071-block-and-member-grammar.md)) |
| **Forge** | Git host, MR, CI, org — **delivery and governance** for planet repos |
| **Extension plugins** ([DASHSPEC-0032](https://github.com/AI-Guiders/dash-spec/blob/develop/design/DASHSPEC-ADR-0032-extension-blocks-and-plugins.md)) | Product blocks on schema-driven generic parse — no per-plugin lexer |

**Observation (design health):** platform repos **enrich each other** when boundaries stay clear — shared modeling packages, ADR cross-links, embassy patterns, conformance vectors. That mutual lift is a **positive signal** (good federation style), not accidental coupling. This ADR names EaC on **GDL as the common declare rail** while preserving **domain vocabularies** per product.

**Non-goals:** one mega-DSL file; runtime interpretation of full GDL on every planet ([0059](./GUIDERS-ADR-0059-gdl-hyperlane.md) emit path); replacing Forge/API enforcement with spec comments alone.

---

## Decision

### 1. EaC pipeline (normative)

All federation EaC **SHOULD** follow:

```text
DECLARE  →  EMIT (optional codegen)  →  RESOLVE  →  ENFORCE
   │              │                        │            │
 GDL quarry    gdlc / planet tool      effective      Host API,
 or approved   + Language Profile      bundle         Forge CI,
 surface file  ([0067](./GUIDERS-ADR-0067-language-profile-federation-model.md))              drift gate       policy engine
```

- **DECLARE:** git-tracked text; PR review.
- **EMIT:** build-time or CI-time; **no silent hand-duplication** of emitted constants ([0065](./GUIDERS-ADR-0065-gdl-emit-operational-paths.md)).
- **RESOLVE:** includes, manifests, overlay merge ([0031](./GUIDERS-ADR-0031-policy-as-readable-code-overlay-profiles.md) where applicable).
- **ENFORCE:** server-side for security; CI for shape/drift; Host for UX gates (`when`, entitlements).

### 2. GDL = declare rail; products = quarries / profiles

| Product / concern | Vocabulary (examples) | Artifact family | GDL status |
|-------------------|----------------------|-----------------|------------|
| Federation catalog | commands, surfaces, slots | `*.catalog.gdl` | **Shipped** quarry |
| Platform config | wiring, tiers, contracts | `*.config.gdl` ([0064](./GUIDERS-ADR-0064-config-gdl-quarry-family.md)) | In progress |
| **Forge** | `repository`, `org`, **rights**, **ci**, MR policy | `*.forge.gdl` (name TBD) | **Proposed** quarry — Forge-native terms |
| **DashSpec** | `card`, `filter`, `diagram`, `layout`, entitlements | `.dashspec`, `.dashlayout`, … | **Surface today**; full GDL quarry **deferred** (merge post-Studio); spine shares **Language Profile** `dashspec.block` ([0067](./GUIDERS-ADR-0067-language-profile-federation-model.md)) |
| Planet policy | agent schedule, whitelist, … | TOML / GDL overlay | Transport vs grammar per [0048](./GUIDERS-ADR-0048-authoring-quarry-family.md) |

**Rule:** Forge **does not** express heatmaps; DashSpec **does not** express `merge_request` ACL. **Cross-link** by stable ids (`repo`, `report_id`, `catalog` entry), not by merging grammars.

### 3. Two layers of “rights” (do not conflate)

| Layer | Question | Typical owner |
|-------|----------|----------------|
| **Content entitlement** | Who may *see/run* this report tab or export? | DashSpec declaration or `*.dashpolicy` quarry (future) |
| **Delivery entitlement** | Who may *merge/deploy* spec to an environment? | **Forge** `rights` + CI required checks |

Both are EaC; **enforcement** for content **MUST** remain on Host/API; Forge governs **artifact lifecycle**.

### 4. Cross-enrichment patterns (encouraged)

Federation **SHOULD** prefer these over copy-paste parsers:

| Pattern | Example |
|---------|---------|
| **Shared modeling hyperlane** | `AIGuiders.Platform.Modeling.*` ← dash-spec parse; paths, block close, future Language Profile nodes |
| **Parse spine reuse** | MemberGrammar / BlockGrammar ([DASHSPEC-0071](https://github.com/AI-Guiders/dash-spec/blob/develop/design/DASHSPEC-ADR-0071-block-and-member-grammar.md)) — schema members, bracket rows, child keywords; applicable to new quarries |
| **Embassy consumer** | Forge runs CI on planet repos; planet does not reimplement git host |
| **Extension blocks** | Thin Core + DLL schema ([DASHSPEC-0032](https://github.com/AI-Guiders/dash-spec/blob/develop/design/DASHSPEC-ADR-0032-extension-blocks-and-plugins.md)) — parallel to GDL quarry + emit, not custom lexer |
| **ADR signage** | Platform ADR ↔ planet ADR links (this doc ↔ 0059 ↔ 0071) |
| **Conformance vectors** | Slash/spec fixtures shared across repos ([0018](./GUIDERS-ADR-0018-slash-conformance-vectors.md), planet test suites) |

**Anti-pattern:** planet forks lexical core without ADR; runtime-only GDL parse without emit drift gate; security rules only in UI `when` clauses.

### 5. Lexical alignment note (DashSpec ↔ GDL)

GDL v0 core ([0059](./GUIDERS-ADR-0059-gdl-hyperlane.md)) prefers `keyword … end keyword` **without `{ }`**. DashSpec ([ADR-0036](https://github.com/AI-Guiders/dash-spec/blob/develop/design/DASHSPEC-ADR-0036-end-blocks-page-toolbar.md)) supports **dual** brace / end-keyword close. **EaC does not require immediate syntax unification** — Language Profile documents the fork; long-term merge is a quarry migration, not a blocker for Forge/policy quarries on GDL.

### 6. CI / Forge integration (intent)

For a planet repo that ships DashSpec + GDL catalog:

1. **gdlc emit** + drift check (constants, command ids).
2. **DashSpec lint** / parse tests (Core.Tests, LUS fixtures).
3. **Forge CI** policy (who can merge, required jobs).
4. Optional: **policy quarry** lint when `*.forge.gdl` / rights blocks exist.

Single MR may carry multiple artifact families; **each** runs its own enforce step.

---

## Consequences

- New product-specific declare surfaces **SHOULD** start as **GDL quarries** (or Language Profiles) when they need emit/SSOT; ad-hoc surface files remain valid until migration cost is justified.
- **Forge rights / CI** vocabulary lives in **Forge quarry**, not in `.dashspec`.
- Report entitlements and dashboard behavior stay in **DashSpec family** (or dashpolicy quarry), enforced at Host.
- Platform modeling investments (block/member grammar, Modeling.Parse split per [DASHSPEC-0051](https://github.com/AI-Guiders/dash-spec/blob/develop/design/DASHSPEC-ADR-0051-language-affinity-modeling-execution.md)) **benefit all quarries** that share schema-driven members.
- Document **cross-enrichment** in ADR “Related” sections when a change spans repos — treat it as normal federation hygiene.

## Open items

| Item | Owner |
|------|--------|
| Name and grammar sketch for `*.forge.gdl` (rights, ci, repository) | Forge + platform |
| `dashpolicy` / authorization quarry vs DashSpec inline blocks | dash-spec ADR |
| Drift gate template in Forge CI for gdlc + dashspec | planet template repos |
| Language Profile `dashspec.block` ↔ F# BlockGrammar parity checklist | guiders-fsharp + dash-spec |
