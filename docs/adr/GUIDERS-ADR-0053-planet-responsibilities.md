# GUIDERS-ADR-0053: Planet responsibilities (domain · business logic · ecosystem)

| | |
|---|---|
| **Status** | Accepted |
| **Date** | 2026-09-01 |
| **Tags** | #guiders #federation #planet #platform #commandplane #dashspec |
| **Related** | GUIDERS-ADR-0001 · GUIDERS-ADR-0006 · GUIDERS-ADR-0009 · GUIDERS-ADR-0010 · GUIDERS-ADR-0015 · GUIDERS-ADR-0046 · GUIDERS-ADR-0047 · GUIDERS-ADR-0048 |

## Context

Federation ADRs already split **platform boundary** ([0001](./GUIDERS-ADR-0001-platform-boundary.md)), **mechanics** ([0010](./GUIDERS-ADR-0010-platform-mechanics.md)), and **Catalog · Registry · Command · Surface** ([0009](./GUIDERS-ADR-0009-command-surface-pattern.md)). Operators still blur the line in practice: a planet adapter reimplements catalog step logic, or parses wire paths as strings when typed catalog IR already names the active slot.

Example (DashSpec, 2026-09): `view` completion displayed full `CommandPath` tokens because display code matched `CommandPath.StartsWith("view ")` instead of reading `phrase` + `fills` from `dash.catalog` and resolving labels from domain context. The platform completion engine already returned correct `StepSegment` values — the debt was planet-side.

This ADR states the **operator mental model** in one place so review, quarry, and adapter code share the same gate.

## Decision

### 1. Planet = three concerns

A **planet** (sovereign product repo — DashSpec, Forge, CIDE, …) owns:

| Concern | What it is | Examples |
|---------|------------|----------|
| **Domain** | Subject vocabulary, entities, spec artifacts | `.dashspec`, cards/views, filters, MR/issue model, handoff paths |
| **Business logic** | What a command *means* and what changes when it runs | entity resolvers, filter apply, view switch, ACL on suggest |
| **Ecosystem** | Planet-local plugins, integrations, operator tooling | `DashSpec.Plugin.*`, dev file watcher, SSCADRepo handoff, planet expanders |
| **Embassy** | JS/native-platform runtime for browser surfaces — **not** a second command plane | `guiders-js` (`@aiguiders/command-plane-slash`, `@aiguiders/ir-invocation`) — mirrors platform mechanics ([0054](./GUIDERS-ADR-0054-phrase-slot-completion.md), [GUIDERS-JS-ADR-0001](https://github.com/AI-Guiders/guiders-js/blob/main/design/GUIDERS-JS-ADR-0001-js-embassy.md)) |

Everything else is **not** planet scope.

### 2. Platform = federation mechanics

The **platform** (`guiders-platform`, `guiders-js` embassy) owns headless, product-agnostic mechanics:

| Family | Platform owns |
|--------|---------------|
| **Command plane** | slash DOI, line resolver, step completion, constructors, pickers, `ArgCompletionItem` |
| **Catalog** | `.catalog` grammar, `IR.Command`, index merge, `CatalogPathCompletion`, visitor projections |
| **Invocation** | binding roles, chord-root / suggest-dismiss *mechanics* (not planet gesture tables) |
| **Authoring / Notations** | declare-time walk, wire IR, conformance vectors |
| **Federation** | NuGet contracts, hyperlane versioning, pattern library ([0049](./GUIDERS-ADR-0049-federation-pattern-library.md)) |

Platform does **not** know planet nouns (`card`, `LogUseFunc app_name`, `forge:issue`). Planets supply those through **catalog content**, **context**, and **handlers**.

### 3. Tagline (normative)

Extends [ADR-0015](./GUIDERS-ADR-0015-invocation-mechanics-slash-melody-binding.md):

> **Configuration is planetary; composition is federated; mechanics are platform.**

Add for command-plane work:

> **Platform owns the verb machinery; planet owns the noun resolution.**

| Layer | Question | Owner |
|-------|----------|-------|
| **Which step now?** | Active slot, next segment, branch drill-down | **Platform** — catalog `fills`, completion engine, `StepSegment` |
| **What to show?** | Human label/help for a slot value | **Planet** — domain context + slot metadata (today: adapter; tomorrow: codegen) |
| **What to do on accept?** | Side effects, persistence, navigation | **Planet** — `IPlatformCommand` handlers, HTTP/MCP execute |

### 4. Planet adapter (thin wire)

Planet code between host UI and platform SHOULD be limited to:

```text
.catalog declare     →  phrase · fills · expand · bindings content
Domain context       →  entities available for slot resolution (cards, surfaces, issues, …)
Label projection     →  slot value + context → primary/secondary display (until catalog codegen carries slot metadata)
Registry host        →  register planet commands; build invocation context
Surface host         →  Blazor CCL, forge-slash.js, palette — invokers only
```

Planet adapters MUST NOT:

| Anti-pattern | Why |
|--------------|-----|
| Fork catalog trie / next-segment discovery | Use `CatalogPathCompletion` ([0046](./GUIDERS-ADR-0046-catalog-path-completion.md)) |
| Reimplement slash resolve or execute semantics | Use `SlashLineResolver` + registry ([0009](./GUIDERS-ADR-0009-command-surface-pattern.md)) |
| Parse `CommandPath` or typed line tokens to infer **active slot** when catalog IR has `phrase` + `fills` | Slot index is catalog SSOT ([0047](./GUIDERS-ADR-0047-command-for-doi.md)); planet only resolves **values** |
| Embed planet business rules in platform packages | Violates [0001](./GUIDERS-ADR-0001-platform-boundary.md) DAG |

### 5. Worked example — `card.view` (DashSpec)

**Catalog** (`dash.catalog`):

```text
command = card.view
phrase  = pick-view          # expands to "view {card} {view}"
fills   = card, view
expand  = card-views
```

| Concern | Owner | Artifact |
|---------|-------|----------|
| Next segment after `view ` / `view revenue ` | Platform | `CatalogPathCompletion` → `StepSegment` |
| Active slot (`card` vs `view`) | Platform + catalog IR | `fills` order + typed body bound token count |
| Card title, view label in suggestion list | Planet | `DashboardFilterContext` + `DashboardCatalogCompletion` |
| Switch dashboard view on accept | Planet | view command handler / entity resolver |

**Fixed debt (2026-09):** display no longer inspects `CommandPath.StartsWith("view ")`; it calls catalog phrase/fills helpers and domain context.

### 6. Catalog content vs catalog mechanics (recap)

From [0010](./GUIDERS-ADR-0010-platform-mechanics.md) §5 — unchanged, cited here as planet gate:

| | Owner |
|---|--------|
| **Which commands exist**, phrases, fills, expand hooks, binding rows | **Planet** (`.catalog` in product repo) |
| **How** index, merge, completion, visitor projection work | **Platform** |
| **Execute** registry + handlers | **Planet** hosts registry; **platform** supplies contracts |

Planet DSLs (`.dashspec`, Forge TOML, …) stay **sovereign** ([0048](./GUIDERS-ADR-0048-authoring-quarry-family.md)); they may adopt `Authoring.*` kits without merging repos.

### 7. Federation follow-ups (non-blocking)

These tighten the adapter further; they do not change the responsibility split:

| Item | Effect |
|------|--------|
| `defaults.completion.path = phrase-slots` in catalog | Completion mode declared in SSOT, not inferred |
| Slot metadata on `ArgCompletionItem` from codegen | Planet display needs less hand-written slot switches |
| Control Center `keyboard` section (WitDB) | Operator overrides for bindings; mechanics stay platform |

## Non-goals

- Merging planet repos into `guiders-platform`
- Platform packages that reference DashSpec/Forge domain types
- Prescribing planet UI framework (Blazor vs WPF vs JS) — only adapter thickness

## Consequences

- **PR review:** classify every change as *verb machinery* (platform quarry) or *noun resolution* (planet adapter). Mixed PRs need explicit split.
- **Quarry:** when planet code duplicates platform completion/resolve, delete planet fork — do not document the fork as a pattern.
- **Conformance:** slash vectors stay platform; planet supplies catalog fixtures + context fakes for label/handler tests.
- **Architecture Hub** §5 links here for the operator-facing planet/platform gate.

## Glossary

| Term | Definition |
|------|------------|
| **Verb machinery** | How commands are discovered, completed, resolved, and dispatched — platform |
| **Noun resolution** | What domain values mean and how they map to labels and effects — planet |
| **Planet adapter** | Product code that declares catalog content, builds context, registers handlers, hosts surfaces |
| **Embassy** | Thin JS transport (`guiders-js`) — not a second command plane |
