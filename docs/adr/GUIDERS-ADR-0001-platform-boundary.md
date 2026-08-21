# GUIDERS-ADR-0001: Platform boundary

**Status:** accepted (2026-08-21)  
**Tags:** #guiders #platform #nuget #cdp #glass #forge  
**Related:** CDP-ADR-0021 (Windows-first glass) · CDP-ADR-0198 (sidecar bridge) · buffer plane OOA&D peel (`CitizenBuffer*`)

---

## Context

AI Guiders products share architecture without sharing a repo:

- **CDP** (`cdp-mcp`) — agent habitat, `@intent`, latches, DocumentEditPlane
- **Glass** (`cascade-ide`) — operator projector, RunPalette / melody / `op=run`
- **Forge** — durable API + MCP bridge
- **Avalonia CIDE** — EOL quarry; distill contracts, do not extend on Windows primary

Duplication lives in: route+execute organs, pulse/ack shapes, alias tables, CRS/correspondence, desk latch IPC.

DashSpec is **out of scope** for platform v0 (separate product repo; no slash-command layer required).

## Decision

1. **One platform monorepo** — `guiders-platform` publishing **4–6 NuGet packages**, not one package per organ and not one repo per package.
2. **Products keep their repos** — consume platform via `PackageReference` on **nuget.org**.
3. **GitHub + AMS/Forge hosts** — CI/deploy targets only; **one canonical git source** per product (no mirror repos).
4. **Platform = contracts + kits**, not UI or habitat implementation:
   - Abstractions (outcome, route envelope, pulse)
   - Routing (`Route` + `Execute` + test override seam — `CitizenBuffer*` pattern)
   - CommandPlane (wave 2) — catalog TOML + alias resolution for Glass / Citizen / Forge
   - Correspondence (wave 2) — Toml ADR + DocReverse without UI
   - Desk (wave 3) — latch/seat contracts
5. **Dependency DAG** — Abstractions ← Routing ← products; **no** WPF/Avalonia refs in platform; **no** habitat → glass cycles.

```
AIGuiders.Platform.Abstractions
        ↑
AIGuiders.Platform.Routing
        ↑
  cdp-mcp · GlassCore · Forge.*
```

## v0 deliverable (this repo)

- `AIGuiders.Platform.Abstractions` — stable outcome/route/pulse types
- `AIGuiders.Platform.Routing` — `IIntentOrgan<TRoute,TOutcome>`, `DispatchCallOverride`
- ADR + CI build/test
- **No** cdp-mcp wire yet (wave 2: ProjectReference → PackageReference after first pack)

## Non-goals

- Physical merge of cdp-mcp + cascade-ide
- DashSpec slash registry
- Repo-per-NuGet explosion
- Moving Citizen organs into platform (handlers stay in cdp-mcp; **pattern** lives in platform)

## Consequences

- Semver on Abstractions/Routing: breaking changes = major bump
- Peel new buffer/meta organs in cdp-mcp using platform interfaces
- Avalonia CIDE mined into platform packages over time, not copied as UI
