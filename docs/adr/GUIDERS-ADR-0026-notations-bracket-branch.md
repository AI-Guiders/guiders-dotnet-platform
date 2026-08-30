# GUIDERS-ADR-0026: Notations.Bracket branch · paired-delimiter wire quarry

| | |
|---|---|
| **Status** | Accepted (Phase 0 — plan + Core IR stub) |
| **Date** | 2026-08-30 |
| **Tags** | #guiders #notations #bracket #quarry #anchor #keyboard |
| **Relates to** | [GUIDERS-ADR-0021](GUIDERS-ADR-0021-notations-quarry-family.md) · [GUIDERS-ADR-0025](GUIDERS-ADR-0025-language-intelligence-boundary.md) · [GUIDERS-ADR-0016](GUIDERS-ADR-0016-input-notation-quarry-family.md) |

---

## Context

**Bracket** appears in multiple federation wires today — always as **paired delimiters + inner payload**, but parsers are duplicated:

| Wire | Example | Today | Consumer |
|------|---------|-------|----------|
| Keyboard angle | `<C-k>`, `<S-Tab>` | `QuarryBracketTokenParser`, Vim grammar | `Notations.Keyboard.*` → chord step |
| Keyboard square | `[inner]` (Vim chord) | `VimChordNotationParser.ParseBracketInner` | Melody / Vim reader |
| CSX Anchor | `[F:file.cs;M:Foo]` | CIDE CSX (planet) | locate / sniper corridor |
| Sniper pad | `L:12-34` (defer) | CDP EditSniper | corridor scope |

Operator insight (2026-08-30): **bracket lexing is notation**, not language intelligence. Same hyperlane as Keyboard / Command / Argument:

```text
WIRE (paired delimiters)  →  IR (NormalizedBracketWire)  →  MECHANIC / resolve
```

**LanguageIntelligence** resolves **meaning** (Anchor → `Locus`, tier). **Notations.Bracket** parses **surface** (delimiters, inner text, slot split).

---

## Decision

### 1. Fourth Notations branch

Extend [ADR-0021](GUIDERS-ADR-0021-notations-quarry-family.md) umbrella:

```text
Notations
├── Keyboard.*
├── Command.*
├── Argument.*
└── Bracket.*          ← NEW (this ADR)
```

### 2. Core IR

```text
Notations.Bracket (Core)     NormalizedBracketWire, BracketPairKind, BracketSlot, IBracketNotationReader
    ├── .Angle               `<…>` keyboard special-key (migrate QuarryBracketTokenParser)
    ├── .Square              `[…]` generic inner (Vim chord subset)
    ├── .Anchor              CSX `[F:…;M:…]` slot grammar → slots + inner
    └── .All                 optional meta-bundle
```

```csharp
record NormalizedBracketWire(
    BracketPairKind Pair,           // Angle | Square
    string Inner,
    IReadOnlyList<BracketSlot>? Slots,
    string? Raw);

record BracketSlot(string Key, string Value);
```

**Compose with other branches:** Keyboard readers may call `Bracket.Angle` internally; Anchor builders emit raw wire → `Bracket.Anchor` reader → LI resolver.

### 3. Boundary vs LanguageIntelligence

| Layer | Owns |
|-------|------|
| **Notations.Bracket.*** | Delimiter pair, inner string, slot tokenization (`;`, `:` rules per dialect) |
| **LanguageIntelligence.Anchors** | `AnchorIntent`, `IAnchorResolver`, `Locus`, `ResolveTier` |
| **CDP / CIDE** | Live buffer, EditSniper UI, CSX public `Anchor` entity name |

`LanguageIntelligence.AnchorWire` (Phase 0 stub) remains the **resolve input bag**; Phase 2 prefer **`NormalizedBracketWire`** from Notations as the typed handoff (raw string fallback ok for one release).

### 4. Migration phases

| Phase | Action |
|-------|--------|
| **0 (now, v0.19.1)** | This ADR; `Notations.Bracket` Core stub; cross-links in ADR-0021/0025 |
| **1** | Shared angle lexer in `Notations.Bracket.Angle`; Keyboard.Quarry delegates (no behavior change) |
| **2** | `Notations.Bracket.Anchor` + conformance `notation/bracket-anchor`; LI `IAnchorResolver` consumes IR |
| **3** | Optional `Bracket.Square` unify Vim inner parse; sniper `L:` wire as product or `Bracket.Corridor` defer |

### 5. Conformance backlog

| Spec | Proves | Phase |
|------|--------|-------|
| `notation/bracket-angle` | `<C-a>` → inner + modifier split | 1 |
| `notation/bracket-anchor` | `[F:x;M:y]` → slots | 2 |
| `language-intelligence/anchor-resolve` | normalized wire → locus + tier | 2 (ADR-0025) |

---

## Non-goals

- Replacing `NormalizedKeySequence` — angle brackets stay **input** to keyboard IR, not a second keyboard IR
- Anchor semantic resolve inside Notations (Roslyn stays LI adapter)
- Mandating one delimiter pair for all domains

---

## Consequences

- Keyboard quarry and CSX anchor share **bracket lexicon** docs + optional shared lexer — less drift between Neovim oracle and C# quarry
- ADR-0025 Phase 2 anchor wire conformance splits: **parse** (Notations) vs **resolve** (LanguageIntelligence)
- Forge / CIDE native ports target **notation/bracket-*** vectors first

---

## References

- `QuarryBracketTokenParser`, `VimChordNotationParser` — angle/square seeds
- CIDE Anchor rename (Bracket → Anchor entity; wire stays bracket-shaped)
- [GUIDERS-ADR-0025 LanguageIntelligence boundary](GUIDERS-ADR-0025-language-intelligence-boundary.md)
