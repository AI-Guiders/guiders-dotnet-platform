# GUIDERS-ADR-0064: Config GDL quarry (declare · wire · contract · Sat)

| | |
|---|---|
| **Status** | **Accepted** (quarry + contract model — implementation **In progress**) |
| **Date** | 2026-09-03 |
| **Tags** | #guiders #federation #authoring #gdl #config #paths #conformance #ai-era |
| **Related** | [0047](./GUIDERS-ADR-0047-command-for-doi.md) · [0048](./GUIDERS-ADR-0048-authoring-quarry-family.md) · [0050](./GUIDERS-ADR-0050-paths-guild-logical-physical.md) · [0059](./GUIDERS-ADR-0059-gdl-hyperlane.md) · [0028](./GUIDERS-ADR-0028-documentation-guild-correspondence-family.md) · [GUIDERS-FSHARP-ADR-0002](https://github.com/AI-Guiders/guiders-fsharp/blob/main/docs/adr/GUIDERS-FSHARP-ADR-0002-model-guild-fsharp-ownership.md) · KB [008](https://github.com/AI-Guiders/kb/blob/main/knowledge/adr/008-workspace-scope-map-hot-mcp-and-public-cut.md) |

## Context

Federation already separates **authoring** (GDL declare) from **wire transport** (TOML env drops, runtime read) ([0048](./GUIDERS-ADR-0048-authoring-quarry-family.md) §2, [0059](./GUIDERS-ADR-0059-gdl-hyperlane.md) §7).

AI-era stacks (CDP, agent-notes MCP, installers) need more than flat TOML:

1. **Wiring** — where roots, hot files, and scope maps live (operator-edited).
2. **Interpretation** — how to read paths, section slices, and root aliases (author-declared **notation**, not guesswork).
3. **Contracts** — what must hold after install or open (`requires` / `ensures`), with **evidence** (`facts`), not «наверное ок».
4. **Self-check** — runtime or CI observers that evaluate `Sat(Q)` and return actionable gaps.

Today: `agent-notes-mcp.toml`, `workspace-scope-map-v1.md`, and `agent-notes.md` are **transport**; semantics live in playbooks and agent memory. A broken newcomer install (empty personal L0, template scope-map) passes TOML parse but fails operationally — no declared postcondition.

The **`config`** GDL quarry addresses **environment wiring, wire interpretation, and verifiable postconditions** ([0059](./GUIDERS-ADR-0059-gdl-hyperlane.md) intent stack). Operator-facing term: **configuration** or **setup profile** (`*.config.gdl`).

## Decision

### 1. New GDL quarry: `config`

| Item | Norm |
|------|------|
| Quarry token | `config` |
| Canonical file | `*.{name}.config.gdl` (e.g. `cdp-newcomer.config.gdl`) |
| Intent | *How the environment is wired, how wire is interpreted, what must hold* |
| IR (Modeling SSOT) | `ConfigurationPack` graph — sources, defaults, contracts, facts (F# package TBD; see §8) |

Registered in [0059](./GUIDERS-ADR-0059-gdl-hyperlane.md) quarry registry and intent stack:

```text
*.catalog.gdl   — what you can do
*.deck.gdl      — where you look
*.display.gdl   — where it lands physically
*.config.gdl    — how the stack is wired and what must hold (interpretation + Sat)
```

### 2. Three layers (normative)

| Layer | Who writes | Artifact | Role |
|-------|------------|----------|------|
| **L1 Author** | federation / installer / org maintainer | `*.config.gdl`, optional `*.configbundle.gdl` | `defaults`, `sources`, `contracts`, `facts` |
| **L2 Operator** | human / agent on machine | TOML, ini, scope-map lines, md sections | **wire** — interpreted per L1 `defaults` |
| **L0 Derived** | **nobody edits** | `ConfigurationPack` IR | materialize(L1 ∪ L2) → observers |

Operators **do not** author `requires` / `ensures` or grammar ids unless they ship a custom pack. Typical user only edits L2.

### 3. Document shape (v0)

Same Authoring kit as catalog ([0048](./GUIDERS-ADR-0048-authoring-quarry-family.md) §3): `keyword … end keyword`, `* table`, kv sugar, `import` ([0052](./GUIDERS-ADR-0052-unified-import-directive.md)).

| Section | Purpose |
|---------|---------|
| `defaults` … `end defaults` | **Grammar ids** for how L2 wire is read (see §4) |
| `sources table` | Bind IR nodes to files: kind, path template, slice |
| `contracts table` | `id`, `requires`, `ensures` (predicate refs or named checks) |
| `facts table` | `contract` → `verified_by` (Correspondence-aligned evidence) |
| `grammars table` (optional) | Human/agent discoverability in-file (§6) |
| `helps table` (optional) | Same as catalog — short operator hints |

Example (illustrative ids — **must** exist in registry before ship):

```text
config cdp-newcomer

based on adr:GUIDERS-ADR-0064

import <federation/config/paths-grain>

defaults
  grammar.path.physical  = path-absolute-platform
  grammar.path.scope_map = path-arrow-scope-id
  grammar.knowledge.root = root-alias
  grammar.hot.section    = markdown-section-tags
  grammar.hot.public_cut = markdown-marker-public-cut
end defaults

sources table
  | id           | kind              | path                                      | slice            |
  | personal-hot | markdown_sections | {personal}/agent-notes.md                 | above_public_cut |
  | l0-manifest  | json              | {personal}/knowledge/META/memory-architecture-v1.json | key:l0 |
end sources

contracts table
  | id              | requires                         | ensures                                      |
  | hot.personal.l0 | primary_is_personal, public_root | hot_l0_sections_present, read_hot_context_ok |
end contracts

facts table
  | contract        | verified_by                              |
  | hot.personal.l0 | golden:GS-config-hot-l0-newcomer         |
  | hot.personal.l0 | install-cdp.personal-seed@e1417c4        |
end facts
```

L2 operator wire (unchanged ergonomics):

```toml
[knowledge]
primary = "personal"
```

```text
C:/Users/PC/Desktop/Uchoba/ZnaniaMCP => znania
```

### 4. Grammar defaults — notation contract

Follow [0047](./GUIDERS-ADR-0047-command-for-doi.md) keyboard rule:

- Values in `defaults` are **grammar ids** (kebab-case), e.g. `path-absolute-platform`, `path-arrow-scope-id`.
- Grammar ids in `.config.gdl` use the same convention as catalog `defaults` ([`docs/grammar/notation/README.md`](../grammar/notation/README.md)); conformance spec paths live under `docs/conformance/`.
- **One id per slot.** OS-specific profiles use **separate** `*.config.gdl` documents or future `when os = …` blocks ([0059](./GUIDERS-ADR-0059-gdl-hyperlane.md) `Authoring.Expression`).
- Unknown grammar id → **compile error** at `config validate` time.
- Wire mismatch under declared grammar → **`grammar-wire-mismatch`** diagnostic.

Keys use namespace prefix `grammar.<domain>.<slot>` (parallel to `grammar.keyboard.binding` in catalog).

Path kinds align with [0050](./GUIDERS-ADR-0050-paths-guild-logical-physical.md): wire path ≠ logical ≠ physical; config grammars declare which **path kind** a surface uses.

### 5. Wire vs IR

| Layer | Owns |
|-------|------|
| `*.toml`, `*.ini`, scope-map lines, md sections | operator **wire** |
| `grammar.*` in `defaults` | which reader parses wire |
| `Authoring.Config` + `ConfigGrammarRegistry` | parse `.config.gdl` → `ConfigurationPack` |
| `Platform.Modeling.Config` (F#) | **SSOT IR** types |
| Observers (`config check`, `memory_health`, install hook) | `Sat(ensures)` |

**Strings do not cross the materialize boundary into contract semantics** — same rule as presentation topology ([0058](./GUIDERS-ADR-0058-presentation-topology-ir.md) §1).

### 6. Grammar discoverability

Grammar ids are listed in registry and docs — v0 requires:

| Mechanism | Location / behavior |
|-----------|---------------------|
| **Registry SSOT** | `ConfigGrammarRegistry` (code) — id → reader + path kind |
| **Public catalog** | `docs/grammar/config/README.md` — table: id, wire example, conformance link |
| **Conformance vectors** | `docs/conformance/config/*.spec.json` |
| **Validate CLI / CI** | `config validate <file>` — unknown id, wire mismatch, unresolved `{personal}` |
| **Stdlib import** | `import <federation/config/...>` / `*.configbundle.gdl` — operators use shipped packs, not raw id lists |
| **Optional `grammars table`** | In-document documentation for custom org packs |
| **LSP completion** (later) | Grammar id completion on `defaults` values |

Authors extend the guild by: registry entry + spec vectors + README row.

### 7. Contracts and facts (Correspondence-aligned)

- `requires` — preconditions (roots exist, primary alias, files present).
- `ensures` — postconditions (`hot_l0_sections_present`, `scope_resolves`, …) — implemented as named checks in Execution, not free-form prose in operator files.
- `facts` — evidence rows (`golden:…`, `install-…@commit`, `memory_health@timestamp`) — same **verified_by** spirit as [GUIDERS-FSHARP-ADR-0006](https://github.com/AI-Guiders/guiders-fsharp/blob/main/docs/adr/GUIDERS-FSHARP-ADR-0006-adr-lifecycle-verifiable-facts.md) and [0028](./GUIDERS-ADR-0028-documentation-guild-correspondence-family.md).

Installers **should** run `Sat(ensures)` for the applied pack before reporting success (pilot: CDP `Install-Cdp` + `hot.personal.l0`).

### 8. Packages (target)

NuGet prefix follows [FSHARP-0002](https://github.com/AI-Guiders/guiders-fsharp/blob/main/docs/adr/GUIDERS-FSHARP-ADR-0002-model-guild-fsharp-ownership.md):

```text
AIGuiders.Platform.Modeling.Config          F# — ConfigurationPack IR, Sat helpers
AIGuiders.Platform.Authoring.Config       F# — .config.gdl parser, validate
AIGuiders.Platform.Authoring.Config.Bundles F# — federation stdlib packs (cdp-newcomer, …)
AIGuiders.Platform.Execution.Config       C# — materialize, observers, install hooks
```

Planets (CDP, agent-notes-mcp) **consume** IR; do not fork parallel DTO contract models ([0005](https://github.com/AI-Guiders/guiders-fsharp/blob/main/docs/adr/GUIDERS-FSHARP-ADR-0005-federation-reframe-cdp-features.md) rule 1).

### 9. TOML boundary (normative)

| Use TOML / ini / md wire | Use `*.config.gdl` |
|--------------------------|-------------------|
| Operator paths, primary alias, env drops | Declare interpretation + contracts + facts |
| Runtime read via existing MCP / CDP | CI validate, install gate, `config check` |
| Emitted tier-D from other quarries | Author declares stack profile |

**Rule:** artifacts that need `defaults`, multi-source binding, or `ensures` belong in the **`config`** GDL quarry.

### 10. Pilot vertical slice

| Node | Pack | Observer |
|------|------|----------|
| `hot.personal.l0` | `cdp-newcomer.config.gdl` | `memory_health`, `read_hot_context`, Install-Cdp post-step |

Scope-map binding (`path-arrow-scope-id`) and `scope-*` hot sections are **L2**; contract may add `hot.scope_resolves` in a follow-on slice.

## Migration phases

| Phase | Deliverable |
|-------|-------------|
| **P0** | This ADR; amend 0059 registry; `docs/grammar/config/README.md` stub; pilot pack spec (markdown-only) |
| **P1** | `Authoring.Config` parse + validate; `ConfigGrammarRegistry` v0 (path + root + hot slice grammars); conformance vectors |
| **P2** | `Modeling.Config` IR + `config check` in CDP/installer; Install-Cdp calls `Sat(hot.personal.l0)` |
| **P3** | `configbundle` stdlib grains; LSP; Correspondence edges from `facts` → golden |

## Consequences

**Positive**

- Self-check replaces «TOML parsed ⇒ success» for stack installs.
- Reuses GDL catalog ergonomics (`defaults`, tables, `import`).
- Operators keep familiar wire files; authors ship verifiable packs.

**Negative**

- New quarry + registry + conformance surface to maintain.
- Until P1 ships, `.config.gdl` is **normative signage** — implementers follow ADR, not invented ids.

## Non-goals (v0)

- Replacing all TOML in CDP/cockpit with GDL.
- Free-form `ensures` prose in operator TOML.
- In-string grammar unions in a single `defaults` slot.
