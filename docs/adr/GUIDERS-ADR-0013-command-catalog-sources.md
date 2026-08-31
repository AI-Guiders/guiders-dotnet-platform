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

Platform owns catalog **mechanics** (`CommandCatalogIndex`, merge, resolve). Content loaders were ad-hoc per product.

## Decision

### 1. Split contract, transport, format

| Layer | Package | Responsibility |
|-------|---------|----------------|
| **Contract** | `AIGuiders.Platform.CommandPlane` | `ICommandSource`, `CommandSource.From*`, `CommandDescriptor`, `CommandDescriptorMapper` |
| **Format** | `Sources.Json` · `.Toml` · `.Xml` | `ICommandFormatReader` + `*CommandSources.From*` / `FromFile(.ext)` |
| **Transport** | `Sources.File` · `.Database` | File path + extension dispatch, embedded resources; DB delegate |
| **Meta-bundle** | `CommandPlane.Catalog.Sources` | `CommandSources.*` re-exports for all-in-one embed |

**FromFile is one transport** — only the format (JSON/TOML/XML/…) differs. `CommandSource.FromFile(path, reader)` is the Core primitive; `FileCommandSources.FromFile(path)` picks the reader by extension.

Core stays **zero-dependency** (except BCL). Tomlyn only in `Sources.Toml`.

### 2. API

```csharp
var catalog = CommandCatalogComposer.Build(
    CommandSource.From(bundledDescriptors, "bundled"),
    CommandSources.FromJson(json),
    CommandSources.FromToml(toml),
    CommandSources.FromDb(() => repo.LoadCommands(), "db:PortalDB"),
    // DB-only embed: DatabaseCommandSources.From(() => repo.LoadCommands(), "db:PortalDB"),
);
```

Sink remains `CommandCatalogIndex.FromDescriptors` + `Merge` — sources are adapters **into** descriptors.

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

Custom backends: implement `ICommandFormatReader`, `DatabaseCommandSources.From(loader)`, or a future `Sources.Database.*` provider package.

Embedded plugin catalogs:

```csharp
pluginAssembly.FromAssemblyResource("commands.toml");
```

Extension on `Assembly` in `CommandPlane.Catalog.Sources.File` — resolves manifest resource by suffix, format from file extension.

### 4. Non-goals

- Full `intent-catalog.toml` CIDE schema in platform (nested slash forms stay in CIDE until migration)
- Registry / executor loading — catalog discovery only ([GUIDERS-ADR-0009](GUIDERS-ADR-0009))

## Consequences

- CIDE `IntentCatalogLoader` can migrate to emit `CommandDescriptor` + `CommandSources.FromToml` incrementally
- Forge capabilities JSON can share `JsonCommandFormatReader` subset or stay bespoke until aligned
- Products without file formats reference CommandPlane only; pin `Sources.Json` / `.Toml` / `.Xml` à la carte, or `Sources` meta-bundle for all formats
