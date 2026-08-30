# Notations.Bracket migration checklist

Canonical plan: [GUIDERS-ADR-0026](../adr/GUIDERS-ADR-0026-notations-bracket-branch.md).

## Contract (federation SSOT)

| Field | Default | Role |
|-------|---------|------|
| `StartTerminal` / `EndTerminal` | `[` `]` | paired delimiters |
| `AxisSeparator` | `;` | splits **axes** |
| `PairDelimiter` | `:` | splits **key** / **value** within an axis |
| `Axes[]` | — | normalized output (`BracketAxis`) |

Planets ship **profiles** (concrete terminals + delimiters + axis key vocabulary). Platform ships **contract + IR + reference parser**.

## Current state (Phase 0)

| Location | Status |
|----------|--------|
| `Notations.Bracket` Core | `BracketNotationProfile`, `BracketAxis`, `BracketProfiles` constants |
| `Notations.Keyboard.Quarry/QuarryBracketTokenParser` | Angle opaque profile seed — Phase 1 → `BracketReader` |
| CIDE CSX `[F:…;M:…]` | Planet profile `bracket.square-kv` — conformance Phase 2 |

## Phase 1

```
Implement BracketReader(wire, profile) in Core
Keyboard.Quarry → BracketProfiles.AngleOpaque
```

## Phase 2

```
notation/bracket-square-kv fixtures (CSX anchor wire)
LI IAnchorResolver ← NormalizedBracketWire.Axes (F/M meaning in adapter)
```

## Anti-patterns

- Hard-coded `[` `]` parser per product
- Axis key semantics (`F`, `M`, `L`) in Notations package
- Duplicating bracket lexers inside `LanguageIntelligence.*`
