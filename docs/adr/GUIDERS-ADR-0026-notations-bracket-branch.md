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
| **ListSeparator** | splits axes in the pair | `;` |
| **KvSign** | **KV = Key + Sign + Value** (first sign only) | `:` |
| **AxisShape** | how inner tokenizes into axes | `KeyValue` |
| **StripOuterTerminals** | accept inner or wrapped wire | `true` |
| **RespectBracketDepthOnAxisSplit** | `;` only at `[` `]` depth 0 | `true` |
| **NestedAxisKeys** | keys whose value re-parses as bracket | `Anchor` (CDP) |

Parsed IR:

```text
NormalizedBracketWire
├── ProfileId
├── Axes[]
│   └── BracketAxis { Key, Value, Nested? }
└── Raw
```

**Universal KV atom:** `Key` + `Sign` + `Value` — only the **first** `Sign` splits (value may contain more).

| Surface | ListSeparator | KvSign | Example |
|---------|---------------|--------|---------|
| Bracket envelope | `;` | `:` | `[F:file; S:for:2]` |
| Console args | space | `=` | `doc=README op=scene` |
| Inner axis value (pass 2) | — | `:` (planet default) | `for:2` inside `S:` |

Same primitive (`NotationKvPair`); different profile signs. Not separate grammars for «colon vs equals».

### 2.1 Compose path / nested bracket inside values

Bracket list pass yields KV axes. Value pass (optional):

| Example | Pass 2 class |
|---------|----------------|
| `pilot/issues/7` | `command.path` |
| `for:2` | `notation.kv` with `KvSign ':'` |
| `[F:…;M:…]` | `bracket.nested` |
| `12-34` | `line.range` |

Forge compound: `[FRG:pilot/issues/7; F:…]` — FRG value is path; tail axes reuse same bracket profile.

Example CDP profile `bracket.cdp-square-kv`:

```text
[F:Program.cs;M:Foo.Bar;Anchor:[F:Inner.cs;L:10]]
  → Axes[0] { F, Program.cs }
  → Axes[1] { M, Foo.Bar }
  → Axes[2] { Anchor, [F:Inner.cs;L:10], Nested=… }
```

Example keyboard angle profile (`Start=`<`, End=`>`, **Opaque** inner — no axis split):

```text
<C-S-Tab>
  → Axes[0] { Key=_ , Value=C-S-Tab }   // planet maps inner → Keyboard IR
```

**Planet extensions (not federation Core):**

| Extension | CDP/CIDE usage | Owner |
|-----------|----------------|-------|
| `BracketAxisAliasMap` | `F`↔`File`, `M`↔`Member`, case-insensitive | CIDE ADR 0186 / `BracketLocate.AxisAlias` |
| `BracketAxisValuePlan` | `S`→`argument.colon`, `F`→`command.path`, … | Planet profile table |
| H1 layout (space, no `;`) | `[M:Run S:for:2]`, `[file.cs M:Run]` | CIDE — separate profile `bracket.cide-h1` (defer) |
| Family classify (code/xml/nav) | `BracketLocate.ClassifyFamily` | **LanguageIntelligence** / CDP resolve — not notation |

**Rule:** Platform owns **contract + reference parser + IR**. Planets own **profiles**, alias maps, and conformance vectors.

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
| **1** | ✓ `BracketReader` + `NotationKvPair`/`NotationListSplit` in Core; conformance `notation/bracket-cdp-square-kv` |
| **2** | Conformance `notation/bracket-cdp-square-kv`; LI resolver consumes `Axes[]` |
| **3** | Forge `FRG` compound profile; CIDE H1 profile — promote when vectors exist |

### 6. CDP/CIDE audit (2026-08-30)

SSOT parser: `guiders-core` **`BracketLocate`** (`Cdp.ScriptableIde`).

| Behavior | In CDP code | In contract v0.19.4 |
|----------|-------------|---------------------|
| `[` `]` + `;` + first-`:` KV | ✓ `SplitAxes` | ✓ profile defaults |
| Depth-aware `;` split | ✓ `SplitTopLevel` | ✓ `RespectBracketDepthOnAxisSplit` |
| Strip outer `[` `]` | ✓ `StripOuterBrackets` | ✓ `StripOuterTerminals` |
| Nested `Anchor:[…]` | ✓ recursive `Parse` | ✓ `NestedAxisKeys` + `BracketAxis.Nested` |
| Axis aliases F/M/L/… | ✓ `AxisAlias` | ✓ `BracketAxisAliasMap` (planet) |
| Value `:` after first | ✓ `K:Parameter:x`, `S:if:2` | ✓ first-colon rule documented |
| Value compose (path / colon / range) | ✓ ad hoc in resolve | ✓ `BracketAxisValueClasses` + `ValuePlan` |
| Family classify code/xml/nav | `ClassifyFamily` | **LI/CDP** — out of Notations |

EditSniper / peek / land reuse **`BracketLocate.Parse`** — same profile, not a second grammar.

### 7. Conformance backlog

| Spec | Profile | Proves |
|------|---------|--------|
| `notation/bracket-cdp-square-kv` | `bracket.cdp-square-kv` | axes + nested Anchor |
| `notation/bracket-angle-opaque` | `bracket.angle-opaque` | keyboard inner |
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

- `QuarryBracketTokenParser`, `VimChordNotationParser` — opaque profile seeds
- `Cdp.ScriptableIde.BracketLocate` — reference quarry for `bracket.cdp-square-kv`
- CIDE `BracketCodeReferenceParser` — H1 defer profile
- Forge ADR 0159 `[FRG:…]` — compound profile defer
- [GUIDERS-ADR-0025 LanguageIntelligence boundary](GUIDERS-ADR-0025-language-intelligence-boundary.md)
