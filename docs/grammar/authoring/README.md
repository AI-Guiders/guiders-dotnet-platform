# Authoring document grammars

Federation **declare-time** DSL structure (blocks, tables, kv) — distinct from [string wire grammars](../notation/README.md).

## Layers

| Layer | SSOT | Runtime package |
|-------|------|-----------------|
| Document structure | `catalog.ebnf` | `Authoring.Core` (`BlockReader`, `TableSurface`, `KvSurface`) |
| Branch semantics | ADR-0047 | `Authoring.Command.Catalog` |
| Wire strings | `notation/*.ebnf` | `Notations.*` |

## Authoring.Core kit (ADR-0048 §3)

| Type | Role |
|------|------|
| `AuthoringSource` | comment strip, line numbering |
| `BlockReader` | `keyword … end keyword`, `* table` openers |
| `TableSurface` | pipe tables → row maps |
| `KvSurface` | `key = value` blocks |
| `IndentedTreeParser` | nested indent surfaces (`channels`) |
| `AuthoringDocumentWalker<T>` | preamble + blocks + handler dispatch |
| `SectionHandlerRegistry<T>` | keyword → `IAuthoringSectionHandler<T>` |
| `IAuthoringSectionHandler<T>` | branch section semantics |

Branch parsers (`CatalogParser`) compose the kit — they do **not** reimplement block/table lexing.

Pattern catalog: [GUIDERS-ADR-0049](../../adr/GUIDERS-ADR-0049-federation-pattern-library.md).

## `.catalog`

See [catalog.ebnf](catalog.ebnf) and [GUIDERS-ADR-0047](../../_wip-adr-0047/GUIDERS-ADR-0047-command-for-doi.md).
