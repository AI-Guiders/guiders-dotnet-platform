# GUIDERS-ADR-0050: Paths guild (logical · physical boundary)

| | |
|---|---|
| **Status** | Accepted |
| **Date** | 2026-09-01 |
| **Tags** | #guiders #paths #workspace #correspondence #notations |
| **Related** | GUIDERS-ADR-0021 · GUIDERS-ADR-0028 · GUIDERS-ADR-0047 · GUIDERS-ADR-0048 |

## Context

Federation already separates **wire paths** (slash/console/catalog phrase — [ADR-0047](./GUIDERS-ADR-0047-command-for-doi.md), [ADR-0048](./GUIDERS-ADR-0048-authoring-quarry-family.md)) from filesystem operations. Repo/workspace code still duplicated `Replace('\\','/')`, `TryRel`, and `Path.GetFullPath` in correspondence, navigation, and workspace policy.

.NET `System.IO.Path` is OS-local. Portable PDB / Source Link address **build-time** symbols, not runtime logical identity across machines.

## Decision

### 1. Three path kinds (do not conflate)

| Kind | Example | Guild |
|------|---------|-------|
| **Wire path** | `buffer open`, `filter.date`, `import <grain/date-filter>` | `Notations.*` / Authoring — not filesystem |
| **Logical path** | `src/Foo/Bar.cs`, `docs/adr/0047-…` | **`AIGuiders.Platform.Paths`** |
| **Physical path** | `D:\…`, `/home/runner/…` | BCL + **TruePath** at IO boundary only |

### 2. Package `AIGuiders.Platform.Paths`

| Type | Role |
|------|------|
| `LogicalPath` | Repo/workspace-relative; canonical `/`; comparison helpers |
| `PathBoundary` | `ToLogical(workspaceRoot, absolute)` / `ToPhysical(workspaceRoot, logical)` |
| TruePath (`AbsolutePath`) | Physical normalization behind `PathBoundary`; not exposed as public API surface |

Doc/correspondence helpers (`GuessTitle`, markdown line excerpt) remain in `Documentation.Correspondence.Core`; they call `LogicalPath` for normalize/rel/match.

### 3. Conformance

`docs/conformance/paths/logical-normalize.spec.json` — normalize + doc-path vectors (same quarry pattern as `notation/*.spec.json`).

### 4. Migration

| Consumer | Change |
|----------|--------|
| `CorrespondencePaths` | Delegate normalize / TryRel / PathsMatch to `Paths` (shipped) |
| `WorkspaceExploreCorrPolicy` | Follow-up: use `LogicalPath` for rule keys |
| Navigation `RelativePath` | Follow-up: align naming with `LogicalPath` where repo-relative |

### Anti-patterns

| Do not | Because |
|--------|---------|
| Use `Path.Combine` for logical keys in IR/catalog | OS separators leak into SSOT |
| Re-export TruePath types from federation packages | Physical layer is implementation detail |
| Treat slash command path as `LogicalPath` | Wire alphabet — Notations branch |

## Consequences

- New NuGet: `AIGuiders.Platform.Paths` (depends on TruePath privately at compile time; consumers take only logical API).
- Planets reference quarry package for workspace/doc correlation; native ports may wrap same rules in TS/Kotlin.
