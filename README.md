# Guiders Platform

Shared contracts and routing kit for AI Guiders products (CDP habitat, Glass, Forge).

**Start here:** [Federation Constitution](docs/GUIDERS-FEDERATION-CONSTITUTION.md) — sovereign planets, open embed, hyperlanes.  
**Living pains:** [GUIDERS pain inventory](docs/GUIDERS-pain-inventory.md) — friction → hyperlane (notation, conformance, MCPlane).  
**Adoption alliance (generated):** [ADOPTION-ALLIANCE.generated.md](docs/ADOPTION-ALLIANCE.generated.md) — `dotnet run --project tools/AdoptionReport -- --write docs/ADOPTION-ALLIANCE.generated.md`  
**Normative charter:** [GUIDERS-ADR-0006](docs/adr/GUIDERS-ADR-0006-confederation-charter.md).

**Not in scope:** product apps, DashSpec, Avalonia UI forks.

## Packages (v0.2)

| Package | Role |
|---------|------|
| `AIGuiders.Platform.Abstractions` | `IntentOutcome`, `RoutedIntent`, pulse truncation |
| `AIGuiders.Platform.Routing` | `IIntentOrgan`, `DispatchCallOverride`, route refusal helpers |
| `AIGuiders.Platform.CommandPlane` | Core: GoF command, catalog descriptors, `ICommandSource` |
| `AIGuiders.Platform.CommandPlane.Slash` | Slash catalog index, resolve, completion, editor bundle |
| `AIGuiders.Platform.CommandPlane.Sources.Json` | JSON format → Core |
| `AIGuiders.Platform.CommandPlane.Sources.Toml` | TOML format → Core (+ Tomlyn) |
| `AIGuiders.Platform.CommandPlane.Sources.Xml` | XML format → Core |
| `AIGuiders.Platform.CommandPlane.Sources.File` | File transport: `FromFile`, embedded resource, extension dispatch |
| `AIGuiders.Platform.CommandPlane.Sources.Database` | DB transport: delegate loader → Core |
| `AIGuiders.Platform.CommandPlane.Sources` | Meta-bundle: all transports + formats |
| `AIGuiders.Platform.InputNotation` | **Legacy alias** → `Notations.Keyboard` (obsolete type forwards) |
| `AIGuiders.Platform.InputNotation.Quarry` | Legacy metapackage → `Notations.Keyboard.Quarry` |
| `AIGuiders.Platform.InputNotation.Vim` | Legacy metapackage → `Notations.Keyboard.Vim` |
| `AIGuiders.Platform.InputNotation.Neovim` | Legacy metapackage → `Notations.Keyboard.Neovim` |
| `AIGuiders.Platform.InputNotation.Emacs` | Legacy metapackage → `Notations.Keyboard.Emacs` |
| `AIGuiders.Platform.InputNotation.KeyGesture` | Legacy metapackage → `Notations.Keyboard.KeyGesture` |
| `AIGuiders.Platform.InputNotation.All` | Legacy metapackage → `Notations.Keyboard.All` |
| `AIGuiders.Platform.Notations` | Core IR: `NormalizedArgTail`, notation surfaces |
| `AIGuiders.Platform.Notations.Keyboard` | Core IR: `NormalizedKeySequence`, `IKeyboardNotationReader` |
| `AIGuiders.Platform.Notations.Keyboard.Quarry` | Shared quarry lexer, normalizer, spec conformance |
| `AIGuiders.Platform.Notations.Keyboard.Vim` | Vim-doc wire (`&lt;C-k&gt;`, CIDE quarry) → Core |
| `AIGuiders.Platform.Notations.Keyboard.Neovim` | Neovim `:help key-notation` wire → Core |
| `AIGuiders.Platform.Notations.Keyboard.Emacs` | Emacs `kbd` wire (`C-x`) → Core |
| `AIGuiders.Platform.Notations.Keyboard.KeyGesture` | KeyGesture wire (`Ctrl+K`) → Core |
| `AIGuiders.Platform.Notations.Keyboard.All` | Meta-bundle: facade `KeyboardNotationParser` |
| `AIGuiders.Platform.Notations.Argument.Kv` | `key=value` tail → slots |
| `AIGuiders.Platform.Notations.Argument.Delimited` | Colon-delimited tail (`wire_class=colon`) → slots |
| `AIGuiders.Platform.Notations.Command.Slash` | Slash body tokenize → path + tail |
| `AIGuiders.Platform.Notations.Command.Console` | Console invocation parity harness |
| `AIGuiders.Platform.CommandPlane.Melody` | Melody descriptors, line profile, policy (ADR-0015) |
| `AIGuiders.Platform.CommandPlane.Binding` | Binding catalog core: merge, index, gesture normalize |
| `AIGuiders.Platform.CommandPlane.Binding.Sources.Toml` | `hotkeys.toml` flat map (CIDE quarry) |
| `AIGuiders.Platform.CommandPlane.Binding.Sources.Json` | JSON bindings object → Core |
| `AIGuiders.Platform.CommandPlane.Binding.Sources.File` | File transport + extension dispatch |
| `AIGuiders.Platform.CommandPlane.Binding.Sources.Database` | DB delegate → Core |
| `AIGuiders.Platform.CommandPlane.Binding.Sources` | Meta-bundle: `BindingSources.*` |
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
