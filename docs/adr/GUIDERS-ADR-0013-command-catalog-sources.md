# GUIDERS-ADR-0013: Command catalog sources (JSON, TOML, XML, DB)

| | |
|---|---|
| **Status** | Accepted |
| **Date** | 2026-08-29 |
| **Tags** | #guiders #commandplane #catalog #sources |
| **Related** | GUIDERS-ADR-0003 · GUIDERS-ADR-0009 · GUIDERS-ADR-0010 · GUIDERS-ADR-0011 |

## Context

Products load slash catalog **content** from many backends:

| Product | Today |
|---------|--------|
| CIDE | `IntentCatalogLoader` (TOML) |
| Forge | capabilities JSON + overlay |
| DashSpec | `DashboardCommandCatalogBuilder` (code) |
| Glass | in-code `Command[]` |

Platform owns catalog **mechanics** (`SlashCatalogIndex`, merge, resolve). Content loaders were ad-hoc per product.

## Decision

### 1. Split contract vs format

| Layer | Package | Responsibility |
|-------|---------|----------------|
| **Contract** | `AIGuiders.Platform.CommandPlane` | `ICommandSource`, `CommandSource.From(...)`, `ICommandFormatReader`, `SlashCatalogComposer` |
| **Built-in formats** | `AIGuiders.Platform.CommandPlane.Sources` | JSON, TOML, XML readers; `CommandSources.FromJson/Toml/Xml/Db/File` |

Core stays **zero-dependency**. Tomlyn only in Sources package.

### 2. API

```csharp
var catalog = SlashCatalogComposer.Build(
    CommandSource.From(bundledDescriptors, "bundled"),
    CommandSources.FromJson(json),
    CommandSources.FromToml(toml),
    CommandSources.FromDb(() => repo.LoadCommands(), "db:PortalDB"),
);
```

Sink remains `SlashCatalogIndex.FromDescriptors` + `Merge` — sources are adapters **into** descriptors.

### 3. Document shapes (v1)

Flat command list — snake_case or camelCase field names:

| Field | Required |
|-------|----------|
| `commandId` / `command_id` | yes |
| `path` | yes |
| `domain`, `object`, `intent` | default `""` |
| `help`, `group`, `argTail`, `argHint`, `pathAliases`, `argPickerChoices` | optional |

Containers:

- **JSON:** `{ "commands": [ ... ] }` or top-level array
- **TOML:** `[[command]]` or `[[commands]]`
- **XML:** `<commands><command .../></commands>`

Custom backends: implement `ICommandFormatReader` or `CommandSource.From(loader)`.

### 4. Non-goals

- Full `intent-catalog.toml` CIDE schema in platform (nested slash forms stay in CIDE until migration)
- Registry / executor loading — catalog discovery only ([GUIDERS-ADR-0009](GUIDERS-ADR-0009))

## Consequences

- CIDE `IntentCatalogLoader` can migrate to emit `SlashCommandDescriptor` + `CommandSources.FromToml` incrementally
- Forge capabilities JSON can share `JsonCommandFormatReader` subset or stay bespoke until aligned
- Products without file formats reference CommandPlane only; optional Sources package for JSON/TOML/XML
