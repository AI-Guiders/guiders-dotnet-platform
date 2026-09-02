# Authoring conformance — `.catalog`



Conformance fixtures for federation command catalogs ([GUIDERS-ADR-0047](../../adr/GUIDERS-ADR-0047-command-for-doi.md)).



## Packages



| Package | Role |

|---------|------|

| `AIGuiders.Platform.Authoring.Core` | Diagnostics, indented/tree parsers, `KvDesugar` |

| `AIGuiders.Platform.Authoring.Command.Catalog` | `.catalog` parser, grammar registry, wire validate |

| `AIGuiders.Platform.Authoring.Command.Bundles` | Federation `.catalogbundle` stdlib (`grain/date-filter`, …) |

| `AIGuiders.Platform.Authoring.Conformance` | `CatalogConformance.ValidateDocument` entry |

| `AIGuiders.Platform.CommandPlane.Catalog.CodeGen` | MCP JSON + C# catalog emitter |



## Grammar SSOT



| Layer | Location |

|-------|----------|

| Document DSL (blocks/tables) | `docs/grammar/authoring/catalog.ebnf` + `Authoring.Core` kit |

| String wire grammars | `docs/grammar/notation/` + `NotationGrammarRegistry` |



## Spec vectors



| Path | Covers |

|------|--------|

| `catalog/profiles-bundle.spec.json` | `import` + `profiles … bundle` expand, grammar mismatch |
| `cockpit/dark-cockpit.spec.json` | Dark Cockpit principle ([0057](../../adr/GUIDERS-ADR-0057-cockpit-logic-authoring-quarry.md)) |



## Running tests



```bash

dotnet test tests/AIGuiders.Platform.Authoring.Tests -c Release

```



## Compile errors (v0)



- `grammar-wire-mismatch` — cell does not parse under declared `grammar.*` id

- `missing-grammar-declaration` — line channel without `grammar` block; bindings/melodies without `grammar.keyboard.*`

- `unknown-grammar-id` — id not in federation registry

- `unknown-bundle` — `profiles … bundle` without federation library or missing import

- `unknown-profile` — `commands` row references undefined profile name

