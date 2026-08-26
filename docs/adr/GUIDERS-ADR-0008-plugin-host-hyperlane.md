# GUIDERS-ADR-0008: PluginHost hyperlane

| | |
|---|---|
| **Status** | Accepted |
| **Date** | 2026-08-26 |
| **Tags** | #guiders #pluginhost #forge #anpm #federation |
| **Related** | GUIDERS-ADR-0003 · GUIDERS-ADR-0006 · GUIDERS-VISION-0001 · FORGE-ADR-0060 |

## Decision

### 1. Fourth hyperlane (not Platform)

| Layer | Home |
|-------|------|
| Repo | **`guiders-plugin-host`** (sibling monorepo) |
| NuGet | **`AIGuiders.PluginHost.*`** |
| Platform repo | ADR + conformance **docs only** — no loader in `AIGuiders.Platform.*` |

GUIDERS-ADR-0003 non-goals (**plugin host in platform**) unchanged.

### 2. Package family (v0.1.0)

| Package | Role |
|---------|------|
| `Abstractions` | `host-runtime.manifest.json` schema, pkg manifest sketch |
| `Runtime` | ALC, bootstrap, assembly resolver, pkg verify |
| `Build` | MSBuild targets + manifest writer tool |
| `Conformance` | bundle-order helpers for consumer CI |

### 3. Membership

1. Reference `AIGuiders.PluginHost.Runtime` (or Conformance in tests).
2. Ship `host-runtime.manifest.json` on publish (hosts) or validate pkg layout (ANPM).
3. Pass conformance checklist ([plugin-host-conformance.md](../conformance/plugin-host-conformance.md)).

### 4. Consumers

| Planet | Role |
|--------|------|
| `agent-forge` | Embassy — `IForgePlugin` product law |
| `agent-nuget-pm` | `anpm.plugin.verify` — layout, not load |
| Future hosts | optional opt-in |

## Non-goals

- MEF / global `IPluginModule` replacing Forge registries in v1.
- Mandatory for all confederation planets.

## Consequences

- Federation table extended (GUIDERS-ADR-0006).
- ANPM pin manifests may include `AIGuiders.PluginHost.*` slices.
- Bug class «DLL on disk, wrong ALC» fixed in PluginHost once.
