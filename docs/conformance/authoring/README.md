# Authoring conformance — `.catalog`

Conformance fixtures for federation command catalogs ([GUIDERS-ADR-0047](../../adr/GUIDERS-ADR-0047-command-for-doi.md)).

## Packages

| Package | Role |
|---------|------|
| `AIGuiders.Platform.Authoring.Core` | Diagnostics, indented/tree parsers |
| `AIGuiders.Platform.Authoring.Command.Catalog` | `.catalog` parser, notation validator, summary |
| `AIGuiders.Platform.Authoring.Conformance` | `CatalogConformance.ValidateDocument` entry |
| `AIGuiders.Platform.CommandPlane.Catalog.CodeGen` | MCP JSON + C# catalog emitter |

## Running tests

```bash
dotnet test tests/AIGuiders.Platform.Authoring.Tests -c Release
```

Fixtures live under `tests/AIGuiders.Platform.Authoring.Tests/Fixtures/Authoring/`.

## Toolchain

The [authoring-toolchain](https://github.com/AI-Guiders/authoring-toolchain) repo wraps the same parser:

```bash
authoring validate path/to/planet.catalog
authoring summary path/to/planet.catalog
authoring emit path/to/planet.catalog --namespace MyApp.Generated --class DashCatalog
```

## Compile errors (v0)

- `notation-wire-mismatch` — binding/melody wire does not match declared keyboard notation
- `missing-notation-declaration` — channel line without `command-notation` / `argument-notation`
- `missing-catalog-header` — file without `catalog <planet>`
