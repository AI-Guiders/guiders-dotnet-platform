# GUIDERS-ADR-0041: Catalog kernel + domain profiles

| | |
|---|---|
| **Status** | Accepted |
| **Date** | 2026-08-31 |
| **Tags** | #guiders #catalog #sources #combinations #federation |
| **Related** | GUIDERS-ADR-0029 · GUIDERS-ADR-0030 · GUIDERS-ADR-0039 · GUIDERS-ADR-0040 · GUIDERS-ADR-0017 |

## Context

Operator shorthand: «каталогу пофиг чего каталог → `Catalog<T>`».

Partially true. [GUIDERS-ADR-0029](GUIDERS-ADR-0029-platform-sources-lift.md) already genericized **load + layered merge** via `ISource<T>` and `Combinations.*`. [GUIDERS-ADR-0039](GUIDERS-ADR-0039-command-catalog-family.md) extracted command IR into `CommandPlane.Catalog`; binding ships a parallel `BindingCatalogIndex`.

What is **not** shared today: index projection rules and resolve semantics. `CommandCatalogIndex` (longest-prefix path, ship-first merge) and `BindingCatalogIndex` (exact key, overlay-wins) duplicate dictionary + merge shape with different profiles.

Naive `Catalog<T>` collapses to `ISource<IReadOnlyList<T>>` — already exists. The missing abstraction is **profile-driven index + resolve**, not a single type parameter.

## Decision

### 1. Two axes (same pattern as Sources format × transport)

```text
AXIS A — Content spine (generic, done)
  ISource<TDescriptor>
  IFormatReader<TDescriptor>
  Combinations layered merge

AXIS B — Catalog profile (per guild, this ADR)
  ICatalogProfile<TDescriptor, TKey, TEntry>
    • Project(descriptor) → (keys, entries)
    • MergePolicy (ship-first | overlay-wins | …)
    • Resolve(query) → entry
```

**Rule:** `Platform.Sources` owns axis A. **`Platform.Catalog`** (new kernel package) owns axis B mechanics. Guild packages (`CommandPlane.Catalog`, `CommandPlane.Binding`, future `CommandPlane.Melody`) ship **profiles + thin typed facades**.

### 2. Kernel surface (target)

| Type | Role |
|------|------|
| `CatalogIndex<TKey, TEntry>` | Internal dictionary + merge hook |
| `ICatalogMergePolicy<TKey, TEntry>` | Ship-first, overlay-wins, … |
| `ICatalogProfile<TDescriptor, TKey, TEntry>` | Descriptor → keys/entries + resolve |
| `CatalogComposer` | `Compose(ISource<TDescriptor>…, profile)` → typed index facade |

Guild facades remain ergonomic names (`CommandCatalogIndex`, `BindingCatalogIndex`) — thin wrappers over kernel + profile singleton.

### 3. Profile map

| Guild | `TDescriptor` | `TKey` | Merge | Resolve |
|-------|---------------|--------|-------|---------|
| Command | `CommandDescriptor` | path string | ship-first | longest-prefix tokens |
| Binding | `BindingDescriptor` | binding_key | overlay-wins | exact key |
| Melody (future) | `MelodyDescriptor` | slug | TBD | slug + tail |

### 4. Out of kernel scope

| Concern | Owner |
|---------|--------|
| Arg runtime suggestions | `CommandPlane.ArgSuggestions` (ADR-0040) |
| Slash/Melody/Binding surface projectors | `CommandPlane.Slash`, … |
| Descriptor field schemas (arg tail, wire) | Guild IR types |
| Content transports | `*.Catalog.Sources.*` / `*.Binding.Sources.*` |

### 5. Migration (one wave, no type-forwards)

1. ~~Introduce `AIGuiders.Platform.Modeling.Catalog` kernel package.~~ ✓
2. ~~Refactor `CommandCatalogIndex` + `BindingCatalogIndex` to profile + facade (behavior unchanged).~~ ✓
3. Point `Combinations.Catalog` / `Combinations.Binding` meta-bundles at kernel + guild.
4. Conformance vectors must pass unchanged — refactor is structural only. ✓

## Consequences

- `Catalog<T>` is not a public API name; **`ISource<T>` + `ICatalogProfile<…>`** is the honest split.
- Third catalog (Melody) validates the kernel without another copy-paste index.
- Command-specific IR stays in `CommandPlane.Catalog`; only index/merge/compose mechanics lift.

## Non-goals

- One mega-descriptor type for slash + binding + melody (rejected in ADR-0015).
- Generic arg-suggestion broker (already ADR-0040).
- Workspace / correspondence catalogs in this wave (separate domain; may reuse kernel later).
