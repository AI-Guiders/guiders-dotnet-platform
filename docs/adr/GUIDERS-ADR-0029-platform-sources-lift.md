# GUIDERS-ADR-0029: Platform.Sources lift — transport kernel + merge combinator

| | |
|---|---|
| **Status** | Accepted |
| **Date** | 2026-08-30 |
| **Tags** | #guiders #sources #workspace #toml #merge |
| **Relates to** | GUIDERS-ADR-0013 · GUIDERS-ADR-0017 · GUIDERS-ADR-0028 · CIDE ADR-0061 |

---

## Context

Command catalogs, binding catalogs, and workspace correspondence each had **parallel source stacks**:

| Domain | Old stack | Transport |
|--------|-----------|-----------|
| Slash commands | `CommandPlane.Sources.*` | json/toml/xml per package |
| Bindings | `CommandPlane.Binding.Sources.*` | json/toml per package |
| Workspace CRS | `Correspondence.Workspace` TOML models + loader | ad hoc Tomlyn |

TOML is **transport**, not domain. `workspace.toml` is a **configuration hub** shared by correspondence, ADR maps, and future cockpit settings.

Operator rule (this slice): **lift `Source` above plane-specific packages**; merge policy = **combinator over `ISource<T>` layers**, not a bespoke loader class.

---

## Decision

### Kernel (`Platform.Sources`)

```text
Platform.Sources              ISource<T>, IFormatReader<TOut>, DocumentFormat
├── Sources.File              DocumentFormats.Resolve, FileSources.FromFile
├── Sources.Toml              TomlFormatReader<T>, TomlSerialization
└── SourceCatalog.Merge       ordered overlay combinator (baseline + layers)
```

- **`ISource<T>`** — stable `SourceId` + `Load()`.
- **`SourceCatalog.Merge(baseline, overlay, combiner)`** — moved to **`Combinations.Sources.SourceCombination`** (GUIDERS-ADR-0030).
- **`DocumentFormats`** — single extension dispatch for file transport.

### Domain hub (`Configurations.Workspace`)

```text
Configurations.Workspace           WorkspaceDocument + section DTOs + MergeOver()
Configurations.Workspace.Sources   WorkspaceSources (FromText/File/Cascade, MergeOverlay)
```

Correspondence guild **consumes** `WorkspaceDocument`; it no longer owns TOML models or `WorkspaceTomlLoader`.

### Plane adapters (non-breaking)

| Contract | Lift |
|----------|------|
| `ICommandSource` | extends `ISource<IReadOnlyList<SlashCommandDescriptor>>` |
| `IBindingSource` | extends `ISource<IReadOnlyList<BindingDescriptor>>` |
| `CommandSourceFormats.Resolve` | delegates to `DocumentFormats.Resolve` |
| `BindingSourceFormats.Resolve` | delegates to `DocumentFormats.Resolve` (xml rejected at binding layer) |

Slash/binding **catalog composers** (`SlashCatalogComposer`, `BindingCatalogComposer`) remain the **key-level** merge for loaded descriptors. Document-level merge (`SourceCatalog.Merge`) applies when overlaying whole config documents (e.g. embedded defaults + disk `workspace.toml`).

---

## Consequences

- New packages ship in v0.25; Correspondence.Workspace sheds Tomlyn dependency.
- Planets pin `Configurations.Workspace.Sources` for cascade load; correspondence resolve is unchanged at the API surface (`CorrespondenceResolver.TryResolve`).
- Future config hubs (e.g. cockpit.toml) follow the same pattern: domain DTO + `MergeOver` + `*Sources.MergeOverlay`.

---

## Migration (v0.25)

| Remove | Replace with |
|--------|--------------|
| `Correspondence.Core/WorkspaceTomlModels.cs` | `Configurations.Workspace` |
| `Correspondence.Workspace/WorkspaceTomlLoader.cs` | `WorkspaceSources` |
| Duplicate extension switches in plane File packages | `DocumentFormats.Resolve` |
