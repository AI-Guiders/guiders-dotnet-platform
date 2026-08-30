# LanguageIntelligence migration checklist

Canonical plan: [GUIDERS-ADR-0025](../adr/GUIDERS-ADR-0025-language-intelligence-boundary.md).

## Current state (Phase 0)

| Location | Status |
|----------|--------|
| `CommandPlane.Slash/Editor/*` | **Quarry** — ships until Phase 1; do not extend |
| `LanguageIntelligence` Core | IR stubs only (`Locus`, `AnchorWire`, `BufferEditOutcome`, …) |
| CIDE / CDP | Source of truth for sniper, Anchor CSX, live buffer |

## Phase 1 move list (mechanical)

```
CommandPlane.Slash/Editor/EditorLine*.cs     → LanguageIntelligence.Line
CommandPlane.Slash/Editor/EditorText*.cs     → LanguageIntelligence.Markup
CommandPlane.Slash/Editor/Markdown*.cs       → LanguageIntelligence.Markup
CommandPlane.Slash/Editor/EditorBuffer*.cs   → LanguageIntelligence.Edit
CommandPlane/Commands/EditorBufferOutcome    → LanguageIntelligence.BufferEditOutcome
CommandPlane.Slash/Editor/Commands/*         → product registry (CIDE/Forge) or optional Bundled
```

## Phase 2+

- Bracket wire parse: [Notations.Bracket](../notations/bracket/README.md) (`Bracket.Anchor` dialect)
- Anchor resolve conformance + `IAnchorResolver` implementations per adapter package
- Sniper scope alignment with CDP `EditSniper`

## Anti-patterns

- New line/anchor logic in `CommandPlane.Slash`
- Assuming Semantic tier on every language adapter
- Duplicating CDP buffer plane in platform
