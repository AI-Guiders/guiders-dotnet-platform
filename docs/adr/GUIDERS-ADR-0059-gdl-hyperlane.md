# GUIDERS-ADR-0059: GDL — Guiders Declarative Language hyperlane

| | |
|---|---|
| **Status** | **Accepted** (name + hyperlane signage; quarries ship incrementally) |
| **Date** | 2026-09-02 |
| **Tags** | #guiders #federation #gdl #authoring #hyperlane #dsl #declare |
| **Related** | [0048](./GUIDERS-ADR-0048-authoring-quarry-family.md) · [0049](./GUIDERS-ADR-0049-federation-pattern-library.md) · [0051](./GUIDERS-ADR-0051-authoring-project-abstraction.md) · [0052](./GUIDERS-ADR-0052-unified-import-directive.md) · [0057](./GUIDERS-ADR-0057-cockpit-logic-authoring-quarry.md) · [0058](./GUIDERS-ADR-0058-presentation-topology-ir.md) · [0061](./GUIDERS-ADR-0061-language-resolver-center.md) · [0062](./GUIDERS-ADR-0062-ide-solution-session-orchestrator.md) · [0064](./GUIDERS-ADR-0064-config-gdl-quarry-family.md) · [0021](./GUIDERS-ADR-0021-notations-quarry-family.md) · [authoring-toolchain](https://github.com/AI-Guiders/authoring-toolchain) · [GUIDERS-UI-0009](https://github.com/AI-Guiders/guiders-ui-platform/blob/main/docs/adr/GUIDERS-UI-0009-openapi-rest-leg-alignment.md) |

## Context

Federation already ships a **declare-time** surface family — `*.catalog.gdl`, `*.deck.gdl`, `*.cockpit.logic.gdl` (proposed) — parsed by `Authoring.*` into typed IR and emit targets.

We treated this as «config» or «DSL per file type» without a **public language name**. That hides the hyperlane from operators, LSP hosts, and planet adopters — and invites cramming semantics into TOML (`display.toml`, mega workspace drops) where grammar belongs.

**TOML remains excellent transport** ([0048](./GUIDERS-ADR-0048-authoring-quarry-family.md) §4) — flat catalog ship, env, tier-D wire. It is **not** the declare grammar for topology, annunciation, or display binding.

## Decision

### 1. Name the language: **GDL** (Guiders Declarative Language)

| Term | Meaning |
|------|---------|
| **GDL** | Federation **declare-time** language — human writes meaning; tooling parses → IR → emit/evaluate |
| **Authoring hyperlane** | NuGet implementation guild: `AIGuiders.Platform.Authoring.*` |
| **Quarry** | A GDL **grammar branch** — `.{quarry}.gdl` file suffix + section vocabulary + IR output |
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

### 4. File naming — double extension + project manifest

GDL documents use **canonical double extension** `*.{quarry}.gdl`. The **quarry** token (before `.gdl`) selects grammar + IR; `.gdl` marks federation declare-time hyperlane membership.

| Artifact | Example | Role |
|----------|---------|------|
| Project manifest | `studio.gdlproj` | Workspace root, entry documents, import graph ([0051](./GUIDERS-ADR-0051-authoring-project-abstraction.md)) — **not** a quarry |
| Catalog quarry | `commands.catalog.gdl` | Command declare |
| Deck quarry | `report-author.deck.gdl` | Zones, presets, topology |
| Display quarry | `default.display.gdl` | HostIndex → physical screen (**proposed**) |
| Cockpit logic quarry | `studio.cockpit.logic.gdl` | Annunciation rules (**proposed**) |
| Bundle quarry | `date-filter.catalogbundle.gdl` | Federation stdlib grain |

```text
studio/
  studio.gdlproj
  catalog/commands.catalog.gdl
  deck/report-author.deck.gdl
  display/default.display.gdl
  logic/studio.cockpit.logic.gdl
```

**Do not** use glued prefixes (`gdlcatalog`, `gdldeck`) — non-idiomatic for tooling globs and LSP file association.

Bare `.catalog`, `.deck`, and other single-suffix extensions are **not** GDL — parsers and tooling **MUST NOT** accept them.

**Anti-patterns (non-goals):**

| Rejected | Why |
|----------|-----|
| Single mega `studio.gdl` with all quarries | breaks review, quarry boundaries, emit |
| Bare `.gdl` without quarry token | ambiguous branch |
| `display.toml` / YAML declare drops | transport ≠ GDL grammar |

### 5. Quarry registry (federation SSOT)

| Quarry token | Canonical extension | Package | IR / emit | Status |
|--------------|---------------------|---------|-----------|--------|
| `catalog` | `.catalog.gdl` | `Authoring.Command.Catalog` | `IR.Command` | **shipped** |
| `catalogbundle` | `.catalogbundle.gdl` | `Authoring.Command.Bundles` | profile rows | **shipped** |
| `deck` | `.deck.gdl` | `Authoring.Deck` | `DeckDocument`, `PresentationTopology` ([0058](./GUIDERS-ADR-0058-presentation-topology-ir.md)) | **shipped** v0 |
| `cockpit.logic` | `.cockpit.logic.gdl` | `Authoring.Cockpit.Logic` | `CockpitRuleGraph` | **proposed** [0057](./GUIDERS-ADR-0057-cockpit-logic-authoring-quarry.md) |
| `display` | `.display.gdl` | `Authoring.Display.Binding` | `DisplayBindingProfile` | **proposed** |
| `config` | `.config.gdl` | `Authoring.Config` | `ConfigurationPack` ([0064](./GUIDERS-ADR-0064-config-gdl-quarry-family.md)) | **proposed** |
| `configbundle` | `.configbundle.gdl` | `Authoring.Config.Bundles` | federation stdlib grains | **proposed** |

**Planet sovereign** (may adopt GDL core kit): `.dashspec`, `.dashdiagram`, … — DashSpec repo; not federation quarries.

### 6. Intent stack (federation quarries)

```text
*.catalog.gdl        — what you can do
*.deck.gdl           — where you look (zones, presets, topology IR)
*.display.gdl        — where it lands physically (HostIndex → screen)
*.config.gdl         — how the stack is wired and what must hold (interpretation + Sat)
*.cockpit.logic.gdl  — when things light up / project
```

Logical topology (deck quarry) and physical binding (display quarry) stay **separate files** so 1/2/3/6 monitor and ultrawide profiles do not fork deck meaning.

### 7. TOML boundary (normative)

| Use TOML | Use GDL quarry |
|----------|----------------|
| Emitted tier-D wire (`catalog.wire.toml`) | Author declares commands (`*.catalog.gdl`) |
| `Catalog.Sources.Toml` read at runtime | Author declares deck topology (`*.deck.gdl`) |
| Env / melody / workspace **drops** | Display binding (`*.display.gdl`) |
| Third-party interop import | Cockpit annunciation (`*.cockpit.logic.gdl`) |

**Rule:** if the artifact needs blocks, conditions, or topology wire — it is **GDL**, not a new `*.toml` guild.

### 8. Package map (hyperlane target)

NuGet prefix unchanged: **`AIGuiders.Platform.Authoring.*`**

```text
Authoring.Core              GDL document walk, blocks, tables, import hooks
Authoring.Expression        shared expr → ExprNode IR (proposed)
Authoring.Command.*         *.catalog.gdl / *.catalogbundle.gdl quarries
Authoring.Deck              *.deck.gdl quarry
Authoring.Display.Binding   *.display.gdl quarry (proposed)
Authoring.Cockpit.Logic     *.cockpit.logic.gdl quarry (proposed)
Authoring.Config            *.config.gdl quarry (proposed — [0064](./GUIDERS-ADR-0064-config-gdl-quarry-family.md))
Authoring.Conformance       docs/conformance/authoring/*
Authoring.Project           `GdlProject` / `*.gdlproj` manifest + multi-file graph ([0051](./GUIDERS-ADR-0051-authoring-project-abstraction.md))
```

Conformance vectors: `docs/conformance/authoring/<quarry>/*.spec.json` (+ shared `authoring/gdl-core.spec.json` when expression ships).

### 9. LSP and tooling

- One **GDL language service host** registers `*.gdl` + `*.gdlproj`; quarry = extension token(s) before `.gdl` (e.g. `deck`, `cockpit.logic`). Federation contract: [GUIDERS-ADR-0061](./GUIDERS-ADR-0061-language-resolver-center.md) (**Language Resolver Center**) + `Platform.Modeling.Language.Adapters.Gdl`.
- Diagnostics, go-to-def, import graph reuse `Authoring.Project` / `GdlProject`.
- Planets port **quarry vectors**, not ad hoc TOML schemas.

### 10. Compiler pipeline (`gdlc`)

GDL declare-time artifacts compile through a unified front-end **`gdlc`** (target shipping name; today fragmented as `authoring validate|emit`, `deck emit` in [authoring-toolchain](https://github.com/AI-Guiders/authoring-toolchain)).

```text
*.gdlproj / *.gdl
        │  parse (quarry router by *.{quarry}.gdl suffix)
        ▼
   GdlProject + quarry IR
        │
        ├── validate              diagnostics
        ├── emit --lang=<target>  generated artifacts
        └── sat                   contract observers (config quarry — [0064](./GUIDERS-ADR-0064-config-gdl-quarry-family.md))
```

**Entry project file is `*.gdlproj`, not `*.csproj`.** `GdlProject` is a parallel authoring-project node ([0051](./GUIDERS-ADR-0051-authoring-project-abstraction.md), [0062](./GUIDERS-ADR-0062-ide-solution-session-orchestrator.md)) — it **may** appear beside dotnet projects in the same session graph; it is **not** merged into MSBuild project IR.

Normative invocation (target):

```text
gdlc validate --project studio.gdlproj
gdlc emit --lang=cs --project studio.gdlproj --out obj/gdl/
gdlc emit --lang=toml --project studio.gdlproj --out wire/
gdlc sat --project studio.gdlproj
```

Single-file emit remains valid for CI snippets and tests:

```text
gdlc emit --lang=cs catalog/commands.catalog.gdl \
  --namespace MyApp.Generated --class Commands --out Generated/Commands.g.cs
```

#### 10.1 Emit targets

| `--lang` | Output | Typical quarries |
|----------|--------|------------------|
| `cs` | `*.g.cs` — Roslyn-visible C# | `catalog`, `deck` |
| `toml` | tier-D wire drops | `catalog` ([0048](./GUIDERS-ADR-0048-authoring-quarry-family.md) §2) |
| `json` | conformance / IR snapshots | all |
| `fs` | F# Modeling types | future |

Generated C# uses the `*.g.cs` suffix. User extensions live in separate partials (e.g. `Commands.User.cs`) — generated files are **regen-owned**.

**MSBuild (target):** `*.csproj` invokes `gdlc emit` in a `BeforeCompile` target (or checks in `Generated/` and fails CI when stale). The dotnet project **consumes** emit output; it does not replace `gdlproj` as the declare entry.

#### 10.2 Author vs consumer distribution

| Role | Ships | Tooling |
|------|-------|---------|
| **Author** (federation, org, installer) | `*.gdl`, `*.gdlproj`, bundles | `gdlc validate`, `gdlc sat`, re-emit |
| **Consumer** (planet app, NuGet user) | `Generated/*.g.cs`, tier-D wire | ordinary C# IntelliSense; `.gdl` optional |

Consumers **may** compile and edit generated C# without `.gdl` sources — same pattern as consuming a library built from `.proto` without the schema repo. Authors **must** retain `.gdl` as meaning SSOT; hand-editing `*.g.cs` without updating GDL is an escape hatch that the next emit overwrites.

#### 10.3 External analogies (informative)

Federation tooling intentionally rhymes with mature IDL / OpenAPI pipelines. These rows are **orientation**, not a mandate to adopt protobuf or OpenAPI parsers inside GDL.

| Pattern | Familiar example | GDL / `gdlc` |
|---------|------------------|--------------|
| Schema / IDL file | `.proto`, OpenAPI | `*.{quarry}.gdl`, `*.gdlproj` |
| Compiler CLI | `protoc`, `openapi-generator` | `gdlc` |
| Language back-end | `protoc-gen-go` | `gdlc emit --lang=cs|fs|toml|json` (+ plugins later) |
| Generated glue | `*.pb.go` | `*.g.cs` |
| Contract tests | protobuf conformance, OpenAPI validators | `docs/conformance/**`, `gdlc validate` |
| Breaking change gate | buf breaking | IR / vector diff in CI |
| Stable public ids | OpenAPI `operationId` | catalog command ids, deck ids; REST leg defers to planet OpenAPI ([GUIDERS-UI-0009](https://github.com/AI-Guiders/guiders-ui-platform/blob/main/docs/adr/GUIDERS-UI-0009-openapi-rest-leg-alignment.md)) |
| Consumer package | generated stubs without `.proto` | NuGet with `*.g.cs` + wire; `.gdl` not required |

**Boundary (normative):** GDL is **declare-time meaning**, not runtime serialization. Binary / slash / keyboard wire stays under **`Notations.*`** and tier-D transport — not `protoc` bytes. OpenAPI alignment applies to the **HTTP REST leg** in UI platform, not to the GDL parser surface.

GDL adds **postconditions** (`ensures`, `Sat`, `facts` — [0064](./GUIDERS-ADR-0064-config-gdl-quarry-family.md)) on top of the IDL pattern — closer to *meaning + verify* than to schema-only tools.

#### 10.4 Migration (tooling)

| Phase | Deliverable |
|-------|-------------|
| **Now** | Per-quarry `authoring emit`, `deck emit`; ADR signage |
| **Next** | Unified `gdlc` binary; `gdlproj` as default `--project` entry |
| **Then** | MSBuild target + plugin emit registry; `gdlc sat` for config quarry |

## Consequences

- Operators and docs can say **GDL** instead of «various config formats».
- `display.toml` and similar placeholders are **rejected** — binding is `*.display.gdl` quarry under GDL.
- New declare-time surfaces **must** justify a quarry ADR; default path is GDL core + branch handler, not TOML tables.
- **`gdlc`** names the declare-time compile step (validate · emit · sat); consumers may ship generated `*.g.cs` without `.gdl` sources.
- Architecture Hub §7.4 links here as the language name for the Authoring guild.

## Non-goals (v0)

- Mega single-file GDL with all quarries inlined
- Glued extensions (`gdlcatalog`, `gdldeck`)
- Bare single-suffix extensions (`.catalog`, `.deck`, …)
- Replacing `.dashspec` or planet DSL bodies
- GDL as runtime REPL wire (use `Notations.*` if needed later)
- Unified mega-AST across quarries — **IR stays per branch**
