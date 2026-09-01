# Authoring conformance — `.catalog`

Conformance fixtures for federation command catalogs ([GUIDERS-ADR-0047](../../_wip-adr-0047/GUIDERS-ADR-0047-command-for-doi.md)).

## Packages

| Package | Role |
|---------|------|
| `AIGuiders.Platform.Authoring.Core` | Diagnostics, indented/tree parsers |
| `AIGuiders.Platform.Authoring.Command.Catalog` | `.catalog` parser, grammar registry, wire validate |
| `AIGuiders.Platform.Authoring.Conformance` | `CatalogConformance.ValidateDocument` entry |
| `AIGuiders.Platform.CommandPlane.Catalog.CodeGen` | MCP JSON + C# catalog emitter |

## Grammar SSOT

| Layer | Location |
|-------|----------|
| Document DSL (blocks/tables) | `docs/grammar/authoring/catalog.ebnf` + `Authoring.Core` kit |
| String wire grammars | `docs/grammar/notation/` + `NotationGrammarRegistry` |

## Authoring.Core

`BlockReader`, `TableSurface`, `KvSurface`, `IndentedTreeParser`, `InnerBlockFilter` — see `docs/grammar/authoring/README.md`.

## Running tests

```bash
dotnet test tests/AIGuiders.Platform.Authoring.Tests -c Release
```

## Compile errors (v0)

- `grammar-wire-mismatch` — cell does not parse under declared `grammar.*` id
- `missing-grammar-declaration` — line channel without `grammar` block; bindings/melodies without `grammar.keyboard.*`
- `unknown-grammar-id` — id not in federation registry
