# GUIDERS-ADR-0039: Command catalog family (neutral core)

| | |
|---|---|
| **Status** | Accepted |
| **Date** | 2026-08-31 |
| **Tags** | #guiders #commandplane #catalog #sources #slash |
| **Related** | GUIDERS-ADR-0013 · GUIDERS-ADR-0015 · GUIDERS-ADR-0017 · GUIDERS-ADR-0030 |

## Context

Command catalog loaders (`CommandPlane.Sources.*`) hung on the plane root while `SlashCommandDescriptor` and `SlashCatalogIndex` lived in slash-shaped names — even though Forge, DashSpec, and CIDE all load **command** content, not slash mechanics.

Binding already follows the sibling-guild pattern (`CommandPlane.Binding` + `CommandPlane.Binding.Sources.*`). Command catalog should mirror that: **neutral IR in core**, transports in `Catalog.Sources.*`, slash as one consumer/projector.

## Decision

### 1. Neutral catalog IR in `CommandPlane`

| Type | Role |
|------|------|
| `CommandDescriptor` | Cross-product command row (path, DOI, arg tail, pickers, constructors) |
| `CatalogRouteEntry` | Resolved path row in the index |
| `CommandCatalogIndex` | Longest-prefix merge + resolve |
| `CommandCatalogComposer` | Multi-source `Build` |
| `ICommandSource` | `Load()` → descriptors |
| `CommandPickerChoice`, `CommandArgTailKind`, `CatalogPathRole`, `CatalogSemanticFields` | Supporting catalog types |

Slash mechanic (`CommandPlane.Slash`) keeps **projection only**: line resolve, step completion, guidance, conformance harness — all consume `CommandCatalogIndex` / `CatalogRouteEntry`.

### 2. Sources → `CommandPlane.Catalog.Sources.*`

Same axes as [ADR-0013](GUIDERS-ADR-0013-command-catalog-sources.md), renamed package family:

```text
CommandPlane (Core)                    ← CommandDescriptor, CommandCatalogIndex, ICommandSource
    ├── .Catalog.Sources.Json
    ├── .Catalog.Sources.Toml
    ├── .Catalog.Sources.Xml
    ├── .Catalog.Sources.File
    ├── .Catalog.Sources.Database
    └── .Catalog.Sources              ← meta-bundle CommandSources.*
```

Namespace: `AIGuiders.Platform.CommandPlane.Catalog.Sources`.

### 3. Combinations meta-bundle

`AIGuiders.Platform.Combinations.Catalog` replaces `Combinations.Slash`. Combination types (`CommandCatalogCombination`, `CommandCatalogOverlay`) ship in **CommandPlane** assembly, namespace `AIGuiders.Platform.Combinations.Catalog` — same pattern as Binding ([ADR-0030](GUIDERS-ADR-0030-combinations-family.md)).

### 4. API (v2)

```csharp
var catalog = CommandCatalogComposer.Build(
    CommandSource.From(bundled, "bundled"),
    CommandSources.FromJson(json),
    CommandSources.FromToml(toml),
    CommandSources.FromDb(() => repo.LoadCommands(), "db:PortalDB"));
```

### 5. Non-goals

- No transitional type-forwards or `[Obsolete]` aliases — clean rename in platform + consumers in the same wave.
- Slash UI types (`ArgCompletionItem.SlashPath`, conformance JSON `slashPath`) stay slash-surface shaped.

## Consequences

- Products reference `CommandPlane.Catalog.Sources` instead of `CommandPlane.Sources`.
- Slash package loses catalog index/composer; depends on core catalog only.
- Architecture hub + ADR-0013/0015/0029 updated to `Catalog.Sources` naming.
