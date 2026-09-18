# GUIDERS-ADR-0066: Code Center — federation multi-projection edit host

| | |
|---|---|
| **Status** | **Accepted** (product charter; implementation Phase 0) |
| **Date** | 2026-09-17 |
| **Tags** | #guiders #federation #code-center #surface #projection #language-intelligence #avalonedit #editor |
| **Related** | [0025](./GUIDERS-ADR-0025-language-intelligence-boundary.md) · [0021](./GUIDERS-ADR-0021-notations-quarry-family.md) · [0061](./GUIDERS-ADR-0061-language-resolver-center.md) · [0062](./GUIDERS-ADR-0062-ide-solution-session-orchestrator.md) · [0063](./GUIDERS-ADR-0063-anchors-federation-reincarnation.md) · [0067](./GUIDERS-ADR-0067-language-profile-federation-model.md) · [0055](https://github.com/AI-Guiders/guiders-wpf/blob/main/docs/adr/GUIDERS-ADR-0055-surface-wpf-guild-deck-authoring.md) · [STUDIO-ADR-0005](https://github.com/AI-Guiders/dash-spec-studio/blob/main/design/STUDIO-ADR-0005-model-first-language-editor.md) · [Constitution](../GUIDERS-FEDERATION-CONSTITUTION.md) |

## Context

Federation hosts still treat **plain text + AvalonEdit** as the default “editor product”. Planets wire `TextEditor`, TextMate, or ad-hoc `Classify(text)` per feature. That model breaks when:

| Habit | Problem |
|-------|---------|
| Text buffer = SSOT | Large-doc MCP transport; no stable node ids ([0063](./GUIDERS-ADR-0063-anchors-federation-reincarnation.md) §Context) |
| One visual = one file | Diagrams, trees, forms are second-class or duplicate parsing |
| AvalonEdit as public API | Planets fork highlight/undo/IME independently (RoslynPad pattern repeated N times) |
| `Classify(text)` on keystroke | Syntax/semantic work duplicated outside session revision |

Operator direction (2026-09-17): if **text is a projection** and the **graph/model is SSOT**, the host must render **any projection** that walks the same revision-stable model — class diagram, spec tree, property sheet, preview — not only monospace text.

**Code Center** names the federation product that replaces “standard AvalonEdit integration” as the edit **presentation plane**. AvalonEdit remains **substrate** for the text projection only (MIT baseline → in-repo fork), analogous to RoslynPad: heavily customized under a federation-owned surface, not consumed as the editor itself.

**Sibling products (do not merge):**

| Product | Owns |
|---------|------|
| **LRC** ([0061](./GUIDERS-ADR-0061-language-resolver-center.md)) | Workspace verbs: diagnostics, outline, go-to-def, find-usages |
| **LanguageIntelligence** ([0025](./GUIDERS-ADR-0025-language-intelligence-boundary.md)) | Edit intelligence: Anchor, Locus, Sniper, buffer mutate tiers |
| **IDE session orchestrator** ([0062](./GUIDERS-ADR-0062-ide-solution-session-orchestrator.md)) | Solution/project/document session graph |
| **Code Center** (this ADR) | Multi-projection edit **host** + revision bus + surface registry |

LRC answers *what symbol*; LanguageIntelligence answers *where/how to edit*; Code Center answers *how to show and interact* across projections bound to one session revision.

---

## Decision

### 1. Code Center (normative definition)

**Code Center** = federation product: a **document session host** that binds one or more **projection surfaces** to a shared **model revision**, resolves **AnchorIntent → Locus** ([0063](./GUIDERS-ADR-0063-anchors-federation-reincarnation.md)), and delegates **mutations** to LanguageIntelligence + planet buffer plane.

```text
                    ┌─ TextSurface        (AvalonEdit fork — TextEngine peel)
                    ├─ DiagramSurface     (class / ER / flow — graph visitor)
IDocumentSession ───┼─ TreeSurface        (outline / spec browser)
  revision, NodeId  ├─ FormSurface        (property sheet)
                    └─ PreviewSurface     (runtime / viz)
                              │
              AnchorIntent ──► Locus { tier, nodeId, span? }
                              │
              mutate ──► graph txn ──► revision++ ──► all surfaces refresh
```

**Rules:**

- Planets **must not** reference AvalonEdit as their editor integration point; they reference **Code Center / projection contracts**.
- Text is **one** projection; deprecating text-only hosts is intentional migration, not a side effect.
- Every surface subscribes to the **same revision**; stale cross-surface selection is a conformance bug.
- Projections follow federation pattern from [0021](./GUIDERS-ADR-0021-notations-quarry-family.md): *surface view → shared model*, not independent grammars.

### 2. Entity model

Three layers — align with [0063](./GUIDERS-ADR-0063-anchors-federation-reincarnation.md); Code Center adds presentation:

```text
GraphModel / IDocumentSession     SSOT (F# Modeling on planet or federation kernel)
      │ serialize / project
      ▼
ProjectionDescriptor[]            kind, capabilities, preferred tier
      │ render / input
      ▼
IProjectionSurface (WPF / headless)  Code Center host slot
      │ user/agent intent
      ▼
AnchorIntent → Locus → mutate
```

| Type | Role |
|------|------|
| **IDocumentSession** | Federation-facing session: `Revision`, graph access, `TryResolve(AnchorIntent)`, projection metadata |
| **IProjectionSurface** | One visual/interaction surface (text, diagram, tree, …) |
| **ProjectionKind** | `Text` \| `Diagram` \| `Tree` \| `Form` \| `Preview` \| … (extend by sum, not string soup) |
| **CodeCenterHost** | WPF (or headless) registry: bind session, attach/detach surfaces, revision bus |

`ILanguageDocumentSession` ([guiders-wpf Abstractions](https://github.com/AI-Guiders/guiders-wpf)) is **Phase 0 text-only** subset of `IDocumentSession`; rename/evolve in Phase 1, not parallel SSOT.

### 3. Package model (target)

Follow Modeling / Execution split ([GUIDERS-FSHARP-ADR-0002](https://github.com/AI-Guiders/guiders-fsharp/blob/main/docs/adr/GUIDERS-FSHARP-ADR-0002-model-guild-fsharp-ownership.md)):

```text
guiders-fsharp (Modeling)
  AIGuiders.Platform.Modeling.CodeCenter          F# — session envelopes, ProjectionKind, revision events
  (+ planet Modeling.* e.g. DashSpec.Modeling.LanguageEditor — domain graph)

guiders-platform (Execution)
  AIGuiders.Platform.Execution.CodeCenter         C# — headless host, surface registry (future)

guiders-wpf (Surface)
  AIGuiders.Surface.Wpf.CodeCenter                WPF — CodeCenterHost, zone integration
  AIGuiders.Surface.Wpf.TextEngine                TextSurface v0 (AvalonEdit fork root; see §4)
  AIGuiders.Surface.Wpf.Abstractions              IDocumentSession / IProjectionSurface contracts
```

**Rule:** one Code Center road; planets ship **adapters** (`DashSpecLanguageDocumentSession`, diagram visitors), not forked editor hosts.

### 4. AvalonEdit / TextEngine positioning

| Layer | Owner | Notes |
|-------|-------|-------|
| AvalonEdit (fork substrate) | **internal** to TextEngine | IME, undo stack, caret, line rendering only |
| `LanguageTextEditor` | TextEngine | RoslynPad-style customization layer |
| `LanguageEditorHost` | TextEngine v0 | Debounced text ↔ session sync; becomes `TextSurface` adapter |
| **CodeCenterHost** | Surface.Wpf.CodeCenter (Phase 1) | Supersedes “LanguageEditorHost” as product entry |

AvalonEdit is **not replaced** by another text engine; it is **encapsulated**. Federation consumers never “use AvalonEdit”; they use **Code Center → Text projection**.

#### 4.1 AvalonEdit upstream policy (Phase 4 — operator 2026-09-18)

TextEngine consumes **`federation-text-surface`** (`AIGuiders.TextSurface.Substrate`) — AvalonEdit pinned via git submodule, not NuGet. Phase 4 **two-repo** upstream watch model:

| Repo | Role | Rule |
|------|------|------|
| **`AI-Guiders/AvalonEdit`** | Mirror-fork of [icsharpcode/AvalonEdit](https://github.com/icsharpcode/AvalonEdit) | **No federation patches.** Sync tags/main only; CI opens bump PRs on upstream releases. |
| **`AI-Guiders/federation-text-surface`** (or submodule path in `guiders-wpf`) | Product substrate | Git **submodule** → mirror at pinned tag; `LanguageTextEditor` + minimal patch queue live here. Namespace stays `ICSharpCode.AvalonEdit` (merge-friendly). |

**Bump ritual:** upstream release → sync mirror → bump submodule tag → build TextEngine/CodeCenter → Studio smoke → living matrix note.

**Do not:** rename upstream types in the mirror; mix federation patches into the mirror-fork; add direct `PackageReference AvalonEdit` or `using ICSharpCode.AvalonEdit` on planets — use CodeCenter/TextEngine facades (`TextCompletionPresenter`, `CodeCenterEditorControl` input hooks).

Optional internal NuGet (`AIGuiders.TextSurface.AvalonEdit`) may wrap the submodule build for consumers that do not vendor source.

### 5. Resolve tiers across projections

From [0025](./GUIDERS-ADR-0025-language-intelligence-boundary.md):

| Projection | Typical tier | Example |
|------------|--------------|---------|
| TextSurface | Text → Syntax | caret offset, token highlight |
| TreeSurface | Syntax | block outline |
| DiagramSurface | **Semantic** | type box ↔ `TreeNode` / `CodeSymbol` |
| FormSurface | Semantic | property grid on model node |

Cross-projection navigation: click diagram box → `AnchorIntent.TreeNode` → TextSurface scrolls/opens locus on same `NodeId` (conformance vector required).

### 6. Migration (normative path)

| Phase | Deliverable | Status |
|-------|-------------|--------|
| **0** | TextEngine peel + `ILanguageDocumentSession` + planet F# session (syntax tree) | **Shipped** — DashSpec Studio ([STUDIO-ADR-0005](https://github.com/AI-Guiders/dash-spec-studio/blob/main/design/STUDIO-ADR-0005-model-first-language-editor.md)) |
| **1** | `CodeCenterHost`, rename mental model TextEngine → TextSurface, `IDocumentSession` superset | **Shipped** — `Surface.Wpf.CodeCenter` + `SessionLocusResolver` (ship-62) |
| **2** | Second projection (diagram or tree) on same session in one planet (DashSpec) | **Shipped** — `DashSpec.CodeCenter.Plugin` + V12 (ship-62f) |
| **3** | Semantic-tier session; bracket wire read-only shim | **Shipped** — `Platform.Modeling.CodeCenter` Λ_doc slice @ [ide-session §2.12](https://github.com/AI-Guiders/guiders-fsharp/blob/develop/docs/math/ide-session/07-revision-ledger.md#212-revision-ledger-δ-stream--git-subgraph) (ship-62d) |
| **4** | Submodule mirror-fork + deprecate NuGet/direct AvalonEdit refs in federation samples | **Shipped** — `federation-text-surface` @ v6.3.1, TextEngine substrate peel, Studio via CodeCenter facade (ship-avalonedit-phase4) |

**Deprecation:** new planet code **must not** add AvalonEdit package references; use TextEngine until CodeCenter package ships, then CodeCenter.TextSurface.

#### 6.1 Document-scope ledger slice (CodeCenter)

File-scope editor sessions inherit ide-session **§2.12** replay undo — not a simplified delta stack:

```text
εᵢ = (rᵢ, scopeᵢ, θᵢ, Δᵢ, anchorᵢ, γᵢ)     // LedgerEntryDoc
Λ_doc = committed entries only; ephemeral mechanical outside Λ until save batch
tryUndo = replayToRevision(n−1) — Δ-replay or Re-plan per RePlannableΘ registry (LD3)
```

SSOT: `Platform.Modeling.CodeCenter` (`LedgerEntryDoc`, `RePlannableThetaRegistry`, `DocumentSession`). Solution-level Λ ingress (LD4) reuses `Platform.Modeling.Ide.Session.RevisionLedger` — doc slice, not parallel IR.

### 7. Conformance

Vectors (minimum):

1. **Revision sync** — edit in TextSurface bumps revision; DiagramSurface reflects within one bus tick.
2. **Anchor round-trip** — `TreeNode` from diagram resolves to same locus as tree outline + text caret.
3. **No text SSOT** — structured mutate via LanguageIntelligence; full-buffer `set_text` is legacy path only.
4. **Tier honesty** — diagram conformance uses Semantic tier; do not fake with line/column.

---

## Consequences

- **Positive:** One federation editor product; multi-view authoring without duplicate parsers; aligns Agents with AnchorIntent ([0063](./GUIDERS-ADR-0063-anchors-federation-reincarnation.md)).
- **Cost:** Phase 1–2 require host refactor (LanguageEditorHost → CodeCenterHost); package rename is breaking for early adopters (Studio only today).
- **Planet boundary:** DashSpec semantic graph, GDL trees, GPL backends stay on planets; Code Center sells roads and WPF surfaces only.

---

## Non-goals (this ADR)

- Replacing LRC or merging into LanguageIntelligence
- MPS-style projectional-only editing (text projection remains first-class with roundtrip)
- Glass cockpit embed as SSOT ([0055](https://github.com/AI-Guiders/guiders-wpf/blob/main/docs/adr/GUIDERS-ADR-0055-surface-wpf-guild-deck-authoring.md) — projection, not domain)
- Shipping full AvalonEdit source vendoring in Phase 0

---

## Open items

| # | Question | Default |
|---|----------|---------|
| 1 | Package name: `Surface.Wpf.CodeCenter` vs fold into TextEngine until Phase 2? | Split at Phase 1 host |
| 2 | Headless Code Center for CDP buffer preview? | Execution.CodeCenter mirrors WPF contracts |
| 3 | `IDocumentSession` in Abstractions vs Platform.Modeling.CodeCenter? | Contracts in Abstractions; envelopes in F# Modeling |
