# GUIDERS-ADR-0009: Command–Surface pattern (GoF Command + invocation surfaces)

| | |
|---|---|
| **Status** | Accepted |
| **Date** | 2026-08-27 |
| **Tags** | #guiders #commandplane #slash #forge #cide #cdp #glass #pattern #gof |
| **Related** | GUIDERS-ADR-0003 · GUIDERS-ADR-0007 · CIDE ADR-0013/0030/0119 · FORGE-ADR-0065/0064 |

## Name

Two coupled ideas:

1. **GoF Command** — объект с `CommandId`, `CanExecute(context)`, `Execute(context)`; одна реализация эффекта.
2. **Command–Surface** — **one command, many invocation surfaces** (toolbar, slash, CCL, palette, hotkey, MCP).

- **Command** = executor object / registry entry.
- **Invocation surface** = UI invoker; будущий **RelayCommand&lt;T&gt;** в Glass/CIDE — только invoker.

Короткий ярлык: **Command–Surface** (не «одна полоса UI»).

## Platform contract (GoF)

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

**Catalog** (`SlashCommandDescriptor`) = discoverability. **Command class** = execution. Map descriptor → command in product registry.

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

## Anti-patterns

- `applyFormat` / `applyX` per surface.
- DTO + static executor without command classes when behavior is bundled and testable.
- Surface disables another surface on the same host.

## Prior art

| Doc | Phrasing |
|-----|----------|
| CIDE ADR-0013 | единый реестр; toolbar + палитра = one layer |
| CIDE ADR-0119 | slash → `command_id`; не второй исполнитель |
| FORGE-ADR-0062 | one catalog, many surfaces |
