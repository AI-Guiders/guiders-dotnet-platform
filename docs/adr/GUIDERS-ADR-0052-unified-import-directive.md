# GUIDERS-ADR-0052: Unified `import` directive (no `!include` split)

| | |
|---|---|
| **Status** | Accepted (v0 — Authoring.Core parser; DashSpec migration tracked) |
| **Date** | 2026-09-01 |
| **Tags** | #guiders #authoring #dsl #include #import |
| **Related** | GUIDERS-ADR-0047 · GUIDERS-ADR-0048 · GUIDERS-ADR-0050 · GUIDERS-ADR-0051 · DASHSPEC-ADR-0017 |

## Context

Authoring grammars mixed two top-level directives for the same concept («pull another document into scope»):

| Today | Where | Meaning |
|-------|-------|---------|
| `import <grain/date-filter>` | `.catalog` ([0047](./GUIDERS-ADR-0047-command-for-doi.md)) | Federation stdlib wire |
| `!include "diagrams/foo.dashdiagram"` | `.dashspec` ([DASHSPEC-ADR-0017](https://github.com/AI-Guiders/dash-spec/blob/main/design/DASHSPEC-ADR-0017-file-includes-and-stdlib.md)) | Logical file / glob |
| `!include <stdlib/path>` | DashSpec informative (PlantUML parity) | Planet stdlib wire |

Disambiguation was **already aligned** (quoted path vs angle-bracket wire). Only the **keyword** diverged — split brain for authors, LSP, and `Authoring.Project` graph walkers.

DashSpec `extensions { import from "…dll" }` is **plugin load**, not document include — out of scope (stays `import from` inside `extensions` block).

## Decision

### 1. One normative keyword: `import`

All declare-time **document** includes use `import`. **`!include` is deprecated** (parser may accept as transitional alias; authors must not mix in new files).

### 2. Two target shapes (same rule everywhere)

| Form | Target kind | Resolved by | Example |
|------|-------------|-------------|---------|
| `import "…"` | **Logical** | `LogicalPath` + workspace root; glob allowed in quotes | `import "imports/shell.dashinclude"` |
| `import <…>` | **Wire library** | Federation bundle lib / planet stdlib — **not** `Path.Combine` | `import <grain/date-filter>` |

Optional alias (catalog today, shared rule):

```text
import <grain/date-filter> as dash-date
```

### 3. Shared parser in `Authoring.Core`

`AuthoringImportLine.TryParse(line)` → `AuthoringImportDirective` (`LogicalPath` vs `WireLibrary` + optional `as` alias).

Branch grammars (catalog header, DashSpec module body, future workspace) call the shared helper — no per-grammar `import` / `!include` forks.

### 4. `Authoring.Project` graph

| `AuthoringImportDirective` | `AuthoringDocumentKind` |
|----------------------------|-------------------------|
| `LogicalPath` | `LogicalFile` (after resolve + read) |
| `WireLibrary` | `FederationImport` (federation) or planet stdlib resolver |

### 5. Migration

| Surface | Action |
|---------|--------|
| `.catalog` | **No author change** — already `import <…>` |
| `Authoring.Core` | Ship `AuthoringImportLine` (v0) |
| `CatalogDocumentWalkerFactory` | Use shared parser |
| `Authoring.Project` loader | Next wave: expand logical `import "…"` graph |
| **dash-spec** | Amend DASHSPEC-ADR-0017: `!include` → `import`; parser + LSP + tests (planet PR) |
| `authoring-toolchain` | Syntax / snippets: `import` only |

**DashSpec transition:** accept `!include` as deprecated alias until minor bump; lint warns once.

## Consequences

- Authors learn one directive across federation + planets.
- LSP completion keys off `import` + quote vs `<` context.
- Plugin `import from` remains block-local — no collision with top-level `import`.

## Non-goals

- Renaming `@include` **file roots** in `.dashinclude` (module kind, not include directive).
- Merging plugin DLL `import` with document `import`.
