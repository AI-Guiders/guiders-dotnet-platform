# GUIDERS-ADR-0003: Platform SSOT quarry (cross-product)

**Status:** accepted (2026-08-22)  
**Tags:** #guiders #platform #slash #forge #cide #cdp #glass  
**Related:** GUIDERS-ADR-0001 · GUIDERS-ADR-0002 · GUIDERS-ADR-0009 · GUIDERS-ADR-0010 · CIDE ADR-0150/0154/0160 · FORGE-ADR-0015 · FORGE-ADR-0066

---

## Decision

**guiders-platform** is SSOT for **headless mechanics** shared by CIDE, CDP, Glass, and Forge.

| Layer | Platform owns | Products own |
|-------|---------------|--------------|
| Contracts | interfaces, DTOs, event records | — |
| Mechanics | resolver, merge, fold CCU logic (headless) | — |
| Catalog **content** | — | TOML, Forge plugins, Glass subset |
| UI / host | — | Avalonia, WPF, forge-slash.js, MCP wire |
| Execute | request/response **shape** | handlers, MCP, HTTP |

**Done** = mechanics in platform package + product references it — not presence peel alone.

---

## Package map (target)

| Package | SSOT for | Consumers |
|---------|----------|-----------|
| `Cockpit.Abstractions` | IChannel, ICdsRouter, ICockpitComputeUnit | all |
| `Cockpit.DataBus` | IDataBus, events (Build/Tests/Debug/Git/IdeHost/Startup…) | CIDE, cdp-mcp |
| `Cockpit.Channels` | IdeHealth/EnvReady DTOs + CCU kits | CIDE, cdp-mcp, Glass bind |
| `Cockpit.Cds` / `Composition` | routing/compositor DTOs | cdp-mcp |
| `Cockpit.Transport` | IngressEvent, BoundedIngressBus | cdp-mcp |
| **`CommandPlane`** | GoF command, catalog descriptors, `ICommandSource`, `CommandSource` | all mechanics |
| **`CommandPlane.Slash`** | Slash index, resolve, completion (→ Core) | **Forge, CIDE, DashSpec** |
| **`CommandPlane.Catalog.Sources.Json`** | JSON format → Core | Forge |
| **`CommandPlane.Catalog.Sources.Toml`** | TOML format → Core | CIDE |
| **`CommandPlane.Catalog.Sources.Xml`** | XML format → Core | — |
| **`CommandPlane.Catalog.Sources.File`** | File transport (`FromFile`, embedded) | embed products |
| **`CommandPlane.Catalog.Sources.Database`** | DB transport (delegate) | DashSpec, portals |
| **`CommandPlane.Catalog.Sources`** | Meta-bundle | convenience |
| **`InputNotation`** | Core IR: `NormalizedKeySequence`, `IInputNotationReader` | Melody, Binding |
| **`InputNotation.KeyGesture`** | `Ctrl+K` wire (quarry CIDE) | hotkeys.toml, Forge |
| **`InputNotation.Vim`** | `<C-k>` wire (quarry Neovim keycodes) | CIDE |
| **`InputNotation.Emacs`** | (planned) `C-x` wire (quarry Emacs key-parse) | — |
| **`InputNotation`** (meta) | facade `InputNotationParser` | lazy embed |
| **`CommandPlane.Melody`** | Melody descriptor, profile, policy | CIDE/Glass |
| **`CommandPlane.Binding`** | hotkeys catalog, layered merge, gesture normalize | CIDE |
| **`CommandPlane.Binding.Sources.Toml`** | `hotkeys.toml` format | CIDE |
| **`CommandPlane.Binding.Sources`** | meta-bundle | convenience |
| `Routing` | IIntentOrgan | cdp-mcp Citizen |

---

## Cross-product: Slash

**Pattern:** [GUIDERS-ADR-0009](GUIDERS-ADR-0009-command-surface-pattern.md) — **Catalog · Registry · Command · Surface**.

```
Forge host  ──capabilities.commands[]──►  Platform CommandDescriptor
       │                                          ▲
       │ overlay (ADR-0160)                       │ merge
       ▼                                          │
CIDE TOML ──IntentCatalogLoader──► CommandCatalogIndex ──► SlashLineResolver
       │                                          │
Glass subset (GlassSlashCatalog)                 │
CDP melody c: / cdp_glass run                    │
```

- **Forge** publishes descriptors; `/commands/execute` uses platform execute DTOs.
- **CIDE** bundles spine catalog + runtime Forge overlay → same index.
- **Glass** = thin path slice + WPF; no separate ArgTail enum.
- **cdp-mcp** = wire only; resolver from platform when melody/slash runs headless.

---

## Quarry waves

| Wave | Version | Scope |
|------|---------|-------|
| **W0** | 0.3.0 ✓ | Cockpit layer split, ER/IdeHealth input DTOs, CDS/Composition |
| **W1** | 0.3.1 | IdeHealth DataBus events + DebugSessionSnapshot |
| **W2** | 0.3.2 | **CommandPlane**: descriptor, ArgTail, catalog index, line resolver |
| **W3** | 0.3.3 ✓ | IdeHealth CCU fold (headless) + output snapshot |
| **W4** | 0.3.4 ✓ | EnvReady builder kit + path acquisition |
| **W5** | 0.4.0 ✓ | DataBus async policy; product wire (Forge/CIDE/cdp-mcp) |

**Forge command mechanics (all commands):** [FORGE-ADR-0066](https://github.com/AI-Guiders/agent-forge/blob/master/design/FORGE-ADR-0066-forge-all-commands-platform-pattern.md) — registry + catalog visitor + `IPlatformCommand` per wave; editor buffer W0 ✓.

---

## Product adoption checklist

### agent-nuget-pm (ANPM)
- [x] Pin manifest `guiders-platform-0.4.0.pins.json` (10 packages)
- [x] `scripts/Stage-GuidersPlatformFeed.ps1` — pack sibling + stage to offline feed
- [ ] Operator feed sync + `anpm_feed_index` on deployment host

### agent-forge
- [x] `ForgeCommandDescriptor` implements or maps to `CommandDescriptor`
- [x] capabilities JSON stable camelCase per platform schema (`group`, normalized paths)
- [x] `/commands/execute` accepts platform `SlashCommandExecuteRequest`

### cascade-ide (Avalonia — secondary; Forge overlay already wired for Lens)
- [ ] `CatalogRouteEntry` → platform type + CIDE extension struct
- [x] `ForgeSlashCatalogOverlay` → `CommandCatalogIndex` + CommandPlane descriptors (Lens path; not primary product surface)
- [x] Deprecate local `CommandArgTailKind` duplicate

### Glass (WPF — primary human client beside forge-slash.js)
- [ ] `GlassSlashCatalog` paths from platform slice
- [ ] Forge capabilities overlay via `CommandCatalogIndex.Merge` (deferred)
- [ ] WH/ER glances bind channel snapshots, not FS-only peel

### cdp-mcp
- [x] PackageReference CommandPlane when melody/slash headless
- [x] ER builder → platform kit (CDP rows = extension)
- [x] IdeHealth CCU + `cdp_ide_health`

---

## Non-goals

- Full `intent-catalog.toml` in NuGet
- Avalonia/Glass UI in platform
- Forge MCP executor / plugin host in platform
