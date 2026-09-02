# GUIDERS-ADR-0001: Platform boundary



**Status:** accepted (2026-08-21) · **amended** 2026-08-21 (Cockpit canon)  

**Tags:** #guiders #platform #nuget #cdp #glass #forge  

**Related:** GUIDERS-ADR-0002 · CDP-ADR-0021 · CDP-ADR-0198 · buffer plane OOA&D peel (`CitizenBuffer*`)



---



## Context



AI Guiders products share architecture without sharing a repo:



- **CDP** (`cdp-mcp`) — agent habitat, `@intent`, latches, DocumentEditPlane

- **Glass** (`cascade-ide` / WPF projector) — operator projection of cockpit snapshots

- **Forge** — durable API + MCP bridge

- **Avalonia CIDE** — EOL quarry; distill **mechanics** into platform, not UI



Duplication lives in: route+execute organs, pulse/ack shapes, **cockpit layer pipeline**, alias tables, CRS/correspondence, desk latch IPC.



DashSpec is **out of scope** for platform v0 (separate product repo).



## Decision



1. **One platform monorepo** — `guiders-platform` publishing **4–6 NuGet packages** on **nuget.org** (`AIGuiders.*`).

2. **Products keep their repos** — consume platform via `PackageReference`.

3. **Platform = contracts + kits + mechanical canon**, not UI:

   - **Abstractions** — outcome, route envelope, pulse

   - **Routing** — `IIntentOrgan`, test override seam

   - **Cockpit.Abstractions** — CCU, DAL, channel, CDS, compositor contracts
   - **Cockpit.Ids** — IDS overlay seam
   - **Cockpit.DataBus** / **Cockpit.Transport** — kits (ADR 0099/0094)

   - **CommandPlane** (wave 2) — catalog TOML + alias resolution

   - **Correspondence** (wave 2) — Toml ADR + DocReverse without UI

   - **Desk** (wave 3) — latch/seat contracts

4. **WPF Glass = projection only** — binds surface snapshots; does not own CCU/channel mechanics.

5. **Dependency DAG** — Abstractions ← Routing; Cockpit standalone; **no** WPF/Avalonia refs in platform.



```

AIGuiders.Platform.Abstractions

        ↑

AIGuiders.Platform.Routing



AIGuiders.Platform.Cockpit.Abstractions
AIGuiders.Platform.Cockpit.Ids
AIGuiders.Platform.Cockpit.DataBus
AIGuiders.Platform.Cockpit.Transport

        ↑
  cdp-mcp · Glass · Forge.*
```

Monolith `AIGuiders.Platform.Cockpit` **0.1.0** — deprecated.

## v0 deliverable (this repo)

- `AIGuiders.Platform.Abstractions` — stable outcome/route/pulse types
- `AIGuiders.Platform.Routing` — `IIntentOrgan<TRoute,TOutcome>`, `DispatchCallOverride`
- `AIGuiders.Platform.Cockpit.*` — split layer packages (see GUIDERS-ADR-0002)

- ADR + CI build/test + Trusted Publishing `release.yml`



## Non-goals



- Physical merge of cdp-mcp + cascade-ide

- DashSpec slash registry

- Avalonia UI packages in platform

- Moving Citizen organs into platform (handlers stay in cdp-mcp)



## Consequences



- Semver on public contracts: breaking = major bump

- Avalonia quarry → platform mechanics; see GUIDERS-ADR-0002 for miss inventory

- Presence/Depth CLOSED in Glass ≠ platform Done without mechanics row closed


