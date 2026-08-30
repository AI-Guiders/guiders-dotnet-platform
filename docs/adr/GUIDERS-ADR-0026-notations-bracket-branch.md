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
| CSX Anchor | `[F:file.cs;M:Foo]` | CIDE CSX (planet) | locate / sniper corridor |
| Keyboard square | `[inner]` (Vim chord) | `VimChordNotationParser` | Melody / Vim reader |

Operator insight (2026-08-30): **bracket lexing is notation**, not language intelligence. Same hyperlane as Keyboard / Command / Argument:

```text
WIRE + profile  →  IR (NormalizedBracketWire)  →  MECHANIC / resolve
```

**LanguageIntelligence** resolves **meaning** (Anchor → `Locus`, tier). **Notations.Bracket** parses **surface** only.

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

### 2. General wire contract (federation SSOT)

Bracket is **not** a fixed `[` `]` grammar. Federation ships a **parameterized contract**; **planets** (and conformance specs) supply **profiles**:

| Field | Role | Default (CSX-style) |
|-------|------|---------------------|
| **StartTerminal** | opening delimiter | `[` |
| **EndTerminal** | closing delimiter | `]` |
| **AxisSeparator** | splits **axes** inside the pair | `;` |
| **PairDelimiter** | splits **key** vs **value** within an axis | `:` |
| **AxisShape** | how inner tokenizes into axes | `KeyValue` |

Parsed IR:

```text
NormalizedBracketWire
├── ProfileId          (which contract instance was used)
├── Axes[]           ordered axis records
│   └── BracketAxis  { Key, Value }   // KV pair per axis when AxisShape = KeyValue
└── Raw                original wire
```

Example CSX anchor profile (`Start=`[`, End=`]`, `;`, `:`):

```text
[F:Program.cs;M:Foo.Bar]
  → Axes[0] { Key=F, Value=Program.cs }
  → Axes[1] { Key=M, Value=Foo.Bar }
```

Example keyboard angle profile (`Start=`<`, End=`>`, **Opaque** inner — no axis split):

```text
<C-S-Tab>
  → Axes[0] { Key=_ , Value=C-S-Tab }   // or planet maps inner → Keyboard IR
```

**Rule:** Platform owns **contract + reference parser + IR**. Planets own **profile values**, axis key vocabulary (`F`, `M`, `L`, …), and conformance vectors — same pattern as `wire_class` on Argument.

### 3. Package map (target)

```text
Notations.Bracket (Core)
    BracketNotationProfile, BracketAxis, NormalizedBracketWire
    IBracketNotationReader(wire, profile)
    BracketProfiles.*     well-known federation profiles (optional constants)
    BracketReader         reference lexer (Phase 1+)

Notations.Bracket.All   optional meta-bundle
```

**No** mandatory per-shape NuGet splits — profiles are **IDs** in conformance specs (`notation/bracket-square-kv`, `notation/bracket-angle-opaque`). Products may ship extra profiles locally until promoted to federation.

### 4. Boundary vs LanguageIntelligence

| Layer | Owns |
|-------|------|
| **Notations.Bracket** | Terminals, axis split, KV split, `NormalizedBracketWire` |
| **LanguageIntelligence.Anchors** | Axis **meaning** (`F` → file path), `IAnchorResolver`, `Locus`, `ResolveTier` |
| **CDP / CIDE** | Live buffer, EditSniper UI, CSX public `Anchor` entity name |

`LanguageIntelligence.AnchorWire` (Phase 0 stub) remains a resolve input bag; Phase 2 handoff is **`NormalizedBracketWire`** + profile id.

### 5. Migration phases

| Phase | Action |
|-------|--------|
| **0 (now)** | This ADR; Core IR (`BracketNotationProfile`, `BracketAxis`, …) |
| **1** | `BracketReader` implements contract; Keyboard.Quarry angle path uses **Opaque** profile; behavior unchanged |
| **2** | Conformance `notation/bracket-square-kv` (CSX anchor fixtures); LI resolver consumes `Axes[]`, not raw parse |
| **3** | Planet-specific profiles promoted to federation only when vectors exist |

### 6. Conformance backlog

| Spec | Profile | Proves |
|------|---------|--------|
| `notation/bracket-square-kv` | `[` `]` `;` `:` | axis + KV split |
| `notation/bracket-angle-opaque` | `<` `>` opaque | inner blob (keyboard quarry) |
| `language-intelligence/anchor-resolve` | (LI) | `Axes[]` → locus + tier |

---

## Non-goals

- Federation SSOT for axis **key semantics** (`F`, `M`, `L`) — planet / LI adapter
- Replacing `NormalizedKeySequence` — angle brackets feed Keyboard, not a parallel keyboard IR
- Anchor semantic resolve inside Notations (Roslyn stays LI adapter)
- One global delimiter table for all domains (profiles differ)

---

## Consequences

- One parser engine, many profiles — less duplication between CSX, sniper pad, keyboard quarry
- ADR-0025 Phase 2 splits: **parse** (Notations axes) vs **resolve** (LI)
- Forge / CIDE native ports implement **contract + profile table**, not forked lexers

---

## References

- `QuarryBracketTokenParser`, `VimChordNotationParser` — seeds for opaque / square profiles
- CIDE Anchor rename (Bracket → Anchor entity; wire stays bracket-shaped)
- [GUIDERS-ADR-0025 LanguageIntelligence boundary](GUIDERS-ADR-0025-language-intelligence-boundary.md)
