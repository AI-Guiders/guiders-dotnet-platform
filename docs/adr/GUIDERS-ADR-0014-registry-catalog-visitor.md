# GUIDERS-ADR-0014: Registry catalog visitor (W2c)

| | |
|---|---|
| **Status** | Accepted |
| **Date** | 2026-08-29 |
| **Tags** | #guiders #commandplane #catalog #registry |
| **Related** | GUIDERS-ADR-0009 · GUIDERS-ADR-0010 · GUIDERS-ADR-0013 · FORGE-ADR-0066 |

## Context

[GUIDERS-ADR-0009](GUIDERS-ADR-0009-command-surface-pattern.md) splits **Catalog** (discovery) from **Registry** (execution). Products duplicated slash metadata: editor line commands lived in both `EditorCommandRegistry` and `EditorSurfaceCatalog`.

[FORGE-ADR-0066](https://github.com/AI-Guiders/agent-forge/blob/master/design/FORGE-ADR-0066-forge-all-commands-platform-pattern.md) W1: capabilities built by **visiting** registries, not parallel descriptor lists.

## Decision

### 1. Types

| Type | Role |
|------|------|
| `ICatalogDescribed` | Command projects `SlashCommandDescriptor` |
| `ICatalogVisitor` | Receives descriptors during registry walk |
| `SlashCatalogCollector` | Default accumulator visitor |
| `RegistryCatalogBuilder` | `CollectDescriptors`, `BuildIndex`, `ToCommandSource` |

### 2. Registry API

`PlatformCommandRegistry<TContext>`:

- `Register(command)` — uses `ICatalogDescribed` when implemented
- `Register(command, explicitDescriptor)` — override / bridge for migration
- `Accept(visitor, predicate?)` — catalog projection without second store

### 3. Composition

```csharp
SlashCatalogComposer.Build(
    RegistryCatalogBuilder.ToCommandSource(registry),
    CommandSources.FromToml(toml),
);
```

Registry remains SSOT for **execution**; catalog is a **view**.

### 4. Editor W1 slice ✓

- `EditorLineSelectCommand`, `EditorLineDeleteCommand`, `EditorFormatInsertCommand` implement `ICatalogDescribed`
- `EditorSurfaceCatalog.BundledEditorLineCommands()` delegates to `RegistryCatalogBuilder`

## Non-goals

- Forge `ForgeCommandRegistry` migration (Forge W1–W5)
- `RelayPlatformCommand<T>` (Glass/CIDE)

## Consequences

- New bundled command = one class (`IPlatformCommand` + `ICatalogDescribed`), not descriptor duplicate
- Forge `ForgeCapabilitiesService` can switch to `RegistryCatalogBuilder` per FORGE-ADR-0066 W1
