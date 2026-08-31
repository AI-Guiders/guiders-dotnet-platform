# GUIDERS-ADR-0044: Command catalog scope

| | |
|---|---|
| **Status** | Accepted |
| **Date** | 2026-08-31 |
| **Tags** | #guiders #commandplane #catalog #federation |
| **Related** | GUIDERS-ADR-0009 · GUIDERS-ADR-0039 · GUIDERS-ADR-0040 · GUIDERS-ADR-0042 |

## Context

[GUIDERS-ADR-0009](GUIDERS-ADR-0009-command-surface-pattern.md) separates **Catalog** (what the user sees) from **Registry** (execute). `CommandDescriptor.Surfaces` tags **invoker channels** (`dash-ccl`, `editor-inline`) — not product areas (Dashboard vs Control Center).

Products need catalog rows and completion paths to depend on **where** the user works, without duplicating slash mechanics per host tab.

## Decision

### 1. Two axes (do not conflate)

```text
Surfaces (existing)   how invoked — ccl · palette · slash bar · hotkey
Scope (new)           where available — dashboard · controlcenter · editor · …
```

### 2. IR.Command

```text
CommandDescriptor.Scope   IReadOnlyList<string> — empty = visible in all active scopes
```

TOML/catalog fields: `scope` or `scopes` (list).

### 3. Context

```text
ICommandScopedContext : ICommandContext
  ActiveScope   IReadOnlyList<string>   tags active for this invocation
```

Products implement on their context type (`DashboardFilterContext`, …). Legacy contexts without `ICommandScopedContext` skip scope filtering at the product adapter (backward compatible).

### 4. Filter rule

```text
visible  ⇔  descriptor.Scope is empty  ∨  ∃ tag ∈ descriptor.Scope ∩ context.ActiveScope
```

`CommandScopeFilter` in `CommandPlane.Catalog` — apply when composing catalog sources, before `CommandCatalogIndex`.

### 5. DashSpec v1 tags

| Scope | Commands |
|-------|----------|
| *(empty)* | `show …` (host navigation) |
| `dashboard` | `select`, `view`, filter/report/page paths |
| `controlcenter` | reserved for CC-only settings commands |

## Consequences

- Completion and execute share the same filtered index — no host/dashboard drift.
- New host tab = new scope string + catalog tags; no platform enum churn.
- `Surfaces` stays invoker-only; never overload for host plane.
