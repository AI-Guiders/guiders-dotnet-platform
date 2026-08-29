# Guiders Platform

Shared contracts and routing kit for AI Guiders products (CDP habitat, Glass, Forge).

**Start here:** [Federation Constitution](docs/GUIDERS-FEDERATION-CONSTITUTION.md) — sovereign planets, open embed, hyperlanes.  
**Normative charter:** [GUIDERS-ADR-0006](docs/adr/GUIDERS-ADR-0006-confederation-charter.md).

**Not in scope:** product apps, DashSpec, Avalonia UI forks.

## Packages (v0.2)

| Package | Role |
|---------|------|
| `AIGuiders.Platform.Abstractions` | `IntentOutcome`, `RoutedIntent`, pulse truncation |
| `AIGuiders.Platform.Routing` | `IIntentOrgan`, `DispatchCallOverride`, route refusal helpers |
| `AIGuiders.Platform.CommandPlane` | Core: GoF command, catalog descriptors, `ICommandSource` |
| `AIGuiders.Platform.CommandPlane.Slash` | Slash catalog index, resolve, completion, editor bundle |
| `AIGuiders.Platform.CommandPlane.Sources.Json` | JSON catalog reader → Core |
| `AIGuiders.Platform.CommandPlane.Sources.Toml` | TOML catalog reader → Core (+ Tomlyn) |
| `AIGuiders.Platform.CommandPlane.Sources.Xml` | XML catalog reader → Core |
| `AIGuiders.Platform.CommandPlane.Sources` | Meta-bundle: all formats + `FromFile` / embedded resource |
| `AIGuiders.Platform.InputNotation` | Vim + KeyGesture parsers, `NormalizedKeySequence` (quarry from CIDE) |
| `AIGuiders.Platform.CommandPlane.Melody` | Melody descriptors, line profile, policy (ADR-0015) |
| `AIGuiders.Platform.Cockpit.Abstractions` | CCU, DAL, channel, CDS, compositor contracts |
| `AIGuiders.Platform.Cockpit.Ids` | IDS overlay search (ADR 0079) |
| `AIGuiders.Platform.Cockpit.DataBus` | `IDataBus`, `InMemoryDataBus` (ADR 0099) |
| `AIGuiders.Platform.Cockpit.Transport` | `IngressEvent`, `BoundedIngressBus` (ADR 0094) |

Planned: `Correspondence`, `Desk`. Monolith `AIGuiders.Platform.Cockpit` 0.1.0 deprecated.

**Mental model:** aviation phases — [GUIDERS-ADR-0007](docs/adr/GUIDERS-ADR-0007-aviation-mental-model.md); platform mechanics — [GUIDERS-ADR-0010](docs/adr/GUIDERS-ADR-0010-platform-mechanics.md); command surfaces — [GUIDERS-ADR-0009](docs/adr/GUIDERS-ADR-0009-command-surface-pattern.md).

## Build

```bash
dotnet build
dotnet test
dotnet pack -c Release
```

## NuGet Trusted Publishing (nuget.org)

| Поле | Значение |
|------|----------|
| Owner | `AI-Guiders` |
| Repository | `guiders-platform` |
| Workflow | `release.yml` |
| **Package scope (glob)** | **`AIGuiders.Platform.*`** |

Один glob покрывает все семейства: `Abstractions`, `Routing`, `CommandPlane`, `Cockpit.*`, будущие `Correspondence`, `Desk`.

Не используй только `AIGuiders.Platform.Cockpit.*` — CommandPlane и Routing туда не входят.

`release.yml` пушит `artifacts/AIGuiders.Platform.*.nupkg` — новый пакет не нужно дописывать вручную.

## ADR

- [Federation Constitution](docs/GUIDERS-FEDERATION-CONSTITUTION.md) — welcome to all, hyperlanes, how to join
- [GUIDERS-ADR-0015 — Invocation mechanics (Slash · Melody · Binding)](docs/adr/GUIDERS-ADR-0015-invocation-mechanics-slash-melody-binding.md)
- [GUIDERS-ADR-0001 — Platform boundary](docs/adr/GUIDERS-ADR-0001-platform-boundary.md)
- [GUIDERS-ADR-0002 — Avalonia quarry gap](docs/adr/GUIDERS-ADR-0002-avalonia-quarry-gap.md)
- [GUIDERS-ADR-0003 — Platform SSOT quarry](docs/adr/GUIDERS-ADR-0003-platform-ssot-quarry.md)
- [GUIDERS-ADR-0004 — Core monorepo](docs/adr/GUIDERS-ADR-0004-core-monorepo.md)
- [GUIDERS-ADR-0005 — UI Platform monorepo](docs/adr/GUIDERS-ADR-0005-ui-platform-monorepo.md)
- [GUIDERS-ADR-0006 — Confederation charter](docs/adr/GUIDERS-ADR-0006-confederation-charter.md)
- [GUIDERS-ADR-0008 — PluginHost hyperlane](docs/adr/GUIDERS-ADR-0008-plugin-host-hyperlane.md)

## Essays

- [Why aviation, not Pair Programming](docs/essay/why-aviation-not-pair-programming.md) — Pair + CRM + cockpit: one subject, complementary domains (not either/or)

## Consumers

- `cdp-mcp` — habitat; adopt via `PackageReference` from nuget.org
- Glass WPF — projection of cockpit snapshots (CDP-ADR-0021)
- Forge — MCP adapter on shared command ids (later)

## License

Software: [MIT](LICENSE) ([OSI text](https://opensource.org/license/MIT)) · Ethical use: [declaration](https://github.com/AI-Guiders/licensing/blob/main/docs/ethical-use.md)
