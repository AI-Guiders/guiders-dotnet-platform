# GUIDERS-ADR-0002: Avalonia quarry gap (mechanics vs projection)

**Status:** accepted (2026-08-21)  
**Tags:** #guiders #platform #cockpit #glass #avalonia #quarry  
**Related:** GUIDERS-ADR-0001 · CDP-ADR-0021 · cascade-ide ADR 0036/0079/0094/0097/0099/0102 · `cdp-mcp/.cdp/domain/glass.md` PreCondition inventory

---

## Context

Avalonia CIDE is **EOL quarry**, not Windows-primary SSOT (CDP-ADR-0021). WPF Glass is **projection** of the same mechanical pipeline:

```
DAL → Transport → CCU → Channel → CDS → Compositor → Surface snapshot
IDS (overlay domain, orthogonal to CDS)
```

Before mid-August 2026, work stamped **Presence/Depth CLOSED** while **mechanics** remained in Avalonia or as thin WPF peels — **seeming Done** (see `glass.md` PreCondition REOPENED, `IdeSeemingDoneShield`).

Platform v0.1 initially shipped only Abstractions + Routing — **under-scoped** relative to operator intent (mechanics canon, not UI fork).

## Decision

1. **Cockpit split packages** (not one monolith): `AIGuiders.Platform.Cockpit.Abstractions` (contracts), `.Ids`, `.DataBus`, `.Transport` (kits). Monolith `AIGuiders.Platform.Cockpit` 0.1.0 deprecated.
2. **Products** implement domain CCUs, DAL adapters, and surface renderers — not duplicate layer interfaces.
3. **Avalonia** = quarry to **distill** mechanics into platform + headless CDP; **not** extended on Windows primary.
4. **Glass WPF** = `ISurfaceAdapter` / snapshot binders only; **no new mechanics** without platform contract or named `DIG REJECT`.
5. **Done stamp** requires mechanics in platform **or** explicit supersede/reject — not presence peel alone.

## Gap matrix — layer canon (2026-08-21)

| Layer | Avalonia CIDE SSOT | Platform Cockpit v0.2 | cdp-mcp `Cockpit/` | Glass WPF | Miss |
|-------|-------------------|----------------------|-------------------|-----------|------|
| **DAL boundary** | `Features/*/DataAcquisition/` | `IDataAcquisition` marker | `ToolchainPathProbe` (local) | DAL-adjacent peels | Product DAL not wired to platform marker |
| **Transport** | ADR 0094 ingestion | `IngressEvent`, `BoundedIngressBus<T>` | `DeskIngestionBus` (duplicate) | — | cdp-mcp not on platform transport yet |
| **DataBus** | ADR 0099 `IDataBus` | `IDataBus`, `InMemoryDataBus` | duplicate namespace | — | cdp-mcp still local copy |
| **CCU** | ADR 0097 units (IdeHealth, …) | `ICockpitComputeUnit` | ~15 desk units | — | **Mechanics** (IdeHealth fold, EnvReady) still Avalonia |
| **Channel** | per-domain channels | `IChannel`, `IChannelCoordinator` | partial | — | Health/Env channels not distilled |
| **CDS** | ADR 0036 routers | `ICdsRouter` | `AttentionCdsRouter`, go maps | CabinGlass pins | Router **logic** split product-local |
| **Compositor** | ADR 0036 compositors | `ISurfaceCompositor` | `SeatsSurfaceCompositor` | WPF bind | Compositor output DTO not shared NuGet type |
| **IDS** | ADR 0079 palette/overlays | `IIdsFeatureSearch` | `FeatureSearchUnit` | Ctrl+K overlay | Full IDS pipeline still Avalonia-heavy |
| **Routing** | melody / MCP | `AIGuiders.Platform.Routing` | Citizen organs | Glass melody peel | Wire cdp-mcp → NuGet (wave 2) |

**Root miss:** layer **interfaces** duplicated in cdp-mcp and cascade-ide; **domain CCU mechanics** never quarried from Avalonia into headless/platform — only WPF **presence** shipped.

## Gap matrix — surfaces (Avalonia EOL quarry)

From `glass.md` PreCondition + MFD matrix. **Presence CLOSED ≠ mechanics adopted.**

| Surface | Avalonia mechanic SSOT | Glass now | Platform | Verdict |
|---------|------------------------|-----------|----------|---------|
| **IdeHealth / EnvReady** | CCU + channel + DataBus | FS glance stubs | Cockpit contracts only | **MISS** — CCU mechanics in Avalonia |
| **IdeDap full drive** | `IdeDapDebugSession` | latch + bridge | — | **DIG REJECT** defer; densest-for-Glass OK |
| **WNM / SemanticMap** | `WorkspaceNavigationMapView` | Skia multi-hop peel | — | Peel densest; graph mechanics partial |
| **Intercom 0072/0096** | Skia overview/spine | WPF adopted 2026-08-05 | — | **ADOPTED** (mechanics ported) |
| **Intercom ThreadNode 0172** | tree compositor | supersede NorthStar | — | **SUPERSEDE** — not port |
| **Correspondence CRS** | Toml ADR + reverse | full CRS in GlassCore feed | Correspondence pkg planned | Depth CLOSED; platform pkg pending |
| **Events / Hypotheses** | MFD + catalog | JSON glances | — | Presence only |
| **WorkspaceHealth** | CCU | FS glance | — | **MISS** |
| **Build / Test / Git** | panels + parsers | host peels | DAL/CCU local | Mechanics split; parsers in other NuGets |
| **Terminal / WebAi** | Avalonia hosts | VT / WebView2 | — | **CLOSED** host class; Avalonia EOL |

### Stamped CLOSED before 15 Aug that were not full mechanics transfer

| Stamp | What closed | What did not move |
|-------|-------------|-------------------|
| Presence DoD | SoftInstrument + glances | CCU fold logic |
| Depth DoD (CRS) | Resolver wire in feed | `AIGuiders.Platform.Correspondence` |
| Terminal VT / WebView2 | WPF host | — (host swap OK) |
| SemanticMap Skia | Graph peel | WNM parity partial |
| DebugStack DAP | latch + bridge | Full IdeDap UI (rejected) |

## Recovery order

1. **Ship** `AIGuiders.Platform.Cockpit.*` 0.2.x split (this ADR).
2. **cdp-mcp** — replace `Cockpit/DataBus`, `Transport`, interface files with `PackageReference`; keep product CCUs.
3. **Quarry batch A** — IdeHealth + EnvironmentReadiness CCU/channel from Avalonia → headless platform types + CDP CCU.
4. **Quarry batch B** — Correspondence → `AIGuiders.Platform.Correspondence`.
5. **Glass** — bind snapshots only; refuse CLOSED without row in this matrix = `mechanics|supersede|reject`.

## Non-goals

- Copy Avalonia views / MWVM into platform
- Stamp Adopted from PNG / presence peel
- Merge cascade-ide repo into guiders-platform
