# GUIDERS-ADR-0040: Catalog guild extract + federated arg suggestions

| | |
|---|---|
| **Status** | Accepted |
| **Date** | 2026-08-31 |
| **Tags** | #guiders #commandplane #catalog #suggestions #federation |
| **Related** | GUIDERS-ADR-0012 · GUIDERS-ADR-0017 · GUIDERS-ADR-0039 · GUIDERS-ADR-0035 |

## Context

[GUIDERS-ADR-0039](GUIDERS-ADR-0039-command-catalog-family.md) neutralized catalog naming but left IR inside the `CommandPlane` hub assembly. Binding already ships as `CommandPlane.Binding` + `CommandPlane.Binding.Sources.*` — catalog deserves the same guild boundary.

Arg completion used a monolithic `ICommandPickerChoiceSource` per host. Planets could not register federated providers by id prefix; `FreeText` became the default when no adapter was wired. Products need **runtime suggestion sources** (session fields, FS browse, HTTP) separate from **catalog content loaders** (TOML/JSON).

## Decision

### 1. Extract `CommandPlane.Catalog` guild

```text
CommandPlane (hub)           ← IPlatformCommand, ICommandContext, registry contracts
CommandPlane.Catalog       ← CommandDescriptor, CommandCatalogIndex, ICommandSource, merge
CommandPlane.Catalog.Sources.*  ← content transports (unchanged family)
```

Types keep namespace `AIGuiders.Platform.CommandPlane` for stable wire; assembly/package is `AIGuiders.Platform.CommandPlane.Catalog`.

### 2. New `CommandPlane.ArgSuggestions` guild

| Type | Role |
|------|------|
| `IArgSuggestionProvider` | Planet-owned backend (DB, FS, session) |
| `ICommandArgSuggestionBroker` | Platform router by suggestion id |
| `CommandArgSuggestionRegistry` | Fluent registration (`RegisterExact`, `RegisterPrefix`) |
| `ArgSuggestionRequest` | `SuggestionId`, `Partial`, `Route`, `CanonicalPath` |
| `StubArgSuggestionBroker` | Conformance / unit tests |

Wire (canonical + alias):

```text
suggest:<id>     ← canonical federated id
picker:<id>      ← alias (existing catalog rows)
```

`CommandArgTailPolicy.ExtractSuggestionId` parses both. `SlashArgCompletion` calls the broker — not a single host adapter.

### 3. FreeText = last resort

`arg_tail = required` without `suggest:` / `picker:` / static choices / constructor → `ArgInputMode.FreeText`. Products SHOULD register providers instead of relying on hints.

### 4. Planet wiring

```csharp
var broker = new CommandArgSuggestionRegistry()
    .RegisterPrefix("dash.field.", new DashFieldSuggestionProvider(session))
    .RegisterExact("forge.fs.cwd", new ForgeDirectorySuggestionProvider())
    .Build();

SlashCompletion.GetResult(catalog, line, broker);
```

Conformance: `pickerStubs` in slash vectors feed `StubArgSuggestionBroker`.

### 5. Removed

- `ICommandPickerChoiceSource` — replaced by broker + providers (no type-forward).

## Package map

| Package | Role |
|---------|------|
| `CommandPlane.Catalog` | Catalog IR + `CommandCatalogCombination` |
| `CommandPlane.ArgSuggestions` | Federated arg suggestion broker |
| `CommandPlane.Slash` | Consumes catalog + broker; slash projection only |
| `Combinations.Catalog` | Meta-bundle → `CommandPlane.Catalog` |

## Consequences

- Hub `CommandPlane` references `CommandPlane.Catalog`; mechanics packages reference Catalog (+ ArgSuggestions where needed).
- DashSpec registers `DashboardFilterSuggestionProvider` via `RegisterPrefix("dash.field.", ...)`.
- Future browse providers (`suggest:fs.entries`) ship as planet packages, not platform core.
