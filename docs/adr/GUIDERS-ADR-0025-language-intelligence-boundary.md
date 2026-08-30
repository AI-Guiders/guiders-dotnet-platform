# GUIDERS-ADR-0025: LanguageIntelligence family · migration from CommandPlane editor quarry

| | |
|---|---|
| **Status** | Accepted (migration plan — **Phase 0**; editor quarry still in Slash until Phase 1) |
| **Date** | 2026-08-30 |
| **Tags** | #guiders #language-intelligence #anchor #sniper #commandplane #editor #breaking-planned |
| **Relates to** | [GUIDERS-ADR-0009](GUIDERS-ADR-0009-command-surface-pattern.md) · [GUIDERS-ADR-0010](GUIDERS-ADR-0010-platform-mechanics.md) · [GUIDERS-ADR-0021](GUIDERS-ADR-0021-notations-quarry-family.md) · [Constitution § Planets are not SSOT](../GUIDERS-FEDERATION-CONSTITUTION.md#planets-are-not-federation-ssot) |

---

## Context

Forge W0 placed a **plain-text editor quarry** inside `CommandPlane.Slash/Editor` (line range, markdown wrap, bundled `Editor*Command`). It validated Catalog · Registry · Command (ADR-0009) but **conflated slash invocation with document edit intelligence**.

Operator review (2026-08-30):

- Line range, sniper, **Anchor** (entity; bracket = wire), buffer mutate live in **CIDE / CDP** (Roslyn, EditSniper, CSX Anchor, buffer plane).
- C# depth comes from **Roslyn**; JS/TS/PS need **tiered adapters**, not one Roslyn everywhere.
- If federation can offer **language-neutral IR + resolve contract + conformance**, Anchors/edit ops belong in **Platform** — in a **sibling guild**, not `CommandPlane.*`.

**Goal:** same hyperlane pattern as `Notations.*` and `CommandPlane.Slash` — wire → IR, vectors, à la carte NuGet — without pretending all languages reach Roslyn semantic depth.

---

## Decision

### 1. New guild: `LanguageIntelligence.*`

Separate from **CommandPlane** (invocation: slash / melody / binding) and from **CDP buffer habitat** (organs, Citizen, Meta wire).

```text
LanguageIntelligence (Core)     Anchor, Locus, TextEdit, ResolveTier, SniperScope, BufferEditOutcome
    ├── .Line                   line-at-offset, line range parse/delete (today: Slash/Editor quarry)
    ├── .Markup                 dialect wrap/insert catalog (today: MarkdownTextDialectCatalog)
    ├── .Anchors                bracket wire, structured builder, IAnchorResolver (Phase 2)
    ├── .Sniper                 scope model + corridor rules (Phase 2; CDP EditSniper aligns)
    ├── .Adapters.Roslyn        C# semantic (optional package)
    ├── .Adapters.TypeScript    tsserver/LSP-shaped (optional)
    ├── .Adapters.TreeSitter    syntax-only multi-lang (optional)
    └── .All                    meta-bundle for tests / full embed
```

**CommandPlane** keeps: slash path → `commandId`, catalog, completion, `PlatformCommandRegistry` + `ICatalogDescribed` bridge.  
**Execution** of buffer edits: product registry calling **LanguageIntelligence** + planet buffer host.

### 2. Resolve tiers (honest cross-language contract)

Platform does **not** promise Roslyn everywhere. Every `Locus` carries:

| Tier | Meaning | Typical adapter |
|------|---------|-----------------|
| **Text** | byte/line range only | plain buffer, PS script block |
| **Syntax** | tree node / span | tree-sitter, partial LS |
| **Semantic** | symbol-bound | Roslyn, tsserver rename/refactor |

Pulse/MCPlane can surface tier in agent ingress (align ADR-0020).

### 3. Anchor model (target)

- **Entity:** `Anchor` (locate locus) — CIDE CSX public name; bracket `[F:…;M:…]` = **wire projection only**.
- **Wire parse:** [GUIDERS-ADR-0026](GUIDERS-ADR-0026-notations-bracket-branch.md) **`Notations.Bracket`** — `BracketNotationProfile` (terminals, `;`, `:`) → `Axes[]`; CSX uses square-kv profile.
- **Resolve:** `AnchorIntent` → `IAnchorResolver` → `Locus` (LanguageIntelligence — **no bracket lexing here**).
- **Not** a slash path; consumed by mutate/sniper/CSX, MCPlane, Forge Lens (FORGE-ADR-0003).

### 4. What stays where (boundary)

| Concern | SSOT |
|---------|------|
| `/editor line delete` path, arg_tail policy | CommandPlane catalog |
| Line delete **mechanics** | LanguageIntelligence.Line |
| Live buffer, undo, sniper UI loop | CIDE / CDP planet |
| Roslyn rename / go-to-def | LanguageIntelligence.Adapters.Roslyn or planet pin |

---

## Migration phases

### Phase 0 — **now** (v0.19.0)

- [x] This ADR (plan SSOT)
- [x] `LanguageIntelligence` Core stubs (IR records, tiers — no behavior yet)
- [ ] Editor quarry **unchanged** in `CommandPlane.Slash/Editor` (marked deprecated in ADR only)
- [ ] Conformance backlog row in `docs/conformance/README.md`

### Phase 1 — extract quarry (target v0.20.0, **breaking**)

Move without semantic change:

| From `CommandPlane.Slash/Editor` | To |
|----------------------------------|-----|
| `EditorLineRangeParser`, `EditorLineTextOps`, … | `LanguageIntelligence.Line` |
| `MarkdownTextDialectCatalog`, `EditorTextTransform`, … | `LanguageIntelligence.Markup` |
| `EditorBufferContext`, `EditorTextEditResult`, … | `LanguageIntelligence.Edit` |
| `Editor*Command`, `EditorCommandRegistry` | **CIDE / Forge product** packages OR `LanguageIntelligence.Bundled` optional quarry |

- `EditorBufferOutcome` → `BufferEditOutcome` in LI.Core; `CommandOutcome` stays success/error (payload via planet or generic bag later).
- `CommandPlane.Slash`: delete `Editor/` tree.
- One-release type forward aliases optional (`[Obsolete]` wrappers in Slash).

### Phase 2 — Anchor + Sniper IR (v0.21+)

- `Notations.Bracket`: profile-driven parse → `NormalizedBracketWire.Axes[]` ([ADR-0026](GUIDERS-ADR-0026-notations-bracket-branch.md)).
- `LanguageIntelligence.Anchors`: `IAnchorResolver`, conformance `language-intelligence/anchor-resolve` (wire parse spec: `notation/bracket-square-kv`).
- `LanguageIntelligence.Sniper`: `SniperScope`, corridor peek rules; align CDP `EditSniper` / CIDE bracket L: line corridor tests.
- CIDE CSX `Anchor.*` builder emits platform `AnchorWire`.

### Phase 3 — Adapters à la carte

- Ship `Adapters.Roslyn` first (C# dogfood).
- JS/TS via LSP client or tree-sitter — **same IR**, lower tier where needed.
- Conformance: per-adapter optional oracle (like keyboard quarry).

### Phase 4 — Registry home (optional)

- Evaluate moving `PlatformCommandRegistry` to `CommandPlane` core (already generic) — **not** into LanguageIntelligence.

---

## Package dependency DAG (target)

```text
LanguageIntelligence (Core)
    ↑
    ├── .Line / .Markup / .Anchors / .Sniper
    └── .Adapters.*  → Core (+ external SDKs)

CommandPlane (Core)          — no reference to LanguageIntelligence
CommandPlane.Slash           — no Editor/; slash only

Products:
  Forge editor executor      → LI.Line + LI.Markup (JS port of vectors)
  CIDE buffer + CSX          → LI.* + Adapters.Roslyn + CDP buffer plane
  Slash catalog              → CommandPlane only
```

---

## Conformance backlog (LanguageIntelligence)

| Spec | Surface | Phase |
|------|---------|-------|
| `language-intelligence/line-range` | parse + delete line range | 1 |
| `language-intelligence/markup-wrap` | bold/wrap transform | 1 |
| `notation/bracket-cdp-square-kv` | CDP profile → axes | 2 ([ADR-0026](GUIDERS-ADR-0026-notations-bracket-branch.md)) |
| `notation/bracket-angle-opaque` | `<…>` opaque inner | 1 |
| `language-intelligence/anchor-resolve` | normalized wire → locus + tier | 2 |
| `language-intelligence/sniper-scope` | from/till/wire | 2 |
| `language-intelligence/locus-tier` | resolver advertises tier | 2 |

---

## Non-goals

- Replacing CDP buffer plane or Citizen organs in platform
- Mandating Roslyn semantic parity on JS/PS
- Putting Anchor resolve inside `SlashLineResolver`
- Deleting editor quarry **without** Phase 1 destination packages (no “delete only”)

---

## Consequences

- **Until Phase 1:** consumers may still reference `CommandPlane.Slash` editor types; treat as **deprecated quarry**.
- **After Phase 1:** breaking NuGet; Forge/CIDE update pins + usings.
- Constitution “roads not domain”: LanguageIntelligence = edit/locate roads; planets = buffer host + UI.

---

## References

- CIDE Anchor rename (Bracket → Anchor), Body.At, EditSniper — agent-notes / cascade-ide
- CDP buffer plane, anchor L: corridor — `cdp-mcp/.cdp/domain/buffer.md`
- Forge W0 editor — ADR-0009 (pattern kept; location moves)
