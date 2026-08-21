# Guiders Platform

Shared contracts and routing kit for AI Guiders products (CDP habitat, Glass, Forge).

**Not in scope:** product apps, DashSpec, Avalonia UI forks.

## Packages (v0.1)

| Package | Role |
|---------|------|
| `AIGuiders.Platform.Abstractions` | `IntentOutcome`, `RoutedIntent`, pulse truncation |
| `AIGuiders.Platform.Routing` | `IIntentOrgan`, `DispatchCallOverride`, route refusal helpers |
| `AIGuiders.Platform.Cockpit` | UI-agnostic layer canon: DAL marker, transport, CCU, channel, CDS, compositor, DataBus, IDS search |

Planned: `CommandPlane`, `Correspondence`, `Desk`, `Testing`.

## Build

```bash
dotnet build
dotnet test
dotnet pack -c Release
```

Publish: **nuget.org** (`AIGuiders.Platform.*`). Trusted Publishing policy glob: `AIGuiders.Platform.*`.

## ADR

- [GUIDERS-ADR-0001 — Platform boundary](docs/adr/GUIDERS-ADR-0001-platform-boundary.md)
- [GUIDERS-ADR-0002 — Avalonia quarry gap](docs/adr/GUIDERS-ADR-0002-avalonia-quarry-gap.md)

## Consumers

- `cdp-mcp` — habitat; adopt via `PackageReference` from nuget.org
- Glass WPF — projection of cockpit snapshots (CDP-ADR-0021)
- Forge — MCP adapter on shared command ids (later)

## License

[Hippocratic License 2.1](LICENSE) (Ethical Source / SPDX `Hippocratic-2.1`).
