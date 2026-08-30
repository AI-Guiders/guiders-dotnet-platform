# Notations.Bracket migration checklist

Canonical plan: [GUIDERS-ADR-0026](../adr/GUIDERS-ADR-0026-notations-bracket-branch.md).

## Current state (Phase 0)

| Location | Status |
|----------|--------|
| `Notations.Keyboard.Quarry/QuarryBracketTokenParser` | **Angle bracket** lexer — stays until Phase 1 extract |
| `Notations.Keyboard.Vim/VimChordNotationParser` | **Square** inner parse — candidate for `Bracket.Square` |
| CIDE CSX `[F:…;M:…]` | Planet wire — target `Notations.Bracket.Anchor` |
| `Notations.Bracket` Core | IR stubs (`NormalizedBracketWire`, …) |

## Phase 1 move list

```
Notations.Keyboard.Quarry/QuarryBracketTokenParser  → Notations.Bracket.Angle (delegate from Keyboard)
```

## Phase 2

```
CIDE CSX anchor wire grammar  → Notations.Bracket.Anchor
LI.Anchors IAnchorResolver    ← consumes NormalizedBracketWire (not raw parse)
```

## Anti-patterns

- New bracket lexers inside `LanguageIntelligence.*`
- Duplicating `<…>` parse in every keyboard dialect package
- Treating CSX bracket wire as federation SSOT before conformance vectors exist
