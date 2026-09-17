# GUIDERS-ADR-0063: Anchors — Federation reincarnation (v1)

| | |
|---|---|
| **Status** | Accepted (architecture); **v1 entity semantics superseded by §9–§10** (RelationSpec era, 2026-09-17) |
| **Date** | 2026-09-02 |
| **Tags** | #guiders #federation #anchor #language-intelligence #edit-plane #buffer #sniper #first-class |
| **Related** | [GUIDERS-ADR-0025](./GUIDERS-ADR-0025-language-intelligence-boundary.md) · [GUIDERS-ADR-0026](./GUIDERS-ADR-0026-notations-bracket-branch.md) · [GUIDERS-ADR-0027](./GUIDERS-ADR-0027-mdlinker-doc-anchor-check.md) · [GUIDERS-ADR-0061](./GUIDERS-ADR-0061-language-resolver-center.md) · [GUIDERS-ADR-0062](./GUIDERS-ADR-0062-ide-solution-session-orchestrator.md) · [GUIDERS-ADR-0066](./GUIDERS-ADR-0066-code-center-federation-product.md) · [GUIDERS-ADR-0067](./GUIDERS-ADR-0067-language-profile-federation-model.md) · [CDP BUF-001](https://github.com/AI-Guiders/agent-notes/blob/main/knowledge/work/projects/door-to-singularity/cdp-mcp/subprojects/cdp-buffer-v1-known-gaps.md) · [Constitution](../GUIDERS-FEDERATION-CONSTITUTION.md) |

## Context

**Anchor** = federation entity for *where to edit* (locus intent), consumed by buffer mutate, sniper, peel, CSX, MCPlane, evidence loop.

[ADR-0025](./GUIDERS-ADR-0025-language-intelligence-boundary.md) placed Anchors in `LanguageIntelligence.*` and stated bracket wire is **projection only**. Implementation still reflects **pre-Federation** habits:

| Pre-Federation | Problem |
|----------------|---------|
| `[F:…;M:…;K:…]` as agent mental model | Bracket taxed as the thing, not Anchor |
| `X:Project/…` axis (csproj lift) | **До-платформенный** wire; not Federation SSOT |
| Line/`L:` as default | Fragile; ignores typed `DocumentSurface` |
| Monolithic `set_text` | BUF-001: large doc kills MCP (blob transport) |
| Per-planet string dialects | No shared intent → locus contract |

Operator requirement (2026-09-02): **Federation-first reincarnation** — typed trees, session-stable ids, structured transport; bracket profile remains **compatibility**, not canon.

**Out of scope (this ADR):** ANUI slice, GDL→IL fork, IDE session graph IR ([0062](./GUIDERS-ADR-0062-ide-solution-session-orchestrator.md) owns session; Anchors own *edit locus*).

---

## Decision

> **Historical (v1 — 2026-09-02):** §1–§7 describe the **AnchorIntent** era. Federation SSOT is now **`RelationSpec`** + graph `Relation` (§9–§10). Retained for migration context only.

### 1. Entity model (normative)

Three layers — do not collapse:

```text
AnchorIntent     what the agent/human wants to locate
      │ resolve (IAnchorResolver, per DocumentSurface)
      ▼
Locus            resolved span + ResolveTier + optional SymbolRef
      │ mutate (buffer txn, sniper place, CSX)
      ▼
BufferEditOutcome / TextEdit
```

| Type | Role |
|------|------|
| **AnchorIntent** | Serializable request; may be partial / fuzzy |
| **Locus** | Resolved coordinates in a buffer revision |
| **ResolveTier** | `Text` \| `Syntax` \| `Semantic` ([0025](./GUIDERS-ADR-0025-language-intelligence-boundary.md)) |
| **DocumentSurface** | Typed document kind + in-memory tree (`PlainLines`, `XmlTree`, `HtmlTree`, `MdBlockAst`, `CodeAst`, …) |
| **NodeId** | Stable id within `(doc_id, surface_version)` |

**Anchor** (public name) = `AnchorIntent` in API; never teach agents that "Bracket" is the entity ([kj anchor-not-bracket](https://github.com/AI-Guiders/agent-notes/blob/main/knowledge/work/projects/door-to-singularity/cascade-ide/scratch/kolb-journal/drafts/kj-20260722-1624-anchor-not-bracket.md)).

### 2. AnchorIntent sum (Federation SSOT sketch)

Modeling lives in F# (`Platform.Modeling.LanguageIntelligence.Anchors` — guiders-fsharp). Shape:

```fsharp
type AnchorIntent =
    | TreeNode of docId: DocId * nodeId: NodeId
    | CodeSymbol of docId: DocId * symbol: SymbolRef
    | TextRange of docId: DocId * range: TextRange
    | Evidence of findingId: FindingId
    | LegacyWire of profile: string * wire: string   // migration shim → normalize (§8)
```

**Rules:**

- **Prefer** `TreeNode` / `CodeSymbol` over `TextRange` when surface provides a tree.
- **No new universal axis letters** (`Y:`, `Z:`, …). New surfaces extend the **sum**, not bracket alphabet soup.
- `LegacyWire` is **not destiny** — boundary decode only; internal SSOT = typed `AnchorIntent` ([§8](#8-post-modeling-rethink-horizon)).

### 3. DocumentSurface + NodeId

On `buffer open` / parse:

1. Host chooses `DocumentSurface` from language + policy (not from file extension alone).
2. Parser builds **typed tree** (XML/HTML/MD/code) — **no regex SSOT**.
3. Host assigns **NodeId** per node; maps survive until `surface_version` bumps (structural edit).

```text
cdp_buffer open  →  doc_id, surface, surface_version, node_index
Anchor.TreeNode(doc_id, node_id)  →  resolve  →  Locus
```

Line/column remains **Text** tier fallback, not primary API.

### 4. Transport (MCP / JSON primary)

**Primary wire (Federation v1):** structured JSON matching `AnchorIntent` CLIMutable types ([0061](./GUIDERS-ADR-0061-language-resolver-center.md) pattern — types are SSOT, not hand-written JSON schema).

Example:

```json
{
  "kind": "TreeNode",
  "docId": "doc-6",
  "nodeId": "h-§7.4"
}
```

**Secondary (legacy):** bracket string under profile `notation/bracket-cdp-square-kv` → `NormalizedBracketWire` → `LegacyWire` adapter → `AnchorIntent`.

| Path | When |
|------|------|
| JSON `AnchorIntent` | All new MCP tools, CSX builders, agents |
| Bracket `ToWire()` | Logs, human copy-paste, pre-Federation scripts |
| `X:` axis | **Only** inside legacy profile resolution for XmlSurface csproj lift |

### 5. Resolvers (per surface, Federation-first)

```text
IAnchorResolver
  Resolve(surface, intent, revision) → Result<Locus, ResolveError>
```

| Surface | Intent | Resolver package |
|---------|--------|------------------|
| C# / F# | `CodeSymbol` | `LanguageIntelligence.Adapters.Roslyn` / FCS |
| XML / csproj | `TreeNode` | `LanguageIntelligence.Adapters.Xml` (tree; legacy `X:` via LegacyWire) |
| Markdown ADR | `TreeNode` (block id / §) | `LanguageIntelligence.Adapters.Markdown` |
| HTML | `TreeNode` | `LanguageIntelligence.Adapters.Html` |
| Plain | `TextRange` | `LanguageIntelligence.Line` |
| Evidence | `Evidence` | `LanguageIntelligence.Evidence` (IdeReport / invariant finding) |

LRC ([0061](./GUIDERS-ADR-0061-language-resolver-center.md)) and LanguageIntelligence **share** `doc_id` / revision from buffer host; LRC does not define Anchor wire.

### 6. Edit plane integration (buffer / sniper / txn)

Aligns with BUF-001 modeling direction:

```text
begin(txn) → apply(intent, edit)* → commit
```

- Mutate ops reference **AnchorIntent** or resolved **Locus**, never full-file body for existing docs.
- Sniper `place` = `Before` \| `After` \| `Into` \| `Replace` on resolved locus.
- `commit` → IDE session `FileChange` ([0062](./GUIDERS-ADR-0062-ide-solution-session-orchestrator.md) scope §5.2).

### 7. Package map

| Layer | Package | Repo |
|-------|---------|------|
| IR (`AnchorIntent`, `Locus`, `NodeId`, `DocumentSurface`) | `AIGuiders.Platform.Modeling.LanguageIntelligence.Anchors` | guiders-fsharp |
| Bracket notation | `AIGuiders.Platform.Modeling.Notations.Bracket` | guiders-fsharp (exists) |
| Resolvers + buffer bridge | `AIGuiders.Platform.Execution.LanguageIntelligence.*` | guiders-platform |
| MCP serialize | CDP `cdp_buffer` / MetaToolCatalog | cdp-mcp |

**Dependency:** `Execution.LanguageIntelligence` → `Modeling.LanguageIntelligence.Anchors` + `Modeling.Notations.Bracket`. Planets implement hosts; federation owns contracts.

---

## Migration phases

| Phase | Deliverable |
|-------|-------------|
| **A0** (now) | This ADR; BUF-001 + agent-notes capture |
| **A1** | F# `AnchorIntent`, `Locus`, `DocumentSurface`, `NodeId` + golden round-trip JSON |
| **A2** | `cdp_buffer` accepts JSON `anchor` field alongside `edit_op=anchor` string; refuse large `set_text` |
| **A3** | Xml + Markdown tree resolvers; `TreeNode` for ADR/csproj |
| **A4** | CSX public API rename `Bracket` → `Anchor`; `ToWire()` retained |
| **A5** | Deprecate teaching bracket string as primary in agent canon |

**Not gated on:** IDE session orchestrator Phase 2, ANUI, GDL→IL.

---

## Consequences

### Positive

- One edit locus model across C#, XML, MD, HTML, evidence.
- Session-stable `NodeId` reduces line-fragile edits.
- Structured MCP transport fixes large-payload path (ops, not blobs).
- Pre-Federation `X:`/`F:`/`M:` contained in `LegacyWire` profile.

### Negative / cost

- Host must maintain `surface_version` + node index.
- Dual transport during migration (JSON + bracket).
- F# modeling package + resolver adapters are real work.

---

## Non-goals

- Replacing LSP positions or Roslyn `Location` internally — adapters map **to** `Locus`.
- Universal XPath/string path as Federation SSOT.
- Regex-based markdown/XML mutate.
- Merging LRC and LanguageIntelligence into one guild.

---

## Open questions

1. **NodeId stability** on concurrent edit — bump `surface_version` vs CRDT (v1: single-writer txn).
2. **MdBlockAst** inline markup — token stream vs full AST depth.
3. **Cross-file** `CodeSymbol` — session graph project boundary ([0062](./GUIDERS-ADR-0062-ide-solution-session-orchestrator.md)).

---

## 8. Post-Modeling rethink horizon

Pre-Federation code was built **without** a federation Modeling layer — tactical wires, planet-local DTOs, stringly MCP. Now `Platform.Modeling.*` (F#) is the **Enterprise**; pre-Federation artifacts are **shims until normalized**, not permanent second-class citizens.

| Pre-Federation | Modeling-era target |
|----------------|---------------------|
| `LegacyWire` / bracket axes | `WireDecode` at host boundary → **only** typed `AnchorIntent` inside |
| `X:` csproj path string | `TreeNode` on `XmlTree` |
| `set_text` blob mutate | txn + span ops on `DocumentSurface` |
| `SessionPolicy` bag | policy on graph \( \psi, \lambda, \rho_0 \) ([0062](https://github.com/AI-Guiders/guiders-fsharp/blob/main/docs/math/ide-session-axioms-v0.md)) |
| `IdeWorkspaceWarm` / MSBuild SSOT | session orchestrator + ports |
| Planet-local probe sidecars | capability attributes on graph |

**Naming:** prefer `WireProjection` / `BracketCompat` over «Legacy» in new code once A2 ships; keep `LegacyWire` in ADR until rename lands.

**Rule:** nothing is sacred except **conformance contracts**. If Modeling can express it better — migrate; keep wire adapters thin at the hull, not in the cargo bay.

---

## 9. Federation TO-BE supersession (2026-09-17)

[Model execution split audit](https://github.com/AI-Guiders/guiders-fsharp/blob/develop/docs/federation/model-extraction-living-matrix.md) replaces **Anchor** as primary public entity with **`RelationSpec`** + graph edge `Relation`:

| Pre-0063 (this ADR v1) | TO-BE SSOT |
|------------------------|------------|
| `AnchorIntent` sum | `RelationSpec` cases (`CodeEdit`, `DocToCode`, `Diag`, `Nav`, `Resource`, …) |
| `NavigationAnchor` (platform) | **deleted** — `NavSeed` in `Navigation` + `Relations` |
| `ResolveTier.Text` | **deleted** — stored `Locus` = `Syntax` \| `Semantic` only |
| `LanguageIntelligence.Anchors` package | **`LanguageIntelligence.Relations`** |
| Bracket `[F:…;M:…;L:…]` agent model | `Kind:` canon wire → `RelationSpec` at parse boundary only |

**Attach UX** (plan §4): human verbs → `AttachSchema` steps → `RelationSpec` witness → `Relation` in session graph `G`. Bracket remains **WireProjection**, not SSOT.

Implementation status: Phase 1 Relations kernel **shipped**; Navigation.Code **NavSeed-only** on `develop`; FCS host IO **shipped** @ Execution; legacy wire cutover complete (`RelationWireBoundary` @ Execution; Kind: canon in Modeling); FCS session patch apply IO **shipped** @ Execution (`FcsSessionPatchApplier`); 0063 product phases (A2–A5) remain paused until split-audit checklist closes.

---

## 10. TO-BE normative (RelationSpec era)

This section is the **current** federation contract for edit locus, attach, and wire. It aligns with [model execution split audit](https://github.com/AI-Guiders/guiders-fsharp/blob/develop/docs/federation/model-extraction-living-matrix.md) and math ledger field **`relation_spec_i`** ([ide-session §12](https://github.com/AI-Guiders/guiders-fsharp/blob/develop/docs/math/ide-session/12-relation-spec.md)).

### 10.1 Entity model

Three layers — do not collapse (witness → ephemeral outcome → persistent edge):

```text
RelationSpec       typed witness (partial intent before interpret)
      │ Resolve(spec, ctx)     ⇀  Locus | NavSeed | Artifact     (ephemeral)
      │ Materialize(spec, ctx) ⇀  Relation ⊆ G                   (persistent u R v)
      ▼
BufferEditOutcome / TextEdit / Navigation.Scene projection
```

| Type | Role | Package |
|------|------|---------|
| **`RelationSpec`** | Serializable witness; bracket/JSON are projections | `Modeling.LanguageIntelligence.Relations` |
| **`Locus`** | Resolved span @ revision; **`Syntax` \| `Semantic` only** (no Text tier) | Relations |
| **`Relation` / `RelationType`** | Materialized typed edge in session graph `G` | `Modeling.Ide.Session` |
| **`NavSeed`** | Navigation entry (path + optional line/command) | Relations + `Navigation` |
| **`ResolveCtx`** | Active doc, registry, diagnostic index, max tier | Relations (types); Execution (interpret) |

**Public naming:** teach agents **`RelationSpec`** and attach verbs — not bracket axes, not `AnchorIntent`, not `NavigationAnchor` (deleted).

**Witness vs graph:** `RelationSpec` is intent before interpret; **`Relation` in G** is SSOT for persisted correspondence. `Navigation.Scene.Edge.Kind` is a **derived label** from `G` — not a parallel edge SSOT.

### 10.2 RelationSpec cases (sketch)

Normative shape lives in F# Relations kernel; cases include:

| Case | Resolve / materialize |
|------|------------------------|
| `CodeEdit` | active buffer → `Locus` |
| `DocToCode` | document place → `Relation` (correspondence) |
| `Diag` | `DiagnosticRef` → `Locus` |
| `Address` | `AddressRef` → artifact (optional hint) |
| `Nav` | `NavSeed` → scene / action |
| `Resource` | resource path (+ optional code tail) |

**CodeTarget policy:** persisted targets = `Symbol` \| `TreeNode` only. Line hints snap to AST; snap failure → `Result.Error` (no Text-tier fallback).

**Path policy ([ADR-0050](./GUIDERS-ADR-0050-paths-guild-logical-physical.md)):** wire `File:` → `LogicalPath.Create` at parse boundary; kernel forbids bare `string` paths.

### 10.3 Transport

| Path | When |
|------|------|
| JSON `RelationSpec` | MCP tools, CSX builders, agents (primary) |
| Bracket **`Kind:`** canon | md/prose; `[Kind:CodeEdit; File:…; Member:…]` |
| Legacy `F:`/`M:`/`L:` | **boundary parse only** — `RelationWireBoundary` @ Execution; Kind: canon in Modeling |

Delete from agent canon: bracket as entity, `NavigationAnchor`, `AnchorIntent`, `ResolveTier.Text`, Family/FRG axis routers.

### 10.4 Resolvers (Execution)

```text
IResolveRelation / IMaterializeRelation
  Resolve(spec, ctx)     → Result<Locus, _>
  Materialize(spec, ctx) → Result<Relation, _>
```

| Surface | Spec case | Execution package |
|---------|-----------|-------------------|
| C# / F# | `CodeEdit`, `CodeTarget.Symbol` | `Execution.LanguageIntelligence.Adapters.*` |
| XML / csproj | `TreeNode` | Xml adapter |
| Markdown ADR | `DocToCode` / `TreeNode` | Markdown adapter |
| Diagnostics | `Diag` | ingest → `DiagnosticIndex` → resolve |
| Navigation | `Nav` | `Navigation.Code` + scene projection |

LRC ([0061](./GUIDERS-ADR-0061-language-resolver-center.md)) ingests diagnostics; **`LanguageDiagnostic.Id`** = rule code (CS0246), not session `DiagnosticRef`.

### 10.5 Attach UX (human)

Machine layer = `RelationSpec` + optional `ToWire` / `ToJson`. Human attach = CommandPlane pipeline:

```text
attach [verb]  →  AttachSchema steps  →  RelationSpec + preview label
```

Verbs: `error` → `Diag`; `issue` → `Resource`; `document` → `DocToCode`; `code` → `CodeEdit`; `nav` → `Nav`; `manual` → Kind picker + browse-all.

Gestural bypass: squiggle → `Diag`; sniper → `CodeEdit`; MdLinker → `DocToCode`.

### 10.6 Package map (TO-BE)

| Layer | Package | Repo |
|-------|---------|------|
| IR (`RelationSpec`, `Locus`, `NavSeed`, identity) | `Modeling.LanguageIntelligence.Relations` | guiders-fsharp |
| Graph algebra (`Relation`, `RelationType`) | `Modeling.Ide.Session` | guiders-fsharp |
| Bracket wire (`Kind:` canon) | `Modeling.Notations.Bracket` | guiders-fsharp |
| Attach schema | `Modeling.CommandPlane` | guiders-fsharp |
| Resolvers + registry + attach brokers | `Execution.LanguageIntelligence.Relations` | guiders-platform |
| FCS host + probe + patch apply IO | `Execution.Language.Adapters.Fcs` | guiders-platform |
| MCP serialize | CDP `cdp_buffer` / MetaToolCatalog | cdp-mcp |

**Dependency:** Execution → Modeling Relations + Notations.Bracket. Modeling **never** calls `File.*`, MSBuild, or FCS host threads.

### 10.7 Migration phases (split-audit aligned)

| Phase | Deliverable | Status @ develop |
|-------|-------------|------------------|
| **R1** | Relations kernel + Kind: wire + NavSeed | **shipped** |
| **R2** | Scene projection; attach schema + contextual pickers | **shipped** |
| **R3** | FCS host/probe/projinfo IO @ Execution | **shipped** |
| **R4** | Legacy wire shim delete; ADR/math amend; relation-spec-witness conformance | **shipped** (math §12 + `relation-spec-witness.spec.json`; legacy F/M/L parse boundary only @ Execution) |
| **R5** | Modeling tree renames (§6 plan); residual Execution IO trim | **in progress** (RelationSeamRegistry + Roslyn profile registration ✓; E_dep ingest + FcsLanguageBackend trim remain) |
| **A2–A5** | CDP JSON anchor field, CSX rename, agent canon | **paused** until R5 green |

### 10.8 Non-goals (unchanged from v1)

- Replacing LSP positions or Roslyn `Location` internally — adapters map **to** `Locus`.
- Universal XPath/string path as Federation SSOT.
- Regex-based markdown/XML mutate.
- Merging LRC and LanguageIntelligence into one guild.

---

*Supersedes informal anchor guidance in pre-Federation CDP docs for **entity semantics**; [0025](./GUIDERS-ADR-0025-language-intelligence-boundary.md) guild boundary remains. Bracket notation: [0026](./GUIDERS-ADR-0026-notations-bracket-branch.md).*
