# GUIDERS-ADR-0059: GDL — Guiders Declarative Language hyperlane

| | |
|---|---|
| **Status** | **Accepted** (name + hyperlane signage; quarries ship incrementally) |
| **Date** | 2026-09-02 |
| **Tags** | #guiders #federation #gdl #authoring #hyperlane #dsl #declare |
| **Related** | [0048](./GUIDERS-ADR-0048-authoring-quarry-family.md) · [0049](./GUIDERS-ADR-0049-federation-pattern-library.md) · [0052](./GUIDERS-ADR-0052-unified-import-directive.md) · [0057](./GUIDERS-ADR-0057-cockpit-logic-authoring-quarry.md) · [0058](./GUIDERS-ADR-0058-presentation-topology-ir.md) · [0021](./GUIDERS-ADR-0021-notations-quarry-family.md) |

## Context

Federation already ships a **declare-time** surface family — `.catalog`, `.deck`, `.cockpit.logic` (proposed) — parsed by `Authoring.*` into typed IR and emit targets. Authors use shared conventions (`keyword … end keyword`, `* table`, `import`, `#` comments) via `Authoring.Core` ([0048](./GUIDERS-ADR-0048-authoring-quarry-family.md), [0049](./GUIDERS-ADR-0049-federation-pattern-library.md)).

We treated this as «config» or «DSL per file type» without a **public language name**. That hides the hyperlane from operators, LSP hosts, and planet adopters — and invites cramming semantics into TOML (`display.toml`, mega workspace drops) where grammar belongs.

**TOML remains excellent transport** ([0048](./GUIDERS-ADR-0048-authoring-quarry-family.md) §4) — flat catalog ship, env, tier-D wire. It is **not** the declare grammar for topology, annunciation, or display binding.

## Decision

### 1. Name the language: **GDL** (Guiders Declarative Language)

| Term | Meaning |
|------|---------|
| **GDL** | Federation **declare-time** language — human writes meaning; tooling parses → IR → emit/evaluate |
| **Authoring hyperlane** | NuGet implementation guild: `AIGuiders.Platform.Authoring.*` |
| **Quarry** | A GDL **grammar branch** — file extension + section vocabulary + IR output |
| **Wire literal** | Parenthesized or quoted surface inside a quarry (`(MFD)(F)`, path tails) — parsed by branch or `Notations.*` at emit boundary |

**GDL is not a runtime wire alphabet.** Slash lines, keyboard chords, MCP JSON args stay under **`Notations.*`** ([0021](./GUIDERS-ADR-0021-notations-quarry-family.md)).

### 2. Hyperlane placement (third declare pillar)

```text
                         Federation platform
    ┌────────────────────┬────────────────────┬────────────────────┐
    │  GDL (Authoring.*) │  Notations.*       │  CommandPlane.*    │
    │  declare → IR      │  wire → IR         │  mechanics         │
    │  codegen / emit    │  quarry + specs    │  catalog/registry  │
    └─────────┬──────────┴─────────┬──────────┴─────────┬──────────┘
              │                    │                      │
              └────────────────────┴──────────────────────┘
                          product surfaces + MCP hosts
```

Constitution readers: **GDL = name**; **Authoring.\* = packages**; same relationship as «slash notation» vs `Notations.Command.Slash`.

### 3. GDL lexical core (normative v0)

All federation quarries **SHOULD** use these unless a planet sovereign DSL documents an explicit fork:

| Construct | Form | Owner |
|-----------|------|-------|
| Block | `keyword … end keyword` | `Authoring.Core` |
| Table | `*name table` … `end *name` | `Authoring.Core` |
| KV sugar | dotted keys → table rows | `Authoring.Core` |
| Comment | `#` to EOL | `Authoring.Core` |
| Import | `import "logical"` · `import <wire/lib>` ([0052](./GUIDERS-ADR-0052-unified-import-directive.md)) | `Authoring.Core` |
| Expression | `when <expr>` — literals, compares, `and`/`or`/`not`, fact refs | `Authoring.Expression` (**proposed**) |

**No `{ }` blocks** — DashSpec parity ([0048](./GUIDERS-ADR-0048-authoring-quarry-family.md) §3).

### 4. Quarry registry (federation SSOT)

Domain **file extensions** identify the quarry. There is **no** requirement to rename `.deck` → `.gdl`; `.deck` **is** a GDL quarry.

| Extension | Package | IR / emit | Status |
|-----------|---------|-----------|--------|
| `.catalog` | `Authoring.Command.Catalog` | `IR.Command` | **shipped** |
| `.catalogbundle` | `Authoring.Command.Bundles` | profile rows | **shipped** |
| `.deck` | `Authoring.Deck` | `DeckDocument`, `PresentationTopology` ([0058](./GUIDERS-ADR-0058-presentation-topology-ir.md)) | **shipped** v0 |
| `.cockpit.logic` | `Authoring.Cockpit.Logic` | `CockpitRuleGraph` | **proposed** [0057](./GUIDERS-ADR-0057-cockpit-logic-authoring-quarry.md) |
| `.display` | `Authoring.Display.Binding` | `DisplayBindingProfile` | **proposed** — physical screen map; **not** TOML |
| `.gdl` | — | — | **reserved** — optional multi-quarry document; **no v0 parser** |

**Planet sovereign** (may adopt GDL core kit): `.dashspec`, `.dashdiagram`, … — DashSpec repo; not federation quarries.

### 5. Intent stack (federation quarries)

```text
.catalog        — what you can do
.deck           — where you look (zones, presets, topology IR)
.display        — where it lands physically (HostIndex → screen)
.cockpit.logic  — when things light up / project
```

Logical topology (`.deck`) and physical binding (`.display`) stay **separate quarries** so 1/2/3/6 monitor and ultrawide profiles do not fork deck meaning.

### 6. TOML boundary (normative)

| Use TOML | Use GDL quarry |
|----------|----------------|
| Emitted tier-D wire (`catalog.wire.toml`) | Author declares commands (`.catalog`) |
| `Catalog.Sources.Toml` read at runtime | Author declares deck topology (`.deck`) |
| Env / melody / workspace **drops** | Display binding (`.display`) |
| Third-party interop import | Cockpit annunciation (`.cockpit.logic`) |

**Rule:** if the artifact needs blocks, conditions, or topology wire — it is **GDL**, not a new `*.toml` guild.

### 7. Package map (hyperlane target)

NuGet prefix unchanged: **`AIGuiders.Platform.Authoring.*`**

```text
Authoring.Core              GDL document walk, blocks, tables, import hooks
Authoring.Expression        shared expr → ExprNode IR (proposed)
Authoring.Command.*         .catalog / .catalogbundle quarries
Authoring.Deck              .deck quarry
Authoring.Display.Binding   .display quarry (proposed)
Authoring.Cockpit.Logic     .cockpit.logic quarry (proposed)
Authoring.Conformance       docs/conformance/authoring/*
Authoring.Project           multi-file graph, LSP host abstraction ([0051](./GUIDERS-ADR-0051-authoring-project-abstraction.md))
```

Conformance vectors: `docs/conformance/authoring/<quarry>/*.spec.json` (+ shared `authoring/gdl-core.spec.json` when expression ships).

### 8. LSP and tooling

- One **GDL language service host** branches on extension / quarry id.
- Diagnostics, go-to-def, import graph reuse `Authoring.Project`.
- Planets port **quarry vectors**, not ad hoc TOML schemas.

## Consequences

- Operators and docs can say **GDL** instead of «various config formats».
- `display.toml` and similar placeholders are **rejected** — binding is `.display` quarry under GDL.
- New declare-time surfaces **must** justify a quarry ADR; default path is GDL core + branch handler, not TOML tables.
- Architecture Hub §7.4 links here as the language name for the Authoring guild.

## Non-goals (v0)

- `.gdl` multi-quarry single-file parser
- Replacing `.dashspec` or planet DSL bodies
- GDL as runtime REPL wire (use `Notations.*` if needed later)
- Unified mega-AST across quarries — **IR stays per branch**
