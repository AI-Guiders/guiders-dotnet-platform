# GUIDERS-ADR-0045: Command authoring DX (code-first)

| | |
|---|---|
| **Status** | Accepted |
| **Date** | 2026-08-31 |
| **Tags** | #guiders #commandplane #catalog #federation #dx |
| **Related** | GUIDERS-ADR-0009 · GUIDERS-ADR-0014 · GUIDERS-ADR-0044 |

## Context

Federation embed proof (ADR-0013/0014): **registry = execute**, **catalog = discoverability**. Products should not maintain parallel TOML + C# descriptor copies.

Editor already follows code-first (`ICatalogDescribed` + `RegistryCatalogBuilder`). DashSpec and Forge consumers still hit **object-initializer hell** for dynamic paths (N filters → N catalog rows) and duplicate registry/catalog wiring.

Config files (TOML/JSON) remain valid for **bundled static** catalogs and third-party drops — but **product commands** should default to C#.

## Decision

### 1. Fluent descriptor builder

```text
CommandDescriptors.Describe(commandId)
  .Path(...)
  .Help(...)
  .Scope(...)
  .Surfaces(...)
  .Build()
```

Package: `CommandPlane.Catalog`. Replaces 15-field `new CommandDescriptor { … }` for authors.

### 2. Multi-path expansion

One `commandId`, many catalog paths (report pages, host tabs, …):

```text
CommandDescriptorRows.ForCommand(commandId, rows, configureDefaults)
CommandDescriptorRows.Map(commandId, items, path, help, configure)
```

Product supplies **data**; federation supplies **shape**.

### 3. Registry catalog row

```text
registry.RegisterCatalog(command, builder => builder.Path(...).Help(...))
```

Sugar over `Register(command, explicitDescriptor)` (ADR-0014).

### 4. Assembly recipe

```text
CommandCatalogAssembly.Build(registry, expandedRows, activeScope?, additionalSources...)
```

Merges:

1. `RegistryCatalogBuilder.ToCommandSource(registry)` — `ICatalogDescribed` + explicit rows
2. expanded rows (context-bound paths), optional `CommandScopeFilter`
3. plugin/file sources

Products (DashSpec, Forge, CIDE) call this; they do **not** fork compose logic.

### 5. Authoring tiers

| Tier | When | Mechanism |
|------|------|-----------|
| **A — static 1:1** | Bundled command, fixed path | `ICatalogDescribed` on command class |
| **B — static 1:N** | One executor, many paths | `CommandDescriptorRows` + registry without `ICatalogDescribed` |
| **C — dynamic instance** | Per-entity command id (field filter) | `RegisterCatalog` with context-bound builder |
| **D — config drop** | Third-party / ops-owned | TOML/JSON via `CommandSources` (unchanged) |

## Consequences

- New bundled command ≈ one class + builder chain; no mandatory config file.
- Dynamic catalogs stay in product expanders but use federation types — not local `NavDescriptor` helpers.
- Source generators / attributes deferred until builder API stabilizes.

## Consumers

| Planet | Adoption |
|--------|----------|
| **LanguageIntelligence** | `EditorCatalogProjections` → `CommandDescriptors` |
| **DashSpec** | `CommandCatalogAssembly` + expander; drop empty bundled TOML |
| **Forge** | target W2d — same recipe |
