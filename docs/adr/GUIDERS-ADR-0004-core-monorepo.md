# GUIDERS-ADR-0004: Core monorepo (Platform sibling)

**Status:** accepted (2026-08-22)  
**Tags:** #guiders #core #nuget #monorepo  
**Related:** GUIDERS-ADR-0001 · GUIDERS-CORE-0001

---

## Context

GUIDERS-ADR-0001 split **platform mechanics** (`guiders-platform`) from product repos. Backend libraries (`Cdp.Core`, `TerminalMcp.Core`, `AgentNotes.Core`, …) remained fragmented across `*-core` repos.

## Decision

1. **`guiders-core` monorepo** — sibling to `guiders-platform`, not nested inside it.
2. **Platform = cross-product contracts** (CommandPlane, Cockpit.*, Routing).
3. **Core = product backends** (CDP organs, MCP shared libraries, parsers, index, terminal habitat).
4. **Products** consume both via NuGet or sibling `ProjectReference` (`GuidersPlatformRoot`, `GuidersCoreRoot`).
5. **Submodule optional** — dev layout is sibling folders under `open/`.

## Non-goals

- UI/product apps (`cdp-mcp` exe, Glass, Forge server) stay in their repos.
- `ai-native-ui` (Anui) — federation role: ASP evidence/ingest/audit; see GUIDERS-UI-0004 · ANUI-ADR-0004 (not «fold when convenient»).

## Consequences

- Two monorepos, independent versioning and CI.
- ANPM pin manifests can reference `guiders-core-*` per package.
