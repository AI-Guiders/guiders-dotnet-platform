# GUIDERS-ADR-0048: Authoring quarry family (declare → IR → emit)

| | |
|---|---|
| **Status** | Accepted (implementation wave 2026-09-01) |
| **Date** | 2026-09-01 |
| **Tags** | #guiders #federation #authoring #guild #dsl #quarry #dx |
| **Related** | GUIDERS-ADR-0021 · GUIDERS-ADR-0042 · GUIDERS-ADR-0045 · [GUIDERS-ADR-0047](./GUIDERS-ADR-0047-command-for-doi.md) · [GUIDERS-ADR-0049](./GUIDERS-ADR-0049-federation-pattern-library.md) · [GUIDERS-ADR-0051](./GUIDERS-ADR-0051-authoring-project-abstraction.md) · DASHSPEC-ADR-0017 · DASHSPEC-ADR-0024 |

## Context

Federation already names **`Notations.*`** as the hyperlane guild for **wire-in** alphabets (keyboard, slash path, arg tail, bracket payloads) → neutral IR at **resolve time**.

Separately, products and federation ship many **authoring** surface languages — files engineers edit to declare SSOT:

| Family | Examples | Today |
|--------|----------|-------|
| Federation command catalog | `.catalog`, `.catalogbundle` | Mis-filed under `Notations.Command.Catalog` in [0047](./GUIDERS-ADR-0047-command-for-doi.md) |
| Planet dashboard spec | `.dashspec`, `.dashdiagram`, `.dashpalette`, … | DashSpec repo only ([DASHSPEC-ADR-0017](https://github.com/AI-Guiders/dash-spec/blob/main/design/DASHSPEC-ADR-0017-file-includes-and-stdlib.md)) |
| Config drops | command TOML, melody TOML, workspace TOML | `Sources.*` — transport, not grammar family |
| Conformance | `notation/*.spec.json` | Hyperlane vectors |

We treated authoring DSLs as «just config» or tucked them under **Notations**. That blurs layers:

```text
AUTHORING (design-time)     declare matrices → typed IR → codegen
WIRE (runtime)              human types line → Notations → IR → registry
MECHANICS                   Slash · Melody · Binding · executors
```

[GUIDERS-ADR-0021](GUIDERS-ADR-0021-notations-quarry-family.md) §12 already says `.catalog` sits **above** Notations and **emits** tier-D wire. This ADR names the guild and shared kit.

**Planet boundary** ([Constitution § Planets are not SSOT](../GUIDERS-FEDERATION-CONSTITUTION.md#planets-are-not-federation-ssot)): `.dashspec` grammar stays **DashSpec sovereign**. Federation ships **Authoring.Core** patterns + cross-planet grammars (`.catalog`), not planet product DSL bodies.

## Decision

### 1. Authoring is a sibling hyperlane guild

```text
                         Federation platform
    ┌────────────────────┬────────────────────┬────────────────────┐
    │  Authoring.*       │  Notations.*       │  CommandPlane.*    │
    │  declare → IR      │  wire → IR         │  mechanics         │
    │  codegen / emit    │  quarry + specs    │  catalog/registry  │
    └─────────┬──────────┴─────────┬──────────┴─────────┬──────────┘
              │                    │                      │
              └────────────────────┴──────────────────────┘
                          product surfaces + MCP hosts
```

Same constitution rule as Notations: **reference quarry on NuGet**; planets may port parsers or adopt `Authoring.Core` block/table kit.

### 2. Two layers — normative split

| Layer | Question | When | Guild | Example |
|-------|----------|------|-------|---------|
| **Authoring** | What did the engineer **declare**? | Build / CI / codegen | **`Authoring.*`** | `commands table` row → `CommandCatalogEntry` |
| **Wire (Notation)** | What did the human **type**? | Runtime resolve | **`Notations.*`** | `/select filter usage_date today` → `NormalizedCommandLine` |

**Emit boundary:** Authoring codegen produces tier-D artifacts (`catalog.wire.toml`, `ArgTail` strings, MCP schema). **Notations** consume those at resolve time — Authoring parsers do **not** replace `Notations.Command.Slash`.

### 3. Shared authoring patterns (v0 — from `.catalog` wave)

Cross-grammar conventions ([0047](./GUIDERS-ADR-0047-command-for-doi.md) §4):

| Pattern | Meaning |
|---------|---------|
| `keyword … end keyword` | Block delimiters (**DashSpec parity**; no `{ }` blocks) |
| `* table` | Matrix surfaces (`helps table`, `commands table`, …) |
| kv sugar | Dotted keys desugar → table rows (`command.foo.summary` → helps row) |
| `import "path"` / `import <path>` | Logical file or federation/planet stdlib wire ([0052](./GUIDERS-ADR-0052-unified-import-directive.md)); **not** `!include` |
| planet content only | `catalog dash`, `@dashboard` — header, not federation grammar fork |

| Surface | Correspondence |
|---------|----------------|
| `variables` | *name* → *kind* |
| `helps` | *target* + *field* → *text* |
| `phrases` | *name* → *phrase* |
| `profiles` | *profile* → arg-menu rows |
| `commands` | *command* → wiring columns |

`Authoring.Core` owns reusable parse kit for these surfaces; branch grammars compose it.

**Shipped (2026-09-01):** `AuthoringSource`, `BlockReader`, `TableSurface`, `KvSurface`, `InnerBlockFilter`, `IndentedTreeParser`, `TableRowParser`; normative `docs/grammar/authoring/catalog.ebnf`.

### 4. Package map (target)

NuGet prefix: **`AIGuiders.Platform.Authoring.*`**

```text
Authoring.Core
  BlockReader (end keyword), TableSurface, KvDesugar, IncludeResolver hooks
  IAuthoringDocument, diagnostics, spec loader hooks

Authoring.Command.Catalog
  .catalog grammar + parser → IR.Command (CommandCatalogEntry, …)
  NOT under Notations.*

Authoring.Command.Bundles
  Federation stdlib: grain/date-filter.catalogbundle, value/* bundles
  import <grain/…> resolution

Authoring.Command.CodeGen          (optional split from CommandPlane.Catalog.CodeGen)
  Roslyn + dotnet catalog emit — or stay in CommandPlane.Catalog.CodeGen consuming Authoring.Command.Catalog AST

Authoring.Conformance
  docs/conformance/authoring/* vectors (grammar round-trip, desugar)

Authoring.All                      optional meta-bundle
```

**Rename (0047 wave):** `Notations.Command.Catalog` → **`Authoring.Command.Catalog`**. No type alias period in greenfield; if a stub existed, one-way move with search-replace in same PR.

**Stays in CommandPlane / Sources (not Authoring):**

| Package | Role |
|---------|------|
| `CommandPlane.Catalog` | expand recipes, assembly, registry merge |
| `CommandPlane.Catalog.CodeGen` | emit CLI host (may depend on `Authoring.Command.Catalog`) |
| `Catalog.Sources.Toml` | **tier-D transport** — read emitted wire, not author grammar |
| `CommandDescriptors` fluent API | tier A–C code-first ([ADR-0045](GUIDERS-ADR-0045-command-authoring-dx.md)) |

### 5. Inventory — federation vs planet

#### Federation grammars (Authoring guild SSOT)

| Grammar | Package | IR output |
|---------|---------|-----------|
| `.catalog` | `Authoring.Command.Catalog` | `IR.Command` + vocabulary index |
| `.catalogbundle` | `Authoring.Command.Bundles` | profile rows merged into `profiles` |

#### Planet grammars (sovereign; may adopt Authoring.Core)

| Planet | DSL roots | SSOT repo |
|--------|-----------|-----------|
| **DashSpec** | `.dashspec`, `.dashinclude`, `.dashdiagram`, `.dashpresentation`, `.dashtransform`, `.dashpalette`, `.dashlayout`, `.dashtooltip`, `.dashcatalog` | dash-spec |
| **Forge / CIDE** | catalog TOML, palette modules | product repos |
| **CDP** | CSX, bracket square-kv profiles | cdp-mcp / cascade-ide |

Federation **may** later add `Authoring.Workspace` for shared workspace TOML shapes — only after second consumer ADR.

#### Not authoring (do not move)

| Artifact | Guild |
|----------|-------|
| Slash / console typed line | `Notations.Command.*` |
| Arg tail wire | `Notations.Argument.*` |
| Keyboard wire | `Notations.Keyboard.*` |
| `notation/*.spec.json` | Conformance hyperlane |
| C# `ICatalogDescribed` | CommandPlane tier A |

### 6. Pipeline (command catalog)

```text
<planet>.catalog (content)
       │
       ▼
Authoring.Command.Catalog parser
       │
       ▼
IR.Command (CommandCatalogEntry, ArgTailProfile, …)     ← ADR-0042 spine
       │
       ├── Roslyn CodeGen → {Planet}Catalog.g.cs
       └── dotnet catalog emit → mcp-tools.json, catalog.wire.toml (tier D)
                │
                ▼
       Notations.Command.Slash + Argument.* at runtime resolve
                │
                ▼
       CommandPlane registry.TryExecute(commandId)
```

### 7. Relationship to ADR-0047

[GUIDERS-ADR-0047](./GUIDERS-ADR-0047-command-for-doi.md) is the **first branch spec** under this guild:

- Grammar: `variables`, `helps table`, `phrases table`, `profiles`, `defaults`, `commands table`
- DOI-first CommandId, `expand … fills`, bundle `import <grain/date-filter>`
- CodeGen + hard-cut migration table

0048 = **family map**; 0047 = **Command.Catalog branch** detail.

### 8. Conformance

| Vector bucket | Tests |
|---------------|-------|
| `docs/conformance/authoring/catalog/*.spec.json` | parse → IR snapshot; kv/table desugar equivalence |
| `docs/conformance/authoring/bundles/*.spec.json` | bundle expand → profiles table |

Planet DSL conformance stays in planet repos (e.g. DashSpec parser tests). Federation may **reference** DashSpec block style as **informative** parity, not normative fork.

### 9. Hub & ADR signage

- Architecture Hub §7 — new **Authoring** family row
- ADR-0021 §12 — point to **Authoring.*** not Notations for `.catalog`
- Pain inventory: authoring scatter → **G-011** (proposed tag)

## Consequences

- Mental model matches industry split: **schema/declare** vs **wire/parse**.
- DashSpec keeps `.dashspec` sovereignty; gains optional `Authoring.Core` dependency for block/table lexer later.
- Forge TS slash port still targets **Notations** specs; `.catalog` port targets **Authoring** specs.
- Package count +1 family; solution filter `Authoring.*` in CI.

## Non-goals (v0)

- Merging `.dashspec` into federation monorepo
- Authoring parser for runtime slash lines (stays Notations)
- Universal «one grammar for all products»
- Replacing tier A–C C# builders ([ADR-0045](GUIDERS-ADR-0045-command-authoring-dx.md))

## Open (operator)

| # | Topic | Proposal |
|---|--------|----------|
| 1 | CodeGen package id | `Authoring.Command.CodeGen` vs keep in `CommandPlane.Catalog.CodeGen` |
| 2 | DashSpec adopt Core | optional v1 — shared `BlockReader` only |
| 3 | Workspace authoring | defer until second consumer |

## References

- [GUIDERS-ADR-0021 Notations quarry](GUIDERS-ADR-0021-notations-quarry-family.md)
- [GUIDERS-ADR-0042 IR family](GUIDERS-ADR-0042-intermediate-representation-family.md)
- [GUIDERS-ADR-0047 `.catalog` grammar](./GUIDERS-ADR-0047-command-for-doi.md)
- [DASHSPEC-ADR-0017 file includes](https://github.com/AI-Guiders/dash-spec/blob/main/design/DASHSPEC-ADR-0017-file-includes-and-stdlib.md)
- [DASHSPEC-ADR-0024 authoring layers](https://github.com/AI-Guiders/dash-spec/blob/main/design/DASHSPEC-ADR-0024-document-authoring-layers.md)
