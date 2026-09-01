# GUIDERS-ADR-0051: Authoring project abstraction (host-agnostic document graph)

| | |
|---|---|
| **Status** | Accepted (v0 slice 2026-09-01) |
| **Date** | 2026-09-01 |
| **Tags** | #guiders #authoring #dsl #project #paths |
| **Related** | GUIDERS-ADR-0048 · GUIDERS-ADR-0049 · GUIDERS-ADR-0050 · ATC-ADR-0001 |

## Context

[ADR-0048](./GUIDERS-ADR-0048-authoring-quarry-family.md) split **declare-time** authoring from wire Notations and named planet sovereignty (`.dashspec` stays in dash-spec). Parsers and `Authoring.Core` kit shipped, but every consumer still calls `Parse(text)` or `ParseFile(path)` directly — **no shared project model** between grammar, LSP host, and runtime host.

Planet DSLs (DashSpec, catalog) can run **without** a product host: validate, emit tier-D wire, codegen. Tooling (`authoring-toolchain`) needs one workspace contract: entry document, logical paths, federation imports, diagnostics — not Blazor or SQL.

[ADR-0050](./GUIDERS-ADR-0050-paths-guild-logical-physical.md) fixed logical vs physical paths; authoring projects use `LogicalPath` for repo-relative files and a separate **federation wire** kind for `import <grain/…>` (not filesystem).

## Decision

### 1. `Authoring.Project` lives in `Authoring.Core`

Host-agnostic types (NuGet `AIGuiders.Platform.Authoring.Core`):

| Type | Role |
|------|------|
| `AuthoringDocumentKind` | `LogicalFile` vs `FederationImport` |
| `AuthoringDocumentRef` | Stable id: logical path or federation import wire |
| `ResolvedAuthoringDocument` | Ref + optional source text + display path for diagnostics |
| `AuthoringProject` | Workspace root (physical), entry `LogicalPath`, document graph |
| `AuthoringProjectLoader` | `OpenSingleFile(workspaceRoot, entryPath)` — v0 |

**Not in Core:** grammar-specific parse, IR, emit — stay in `Authoring.Command.*` / planet packages.

### 2. Two document kinds (do not conflate)

| Kind | Example | Resolved how |
|------|---------|----------------|
| **Logical file** | `Catalog/dash.catalog`, `specs/stakeholder.dashspec` | `PathBoundary.ToLogical` + `File.ReadAllText` |
| **Federation import** | `grain/date-filter`, `value/preset` | `ICatalogBundleLibrary` / planet stdlib — **not** `Path.Combine` |

Wire paths inside a grammar (`import <grain/date-filter>`) are **not** `LogicalPath` ([ADR-0050](./GUIDERS-ADR-0050-paths-guild-logical-physical.md) anti-pattern table).

### 3. Planet adapters compose Core + grammar

```text
AuthoringProjectLoader.OpenSingleFile(root, entry)
        │
        ▼
AuthoringProject (entry logical doc)
        │
        ├── CatalogProject.Open  → parse + federation import refs in graph
        └── (future) DashSpecProject.Open → !include graph, multi-root
        │
        ▼
IR / emit / host consume resolved model — not raw ParseFile in UI
```

First consumer: **`CatalogProject`** in `Authoring.Command.Catalog` — proof that project graph + existing `CatalogParser` compose.

### 4. Repo map (no new DSL monorepo)

| Concern | Repo | Artifact |
|---------|------|----------|
| Project contract + kit | `guiders-platform` | `Authoring.Core` |
| Federation grammars | `guiders-platform` | `Authoring.Command.*` |
| LSP / CLI workspace | `authoring-toolchain` | pins `Authoring.*`, uses `AuthoringProject` |
| Planet grammar + analysis | planet repo (e.g. dash-spec) | `DashSpec.Core` / future `DashSpec.Authoring` |
| Planet runtime host | planet repo | `DashSpec.Host` |

**Non-goal:** single `dsls/` monorepo merging planet grammars — violates Constitution planet sovereignty and ADR-0048.

### 5. Diagnostics

| Code | When |
|------|------|
| `EntryFileNotFound` | Entry path missing on disk |
| `EntryOutsideWorkspace` | Entry not under `workspaceRoot` |

Grammar diagnostics unchanged; project + parse diagnostics merge in `CatalogProjectResult`.

## Migration

| Consumer | v0 | Target |
|----------|-----|--------|
| `CatalogParser.Parse` / `ParseFile` | Keep — low-level API | Call via `CatalogProject.Open` in hosts/toolchain |
| `authoring-toolchain` `Authoring.Cli validate` | Parse file path | `CatalogProject.Open(workspace, catalog)` |
| `dash-spec` `dash.catalog` | `CatalogParser` in Host | Pin `CatalogProject`; Core stays host-free |
| DashSpec `.dashspec` graph | Ad hoc includes in Core parser | `DashSpecProject` on `AuthoringDocumentWalker` + `!include` ([DASHSPEC-ADR-0024](https://github.com/AI-Guiders/dash-spec/blob/main/design/DASHSPEC-ADR-0024-document-authoring-layers.md)) |

## Consequences

- `Authoring.Core` takes dependency on `AIGuiders.Platform.Paths` (logical entry only).
- Toolchain gets stable workspace API without importing CommandPlane or planet hosts.
- Planet repos extract headless packages; hosts depend on project + IR, not parser sprawl.

## Open (next waves)

| # | Topic |
|---|--------|
| 1 | Multi-file logical graph (`!include`, glob) — `AuthoringProjectLoader` extensions |
| 2 | `Authoring.Workspace` grammar — only after second consumer ADR ([0048](./GUIDERS-ADR-0048-authoring-quarry-family.md) §5) |
| 3 | Conformance vectors `authoring/project/*.spec.json` |
| 4 | DashSpec adopt `AuthoringProject` + shared LSP host from `authoring-toolchain` |
