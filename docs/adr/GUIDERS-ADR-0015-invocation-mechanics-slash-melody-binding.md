# GUIDERS-ADR-0015: Invocation mechanics (Slash · Melody · Binding)

| | |
|---|---|
| **Status** | Accepted |
| **Date** | 2026-08-29 |
| **Tags** | #guiders #commandplane #slash #melody #binding #federation |
| **Related** | GUIDERS-ADR-0006 · GUIDERS-ADR-0009 · GUIDERS-ADR-0010 · GUIDERS-ADR-0013 · GUIDERS-ADR-0014 · [Federation Constitution](../GUIDERS-FEDERATION-CONSTITUTION.md) |

## Context

[GUIDERS-ADR-0009](GUIDERS-ADR-0009-command-surface-pattern.md) defines **Command–Surface**: one `commandId`, many invocation surfaces (slash, palette, hotkey, MCP).

CIDE already runs **three distinct input languages** before platform extraction:

| Language | User input | Product SSOT today |
|----------|------------|-------------------|
| **Slash** | `/docs adr open` | `[[command.slash]]` in `intent-catalog.toml` |
| **Melody** | `c:slug` + parametric tail | `melody_*` on `[[command]]`, `[[tail_wire_class]]` |
| **Binding** | `Ctrl+Q`, chord root `Ctrl+K` + key | `Hotkeys/hotkeys.toml` + user overlay |

Platform shipped **Slash mechanics** (catalog index, resolve, completion, sources, registry visitor). Melody and binding configuration remain on the planet — correctly.

Question: extend `SlashCommandDescriptor` with hotkeys and melody fields, or sibling mechanics?

## Decision

### 1. Three invocation mechanics — not one fat descriptor

```text
                         commandId (hub)
                              │
        ┌─────────────────────┼─────────────────────┐
        ▼                     ▼                     ▼
   SlashCatalog          MelodyCatalog         BindingCatalog
   (path, argTail)       (slug, wireClass)     (gesture → id)
        │                     │                     │
        └────────── Invocation surfaces (product UI) ─┘
                              │
                              ▼
               PlatformCommandRegistry.TryExecute
```

| Mechanic | Discovery key | Platform package (target) | Execute |
|----------|---------------|---------------------------|---------|
| **Slash** | slash **path** | `CommandPlane` ✓ | via `commandId` |
| **Melody** | melody **slug** + tail | `CommandPlane.Melody` (future) | via `commandId` |
| **Binding** | **gesture** / chord step | `CommandPlane.Binding` (future) | via `commandId` or surface-only id |

**Rejected:** stuffing `SuggestedHotkey`, `MelodySlug`, `WireClass`, chord policy into `SlashCommandDescriptor` as normative SSOT.

**Allowed:** optional **display hints** on slash rows (`SuggestedHotkey` for capabilities JSON) — never override planetary binding SSOT.

### 2. Platform owns mechanics; planets own configuration

| | **Platform (mechanics)** | **Planet (configuration / policy)** |
|---|--------------------------|-------------------------------------|
| **Slash** | `SlashCatalogIndex`, `SlashLineResolver`, `SlashStepCompletion`, `SlashArgCompletion`, `SlashInputGuidance`, `CommandSource`, `SlashCatalogComposer` | Path prefix (`/` in chat), ship TOML/JSON catalog, dynamic picker adapters, overlay merge rules |
| **Melody** | `MelodyCatalogIndex`, slug resolve, `wire_class` tail parsers (pluggable), chord *protocol* (two-step resolve) | Entry prefix (`c:` in palette), `intent-catalog.toml` melody blocks, `[[tail_wire_class]]` tables, `chord_commit` defaults |
| **Binding** | Gesture parse (`KeyGesture` wire), layered merge (ship + user), bind → `commandId` validation | `hotkeys.toml` ship file, `%LocalAppData%` overlay, chord root string (`Ctrl+K`), WPF tunnel / menu wiring |

**Rule (Prime protocol):**

> **Configuration is planetary; composition is federated; mechanics are platform.**

Platform does **not** ship default hotkeys, melody prefixes, or chord roots. It does **not** read product-specific overlay paths. The planet loads config and feeds platform DTOs / `ICommandSource`.

### 3. Catalog · Registry · Command · Surface (unchanged)

[GUIDERS-ADR-0009](GUIDERS-ADR-0009-command-surface-pattern.md) still governs all three mechanics:

- **Catalog** (slash / melody / binding) — what the user can discover; mergeable indices.
- **Registry** — `commandId` → `IPlatformCommand<TContext>`.
- **Command** — one effect, one `Execute`.
- **Surface** — strips planetary prefix, calls resolver, then `TryExecute`.

Binding catalog is **not** a fourth executor — it maps gestures to existing registry ids.

### 4. Projection from one product command block

A single `[[command]]` in CIDE TOML may emit **multiple catalog projections**:

```text
[[command]]  command_id = "…"
    ├── slash forms  → SlashCommandDescriptor[]  → SlashCatalogComposer
    ├── melody form  → MelodyDescriptor[]         → MelodyCatalogComposer (future)
    └── (no binding) — bindings live in hotkeys.toml by command_id
```

`ICatalogDescribed` may grow optional projections:

- `ToSlashDescriptor()` ✓ today
- `ToMelodyDescriptor()` — future, optional

`IntentCatalogLoader` stays on CIDE until melody schema is extracted as **mechanics** only.

### 5. Federation / Welcome to all

| Planet | Slash | Melody | Binding |
|--------|-------|--------|---------|
| CIDE | full | full | full |
| Forge | capabilities | optional | web shortcuts (product) |
| DashSpec | `/select` filters | — | — |
| Third party | JSON/TOML catalog | opt-in package | opt-in or none |

NuGet adoption is **à la carte** — no melody package required for slash-only embed.

## Non-goals (this ADR)

- Implementing `CommandPlane.Melody` or `CommandPlane.Binding` packages (separate waves).
- Migrating `IntentCatalogLoader` or `hotkeys.toml` into platform.
- Prescribing WPF `KeyBinding`, Avalonia, or browser `keydown` wiring.
- Replacing [GUIDERS-ADR-0009](GUIDERS-ADR-0009-command-surface-pattern.md) or [GUIDERS-ADR-0014](GUIDERS-ADR-0014-registry-catalog-visitor.md).

## Consequences

- New slash features → `CommandPlane` / `CommandPlane.Sources`.
- New melody/binding **mechanics** → sibling packages; **content** stays in product TOML/settings.
- Integration reviews: ask **which mechanic**, **who owns config**, **shared commandId**.
- CIDE migration map: `SlashRouteEntry` discovery fields → platform descriptors; execution fields (Intercom, MFD) stay CIDE.

## References

- CIDE [ADR 0030](https://github.com/AI-Guiders/cascade-ide/blob/develop/docs/adr/0030-command-ids-hotkeys-and-ui-registry-layers.md) — hotkeys vs command id layers
- CIDE [ADR 0119](https://github.com/AI-Guiders/cascade-ide/blob/develop/docs/adr/0119-intent-catalog-command-first-forms.md) — command-first catalog
- CIDE `IntentMelody/intent-catalog.toml`, `Hotkeys/hotkeys.toml`
