# Notations.Bracket migration checklist

Canonical plan: [GUIDERS-ADR-0026](../adr/GUIDERS-ADR-0026-notations-bracket-branch.md).

## Contract (federation SSOT)

| Field | CDP default | Role |
|-------|-------------|------|
| `StartTerminal` / `EndTerminal` | `[` `]` | paired delimiters |
| `AxisSeparator` | `;` | splits **axes** (depth-aware) |
| `PairDelimiter` | `:` | splits **key** / **value** (first only) |
| `StripOuterTerminals` | true | `[F:…]` or inner |
| `RespectBracketDepthOnAxisSplit` | true | nested `[` inside values |
| `NestedAxisKeys` | `Anchor` | recursive bracket parse |
| `Axes[]` | — | `BracketAxis { Key, Value, Nested? }` |

Planet-owned: `BracketAxisAliasMap` (F/M/L/…), axis value micro-grammar (`L:12-34`, `S:if:2`), family classify (code/xml/nav).

## CDP/CIDE alignment

| Source | Profile | Status |
|--------|---------|--------|
| `Cdp.ScriptableIde.BracketLocate` | `bracket.cdp-square-kv` | **matches** contract fields |
| EditSniper / peek / land | same wire | reuse profile |
| CIDE `BracketCodeReferenceParser` H1 | `bracket.cide-h1` (space) | **defer** — not in Core v1 |
| Forge `[FRG:…; F:…]` | `bracket.forge-frg-compound` | **defer** — head + tail re-parse |

## Phase 1

```
Implement BracketReader(wire, profile) — port SplitTopLevel + SplitAxes from BracketLocate
Keyboard.Quarry → BracketProfiles.AngleOpaque
```

## Phase 2

```
notation/bracket-cdp-square-kv fixtures (nested Anchor, K:Parameter:x, S:if:2)
LI IAnchorResolver ← Axes[] + BracketAxisAliasMap
```

## Anti-patterns

- Hard-coded `[` `]` parser per product
- Family classify (code/xml/nav) inside Notations
- Duplicating bracket lexers inside `LanguageIntelligence.*`
