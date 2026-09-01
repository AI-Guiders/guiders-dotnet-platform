# GUIDERS-ADR-0049: Federation pattern library

| | |
|---|---|
| **Status** | Accepted |
| **Date** | 2026-09-01 |
| **Tags** | #guiders #patterns #authoring #combinations #commandplane |
| **Related** | GUIDERS-ADR-0030 · GUIDERS-ADR-0014 · GUIDERS-ADR-0048 · GUIDERS-ADR-0047 |

## Context

Walk / visit / fold patterns appear across guiders-platform under different names (`ICatalogVisitor`, `OrderedCombination.Fold`, `BlockReader` loops, ad hoc tree walks). Teams re-invent the same control flow when adding DSL branches, catalog projections, or merge composers.

## Decision

Federation ships a **pattern catalog** (this ADR) — named templates with a **home package**. Not a single mega-`IWalker<T>`.

### Pattern catalog

| Pattern | Question it answers | Home | Types (v0) |
|---------|---------------------|------|------------|
| **Projection visit** | How do I build a *view* from SSOT without duplicating storage? | `CommandPlane` | `ICatalogVisitor`, `RegistryCatalogBuilder` ([ADR-0014](GUIDERS-ADR-0014-registry-catalog-visitor.md)) |
| **Ordered fold** | How do I merge layers with a named collision policy? | `Combinations` | `Combinator<T>`, `OrderedCombination.Fold` ([ADR-0030](GUIDERS-ADR-0030-combinations-family.md)) |
| **Document walk** | How do I parse `keyword … end keyword` declare-time DSL? | `Authoring.Core` | `BlockReader`, `AuthoringDocumentWalker<TContext>`, `SectionHandlerRegistry<T>` |
| **Section dispatch** | How do I route a block body to branch semantics? | Branch (`Authoring.Command.*`) | `IAuthoringSectionHandler<TContext>` implementations |
| **Wire read** | How do I parse invocation wire strings to IR? | `Notations.*` | quarry readers + `notation/*.spec.json` ([ADR-0021](GUIDERS-ADR-0021-notations-quarry-family.md)) |

### Authoring.Core guild primitives (shipped)

```text
AuthoringSource              → lines + comment strip
BlockReader                  → opener + block body + surface kind
AuthoringSectionBlock        → keyword + surface + body
IAuthoringSectionHandler<T>  → branch semantics per section
SectionHandlerRegistry<T>    → keyword → handler
AuthoringDocumentWalker<T>   → preamble + blocks + dispatch
```

Branch parsers (e.g. `.catalog`) supply `TContext`, preamble handler (`catalog` / `import`), section handlers, and post-walk validators.

### Anti-patterns

| Do not | Because |
|--------|---------|
| One `IVisitor<T>` for file parse + registry + merge | Different axes; forces fake unified AST |
| Copy `switch (section)` in every planet DSL | Use `AuthoringDocumentWalker` + handlers |
| Put `.catalog` semantics in `Authoring.Core` | Core = document mechanics only ([ADR-0048](../_wip-adr-0048/GUIDERS-ADR-0048-authoring-quarry-family.md)) |
| Reimplement fold merge inline | Use `Combinations` policies |

### Future (not v0)

- `IVisitor<T>` / `IAccepting<T>` in `Abstractions` when a **second** runtime projection visit besides `ICatalogVisitor` lands (Forge capabilities already aligned).
- DashSpec `.dashspec` adopts `AuthoringDocumentWalker` with its own context + handlers.

## Consequences

- New declare-time DSL = Core walk + branch handlers, not a new parser loop.
- Architecture Hub §7 links here for “which pattern?” decisions.
- `CatalogParser` is a thin façade over `AuthoringDocumentWalker<CatalogParseContext>`.
