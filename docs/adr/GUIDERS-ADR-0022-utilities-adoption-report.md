# GUIDERS-ADR-0022: Platform Utilities — adoption alliance report (draft)

| | |
|---|---|
| **Status** | Draft (v1 shipped in-repo) |
| **Date** | 2026-08-30 |
| **Tags** | #guiders #utilities #adoption #alliance #automation #sources |
| **Relates to** | GUIDERS-ADR-0013 · Constitution § Adoption alliances · G-008 · GUIDERS-ADR-0019 |

## Context

[Constitution § Adoption alliances](GUIDERS-FEDERATION-CONSTITUTION.md#adoption-alliances-real-not-decorative) defines **real pacts** as visible tables: planet · hyperlane · port · spec tags · issues. Manual tables rot ([G-008](GUIDERS-pain-inventory.md#g-008)).

Package managers and manifest formats are legion (NuGet/csproj, npm, cargo, pnpm, …). **Same pattern as [GUIDERS-ADR-0013](GUIDERS-ADR-0013-command-catalog-sources.md):**

```text
SOURCE (wire / transport)  →  IR  →  REPORT (sink)
```

Not one monolithic scanner per planet.

## Decision (v1)

### 1. Package family: `AIGuiders.Platform.Execution.Utilities.Adoption.*`

| Layer | Package | Responsibility |
|-------|---------|----------------|
| **Contract / IR** | `Utilities.Adoption` | `IAdoptionSource`, `AdoptionPin`, `AdoptionFactSet`, `AdoptionHyperlaneProjector`, `AdoptionAllianceBuilder`, `IAdoptionReportWriter` |
| **Sources (format)** | `Utilities.Adoption.Sources` | `CsProjAdoptionReader`, `PackageJsonAdoptionReader`, `ConformanceSpecAdoptionReader` |
| **Sources (transport)** | `Utilities.Adoption.Sources` | `PlanetTreeAdoptionSource` — walk tree, dispatch by extension |
| **Meta-bundle** | `AdoptionSources.*` | `FromPlanetTree()`, `FromCsProjFile()`, … — like `CommandSources` |
| **Report sink** | `Utilities.Adoption.Reports.Markdown` | `MarkdownAllianceReportWriter` |
| **CLI** | `tools/AdoptionReport` | orchestration only (not packed v1) |

```text
planets.json + hyperlane-map.json
        │
        ▼
PlanetTreeAdoptionSource ──┬── CsProjAdoptionReader     ──► AdoptionFactSet (IR)
                           ├── PackageJsonAdoptionReader
                           └── ConformanceSpecAdoptionReader
        │
        ▼
AdoptionHyperlaneProjector ──► PlanetAdoptionRow[] (IR)
        │
        ▼
MarkdownAllianceReportWriter ──► ADOPTION-ALLIANCE.generated.md
```

### 2. IR (neutral facts — not markdown)

| Type | Role |
|------|------|
| `AdoptionPin` | `PackageId`, `Version?`, `AdoptionPortKind` (NuGetPin / ProjectRef / NpmPackage) |
| `AdoptionSpecTag` | conformance vector id |
| `AdoptionFactSet` | per-planet merged facts from all sources |
| `PlanetAdoptionRow` | projected alliance row (hyperlane grouping) |

Hyperlane names come from `hyperlane-map.json`, not from source parsers.

### 3. Config (scan scope SSOT)

```text
docs/adoption/planets.json       — planet id, name, root, issuesUrl
docs/adoption/hyperlane-map.json — package prefix → hyperlane
```

### 4. Regenerate

```bash
dotnet run --project tools/AdoptionReport -- --write docs/ADOPTION-ALLIANCE.generated.md
```

### 5. v1 sources

| Format | Reader | Notes |
|--------|--------|-------|
| `.csproj` | `CsProjAdoptionReader` | `PackageReference` + `ProjectReference` |
| `package.json` | `PackageJsonAdoptionReader` | `@aiguiders/*` scope |
| `*.spec.json` | `ConformanceSpecAdoptionReader` | embedded vectors |

**Defer (future `Sources.*` packages):** `Cargo.toml`, `pyproject.toml`, `packages.lock.json`, Forge capabilities JSON-only embed without NuGet.

### 6. Future splits (when needed)

Same axes as CommandPlane.Catalog.Sources:

```text
FORMAT              TRANSPORT           SINK
csproj / npm / …    planet tree / file  markdown / json / gh-comment
```

À la carte: `Utilities.Adoption.Sources.CsProj` as separate NuGet only when a second consumer needs it without planet walk.

## Consequences

- New package manager = new **format reader** + dispatch line — not a new report tool.
- Alliance table stays honest with one command.
- `utilities.*` guild established beside hyperlanes (Notation, CommandPlane, MCPlane).

## Open questions

1. Pack `adoption-report` as global `dotnet tool`?
2. CI `git diff --exit-code` on generated md?
3. `planets.json` home when `aiguiders-conformance` splits?

## References

- [GUIDERS-ADR-0013 command catalog sources](GUIDERS-ADR-0013-command-catalog-sources.md)
- `src/AIGuiders.Platform.Execution.Utilities.Adoption*/`
- `tools/AdoptionReport/`
