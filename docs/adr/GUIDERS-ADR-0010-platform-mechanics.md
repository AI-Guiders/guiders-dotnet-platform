# GUIDERS-ADR-0010: Platform mechanics (objects, contexts, product adapters)

| | |
|---|---|
| **Status** | Accepted (target architecture; phased quarry) |
| **Date** | 2026-08-27 |
| **Tags** | #guiders #platform #mechanics #commandplane #cockpit #forge #cide #glass |
| **Related** | GUIDERS-ADR-0001 · GUIDERS-ADR-0003 · GUIDERS-ADR-0009 · FORGE-ADR-0066 |

## Context

[GUIDERS-ADR-0001](GUIDERS-ADR-0001-platform-boundary.md) defines platform as **contracts + mechanical canon**, not UI. Products (Forge, CIDE, Glass, cdp-mcp) historically duplicated **mechanics** in peel: slash resolve/execute, editor transforms, catalog merge, CCU fold logic.

[GUIDERS-ADR-0009](GUIDERS-ADR-0009-command-surface-pattern.md) names **Command · Registry · Catalog · Surface** for slash/editor. This ADR generalises **mechanics** as the platform layer those patterns implement — and how products attach **context-specific adapters** without forking semantics.

Ecosystem intent: unify **storage and behaviour** at platform; unify **invocation** via registry + visitor; keep **presentation** in products.

## Decision

### 1. Mechanic = platform object

A **mechanic** is a headless, testable unit of behaviour with:

| Facet | Meaning |
|-------|---------|
| **Contract** | inputs / outputs (DTOs, `CommandOutcome`, channel snapshots) |
| **Implementation** | platform package code (`CommandPlane`, `Cockpit.*`, future `Correspondence`, `Desk`) |
| **Identity** | stable id (`commandId`, channel name, CCU kit id) |
| **Context** | product-supplied invocation payload (`EditorBufferContext`, `ForgeCommandContext`, ER inputs) |

Mechanics are **not** Razor, Avalonia, WPF, or `forge-slash.js`. Those are **surfaces** or **adapters**.

### 2. Layer map (platform quarry)

```text
┌─────────────────────────────────────────────────────────────┐
│  Platform mechanics (NuGet)                                 │
│  CommandPlane · Cockpit.* · Routing · (Correspondence, Desk)  │
└─────────────────────────────────────────────────────────────┘
         ▲                    ▲                    ▲
         │ adapter            │ adapter            │ wire
   Forge View JS         CIDE / Glass          cdp-mcp MCP
   (buffer executor)     (Relay*, palette)     (headless invoke)
```

| Package | Mechanic families (examples) |
|---------|------------------------------|
| **CommandPlane** | slash DOI, ArgTail, line resolver, **GoF Command**, registry, catalog visitor |
| **Cockpit.*** | IdeHealth/EnvReady fold, DataBus events, CDS routing DTOs |
| **Routing** | `IIntentOrgan`, route refusal |
| **Abstractions** | `IntentOutcome`, shared outcome shapes |

\* `RelayPlatformCommand<T>` — product UI adapter (deferred).

### 3. Command mechanics (CommandPlane SSOT)

Slash/editor mechanics use the stack from [GUIDERS-ADR-0009](GUIDERS-ADR-0009-command-surface-pattern.md):

```text
Register(IPlatformCommand<TContext>)  →  Registry
        │
        ├── TryExecute(commandId, context)     ← mechanics
        └── Accept(ICatalogVisitor)            ← catalog view (not second store)
```

Platform types (v1):

- `ICommandContext`, `IPlatformCommand<T>`, `PlatformCommand<T>`, `CommandOutcome`
- `PlatformCommandRegistry<TContext>`
- `ICatalogVisitor<TContext>` (target) — builds `SlashCatalogIndex`, capabilities projections

**Rule:** new slash/editor behaviour = platform mechanic (+ product registration), not surface handler logic.

### 4. One mechanic, many contexts (implementations)

Same **semantic** mechanic may execute in different **hosts**:

| Mechanic | Platform SSOT | Forge | CIDE / Glass | cdp-mcp |
|----------|---------------|-------|--------------|---------|
| Buffer insert bold | `EditorFormatInsertCommand` | `forgeEditorCommand` JS | future Relay | — |
| Issue close | `IPlatformCommand<ForgeCommandContext>` (target) | HTTP execute → MCP | overlay + `ide_execute_command` | MCP wire |
| IdeHealth fold | Cockpit CCU kit | — | channel bind | `cdp_ide_health` |
| Slash line resolve | `SlashLineResolver` | capabilities + slash JS | TOML + overlay | melody headless |

**Anti-pattern:** second implementation of transform/execute semantics in a product peel.  
**Allowed:** thin adapter that builds context and calls platform registry.

### 5. Catalog content vs mechanics

| | Owner | Notes |
|---|--------|-------|
| **Catalog content** (which plugins, TOML paths, tiers) | Products / plugins | Issue plugin registers paths |
| **Catalog mechanics** (index, merge, resolve, visitor) | **CommandPlane** | `SlashCatalogIndex`, `SlashLineResolver` |
| **Command mechanics** (execute) | **CommandPlane** + product registry host | Forge merges plugin commands into host registry |

Descriptor DTOs (`SlashCommandDescriptor`) are **projection records**, not executors.

### 6. Quarry waves (mechanics)

| Wave | Mechanics scope | Product wire |
|------|-----------------|--------------|
| **W2** ✓ | CommandPlane descriptors, ArgTail, catalog index, line resolver | Forge/CIDE capabilities |
| **W2b** ✓ | Editor buffer `IPlatformCommand` + `EditorCommandRegistry` | Forge `forgeEditorCommand` |
| **W2c** ✓ | `ICatalogVisitor`, `ICatalogDescribed`, registry catalog builder; editor catalog from registry | Forge [0066 W1](https://github.com/AI-Guiders/agent-forge/blob/master/design/FORGE-ADR-0066-forge-all-commands-platform-pattern.md) | capabilities = visit |
| **W2e** ✓ | Melody descriptors + `InputNotation` quarry (CIDE `ChordNotation` parsers → platform) | CIDE adapter swap (pending) |
| **W2d** | Forge domain commands as `IPlatformCommand<ForgeCommandContext>` | executor → registry dispatch |
| **W1** ✓ | Cockpit DataBus / IdeHealth events | cdp-mcp, CIDE |
| **W3+** | Correspondence, Desk latch mechanics | Glass, CDP |

Stop line: each wave ships product DoD + tests before next mechanic family migrates.

## Non-goals

- UI kits in platform (`guiders-ui-platform` stays separate — [GUIDERS-ADR-0005](GUIDERS-ADR-0005-ui-platform-monorepo.md)).
- Plugin host / ALC in platform ([GUIDERS-ADR-0008](GUIDERS-ADR-0008-plugin-host-hyperlane.md) — `guiders-plugin-host`).
- Replacing Forge MCP kernel or CIDE `IdeCommands` VM in one step.

## Consequences

- Product PRs that add behaviour must state: **which platform mechanic** extends (or why product-local escape).
- Platform PRs add mechanics without Avalonia/WPF dependencies ([0001](GUIDERS-ADR-0001-platform-boundary.md) DAG).
- ADR cross-links: command surfaces ([0009](GUIDERS-ADR-0009-command-surface-pattern.md)), quarry map ([0003](GUIDERS-ADR-0003-platform-ssot-quarry.md)), Forge rollout ([FORGE-ADR-0066](https://github.com/AI-Guiders/agent-forge/blob/master/design/FORGE-ADR-0066-forge-all-commands-platform-pattern.md)).

## Glossary

| Term | Definition |
|------|------------|
| **Mechanic** | Platform-owned behaviour object + contract |
| **Context** | Product-built invocation payload for a mechanic |
| **Adapter** | Product code that builds context and calls platform (JS executor, HTTP endpoint, Relay) |
| **Surface** | Human discoverability entry (palette, slash popover, CCL) — invoker only |
