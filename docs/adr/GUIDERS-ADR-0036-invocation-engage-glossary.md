# GUIDERS-ADR-0036: Invocation engage glossary (Sigil · DiscoverabilityPrefix · ChordRoot)

| | |
|---|---|
| **Status** | Accepted |
| **Date** | 2026-08-31 |
| **Tags** | #guiders #commandplane #slash #melody #binding #federation #glossary |
| **Related** | GUIDERS-ADR-0009 · GUIDERS-ADR-0015 · GUIDERS-ADR-0017 · GUIDERS-ADR-0021 · [Constitution § Musical metaphor](../GUIDERS-FEDERATION-CONSTITUTION.md) |

## Context

Operators and planets diverge on **how** the user leaves normal input and enters command-ready mode:

| Planet / surface | Engage artifact | Role |
|------------------|-----------------|------|
| CIDE chat / CCL | `/` | text sigil → slash body |
| DashSpec filter bar | `>` | visible prompt; body without slash |
| CIDE palette (Ctrl+Q) | `c:` | browse melody catalog |
| CIDE / Glass | `Ctrl+K` (`cascade_chord`) | binding → melody capture |

[GUIDERS-ADR-0015](GUIDERS-ADR-0015-invocation-mechanics-slash-melody-binding.md) defines **invocation mechanics** (Slash · Melody · Binding) and [GUIDERS-ADR-0009](GUIDERS-ADR-0009-command-surface-pattern.md) defines **invocation surfaces** (toolbar, palette, MCP, …). Neither names the **pre-mechanic engage** layer — the planet-specific cue before platform resolve/execute runs.

[GUIDERS-ADR-0021](GUIDERS-ADR-0021-notations-quarry-family.md) already calls text prefixes **Sigil path** and marks leading `/` as **surface policy**. This ADR unifies the vocabulary tree and states platform boundary: **glossary only — no Core type in this wave.**

## Decision

### 1. Umbrella term: **InvocationEngage** (short: **Engage**)

**InvocationEngage** — how the operator transitions from ordinary input to *command-ready* on a product surface, **before** platform mechanics consume the wire.

```text
InvocationEngage                 ← planet policy (glossary; no platform enum yet)
├── Sigil                        ← text engage in a line
├── DiscoverabilityPrefix        ← catalog peel (not execution)
└── ChordRoot                    ← binding engage by gesture
        │
        ▼  strip / peel / key tunnel (planet native)
InvocationMechanic               ← platform (Slash · Melody · Binding)
        │
        ▼
commandId → Registry → Execute
```

**Rule:** Platform mechanics begin **after** engage is consumed. Platform does **not** ship a canonical sigil, chord root, or palette prefix.

### 2. Child terms (normative glossary)

| Term | Engage kind | Input | After consume | Mechanic | Planet examples |
|------|-------------|-------|---------------|----------|-----------------|
| **Sigil** | text | character(s) in command line | path + arg **body** | Slash (or Console after strip) | `/` CIDE, `>` DashSpec, `:w` Vim-style, `@intent` |
| **DiscoverabilityPrefix** | peel | palette / help prefix | same catalog rows, **no** performance lane | Melody catalog projection only | `c:` in Ctrl+Q ([ADR-0015](GUIDERS-ADR-0015-invocation-mechanics-slash-melody-binding.md)) |
| **ChordRoot** | gesture | hotkey wire | melody capture armed | Binding → Melody handoff | `cascade_chord` = `Ctrl+K` ([ADR-0017](GUIDERS-ADR-0017-binding-catalog-family.md)) |

**Do not collapse** the three kinds into one platform type named Sigil or Leader — notation, discoverability peel, and binding engage stay distinct mechanics downstream.

### 3. Sigil (text engage)

- **Sigil** = planet-chosen prefix or prompt marking *text command mode* in a buffer or dedicated command line.
- Strip (or equivalent) happens in **planet surface adapter** before `Notations.Command.*` readers and `SlashStepCompletion` see the wire.
- `SlashCommandNotation.TryParseLine` requiring `/` is a property of the **slash reader**, not federation SSOT for all planets.
- **Sigil path** in [ADR-0021](GUIDERS-ADR-0021-notations-quarry-family.md) §10 remains the notation-family row; this ADR names the engage role explicitly.

### 4. DiscoverabilityPrefix (not Engage → execute)

Palette `c:` surfaces melody slug, Help, and tail hints — it does **not** define melody execution. Keyboard melody remains sequential keys under **ChordRoot** (CIDE `CascadeChord` / Glass `AwaitMelodyTail`). See [ADR-0015 §2](GUIDERS-ADR-0015-invocation-mechanics-slash-melody-binding.md).

### 5. ChordRoot (gesture engage)

- **ChordRoot** is a **Binding** target (`BindingTargetKind.ChordRoot`, well-known key `cascade_chord`) — not a text sigil.
- Chord root is **never** a melody step ([ADR-0015 §7](GUIDERS-ADR-0015-invocation-mechanics-slash-melody-binding.md)).
- Handoff from chord capture to slash (e.g. CIDE: first key `/` → Cockpit Command Line) is **planet surface policy**, not a fourth engage kind.

### 6. What platform ships (unchanged)

| Layer | Platform owns | Planet owns |
|-------|---------------|-------------|
| Engage | **Nothing** (glossary only) | Sigil, prompt, `c:` policy, chord root string, tunnel/UI |
| Notation | Wire → IR (`Notations.*`) | Which reader + strip rules |
| Mechanic | Slash / Melody / Binding catalog + resolve | Ship TOML, hotkeys, overlay merge |
| Execute | Registry `TryExecute(commandId)` | Context, native ports |

### 7. Future types (deferred)

Introduce a shared planet-side DTO (e.g. `CommandSigilPolicy`) only when a **reused surface kit** (Forge TS, Avalonia command-line host) needs the same strip/prompt contract across products. **Not** in `CommandPlane` Core until then.

Renaming **Engage** later is acceptable while the term remains documentation-only.

## Non-goals

- Merging ChordRoot with Sigil or `DiscoverabilityPrefix` in `BindingTargetKind` / notation Core.
- Prescribing a federation-wide default sigil (`/`, `>`, or other).
- Implementing engage state machines in platform (capture SM stays Melody + planet native ports per [ADR-0024](GUIDERS-ADR-0024-visual-command-tree-capture-stack.md)).

## Consequences

- Docs and reviews use **InvocationEngage** / **Engage** for the umbrella; **Sigil** only for text engage.
- Integration questions: *which engage kind?* → then *which mechanic?* → shared `commandId`.
- Constitution musical-metaphor table may link here for Engage vs `c:` vs Binding chord root.

## References

- CIDE [ADR 0060](https://github.com/AI-Guiders/cascade-ide/blob/develop/docs/adr/0060-keyboard-chord-stack-fms-tactical-strategic.md) — CascadeChord, `c:` discoverability
- CIDE [ADR 0138](https://github.com/AI-Guiders/cascade-ide/blob/develop/docs/adr/0138-cockpit-command-line-and-parametric-ranges.md) — Cockpit Command Line, Ctrl+K `/` handoff
- DashSpec `DashboardFilterCommandDisplay.Prompt` (`>`) — sigil as planet policy
