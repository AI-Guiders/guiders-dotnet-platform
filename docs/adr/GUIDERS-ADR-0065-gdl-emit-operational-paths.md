# GUIDERS-ADR-0065: GDL emit — operational author vs consumer paths

| | |
|---|---|
| **Status** | **Accepted** (operational playbook — tooling **In progress** per [0059](./GUIDERS-ADR-0059-gdl-hyperlane.md) §10.4) |
| **Date** | 2026-09-12 |
| **Tags** | #guiders #federation #gdl #authoring #emit #msbuild #planet #dashspec |
| **Related** | [0059](./GUIDERS-ADR-0059-gdl-hyperlane.md) · [0051](./GUIDERS-ADR-0051-authoring-project-abstraction.md) · [0048](./GUIDERS-ADR-0048-authoring-quarry-family.md) · [0053](./GUIDERS-ADR-0053-planet-responsibilities.md) · [0055](./GUIDERS-ADR-0055-surface-wpf-guild-deck-authoring.md) · [authoring-toolchain](https://github.com/AI-Guiders/authoring-toolchain) |

## Context

[GUIDERS-ADR-0059](./GUIDERS-ADR-0059-gdl-hyperlane.md) §10.2 defines **Author vs consumer distribution** at the architectural level: authors ship `*.gdl` + `*.gdlproj`; consumers may ship `Generated/*.g.cs` without GDL sources.

Today the pipeline is **fragmented**:

| Current state | Location |
|---------------|----------|
| Per-quarry CLI | `authoring emit`, `deck emit` in [authoring-toolchain](https://github.com/AI-Guiders/authoring-toolchain) |
| Unified `gdlc` | Target name; not yet the only entry |
| MSBuild hook | `build/Platform.Gdl.Emit.targets` in authoring-toolchain (**Agent A**, Wave 1) |
| Platform packages | Transitional **C#** `AIGuiders.Platform.Authoring.*` + parallel **F#** `AIGuiders.Platform.Modeling.Gdl.*` |

Operators, planet maintainers, and CI authors need a single **operational** contract: who edits what, what gets committed, what NuGet consumers see, and what DashSpec (and other planets) must not fork.

This ADR is the operational addendum to [0059](./GUIDERS-ADR-0059-gdl-hyperlane.md) §10.2 — not a second distribution model.

## Decision

### 1. Two roles (normative)

| Role | Meaning SSOT | Ships in repo / package | Tooling |
|------|--------------|-------------------------|---------|
| **Author** | `*.gdl`, `*.gdlproj` | GDL sources + generated output (policy below) | `gdlc validate`, `gdlc emit`, `gdlc sat` (today: `authoring emit`, `deck emit`) |
| **Consumer** | Generated C# (and optional tier-D wire) | `Generated/*.g.cs`, wire drops — **no** `.gdl` required | Ordinary `dotnet build`; Roslyn IntelliSense on `*.g.cs` |

Authors **must** retain `.gdl` as declare-time SSOT. Consumers **may** compile without ever opening a GDL file — same pattern as consuming protobuf stubs without the `.proto` repo ([0059](./GUIDERS-ADR-0059-gdl-hyperlane.md) §10.3).

### 2. Author workflow

```text
edit *.catalog.gdl / *.deck.gdl (and siblings)
        │
        ├── local: gdlc validate --project <name>.gdlproj
        │
        └── emit
                │
                ├── gdlc emit --lang=cs --project <name>.gdlproj --out Generated/
                │       (today: authoring emit / deck emit per quarry)
                │
                └── commit policy (pick one per repo; document in README):
                        ├── **Check in** Generated/*.g.cs  → consumers build without gdlc
                        └── **CI regen** only              → CI runs emit; fails if stale vs .gdl
```

**Author repo layout (illustrative):**

```text
studio/
  studio.gdlproj
  catalog/commands.catalog.gdl
  deck/report-author.deck.gdl
  Generated/
    Commands.g.cs          # regen-owned
    ReportAuthorDeck.g.cs  # regen-owned
  Commands.User.cs         # hand partial — planet extensions only
```

**Rules:**

| Rule | Norm |
|------|------|
| Declare entry | `*.gdlproj`, not `*.csproj` ([0059](./GUIDERS-ADR-0059-gdl-hyperlane.md) §10, [0051](./GUIDERS-ADR-0051-authoring-project-abstraction.md)) |
| Generated suffix | `*.g.cs` only — Roslyn-visible, clearly regen-owned |
| User code | Separate partials (`*.User.cs`, expander stubs) — never edit `*.g.cs` for meaning |
| Validate before merge | `gdlc validate` (or per-quarry validate) in CI |
| Stale gate | If `Generated/` is checked in, CI **must** re-emit and diff-fail when output ≠ committed |

**MSBuild integration (Wave 1 target):** dotnet projects **consume** emit output; they do not replace `gdlproj` as the declare entry.

Import in `*.csproj` (when authoring-toolchain ships the target):

```xml
<Import Project="path/to/authoring-toolchain/build/Platform.Gdl.Emit.targets" />
```

Canonical path (placeholder until repo layout stabilizes):  
[authoring-toolchain/build/Platform.Gdl.Emit.targets](https://github.com/AI-Guiders/authoring-toolchain/blob/main/build/Platform.Gdl.Emit.targets)

The target runs `gdlc emit --lang=cs` (or equivalent) in `BeforeCompile`, writing under `Generated/` or `obj/gdl/` per project property. Authors choose check-in vs CI-only via `GdlEmitCheckIn` / stale-check properties defined in that targets file.

### 3. Consumer workflow

```text
NuGet / app references package or project
        │
        └── compiles Generated/*.g.cs (+ optional tier-D wire at runtime)
                │
                └── no .gdl, no gdlc, no gdlproj required at build time
```

| Consumer type | Typical inputs | Does not need |
|---------------|----------------|---------------|
| Planet app (DashSpec, Forge, …) | `AIGuiders.Platform.*` + planet `Generated/*.g.cs` | `.gdl` sources in the consuming solution |
| Downstream NuGet user | Published package with pre-emitted `*.g.cs` | authoring-toolchain CLI |
| Federation library author | Both GDL + Generated in source repo | — (author role) |

Consumers reference **generated constants, maps, and topology tables** — not runtime GDL parsers for catalog/deck meaning ([0048](./GUIDERS-ADR-0048-authoring-quarry-family.md) §2).

### 4. Planet obligations (DashSpec and sovereign repos)

Planets are **authors** for their own declare artifacts ([0053](./GUIDERS-ADR-0053-planet-responsibilities.md)). For GDL quarries adopted from federation:

| Obligation | Norm |
|------------|------|
| **SSOT** | Planet command/deck **meaning** lives in `*.catalog.gdl` / `*.deck.gdl` (or planet sovereign `.dashspec` where not yet migrated to GDL) **in the planet repo** |
| **Hand partials** | Planet ships **only** expander partials, executors, views, and domain glue — e.g. `DashCatalog.User.cs`, `ICommandCatalogExpander` implementations ([0045](./GUIDERS-ADR-0045-command-authoring-dx.md)) |
| **Generated** | `DashCatalog.g.cs`, `DashSpecStudioDeck.g.cs`, zone registries — **emit output**, not hand-maintained ([0055](./GUIDERS-ADR-0055-surface-wpf-guild-deck-authoring.md)) |
| **Platform boundary** | Planet does not fork catalog trie, slash resolve, or GDL parse for shipped meaning — uses platform mechanics + generated tables |

**DashSpec migration note:** legacy code-first catalogs (`DashboardCommandCatalogBuilder`, bundled TOML) are **transitional**. Target: `.dashspec` / `.catalog.gdl` SSOT → emit → `Generated/*.g.cs`; planet keeps resolvers and expanders only.

### 5. Anti-patterns (rejected)

| Anti-pattern | Why | Correct path |
|--------------|-----|--------------|
| **Dual maintenance** — runtime parse of `.gdl` / `.catalog` **and** parallel generated constants for the same ids | Two SSOTs drift; CI cannot gate | Pick one: **emit** for compile-time tables; runtime reads **tier-D wire** or generated constants only |
| **Hand-editing `*.g.cs`** | Next emit overwrites; review noise; hides meaning change | Edit `*.gdl`; re-emit; use `*.User.cs` partials for extensions |
| **Consumer repo carries `.gdl` “just in case”** | Blurs author/consumer contract; invites parse-at-runtime | Consumer references emitted package; authors publish regen |
| **Skipping validate / stale check** | Broken imports or quarry typos ship silent | `gdlc validate` + emit diff in CI |
| **Meaning in MSBuild properties** | Undeclared, unversioned, no conformance vectors | `*.gdl` quarry or documented planet sovereign DSL |

### 6. Transitional platform layout (informative)

Until [0025](./GUIDERS-ADR-0025-language-intelligence-boundary.md) migration completes:

```text
AIGuiders.Platform.Modeling.Gdl.*     F# — parse IR, project graph (SSOT direction)
AIGuiders.Platform.Authoring.*        C# — quarry handlers, emit adapters (transitional)
authoring-toolchain                   CLI + MSBuild targets (gdlc home)
```

Operational rule unchanged: **declare in GDL → emit → consume `*.g.cs`**. Package language split does not grant a second meaning path.

### 7. CI checklist (authors)

| Step | Command / gate |
|------|----------------|
| Parse + quarry diagnostics | `gdlc validate --project *.gdlproj` |
| Emit | `gdlc emit --lang=cs --project *.gdlproj --out Generated/` |
| Stale generated (if checked in) | Re-emit in CI; `git diff --exit-code Generated/` |
| Conformance (when vectors exist) | `docs/conformance/authoring/<quarry>/*.spec.json` |
| Config contracts (optional) | `gdlc sat` for `*.config.gdl` ([0064](./GUIDERS-ADR-0064-config-gdl-quarry-family.md)) |

## Consequences

- Author and consumer repos have an explicit, reviewable contract — no implicit “we parse at runtime sometimes.”
- DashSpec and other planets know: **GDL in repo + hand partials for expanders only**; generated files are toolchain output.
- MSBuild target in authoring-toolchain becomes the default dotnet integration path; `gdlproj` stays the declare entry.
- [0059](./GUIDERS-ADR-0059-gdl-hyperlane.md) §10.2 remains the architectural SSOT; this ADR is the operator playbook.

## Non-goals

- Replacing per-quarry emit CLIs before unified `gdlc` ships (documented as migration, not blocked).
- Mandating check-in vs CI-only regen globally — each repo documents its choice.
- Planet sovereign `.dashspec` body replacement — federation quarries are the GDL path; DashSpec migration is incremental.
