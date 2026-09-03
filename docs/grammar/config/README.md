# Config grammars (`grammar/*` in `*.config.gdl`)

Federation **wire grammars** for operator configuration surfaces (paths, scope-map lines, hot slices, root aliases), referenced from `.config.gdl` via `grammar.<domain>.<slot>` keys in `defaults`.

Document structure (blocks, tables) lives in Authoring.Config (TBD — [GUIDERS-ADR-0064](../../adr/GUIDERS-ADR-0064-config-gdl-quarry-family.md)).

## Axes (v0 planned)

| Axis | `.config.gdl` keys | Grammar ids (planned) | Notes |
|------|-------------------|------------------------|-------|
| Physical path | `grammar.path.physical` | `path-absolute-platform` | [Paths guild](../../adr/GUIDERS-ADR-0050-paths-guild-logical-physical.md); OS normalize in reader |
| Logical path | `grammar.path.logical` | `path-logical-posix` | Repo-relative `/` keys |
| Scope map line | `grammar.path.scope_map` | `path-arrow-scope-id` | `absolute/path => scope_id` |
| Knowledge root alias | `grammar.knowledge.root` | `root-alias` | TOML `primary` / `knowledge_root_id` |
| Hot md sections | `grammar.hot.section` | `markdown-section-tags` | `<!-- section:id -->` |
| Public cut slice | `grammar.hot.public_cut` | `markdown-marker-public-cut` | Slice boundary |

Registry SSOT (code): `ConfigGrammarRegistry` — **not shipped yet** (ADR-0064 P1).

Conformance vectors: `docs/conformance/config/` — **stub pending**.

## Rules

1. Grammar ids in `defaults` are **kebab-case** — same convention as [catalog grammars](../notation/README.md).
2. **Do not** write `notation/...` in `.config.gdl` — that prefix is for conformance spec paths only.
3. **One grammar id per slot** — no `or` unions; use separate config documents per OS/profile or `when` (future).
4. Unknown id or wire mismatch → `config validate` error (`grammar-wire-mismatch`).

## Stdlib packs

Shipped profiles (e.g. `cdp-newcomer.config.gdl`) live under `Authoring.Config.Bundles` (P3). Operators edit L2 wire (TOML, scope-map); pack authors maintain L1.
