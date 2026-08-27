# GUIDERS-ADR-0009: Command–Surface pattern (GoF Command + invocation surfaces)

| | |
|---|---|
| **Status** | Accepted |
| **Date** | 2026-08-27 |
| **Tags** | #guiders #commandplane #slash #forge #cide #cdp #glass #pattern #gof |
| **Related** | GUIDERS-ADR-0003 · GUIDERS-ADR-0007 · GUIDERS-ADR-0010 · CIDE ADR-0013/0030/0119 · FORGE-ADR-0065/0064/0066 |

## Name

Two coupled ideas:

1. **GoF Command** — объект с `CommandId`, `CanExecute(context)`, `Execute(context)`; одна реализация эффекта.
2. **Command–Surface** — **one command, many invocation surfaces** (toolbar, slash, CCL, palette, hotkey, MCP).

- **Command** = executor object / registry entry.
- **Invocation surface** = UI invoker; будущий **RelayCommand&lt;T&gt;** в Glass/CIDE — только invoker.

Короткий ярлык: **Command–Surface** (не «одна полоса UI»).

## Pattern stack (Catalog · Registry · Command · Surface)

Четыре именованных паттерна — **не один механизм**:

```text
  Catalog                    Registry                 Command
  (что показать)             (как найти executor)     (как выполнить)
       │                          │                        │
  SlashCommandDescriptor    PlatformCommandRegistry   IPlatformCommand
  SlashCatalogIndex         EditorCommandRegistry     PlatformCommand
  capabilities.commands[]   Forge CommandCatalog
       │                          │                        │
       └─────────── path / id ────┴──── commandId ───────┘
                                    ▲
                    Invocation surfaces (slash, palette, CCL, toolbar, MCP)
```

| Pattern | Вопрос | SSOT в platform | Не делает |
|---------|--------|-----------------|-----------|
| **Catalog** | «Что пользователь *видит* и как *найти* по path?» | `SlashCommandDescriptor`, `SlashCatalogIndex`, `SlashLineResolver` | `Execute`, правки buffer, MCP |
| **Registry** | «По `commandId` — какой executor?» | `PlatformCommandRegistry<TContext>`, product catalogs (`EditorCommandRegistry`, Forge `CommandCatalog`) | Autocomplete UI, wrap/insert math |
| **Command** (GoF) | «Один эффект — один `Execute`» | `IPlatformCommand<T>`, `PlatformCommand<T>` | Парсинг slash-строки, popover layout |
| **Surface** | «Откуда человек вызвал?» | Product UI (`forge-slash.js`, palette, Relay*) | Собственная бизнес-логика |

\* Relay — invoker-адаптер для Glass/CIDE (deferred).

### Catalog vs Registry (главная граница)

| | Catalog | Registry |
|---|---------|----------|
| Ключ | slash **path**, tier, group, arg_tail, help | **commandId** |
| Merge | `SlashCatalogIndex.Merge` (Forge overlay + TOML) | `Register(command)` per product |
| Consumer | autocomplete, capabilities JSON, trie | `TryExecute(id, context)` |
| Анти-паттерн | Выполнять действие из descriptor без registry | Дублировать path/help для discoverability |

**Wire:** catalog entry несёт `CommandId` → surface резолвит path (catalog) → registry → command.

### Registry of Commands

**Registry** — отдельный паттерн: каталог *исполнителей*, не UI.

- Новая bundled buffer command = **class** + `registry.Register(...)` — не ветка в surface handler.
- Registry может держать singleton commands или factory per id (`EditorFormatInsertCommand` per format).

## Platform contract (GoF Command + Registry)

```text
IPlatformCommand<TContext>
  ├── CommandId
  ├── CanExecute(context)
  └── ExecuteAsync(context) → CommandOutcome

PlatformCommandRegistry<TContext>  — register + TryExecute(commandId, context)
```

| Type | Role |
|------|------|
| `ICommandContext` | Invocation payload marker |
| `PlatformCommand<TContext>` | Sync command base class |
| `CommandOutcome` | Success / error + typed payloads (`EditorBufferOutcome`, …) |

Catalog entries (`SlashCommandDescriptor`) link to registry via `CommandId`; they do not replace registry lookup.

## Rules

1. Surfaces call `registry.TryExecute(commandId, context)` — no embedded transform in popover/toolbar handlers.
2. Surfaces are **parallel** on one work surface (editor: toolbar + inline slash + CCL).
3. Agent parity — same `command_id` as human invokers ([FORGE-ADR-0025](https://github.com/AI-Guiders/agent-forge/blob/master/design/FORGE-ADR-0025-human-command-parity.md)).
4. **RelayPlatformCommand&lt;T&gt;** (WPF/Avalonia) — deferred; wraps `IPlatformCommand` for binding ([CIDE ADR-0013](https://github.com/AI-Guiders/cascade-ide/blob/develop/docs/adr/0013-command-surface-and-discoverability.md)).

## Forge v1 (this ADR wave)

| Family | Command SSOT | JS invoker surfaces |
|--------|--------------|---------------------|
| Editor buffer | `EditorCommandRegistry` + `Editor*Command` classes | `window.forgeEditorCommand.execute` |
| Domain remote | `ForgeCommandExecutor` (existing) | `POST /commands/execute`, command bar |

Glass/CIDE migrate to `IPlatformCommand` + Relay **non-urgent**.

**Forge all commands (roadmap):** [FORGE-ADR-0066](https://github.com/AI-Guiders/agent-forge/blob/master/design/FORGE-ADR-0066-forge-all-commands-platform-pattern.md) — W0 editor ✓, then catalog visitor, MCP-bound domain, executor dispatch only.

## Anti-patterns

- `applyFormat` / `applyX` per surface.
- DTO + static executor without command classes when behavior is bundled and testable.
- Surface disables another surface on the same host.
- **Catalog as executor** — `capabilities.commands[]` handler with inline logic, no registry.
- **Registry as catalog** — registering paths without `SlashCatalogIndex` / descriptor merge for UI.

## Prior art

| Doc | Phrasing |
|-----|----------|
| CIDE ADR-0013 | единый реестр; toolbar + палитра = one layer |
| CIDE ADR-0119 | slash → `command_id`; не второй исполнитель |
| FORGE-ADR-0062 | one catalog, many surfaces |
