# GUIDERS-ADR-0022: Platform Utilities — adoption alliance report (draft)

| | |
|---|---|
| **Status** | Draft (v1 shipped in-repo) |
| **Date** | 2026-08-30 |
| **Tags** | #guiders #utilities #adoption #alliance #automation |
| **Relates to** | Constitution § Adoption alliances · G-008 · GUIDERS-ADR-0019 |

## Context

[Constitution § Adoption alliances](GUIDERS-FEDERATION-CONSTITUTION.md#adoption-alliances-real-not-decorative) defines **real pacts** as visible tables: planet · hyperlane · port · spec tags · issues. Manual tables rot ([G-008](GUIDERS-pain-inventory.md#g-008)).

Operators asked for **`utilities.*`** — small tools reused across federation repos (like MCP doc generators), not one-off scripts per planet.

## Decision (v1)

### 1. Package family: `AIGuiders.Platform.Utilities.*`

| Package / tool | Role |
|----------------|------|
| **`Utilities.Adoption`** | Scan planet trees; map `AIGuiders.Platform.*` pins → hyperlanes; collect `*.spec.json` |
| **`tools/AdoptionReport`** | CLI (not packed v1): emit markdown alliance table |

Future siblings (not v1): `Utilities.ConformanceDiff`, `Utilities.NugetPinAudit`, …

### 2. Config (SSOT for scan scope)

```text
docs/adoption/planets.json      — planet id, display name, relative root, issues URL
docs/adoption/hyperlane-map.json — package prefix → hyperlane name
```

Planet `root` is relative to `docs/adoption/`. Add a planet = one JSON row; no code change.

### 3. Generated artifact

```text
docs/ADOPTION-ALLIANCE.generated.md
```

Regenerate:

```bash
dotnet run --project tools/AdoptionReport -- --write docs/ADOPTION-ALLIANCE.generated.md
```

CI (later): fail if generated file drifts on PR touching `planets.json` or sibling planet pins.

### 4. What v1 scans

| Signal | Source |
|--------|--------|
| NuGet pin | `PackageReference Include="AIGuiders.Platform.*"` |
| Project ref | `ProjectReference` → `AIGuiders.Platform.*.csproj` |
| Conformance embed | `*.spec.json` under planet tree |
| JS conformance (hint) | `@aiguiders/*` in `package.json` |

**Not v1:** Forge slash without NuGet pin; MCP capabilities JSON; manual `port: js-native` overrides (add `planets.json` extensions later).

## Consequences

- Alliance table stays honest with one command.
- `utilities.*` pattern established for federation ops tooling without bloating CommandPlane.
- Embassies visible in generated doc — not logo slides.

## Open questions

1. Pack `adoption-report` as global `dotnet tool`?
2. `planets.json` in `aiguiders-conformance` monorepo when it splits?
3. Pre-commit hook vs CI `git diff --exit-code` on generated md?

## References

- `src/AIGuiders.Platform.Utilities.Adoption/`
- `tools/AdoptionReport/`
- [GUIDERS pain inventory G-008](GUIDERS-pain-inventory.md#g-008)
