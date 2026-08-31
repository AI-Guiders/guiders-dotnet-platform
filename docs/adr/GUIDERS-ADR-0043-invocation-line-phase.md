# GUIDERS-ADR-0043: Invocation line phase and arg mechanics

| | |
|---|---|
| **Status** | Accepted |
| **Date** | 2026-08-31 |
| **Tags** | #guiders #ir #commandplane #slash #invocation |
| **Related** | GUIDERS-ADR-0015 · GUIDERS-ADR-0012 · GUIDERS-ADR-0024 · GUIDERS-ADR-0042 · GUIDERS-ADR-0040 |

## Context

Slash unload (operator wave) exposed conflated types: `SlashInputMode` mixed path-building, arg-tail, and ready-to-run; constructor and locale types were `Slash*`-prefixed inside mechanic guilds.

[GUIDERS-ADR-0015](GUIDERS-ADR-0015-invocation-mechanics-slash-melody-binding.md) defines **engage** (Slash · Melody · Binding). [GUIDERS-ADR-0024](GUIDERS-ADR-0024-visual-command-tree-capture-stack.md) requires one headless projection across engages. Line lifecycle (path → arg → ready) is **orthogonal** to engage and shared by Melody slug/tail capture.

## Decision

### 1. Three axes (not one enum)

```text
Engage (ADR-0015)     How invocation starts — Slash | Melody | Binding
        │
        ▼
Line phase (IR)       Path → Arg → Ready
        │
        ▼ (only when Arg)
Arg mechanic (guilds) Picker | Constructor | TypedInput | PrefixArmed | …
```

| Axis | SSOT | Examples |
|------|------|----------|
| **Engage** | Planet peel + mechanic entry | `/`, chord root, hotkey |
| **Line phase** | `IR.Invocation` | `InvocationLinePhase` |
| **Arg mechanic** | `IR.Invocation` enum; behavior in `CommandPlane.*` guilds | `InvocationArgMechanic` |

### 2. IR.Invocation types

```text
IntermediateRepresentation.Invocation
  NormalizedCommandLine      (existing)
  InvocationLinePhase        Path | Arg | Ready
  InvocationArgMechanic      Picker | FreeText | Optional | Constructor | TypedInput
```

Mechanic **implementations** stay in guild packages (`Constructors`, `PrefixArmed`, `ArgSuggestions`). IR owns labels only.

### 3. Surface projections

| Engage | Projection type | Adds |
|--------|-----------------|------|
| Slash | `SlashInputGuidance` | breadcrumb, slash chrome |
| Melody | (future) `MelodyInputGuidance` | chord tree labels |
| Binding | n/a at line level | direct execute |

Neutral tail guidance: `ArgInputGuidance` (`Phase`, `Mechanic`, placeholder, hint) in `CommandPlane.Constructors` until a dedicated `IR.Invocation.Guidance` package is warranted.

`SlashInputGuidance.Mode` (computed string) preserves conformance wire labels (`Path`, `Picker`, `Ready`, …).

### 4. Slash unload boundaries (same wave)

| Moved to | What |
|----------|------|
| `LanguageIntelligence.{Line,Markup,Edit,Bundled}` | Editor buffer commands (ADR-0025 P1) |
| `CommandPlane.Constructors` | Value constructors, `ArgCompletionItem`, entry completion |
| `CommandPlane` hub | `ICatalogDescribed`, `PlatformCommandRegistry`, `RegistryCatalogBuilder` |
| `IR.Command` | `ArgConstructorBinding` (was `SlashConstructorBinding`) |
| `CommandPlane.Slash` (thin) | line resolve, step/arg completion, slash projection, conformance specs |

No transitional type-forwards.

### 5. Dependency rule

```text
IR.Invocation  ← CommandPlane.* guilds, Slash, Melody (projectors)
                 ← dash-spec / conformance (federation ports)
```

Guilds MUST NOT define line-phase enums. Slash MUST NOT own neutral constructor/locale IR.

## Consequences

- Melody and Binding can adopt `InvocationLinePhase` without referencing `CommandPlane.Slash`.
- Visual Command Tree ([ADR-0024](GUIDERS-ADR-0024-visual-command-tree-capture-stack.md)) keys engage kind separately from line phase.
- Hub §8.3 arg modes table maps to `InvocationLinePhase.Arg` + `InvocationArgMechanic`.
