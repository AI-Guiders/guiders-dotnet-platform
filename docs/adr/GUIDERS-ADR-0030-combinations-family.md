# GUIDERS-ADR-0030: Combinations family — ordered fold + named policies

| | |
|---|---|
| **Status** | Accepted |
| **Date** | 2026-08-30 |
| **Tags** | #guiders #combinations #sources #slash #binding #workspace |
| **Relates to** | GUIDERS-ADR-0029 · GUIDERS-ADR-0015 · GUIDERS-ADR-0017 |

---

## Context

v0.25 lifted `ISource<T>` and put generic merge in `SourceCatalog.Merge`, while slash/binding composers used inline folds with **different** collision semantics (ship-first vs overlay-wins). Merge policy was the right idea but lived in three places.

Operator rule: **Combinations** = family for ordered layer fold + named combinator policies; **Sources** = transport only.

---

## Decision

### Family map

```text
Platform.Combinations                 kernel: Combinator<T>, OrderedCombination, CombinationSemantics
├── Combinations.Sources              lazy ISource layer merge (SourceCombination)
├── Combinations.Workspace            WorkspaceCombinators.FieldOverlay
├── Combinations.Catalog                meta-bundle → types in CommandPlane.Slash
├── Combinations.Binding              meta-bundle → types in CommandPlane.Binding
└── Combinations.All                  meta-bundle: kernel + Sources + Workspace + Slash + Binding
```

### Kernel

- **`Combinator<T>`** — `(baseline, overlay) => merged`.
- **`OrderedCombination.Fold`** — materialized layers.
- **`OrderedCombination.FoldLayers`** — project each layer then fold (catalog composers).
- **`CombinationSemantics`** — documents FieldOverlay | SectionReplace | ShipFirst | OverlayWins.

### Domain policies

| Package | Combinator | Semantics | Collision rule |
|---------|------------|-----------|----------------|
| `Combinations.Workspace` | `FieldOverlay` | FieldOverlay | ADR fields: overlay non-null wins |
| `CommandPlane.Slash` (`Combinations.Catalog` ns) | `ShipFirst` | ShipFirst | `CommandCatalogIndex.Merge` TryAdd |
| `CommandPlane.Binding` (`Combinations.Binding` ns) | `OverlayWins` | OverlayWins | `BindingCatalogIndex.Merge` overwrite key |

Slash/binding combination types ship in **CommandPlane.* assemblies** (namespace `AIGuiders.Platform.Combinations.*`) to avoid circular project refs. NuGet meta-packages `Combinations.Catalog` / `Combinations.Binding` pull the implementing packages.

### Composers

`CommandCatalogComposer` / `BindingCatalogComposer` are thin façades over `*CatalogCombination.Compose`. `WorkspaceSources.MergeOverlay` delegates to `SourceCombination.Merge` + `WorkspaceCombinators.FieldOverlay`.

### Migration from v0.25

| Removed | Replacement |
|---------|-------------|
| `SourceCatalog.Merge` | `SourceCombination.Merge` |
| Inline dict merge in `BindingCatalogComposer` | `BindingCatalogCombination.Compose` |

---

## Consequences

- One fold pattern for workspace documents, slash catalogs, and binding catalogs.
- Planets pin `Combinations.All` or à la carte slices.
- Future hubs (cockpit.toml) add `Combinations.Cockpit` + domain combinator; kernel unchanged.
