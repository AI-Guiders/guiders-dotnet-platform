# GUIDERS-ADR-0046: Catalog path completion

| | |
|---|---|
| **Status** | Accepted |
| **Date** | 2026-08-31 |
| **Tags** | #guiders #commandplane #catalog #slash #discoverability |
| **Related** | GUIDERS-ADR-0011 · GUIDERS-ADR-0012 · GUIDERS-ADR-0024 · GUIDERS-ADR-0045 |

## Context

`SlashStepCompletion` already walks flat `CommandCatalogIndex` paths internally, but the trie logic is private. Products (DashSpec) reimplemented root/branch completion (`select` / `view` / `show`, `select filter`, …) by hand — duplicating catalog rows added in ADR-0045.

ADR-0024 projects **results** (`SlashCompletionResult` → Visual Command Tree) but does not expose **prefix-next-segment** discovery from the index alone.

## Decision

### 1. Public API — `CatalogPathCompletion`

Package: `CommandPlane.Slash`.

```text
CatalogPathCompletion.GetSuggestions(catalog, typedBody)
CatalogPathCompletion.GetSuggestions(catalog, tokens, endsWithSpace, typedBody)
```

Returns `ArgCompletionItem` rows for the **next path segment** by scanning all catalog routes (flat-path catalogs with empty Domain/Object/Intent).

### 2. Branch drill-down

When the typed body equals a **branch prefix** without trailing space (e.g. `select filter`) and child routes exist (`select filter usage_date`, …), treat as `select filter ` and list **child** segments. Avoids forcing trailing space for tree navigation.

### 3. `SlashStepCompletion` delegates

Flat-path mode calls `CatalogPathCompletion` — single SSOT for trie walk. Semantic domain/object/intent mode unchanged.

### 4. Product adapters (DashSpec, Forge)

| Keep in product | Move to federation |
|-----------------|-------------------|
| Label formatting (`DashboardFilterCommandDisplay`) | Next-segment discovery |
| Grammar normalize (`select` peel, `filter` → `select filter`) | Branch drill-down |
| Locale placeholder copy | `CatalogPathCompletion` + `SlashCompletion` |

Products SHOULD NOT fork verb/branch listing when catalog index is authoritative.

## Consequences

- DashSpec deletes ~300 lines of `TryBuild*Choice` tree code.
- Future Forge/CIDE CCL uses same API for flat catalogs.
- Optional explicit branch descriptors (`ArgTail=none` on `select filter`) remain valid but not required when drill-down infers branches.

## Consumers

| Planet | Adoption |
|--------|----------|
| **DashSpec** | `SlashCompletion` only; display adapter stays |
| **CommandPlane.Slash** | `SlashStepCompletion` → `CatalogPathCompletion` |
