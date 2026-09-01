# GUIDERS-ADR-0054: Phrase-slot completion (catalog codegen + embassy)

| | |
|---|---|
| **Status** | Accepted |
| **Date** | 2026-09-01 |
| **Tags** | #guiders #commandplane #catalog #codegen #embassy #phrase-slots |
| **Related** | GUIDERS-ADR-0046 · GUIDERS-ADR-0047 · GUIDERS-ADR-0053 · GUIDERS-JS-ADR-0001 |

## Context

[GUIDERS-ADR-0053](./GUIDERS-ADR-0053-planet-responsibilities.md) states: **platform owns verb machinery; planet owns noun resolution**. DashSpec still reimplemented active-slot inference (`ResolveActiveSlot`, Group→commandId bridges) even though `dash.catalog` already declares `phrase` + `fills`.

Planet adapters must not parse typed lines to discover **which slot** is active when catalog IR names slot order.

## Decision

### 1. Catalog projection — `CatalogPhraseSlotIndex`

Package: `AIGuiders.Platform.Authoring.Command.Catalog`.

Built from `.catalog`:

| Source | Field |
|--------|-------|
| `phrases table` | literal prefix before first `{slot}` |
| `commands.fills` | ordered slot names |
| `helps table` (`variable X` / `label`) | `SlotLabel` metadata |

API:

- `FromDocument(CatalogDocument)`
- `FromEmitted(PhraseSlotCommands, PhraseSlotLabels)` — codegen path

### 2. Completion enrichment — platform

`CatalogPathCompletion` + `PhraseSlotCompletion.Enrich` populate `ArgCompletionItem`:

| Field | Owner |
|-------|-------|
| `CommandId` | route executor id |
| `ActiveSlot` | catalog `fills` index from typed body |
| `SlotLabel` | catalog helps (when present) |

Wired via `SlashCompletionOptions.PhraseSlots`.

### 3. Codegen — `CatalogCatalogEmitter`

Emits per planet:

- `PhraseSlotLabels`
- `PhraseSlotCommands[]`
- `PhraseSlots => CatalogPhraseSlotIndex.FromEmitted(...)`

Runtime parse (`FromDocument`) remains valid; codegen avoids hot-path catalog parse in hosts that embed emitted catalogs.

### 4. Planet adapter shrink

Planet keeps **only noun resolution**:

- `DashboardSlotNouns` (or equivalent) — map `ActiveSlot` + domain context → display label
- command handlers / entity resolvers

**Delete** planet-side: `ResolveActiveSlot`, Group→commandId switches, phrase-prefix token parsers.

### 5. Embassy = native platform for ecosystem

`guiders-js` packages (`@aiguiders/ir-invocation`, `@aiguiders/command-plane-slash`) mirror the same `ArgCompletionItem` fields and `buildPhraseSlotIndex` / `enrichPhraseSlotItem` ports.

Embassy is **not** a second command plane. It is the **native-platform runtime** for JS/browser surfaces — same mechanics as .NET NuGet, different transport.

Planet ecosystem code (Forge suggest HTTP, DashSpec expanders, plugins) stays in product repos; embassy consumes emitted catalog metadata + platform completion ports.

## Consequences

- DashSpec deletes `DashboardCatalogCompletion`; display reads `item.ActiveSlot` from platform.
- New planets: declare `fills` in catalog, emit `PhraseSlots`, implement slot noun map only.
- Conformance: optional `commandId` / `activeSlot` on slash completion vectors (future).
- Architecture Hub §5 / glossary link ADR-0053 + this ADR for planet vs embassy vs platform.

## Non-goals

- Auto-generating planet noun resolvers (still hand-written per domain).
- Replacing planet expanders (`card-views`, `host-surfaces` recipes stay planet C# until expand codegen wave).
