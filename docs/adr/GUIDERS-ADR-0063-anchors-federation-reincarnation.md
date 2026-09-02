# GUIDERS-ADR-0063: Anchors — Federation reincarnation (v1)

| | |
|---|---|
| **Status** | Accepted (architecture; implementation Phase 0) |
| **Date** | 2026-09-02 |
| **Tags** | #guiders #federation #anchor #language-intelligence #edit-plane #buffer #sniper #first-class |
| **Related** | [GUIDERS-ADR-0025](./GUIDERS-ADR-0025-language-intelligence-boundary.md) · [GUIDERS-ADR-0026](./GUIDERS-ADR-0026-notations-bracket-branch.md) · [GUIDERS-ADR-0027](./GUIDERS-ADR-0027-mdlinker-doc-anchor-check.md) · [GUIDERS-ADR-0061](./GUIDERS-ADR-0061-language-resolver-center.md) · [GUIDERS-ADR-0062](./GUIDERS-ADR-0062-ide-solution-session-orchestrator.md) · [CDP BUF-001](https://github.com/AI-Guiders/agent-notes/blob/main/knowledge/work/projects/door-to-singularity/cdp-mcp/subprojects/cdp-buffer-v1-known-gaps.md) · [Constitution](../GUIDERS-FEDERATION-CONSTITUTION.md) |

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
    | LegacyWire of profile: string * wire: string   // compat only
```

**Rules:**

- **Prefer** `TreeNode` / `CodeSymbol` over `TextRange` when surface provides a tree.
- **No new universal axis letters** (`Y:`, `Z:`, …). New surfaces extend the **sum**, not bracket alphabet soup.
- `LegacyWire` parses through [Notations.Bracket](./GUIDERS-ADR-0026-notations-bracket-branch.md) → `NormalizedBracketWire` → adapter-specific intent (migration path).

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

*Supersedes informal anchor guidance in pre-Federation CDP docs for **entity semantics**; [0025](./GUIDERS-ADR-0025-language-intelligence-boundary.md) guild boundary remains. Bracket notation: [0026](./GUIDERS-ADR-0026-notations-bracket-branch.md).*
