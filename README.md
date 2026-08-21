# Guiders Platform

Shared contracts and routing kit for AI Guiders products (CDP habitat, Glass, Forge).

**Not in scope:** product apps, DashSpec, Avalonia UI forks.

## Packages (v0.1)

| Package | Role |
|---------|------|
| `Guiders.Platform.Abstractions` | `IntentOutcome`, `RoutedIntent`, pulse truncation |
| `Guiders.Platform.Routing` | `IIntentOrgan`, `DispatchCallOverride`, route refusal helpers |

Planned: `CommandPlane`, `Correspondence`, `Desk`, `Testing`.

## Build

```bash
dotnet build
dotnet test
dotnet pack -c Release
```

Publish: **nuget.org** (`Guiders.Platform.*`), not GitHub Packages.

## ADR

- [GUIDERS-ADR-0001 — Platform boundary](docs/adr/GUIDERS-ADR-0001-platform-boundary.md)

## Consumers

- `cdp-mcp` — habitat; adopt via `PackageReference` from nuget.org (wave 2)
- `cascade-ide` / Glass — desk + command catalog (later)
- Forge — MCP adapter on shared command ids (later)
