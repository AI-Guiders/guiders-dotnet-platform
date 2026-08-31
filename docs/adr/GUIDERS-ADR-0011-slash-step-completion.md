# GUIDERS-ADR-0011: Slash step completion (Catalog mechanics)

| | |
|---|---|
| **Status** | Accepted |
| **Date** | 2026-08-28 |
| **Relates to** | GUIDERS-ADR-0009 · GUIDERS-ADR-0010 · FORGE-ADR-0066 · DASHSPEC-ADR-0043 |

## Decision

**Step-based slash autocomplete** (domain → object → intent → arg, plus flat-path fallback) lives in **`AIGuiders.Platform.CommandPlane`** as `SlashStepCompletion` + `SlashArgCompletion` ([ADR-0012](GUIDERS-ADR-0012-arg-picker-completion.md)).

| Layer | Owns |
|-------|------|
| **Platform** | `SlashStepCompletion.GetSuggestions`, `ArgCompletionItem`, snapshot over `CommandCatalogIndex`, `DomainOmittedInPath` / alias paths |
| **Product adapter** | Build `CommandCatalogIndex` from TOML, capabilities, bundled descriptors |
| **Surface** | Tab/Enter policy, popover chrome, debounced fetch (`GET /commands/complete`) |

`acceptKey` (default **Tab** on command surfaces) is **not** platform — surface policy per ADR-0009.

## Wire

```text
CommandDescriptor[] → CommandCatalogIndex → SlashStepCompletion
                                              ↘ SlashLineResolver (hide segments)
Surface ← HTTP complete (Forge/DashSpec) or in-proc (CIDE/Glass)
```

## Quarry waves

| Wave | Product | Ship |
|------|---------|------|
| W0 | guiders-platform | `SlashStepCompletion` 0.4.2 |
| W1 | CIDE/Glass | `SlashPlatformCatalog` + thin adapters |
| W2 | agent-forge | `GET /api/v1/commands/complete` + modal fetch |
| W3 | dash-spec | bundled `/select` catalog + slash bar |

## Anti-patterns

- Duplicate `nextPathSegments` in JS/C# peel
- Platform owning popover focus or accept-key binding
