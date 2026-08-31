# GUIDERS-ADR-0031: Policy-as-readable-code — Overlay profiles

| | |
|---|---|
| **Status** | Accepted |
| **Date** | 2026-08-30 |
| **Tags** | #guiders #combinations #overlay #policy #dx |
| **Relates to** | GUIDERS-ADR-0030 · GUIDERS-ADR-0029 · GUIDERS-ADR-0015 · GUIDERS-ADR-0017 |

---

## Context

v0.26 unified **layer fold** (`OrderedCombination`, `SourceCombination`) and **named combinators** (`ShipFirst`, `OverlayWins`, `FieldOverlay`). Domain merge rules still lived partly in imperative methods (`MergeOver`, `MergeAdr`).

Operator norm: **policy-as-readable-code** — named, fluent, top-to-bottom recipes (spirit of FluentValidation DX, not validation semantics).

---

## Decision

### Platform standard

Wherever a **policy** decides how baseline + overlay combine:

1. **Must** expose a named `OverlayPolicy<T>` (or domain alias) with `CombinationSemantics`.
2. **Must** bind to `Combinator<T>` for use with `SourceCombination` / `OrderedCombination`.
3. **Should** declare rules via `Combinations.Overlay` fluent profile when policy is multi-rule or heterogeneous.
4. **Must** have tests on collision/overlay semantics, not only parse/load.
5. **Must not** hide merge logic in composers/loaders without a named policy.

### Package: `Combinations.Overlay`

```text
OverlayPolicy<T>           name + semantics + combinator
OverlayProfile.For<T>()    fluent builder (When, MergeSection, FieldOverlay, ReplaceWhenPresent, Rule)
FieldOverlayBuilder<T>     declarative nullable-field overlay per property
```

### Reference implementation: workspace

`WorkspaceOverlay.FieldOverlay` — readable recipe replacing imperative `MergeOver`:

- `When` overlay has `workspace`
- `FieldOverlay` on `adr.*`
- `ReplaceWhenPresent` on `features`, `correspondence`

`WorkspaceDocumentOverlays.MergeOver()` extension delegates to policy (DTO stays in `Configurations.Workspace` without circular refs).

### Slash / binding

Single-rule policies use `.Rule(...)` inside `OverlayProfile` — same standard, minimal ceremony:

- `CommandCatalogOverlay.ShipFirst`
- `BindingOverlay.OverlayWins`

---

## Consequences

- Imperative `MergeOver` removed from `WorkspaceDocument` DTO; use `WorkspaceDocumentOverlays.MergeOver` or `WorkspaceCombinators.FieldOverlay`.
- Future hubs (cockpit.toml) add `*Overlay` profile + tests before ship.
- Validation of documents (malformed TOML) remains orthogonal; optional FluentValidation later for **pre-merge validate**.

---

## Migration (v0.27)

| Before | After |
|--------|-------|
| `WorkspaceDocument.MergeOver()` instance method | `WorkspaceDocumentOverlays.MergeOver()` extension |
| `MergeAdr` / `MergeSection` private statics | `WorkspaceOverlay.FieldOverlay` profile |

Public combinator entry points (`WorkspaceCombinators`, composers) unchanged.
