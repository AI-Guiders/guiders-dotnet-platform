# Federation coding SSOT

Machine-readable coding contract for **AI-Guiders** C# monorepos (NuGet hyperlanes).

| File | Role |
|------|------|
| `.editorconfig` | Indent, C# style, naming (`_field`, `s_field`), test CA1707 off |
| `.gitattributes` | LF for source; CRLF for `.sln`/`.ps1`; binaries `-text` |
| `global.json` | SDK pin `10.0.100` + `rollForward: latestFeature` |

## Rollout

Copy all three files to the **repo root** (not only `eng/`). This folder is the canonical export; roots are what MSBuild, Roslyn, and git read.

Repos that should carry the bundle:

- `guiders-platform`, `guiders-core`, `guiders-ui-platform`, `guiders-plugin-host`
- `agent-nuget-pm`, `AIGuiders.DotnetTools`, `cdp-mcp`
- Product repos may add **child** `.editorconfig` overlays (`root = false`) for analyzers (e.g. Forge `FORGE010`).

Human norms (when to refactor, traps): agent-notes `code-writing-principles-v1.md`.  
Build contract: `Directory.Build.props` per repo.
