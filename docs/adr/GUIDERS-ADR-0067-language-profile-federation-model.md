# GUIDERS-ADR-0067: Language Profile — concept graph SSOT for multi-language Code Center

| | |
|---|---|
| **Status** | **Accepted** (architecture charter; implementation Phase 0) |
| **Date** | 2026-09-17 |
| **Tags** | #guiders #federation #language-profile #concept-graph #code-center #language-intelligence #gdl #modeling #profile-island #flavour |
| **Related** | [0025](./GUIDERS-ADR-0025-language-intelligence-boundary.md) · [0059](./GUIDERS-ADR-0059-gdl-hyperlane.md) · [0061](./GUIDERS-ADR-0061-language-resolver-center.md) · [0063](./GUIDERS-ADR-0063-anchors-federation-reincarnation.md) · [0066](./GUIDERS-ADR-0066-code-center-federation-product.md) · [0048](./GUIDERS-ADR-0048-authoring-quarry-family.md) · [DASHSPEC-ADR-0051](https://github.com/AI-Guiders/dash-spec/blob/develop/design/DASHSPEC-ADR-0051-language-affinity-modeling-execution.md) · [GUIDERS-FSHARP-ADR-0002](https://github.com/AI-Guiders/guiders-fsharp/blob/main/docs/adr/GUIDERS-FSHARP-ADR-0002-model-guild-fsharp-ownership.md) · [Constitution](../GUIDERS-FEDERATION-CONSTITUTION.md) |

## Context

Federation hosts edit **many language families** — md, yaml, html, xml, C#, F#, C++, GDL quarries, planet DSLs (`.dashspec`, …). Today each family tends to ship:

| Artifact | Typical owner today | Problem |
|----------|---------------------|---------|
| Parser / lexer | planet or adapter | Duplicated surface logic |
| Formatter | separate module | Rules drift from parser |
| Highlighter / classify | TextMate or ad-hoc | Not tied to stable node ids |
| Anchor / locus | bracket wire or line/col | Fragile ([0063](./GUIDERS-ADR-0063-anchors-federation-reincarnation.md)) |
| Diagram / tree view | one-off UI | Second parser, second model |

[Code Center](./GUIDERS-ADR-0066-code-center-federation-product.md) requires a **revision-stable graph** shared by all projections. [LRC](./GUIDERS-ADR-0061-language-resolver-center.md) covers **workspace verbs** on GPL backends; it does **not** define how structural languages (md, xml, block DSL) express concepts, invariants, or cross-projection rules.

**GDL quarries** ([0059](./GUIDERS-ADR-0059-gdl-hyperlane.md)) already approximate this for declare-time: quarry branch + shared lexical core + IR + conformance. That pattern has **not** been named or generalized for arbitrary document languages.

Operator direction (2026-09-17): describe each language’s **concepts and principles** in a **model** such that **rules derive from the model** — parse, validate, format, classify, anchor resolve, and non-text projections are **views/transforms** on one graph, not independent products.

**Out of scope (this ADR):** replacing Roslyn/FCS/tsserver; full BNF grammar generator; MPS-style projectional-only editing.

---

## Decision

### 1. Language Profile (normative definition)

**Language Profile** = federation-named **meta-model** for a document language (or quarry branch): the SSOT for **what exists** (concepts), **what must hold** (invariant laws), **how text maps to graph** (serialize rules), and **how edit/IDE tiers behave** (resolve policy).

```text
LanguageProfile
├── ProfileId              stable id ("dashspec.block", "gdl.catalog", "md.commonmark", …)
├── FlavourRef             optional dialect within family ("commonmark", "gfm", "yaml-1.2", "toml-1.1", …)
├── SurfaceFamily          federation sum (links DocumentSurface in 0063)
├── ConceptOntology        node kinds + typed edges (contains, references, scope, …)
├── InvariantLaws          pure validators on ConceptGraph instances
├── SerializeRules         text ↔ graph roundtrip (one text projection)
├── ResolvePolicy          Text | Syntax | Semantic + optional AdapterSlot
├── IslandRules            optional: how outer profile discovers embedded subgraph boundaries
└── ProjectionHints        concept → diagram / tree / form capabilities
```

**Rule (normative):** planet code **SHOULD NOT** ship standalone formatter/highlight/anchor rule sets that are not traceable to `InvariantLaws` or `ConceptOntology` visitors. Exceptions require ADR note (legacy migration).

### 2. Concept graph (SSOT instance)

For an open document at revision `r`:

```text
text ──parse(SerializeRules)──► ConceptGraph(doc_id, r)
                                      │
                    ┌─────────────────┼─────────────────┐
                    ▼                 ▼                 ▼
             InvariantLaws      classify spans    Anchor.TreeNode(nodeId)
             (validate)         (Syntax tier)     (0063 resolve)
                    │                 │                 │
                    ▼                 ▼                 ▼
             diagnostics        TextSurface        DiagramSurface
             (partial OK)       highlight          visitor
```

| Type | Role |
|------|------|
| **ConceptNode** | Typed node with `NodeId`, span optional, payload DU |
| **ConceptEdge** | `Contains` \| `Embeds` \| `References` \| `Scopes` \| `Orders` \| … (extend by sum) |
| **Region** | `ConceptNode` subtype: `{ profileRef, flavourRef?, subgraph }` — see §4.2 |
| **ConceptGraph** | Root + index; bumps `surface_version` on structural edit |
| **InvariantLaw** | `ConceptGraph → Diagnostic[]` (pure; no IO) |

`NodeId` assignment follows [0063](./GUIDERS-ADR-0063-anchors-federation-reincarnation.md) §3 — stable within `(doc_id, surface_version)`.

### 3. SurfaceFamily (federation sum — aligns DocumentSurface)

Initial normative families (extend by ADR, not string registry):

| SurfaceFamily | Examples | Typical tier | Graph owner |
|---------------|----------|--------------|-------------|
| `BlockText` | dashspec, GDL quarries | Syntax → Semantic | planet F# Modeling |
| `MdBlockAst` | md, md-like | Syntax | planet or shared kernel |
| `XmlTree` | xml, xaml, svg | Syntax | planet or shared kernel |
| `HtmlTree` | html | Syntax | planet or shared kernel |
| `YamlMapping` | yaml, yaml frontmatter | Syntax | planet or shared kernel |
| `TomlMapping` | toml | Syntax | planet or shared kernel |
| `PlainLines` | logs, env, legacy | Text | minimal graph |
| `CodeAst` | C#, F#, C++ | Semantic | **AdapterSlot** (Roslyn, FCS, …) |

Each `SurfaceFamily` has its own `ProfileId`, ontology, and law set. `YamlMapping` and `TomlMapping` are independent families (mapping/sequence/anchor vs table/inline-table/array-of-tables). Either may be a whole file or a **Profile Island** inside md/html/xml (§4.2).

`FlavourRef` selects a dialect **within** one family (`yaml-1.2`, `toml-1.1`, …).

`DocumentSurface` in [0063](./GUIDERS-ADR-0063-anchors-federation-reincarnation.md) **maps 1:1** to `SurfaceFamily` for anchor resolve; Language Profile adds **ontology + laws + flavour + islands** beneath the surface label.

### 4. Three language classes (honest ResolveTier)

From [0025](./GUIDERS-ADR-0025-language-intelligence-boundary.md) — Profile declares class; do not fake semantic depth.

| Class | Description | Profile body | Semantic source |
|-------|-------------|--------------|-----------------|
| **Structural** | Tree/block/mapping languages | full `ConceptOntology` + `InvariantLaws` in F# | graph + cross-ref resolve |
| **GPL** | Roslyn/FCS/LSP backends | thin Profile: `CodeAst` + `AdapterSlot` | external backend via LRC |
| **Hybrid** | embedded islands (razor-ish, templated html) | composite Profile: regions map to sub-profiles | per-region tier |

**GPL rule:** federation Profile **must not** re-parse C# into a parallel AST SSOT; `AdapterSlot` bridges LRC/`ILanguageBackend` symbols to `NodeId` policy defined by planet or federation shim.

### 4.1 Profile Flavour (dialect within a family)

One **SurfaceFamily** may host multiple **flavours** — dialects that share ontology shape but differ in laws and serialize edge cases.

| ProfileId | FlavourRef | Adds / differs |
|-----------|------------|----------------|
| `md.commonmark` | `commonmark` | baseline block/inline AST |
| `md.gfm` | `gfm` | tables, task lists, strikethrough, autolink laws **extends** commonmark |
| `yaml.mapping` | `yaml-1.2` | YAML syntax + schema hooks |
| `toml.document` | `toml-1.1` | [TOML v1.1.0](https://toml.io/en/v1.1.0): tables, inline tables, multiline inline tables, `\e`, optional datetime seconds |
| `html.whatwg` | `whatwg` | vs XML-compatible subset |

**Rules:**

- `ProfileId` **SHOULD** encode base family; `FlavourRef` selects law pack (inheritance or overlay list — planet choice, conformance required).
- Flavour is **not** a new SurfaceFamily unless ontology shape diverges (ADR amendment).
- Code Center caret resolve: active laws = flavour of **Region under caret** (§4.2), not file extension alone.
- Diagnostics from `InvariantLaws` apply **inline in the host document** — no separate “lint pass” product.

### 4.2 Profile Islands (embedded subgraphs)

**Profile Island** = typed **Region** node: outer graph holds boundary; inner **subgraph** parsed and validated by another `ProfileId` (+ optional `FlavourRef`).

```text
FileRoot (outer profile: md.gfm)
├── Region { profile: md.gfm }           → paragraph, headings, …
├── Region { profile: mermaid.diagram }   → fenced ```mermaid … ```
├── Region { profile: md.gfm }
├── Region { profile: yaml.mapping, flavour: yaml-1.2 }  → fenced ```yaml … ``` or frontmatter
└── Region { profile: toml.document, flavour: toml-1.1 }  → fenced ```toml … ```
```

Graph shape (informative):

```text
File
├── MD
├── Mermaid
├── MD
├── YAML          ← YamlMapping laws
├── MD
└── TOML          ← TomlMapping laws
```

**Normative mechanics:**

| Mechanism | Role |
|-----------|------|
| `Region` node | `{ profileRef, flavourRef?, span, subgraph: ConceptGraph }` |
| `Embeds` edge | outer `Contains`/`Embeds` inner root; stable `NodeId` for anchor |
| `IslandRules` | outer profile declares delimiters only (fences, `@` … `@`, `<tag>`, frontmatter `---`) |
| **Delegating parse** | outer parse → discover boundary → inner `LanguageProfile.Parse` on slice |
| **Composite validate** | dispatch `InvariantLaws` per Region; diagnostics merged with parent path |
| **Revision** | inner edit bumps Region `surface_version`; full file reserialize via outer rules |

**Canonical examples (conformance SHOULD cover):**

| Host | Island | Inner profile | Notes |
|------|--------|---------------|-------|
| `md.gfm` | fenced code | `mermaid.diagram` | diagram projection on Region |
| `md.gfm` | fenced code | `yaml.mapping` | inline config validation |
| `md.gfm` | fenced code | `toml.document` | inline config validation |
| `md.gfm` | frontmatter | `yaml.mapping` | same laws as standalone `.yaml` |
| `razor` | `@…` block | `csharp.expression` / `CodeAst` | Roslyn tier on island only |
| `razor` | markup | `html.tree` | HTML laws on markup regions |
| `html` + TagHelpers | element | tag profile overlay | element node → helper semantics + attribute laws |
| `xml` | CDATA / embed | nested profile | policy-driven |

Embedded config islands (`yaml.mapping`, `toml.document`) run `InvariantLaws` on the Region subgraph; diagnostics attach to the fence or frontmatter span in the host document.

**Hybrid class (§4):** composite outer Profile + `IslandRules` + registry of embeddable inner ProfileIds. Outer **must not** duplicate inner semantic rules in ad-hoc string checks.

**Anchor resolve:** `AnchorIntent.TreeNode(doc, regionNodeId)` → active profile = Region’s `profileRef`; drill-in to inner node uses inner graph index. Cross-island navigation is Code Center conformance (§9).

### 5. Derived behaviors (normative pipeline)

Given `ConceptGraph` at revision `r`, these **SHOULD** be implementations of the same SSOT:

| Behavior | Derivation |
|----------|------------|
| **Validate** | run all `InvariantLaws` (composite dispatch per Region — §4.2) |
| **Classify / highlight** | span projection over nodes (Syntax tier) |
| **Format** | layout visitor respecting `InvariantLaws` + serialize policy (e.g. `end` at opener depth = law, not formatter hack) |
| **Outline / tree projection** | filter + order on `ConceptOntology` |
| **Diagram projection** | visitor on Semantic-reachable concepts |
| **Anchor resolve** | `TreeNode` / `CodeSymbol` → `Locus` via graph index ([0063](./GUIDERS-ADR-0063-anchors-federation-reincarnation.md)) |
| **Mutate** | graph txn → `surface_version++` → re-serialize affected spans; LanguageIntelligence applies |

Text is **one serialization projection**, not the master rule store.

### 6. Relationship to GDL quarries

GDL is the **first shipped Language Profile family**:

| GDL piece | Language Profile term |
|-----------|----------------------|
| Quarry token (`catalog`, `deck`, …) | `ProfileId` branch |
| `Authoring.Core` block/table/import | shared **structural concepts** in `BlockText` family |
| Quarry IR | `ConceptGraph` instance shape |
| Conformance / Sat vectors | `InvariantLaws` + golden round-trip |
| `gdlc emit` | projection to C# / other targets (not text-only) |

New GDL quarries **SHOULD** document their Profile slice in quarry ADR; avoid orphan grammars.

### 7. Relationship to Code Center + session stack

```text
LanguageProfile (schema)     planet or federation registry entry
        │
        ▼
ConceptGraph instance        revision-stable SSOT for one document
        │
        ▼
IDocumentSession             federation envelope ([0066](./GUIDERS-ADR-0066-code-center-federation-product.md))
        │
        ▼
CodeCenterHost + projections Text, Diagram, Tree, Form, Preview
```

| Layer | Package (target) | Owner |
|-------|------------------|-------|
| Profile schema + laws | `Platform.Modeling.LanguageProfile` | guiders-fsharp |
| Anchors + surfaces | `Platform.Modeling.LanguageIntelligence.Anchors` | guiders-fsharp ([0063](./GUIDERS-ADR-0063-anchors-federation-reincarnation.md) A1) |
| Session + projections | `Platform.Modeling.CodeCenter` | guiders-fsharp ([0066](./GUIDERS-ADR-0066-code-center-federation-product.md)) |
| Planet ontology | e.g. `DashSpec.Modeling.Parse` / `DashSpec.Modeling.LanguageEditor` | planet |
| Planet adapter | e.g. `DashSpec.Execution.LanguageEditor` | planet |
| WPF host | `Surface.Wpf.CodeCenter` / TextEngine | guiders-wpf |

Federation Profile **does not** contain dashspec `@dashboard` semantics — planet owns ontology; federation owns **shape of a Profile** and cross-planet session/anchor contracts.

### 8. F# modeling sketch (non-normative starter)

Modeling lives in F# per [GUIDERS-FSHARP-ADR-0002](https://github.com/AI-Guiders/guiders-fsharp/blob/main/docs/adr/GUIDERS-FSHARP-ADR-0002-model-guild-fsharp-ownership.md):

```fsharp
type SurfaceFamily =
    | BlockText | MdBlockAst | XmlTree | HtmlTree | YamlMapping | TomlMapping | PlainLines | CodeAst

type FlavourRef = string

type ProfileRef = { ProfileId: string; Flavour: FlavourRef voption }

type ConceptEdgeKind = Contains | Embeds | References | Scopes | Orders

type RegionNode =
    { Id: NodeId
      Profile: ProfileRef
      Span: TextSpan
      Subgraph: ConceptGraph }

type ConceptNode =
    | Region of RegionNode
    | Atom of { Id: NodeId; Kind: string; Span: TextSpan voption; Payload: obj }

type ConceptGraph = { Root: NodeId; Nodes: Map<NodeId, ConceptNode>; Edges: (NodeId * ConceptEdgeKind * NodeId) list }

type InvariantLaw = ConceptGraph -> Diagnostic list

type LanguageProfile =
    { Profile: ProfileRef
      Surface: SurfaceFamily
      BaseProfile: ProfileRef voption          // flavour extends base laws
      Parse: string -> Result<ConceptGraph, Diagnostic list>
      Serialize: ConceptGraph -> string
      Laws: InvariantLaw list
      IslandRules: (ConceptGraph -> RegionNode list) voption
      ResolvePolicy: ResolvePolicy }
```

Planets extend `Kind`/`Payload` as typed DUs in planet packages; federation kernel stays **language-neutral**.

### 9. Conformance (minimum vectors)

Per Profile registration:

1. **Round-trip** — `serialize(parse(text))` satisfies all `InvariantLaws` (golden files).
2. **Law coverage** — each `InvariantLaw` has ≥1 failing + passing vector.
3. **Anchor stability** — `TreeNode` for a concept survives edits outside its subtree until structural change bumps `surface_version`.
4. **Projection coherence** — classify spans and tree outline agree on node boundaries (Syntax tier).
5. **Tier honesty** — Semantic projections disabled or marked partial when Profile class is Structural-only.
6. **Island round-trip** — md file with ` ```yaml ` / ` ```toml ` / ` ```mermaid ` islands: each Region subgraph satisfies inner Profile laws; outer serialize restores fences.
7. **Flavour overlay** — `md.gfm` vectors include GFM-only constructs failing under `md.commonmark` laws.
8. **Inline config diagnostics** — invalid YAML or TOML island produces diagnostics on the Region span in the host document.

### 10. Migration phases

| Phase | Deliverable | Notes |
|-------|-------------|-------|
| **0** | Name + charter (this ADR); map GDL + dashspec as implicit profiles | dashspec syntax tree = proto-ontology |
| **1** | `Platform.Modeling.LanguageProfile` kernel + `SurfaceFamily` + `ProfileRef`/`FlavourRef`; 0063 A1 anchors shipped | unblocks Code Center Phase 1 |
| **2** | Explicit `DashSpecLanguageProfile` planet ADR; laws extracted from formatter/classifier | [DASHSPEC-ADR-0051](https://github.com/AI-Guiders/dash-spec/blob/develop/design/DASHSPEC-ADR-0051-language-affinity-modeling-execution.md) |
| **3** | Shared kernels: `MdBlockAst` (+ flavours), `YamlMapping`, `TomlMapping`, `XmlTree`; **Profile Island** dispatch | md + yaml + toml + mermaid conformance pack |
| **4** | GPL thin profiles + `AdapterSlot` symbol → `NodeId` shim; Razor/TagHelper hybrid pilots | pairs with LRC |

---

## Consequences

- **Positive:** One mental model for md/xml/dsl/gpl; Code Center projections become mechanical; agents target `AnchorIntent` on stable concepts.
- **Cost:** Up-front modeling discipline; legacy formatters/highlighters must migrate or document exceptions.
- **Planet boundary:** Sovereign grammars stay on planets; federation sells Profile **shape**, shared surface families, and session/anchor roads.

---

## Non-goals (this ADR)

- Unified parser generator for all languages in v1
- Replacing LRC or merging Language Profile into `ILanguageBackend`
- Storing full GPL AST inside federation SSOT
- Authoring Language Profile in GDL declare files (F# remains SSOT for laws in v1; GDL declare may follow later)

---

## Open items

| # | Question | Default |
|---|----------|---------|
| 1 | Shared md/xml kernels in federation vs planet-only? | Federation kernel Phase 3; planets may ship earlier locally |
| 2 | `ProfileId` registry location? | `Platform.Modeling.LanguageProfile` module + docs index |
| 3 | Law authoring: code-only vs future declare quarry? | F# `InvariantLaw` lists Phase 1–2 |
| 4 | Flavour inheritance: explicit `BaseProfile` chain vs law list merge? | `BaseProfile` optional field (§8 sketch) |

## First planet instance (informative)

**DashSpec** — `BlockText` family; syntax tree + block formatter + classifier should converge on explicit `DashSpecLanguageProfile` ([DASHSPEC-ADR-0051](https://github.com/AI-Guiders/dash-spec/blob/develop/design/DASHSPEC-ADR-0051-language-affinity-modeling-execution.md)). Dogfoods Code Center TextSurface ([STUDIO-ADR-0005](https://github.com/AI-Guiders/dash-spec-studio/blob/main/design/STUDIO-ADR-0005-model-first-language-editor.md)).

**Federation pilot (informative):** `md.gfm` host with `yaml.mapping`, `toml.document`, and `mermaid.diagram` island profiles — Phase 3 conformance pack.
