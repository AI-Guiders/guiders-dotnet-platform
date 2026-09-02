# GUIDERS-ADR-0015: Invocation mechanics (Slash · Melody · Binding)

| | |
|---|---|
| **Status** | Accepted |
| **Date** | 2026-08-29 |
| **Tags** | #guiders #commandplane #slash #melody #binding #federation |
| **Related** | GUIDERS-ADR-0006 · GUIDERS-ADR-0009 · GUIDERS-ADR-0010 · GUIDERS-ADR-0013 · GUIDERS-ADR-0014 · [Federation Constitution](../GUIDERS-FEDERATION-CONSTITUTION.md) |

## Context

[GUIDERS-ADR-0009](GUIDERS-ADR-0009-command-surface-pattern.md) defines **Command–Surface**: one `commandId`, many invocation surfaces (slash, palette, hotkey, MCP).

CIDE already runs **three distinct input mechanics** before platform extraction:

| Mechanic | User input | Product SSOT today |
|----------|------------|-------------------|
| **Slash** | `/docs adr open` | `[[command.slash]]` in `intent-catalog.toml` |
| **Melody** | chord root, then sequential slug keys + optional parametric tail — e.g. `<Ctrl+K>` `b` `s` | `melody_*` on `[[command]]`, `[[tail_wire_class]]` |
| **Binding** | direct gesture → `commandId` or surface opener — e.g. `Ctrl+Q` (palette), `Ctrl+K` (chord root) | `Hotkeys/hotkeys.toml` + user overlay |

**Not a fourth mechanic:** palette prefix **`c:`** (Command Melody mode in Ctrl+Q) is **discoverability only** — it surfaces the same melody catalog (slug, Help, tail hints) so the user can learn what to play on the keyboard. It does **not** define melody execution. CIDE [ADR 0060](https://github.com/AI-Guiders/cascade-ide/blob/develop/docs/adr/0060-keyboard-chord-stack-fms-tactical-strategic.md): *GlassChord stays Ctrl+K*; Ctrl+Q `c:` is a catalog peel, not the performance lane. Glossary: **DiscoverabilityPrefix** under **InvocationEngage** ([GUIDERS-ADR-0036](GUIDERS-ADR-0036-invocation-engage-glossary.md)).

### Musical metaphor (product canon)

Keyboard as instrument:

| Musical | Invocation |
|---------|------------|
| **Note** | single key after chord root |
| **Chord** | simultaneous keys, or **chord root** gesture (`<Ctrl+K>`) |
| **Melody** | sequential line — `<Ctrl+K>` → steps → execute (slug + optional tail) |
| **Articulation** | how each melody **step** is played — by note or by chord (see §7) |
| **Score on the wall** | `c:` in palette — discoverability, not the instrument |

Platform shipped **Slash mechanics** (catalog index, resolve, completion, sources, registry visitor). Melody and binding configuration remain on the planet — correctly.

Question: extend `CommandDescriptor` with hotkeys and melody fields, or sibling mechanics?

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

| Mechanic | Discovery / resolve key | Platform package (target) | Execute |
|----------|-------------------------|---------------------------|---------|
| **Slash** | slash **path** | `CommandPlane` ✓ | via `commandId` |
| **Melody** | melody **slug** + tail after chord root engaged | `CommandPlane.Melody` (future) | via `commandId` |
| **Binding** | **gesture** → `commandId` or chord root / surface | `CommandPlane.Binding` (future) | via `commandId` or surface-only id |

**Rejected:** stuffing `SuggestedHotkey`, `MelodySlug`, `WireClass`, chord policy into `CommandDescriptor` as normative SSOT.

**Allowed:** optional **display hints** on slash rows (`SuggestedHotkey` for capabilities JSON) — never override planetary binding SSOT.

### 2. Discoverability surfaces vs mechanics

| | Mechanic? | Role |
|---|-----------|------|
| **Slash** `/…` | yes | path resolve + execute |
| **Melody** `<Ctrl+K> …` | yes | slug/tail resolve after root |
| **Binding** hotkeys | yes | gesture → id or engage melody capture |
| **Palette `c:`** | **no** | discoverability prefix in Ctrl+Q — browse melody catalog, Help, parametric hints; same SSOT rows, different surface |

Do not document `c:` as «the melody input language». The melody input language is **sequential keys under chord root** (CIDE `CascadeChord` / Glass `AwaitMelodyTail`).

### 3. Platform owns mechanics; planets own configuration

| | **Platform (mechanics)** | **Planet (configuration / policy)** |
|---|--------------------------|-------------------------------------|
| **Slash** | `CommandCatalogIndex`, `SlashLineResolver`, `SlashStepCompletion`, `SlashArgCompletion`, `SlashInputGuidance`, `CommandSource`, `CommandCatalogComposer` | Path prefix (`/` in chat), ship TOML/JSON catalog, dynamic picker adapters, overlay merge rules |
| **Melody** | `MelodyCatalogIndex`, slug resolve after root, `wire_class` tail parsers (pluggable), await-tail protocol | `intent-catalog.toml` melody blocks, `[[tail_wire_class]]`, `chord_commit` defaults |
| **Binding** | Gesture parse (`KeyGesture` wire), layered merge (ship + user), bind → `commandId` validation, chord root → melody handoff | `hotkeys.toml` ship file, `%LocalAppData%` overlay, chord root string (`Ctrl+K`), palette `c:` prefix policy, WPF tunnel / menu wiring |

**Rule (Prime protocol):**

> **Configuration is planetary; composition is federated; mechanics are platform.**

Platform does **not** ship default hotkeys, melody slugs, chord roots, or palette discoverability prefixes. It does **not** read product-specific overlay paths. The planet loads config and feeds platform DTOs / `ICommandSource`.

### 4. Catalog · Registry · Command · Surface (unchanged)

[GUIDERS-ADR-0009](GUIDERS-ADR-0009-command-surface-pattern.md) still governs all three mechanics:

- **Catalog** (slash / melody / binding) — what the user can discover; mergeable indices.
- **Registry** — `commandId` → `IPlatformCommand<TContext>`.
- **Command** — one effect, one `Execute`.
- **Surface** — strips planetary prefix, calls resolver, then `TryExecute`.

Binding catalog is **not** a fourth executor — it maps gestures to existing registry ids. Palette `c:` rows are **catalog projection** for discoverability, not a separate executor.

### 5. Projection from one product command block

A single `[[command]]` in CIDE TOML may emit **multiple catalog projections**:

```text
[[command]]  command_id = "…"
    ├── slash forms  → CommandDescriptor[]  → CommandCatalogComposer
    ├── melody form  → MelodyDescriptor[]         → MelodyCatalogComposer (future)
    └── (no binding) — bindings live in hotkeys.toml by command_id

MelodyDescriptor[] also feeds palette c: discoverability (same slug/Help; prefix is surface policy).
```

`ICatalogDescribed` may grow optional projections:

- `ToSlashDescriptor()` ✓ today
- `ToMelodyDescriptor()` — future, optional

`IntentCatalogLoader` stays on CIDE until melody schema is extracted as **mechanics** only.

### 6. Federation / Welcome to all

| Planet | Slash | Melody | Binding | `c:` discoverability |
|--------|-------|--------|---------|----------------------|
| CIDE | full | full (`CascadeChord`) | full | palette |
| Forge | capabilities | optional | web shortcuts (product) | — |
| DashSpec | `/select` filters | — | — | — |
| Third party | JSON/TOML catalog | opt-in package | opt-in or none | product choice |

NuGet adoption is **à la carte** — no melody package required for slash-only embed.

### 7. Melody articulation (future platform shape)

**Melody** is always a **sequence** after chord root. **Articulation** names how each step in that sequence is played — not a fourth invocation mechanic.

```text
Binding:  <Ctrl+K>                    ← chord root (enters melody capture)
Melody:   step₁ → step₂ → … → tail?   ← one line, one commandId
          └─ each step has Articulation
```

| Articulation | Step input | Example line |
|--------------|------------|----------------|
| **ByNote** | single key | `<Ctrl+K>` `b` `s` |
| **ByChord** | simultaneous / modifier+key as one step | `<Ctrl+K>` `<Ctrl+R>` `<Ctrl+R>` |

Both rows are **the same mechanic (Melody)** with different per-step articulation. Palette `c:` still projects slug/Help for the line; it does not encode articulation unless the planet chooses to show it.

### Line profile (capture contract)

**Profile** is line-level policy — not a step articulation, not a fourth mechanic:

| Profile | Capture after root | Steps rule |
|---------|-------------------|------------|
| **PureByNote** | await single keys (default) | all `ByNote`; slug may infer steps |
| **PureByChord** | await full gestures | all `ByChord`; explicit steps required |
| **Mixed** | await per `Steps[i].Articulation` | ≥2 articulations; explicit steps required |

```text
         Profile (constraint / UX mode)
              │
              ▼
         Steps[] (normative for resolve)
```

- **Steps** are normative; **Profile** validates and selects capture state machine.
- CIDE/Glass **v1 ship:** `PureByNote` only (current `melody_slug` letters).
- **PureByChord** and **Mixed:** platform-ready; product catalog opt-in when a real command needs them.

**Ship order:** M0 `PureByNote` + slug infer → M1 explicit steps + validation → M2 `PureByChord` capture → M3 `Mixed`.

**Platform contracts** (`AIGuiders.Platform.Execution.CommandPlane.Melody` + `InputNotation`):

```csharp
enum MelodyArticulation { ByNote, ByChord }
enum MelodyLineProfile { PureByNote, PureByChord, Mixed }

class MelodyStep { Articulation, Wire, WireClass? }
class MelodyLine { Slug, Profile, Steps, TailWireClass?, Help? }
class MelodyDescriptor { CommandId, Slug, Profile, Steps, TailWireClass?, Help? }

MelodyLinePolicy.InferProfile / InferStepsFromSlug / Normalize / Validate
IMelodyCatalogDescribed.ToMelodyDescriptor()
```

**Input notation quarry** (W2e ✓ monolith; **W2f** family per [GUIDERS-ADR-0016](GUIDERS-ADR-0016-input-notation-quarry-family.md)):

```csharp
// Target: quarry Neovim keycodes / Emacs key-parse → platform IR
InputNotation (Core) → NormalizedKeySequence, IInputNotationReader
InputNotation.Vim / .KeyGesture / .Emacs
```

Today: monolithic `InputNotation` (CIDE subset + Eto.Parse interim).

**Melody mechanic** (package `AIGuiders.Platform.Execution.CommandPlane.Melody`):

```csharp
enum MelodyArticulation { ByNote, ByChord }
enum MelodyLineProfile { PureByNote, PureByChord, Mixed }
class MelodyDescriptor / MelodyLinePolicy / MelodyStepNotation
IMelodyCatalogDescribed.ToMelodyDescriptor()
```

À la carte packages:

| Package | Role |
|---------|------|
| `CommandPlane` | Core: registry, GoF command, catalog descriptors, `ICommandSource` |
| `CommandPlane.Catalog.Sources.Json` | JSON format → Core |
| `CommandPlane.Catalog.Sources.Toml` | TOML format → Core |
| `CommandPlane.Catalog.Sources.Xml` | XML format → Core |
| `CommandPlane.Catalog.Sources.File` | File transport → format by extension |
| `CommandPlane.Catalog.Sources.Database` | DB transport → Core |
| `CommandPlane.Catalog.Sources` | Meta-bundle |
| `CommandPlane.Slash` | Slash mechanic (index, resolve, completion) → Core |
| `CommandPlane.Melody` | Melody mechanic → `InputNotation` |
| `InputNotation` | Core IR + reader contract |
| `InputNotation.KeyGesture` | KeyGesture wire → Core |
| `InputNotation.Vim` | Vim-doc wire → Core (quarry Neovim) |
| `InputNotation.Emacs` | (planned) Emacs kbd → Core |
| `InputNotation` (meta) | all surfaces |
| `CommandPlane.Binding` | hotkeys catalog merge, gesture → id (ADR-0017) |
| `CommandPlane.Binding.Sources` | meta `BindingSources.*` |

Slash embed: `CommandPlane.Slash` (+ optional `.Sources` for file backends); Sources does not depend on Slash.

CIDE keeps `ChordNotationRenderer` and `KeyGestureChordMatching` as product adapters until adapter swap.

**Rules:**

- Chord root (`<Ctrl+K>`) stays **Binding** — never a melody step.
- One `commandId` may expose multiple `MelodyDescriptor` aliases (same effect, different profile/steps).
- Parametric tail after slug uses the same await-tail protocol; tail slots default to **ByNote** unless `TailWireClass` says otherwise.
- Slug collision across profiles (e.g. `bs` ByNote vs chord-line alias) — defer to implementation wave / planet merge policy.

**Open (defer):** TOML fields (`melody_profile`, explicit `melody_steps[]`) in `intent-catalog.toml`; chord wire notation SSOT. Recursive capture stack + Visual Command Tree projection → [GUIDERS-ADR-0024](GUIDERS-ADR-0024-visual-command-tree-capture-stack.md) (headless projection shipped; native ports deferred).

## Non-goals (this ADR)

- Implementing `CommandPlane.Melody` or `CommandPlane.Binding` packages (separate waves).
- Migrating `IntentCatalogLoader` or `hotkeys.toml` into platform.
- Prescribing WPF `KeyBinding`, Avalonia, or browser `keydown` wiring.
- Replacing [GUIDERS-ADR-0009](GUIDERS-ADR-0009-command-surface-pattern.md) or [GUIDERS-ADR-0014](GUIDERS-ADR-0014-registry-catalog-visitor.md).

## Consequences

- New slash features → `CommandPlane` / `CommandPlane.Catalog.Sources`.
- New melody/binding **mechanics** → sibling packages; **content** stays in product TOML/settings.
- Integration reviews: ask **which mechanic**, **who owns config**, **shared commandId**; do not conflate `c:` with melody mechanic.
- CIDE migration map: `CatalogRouteEntry` discovery fields → platform descriptors; execution fields (Intercom, MFD) stay CIDE.

## References

- CIDE [ADR 0060](https://github.com/AI-Guiders/cascade-ide/blob/develop/docs/adr/0060-keyboard-chord-stack-fms-tactical-strategic.md) — CascadeChord, melody line, `c:` discoverability
- [GUIDERS-ADR-0024](GUIDERS-ADR-0024-visual-command-tree-capture-stack.md) — Visual Command Tree / capture-stack projection (melody + slash)
- CIDE [ADR 0030](https://github.com/AI-Guiders/cascade-ide/blob/develop/docs/adr/0030-command-ids-hotkeys-and-ui-registry-layers.md) — hotkeys vs command id layers
- CIDE [ADR 0119](https://github.com/AI-Guiders/cascade-ide/blob/develop/docs/adr/0119-intent-catalog-command-first-forms.md) — command-first catalog
- CIDE [intent-melody-language-v1.md](https://github.com/AI-Guiders/cascade-ide/blob/develop/docs/intent-melody-language-v1.md) — `c:` grammar (discoverability); keyboard melody is separate layer
- CIDE `IntentMelody/intent-catalog.toml`, `Hotkeys/hotkeys.toml`
