# GUIDERS-ADR-0042: IntermediateRepresentation family

| | |
|---|---|
| **Status** | Accepted |
| **Date** | 2026-08-31 |
| **Tags** | #guiders #ir #notations #catalog #federation |
| **Related** | GUIDERS-ADR-0021 · GUIDERS-ADR-0039 · GUIDERS-ADR-0040 · GUIDERS-ADR-0041 |

## Context

[GUIDERS-ADR-0021](GUIDERS-ADR-0021-notations-quarry-family.md) introduced neutral IR types inside `Notations.*` and guild packages (`CommandPlane.Catalog`, `CommandPlane.Binding`, `CommandPlane.Melody`). [GUIDERS-ADR-0041](GUIDERS-ADR-0041-catalog-kernel-profiles.md) lifted catalog **index/merge** into `Platform.Catalog` but left descriptor DTOs in guild assemblies.

Pain: IR is scattered — parsers, catalog guilds, and mechanics all own “neutral” shapes. Consumers (`dash-spec`, conformance, Forge ports) cannot depend on a single IR spine without pulling notation parsers or CommandPlane mechanics. Deferred extract becomes arch-debt (operator: «выноси сразу»).

## Decision

### 1. `IntermediateRepresentation.*` package family

Neutral DTOs only — no parsers, no index, no surface projectors.

```text
IntermediateRepresentation.Argument     profile, slots, NormalizedArguments, reader ids
IntermediateRepresentation.Keyboard     NormalizedKeySequence + step records
IntermediateRepresentation.Invocation   NormalizedCommandLine, InvocationLinePhase, InvocationArgMechanic
IntermediateRepresentation.Bracket      bracket wire IR (profiles, axes, NormalizedBracketWire)
IntermediateRepresentation.Command      command catalog descriptors + route rows
IntermediateRepresentation.Binding      binding descriptors + entries
IntermediateRepresentation.Melody       melody descriptor + line/step IR
IntermediateRepresentation.Agent          agent envelope IR (DetailTier, NextHint, AgentResponseEnvelope)
IntermediateRepresentation.Language       locate/edit IR (Locus, TextEdit, BracketAnchorSpan, …)
```

`IntentOutcome` stays in **`Abstractions`** — shared host-execute result (Routing + MCPlane projection); not envelope-only.

Future nested splits (only if volume warrants): `IR.Language.Line`, `IR.Language.Sniper` — same family, optional child packages per ADR-0025 phases.

### 2. Boundary rules

| Layer | Owns |
|-------|------|
| **IR.*** | Immutable records/classes, enums, well-known id constants |
| **Notations.*** | Wire parsers → IR; `I*NotationReader` interfaces stay here |
| **Platform.Catalog** | `ICatalogProfile`, index merge kernel (ADR-0041) |
| **CommandPlane.*** guilds | Sources, composers, resolve, completion, capture SM |
| **Planets** | Render chrome only |

**No transitional aliases or type-forwards.** Namespace = package boundary.

### 3. Dependency spine

```text
IR.Argument  ← IR.Command, IR.Melody
IR.Keyboard  ← IR.Binding
IR.Invocation, IR.Bracket — leaf IR packages

CommandPlane.Catalog      → IR.Command + Platform.Catalog
CommandPlane.Binding      → IR.Binding
CommandPlane.Melody       → IR.Melody (+ mechanics pkgs)
Notations.Argument.*      → IR.Argument
Notations.Keyboard.*      → IR.Keyboard + Notations.Keyboard (readers)
Notations.Command.*       → IR.Invocation + Notations.Command
Notations.Bracket         → IR.Bracket + Notations (KvPair helpers)
MCPlane                   → IR.Agent + projection/conformance mechanics
LanguageIntelligence      → IR.Language + IAnchorResolver contract
LanguageIntelligence.Anchors → IR.Language + BracketAnchorWire parse/format
```

`AIGuiders.Platform.Notations.Argument` **core package removed** — superseded by `IR.Argument`.

### 4. Migration (one wave)

1. Create `IntermediateRepresentation.*` packages; `git mv` IR types.
2. Split `BracketNotationModels` — `ToKvPair()` extension stays in `Notations.Bracket`.
3. Rewire all `csproj` / `using` / conformance consumers.
4. Behavior unchanged — structural refactor only; vectors must pass.

## Consequences

- Federation ports (JS slash, Quarry) target **`IntermediateRepresentation.*`** NuGet ids, not `Notations.*` or `CommandPlane`.
- Guild packages shrink to mechanics; catalog field schemas are versioned with IR packages.
- ADR-0041 §4 “Descriptor field schemas → guild IR” is superseded: schemas live in **`IR.Command` / `IR.Binding` / `IR.Melody` / `IR.Agent` / `IR.Language`**.

## Non-goals

- Renaming `ArgConstructorBinding` → `CommandConstructorBinding` (cosmetic; optional later ADR).
- Moving `IntentOutcome` out of `Abstractions` (cross-plane execute result; envelope wraps it).
