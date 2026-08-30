# GUIDERS-ADR-0024: Melody capture stack · Visual Chord Tree (deferred)

| | |
|---|---|
| **Status** | **Deferred** (draft — stubs shipped; product + catalog trie later) |
| **Date** | 2026-08-30 |
| **Tags** | #guiders #melody #binding #chord #discoverability #native-port |
| **Relates to** | [GUIDERS-ADR-0015](GUIDERS-ADR-0015-invocation-mechanics-slash-melody-binding.md) · [GUIDERS-ADR-0017](GUIDERS-ADR-0017-binding-catalog-family.md) · CIDE [ADR 0060](https://github.com/AI-Guiders/cascade-ide/blob/develop/docs/adr/0060-keyboard-chord-stack-fms-tactical-strategic.md) |

---

## Context

Melody today (ADR-0015):

- **Binding** `cascade_chord` enters capture (`ChordRoot`).
- **Melody** resolves one line → one `commandId` (`PureByNote` ship; `PureByChord` / `Mixed` platform-ready).
- **`c:` palette** is discoverability **outside** the performance lane — not in-chord UI.

Operator discussion (2026-08-30): **sub-roots** (trie prefixes like `<C-k><M-t>ra`) are the **same capture state machine recursively** — deeper catalog node, not a fourth invocation mechanic. A **Visual Chord Tree** during chord mode would answer *where am I* and *what next*, complementing muscle memory and `c:`.

**Cost concern:** show the full trie only for exploration; daily use needs **neighborhood** / **minimal** projections to cap RAM and layout work.

---

## Decision (deferred — direction only)

1. **Sub-root = recursive capture**, not `BindingTargetKind.SubRoot`. One SM; context = `{ nodeId, consumedPrefix, profile }`; `push` / `pop` on sub-root entry / cancel.
2. **Visual Chord Tree = native port** (CIDE/Glass). Platform ships **headless projection** only — no WPF/Skia in Core.
3. **View modes** (planet policy; platform enum):
   - **Minimal** — breadcrumb + top *n* next hops (default low-cost).
   - **Neighborhood** — current node children (+ optional near siblings); default daily.
   - **Full** — exploration slice; planet caps depth/count.
4. **Stubs now** in `CommandPlane.Melody` (zero product dependency):
   - `MelodyCaptureStack` / `MelodyCaptureFrame`
   - `IMelodyGraphCatalog` + `MelodyGraphEdge`
   - `MelodyChordTreeProjector` → `MelodyChordTreeProjection`
5. **Deferred:** catalog trie SSOT (`MelodyGroup` / prefix metadata), capture runtime on planets, graph layout, ADR acceptance.

---

## Platform stubs (v0.18.1+)

```csharp
// Capture (recursive SM context)
MelodyCaptureStack, MelodyCaptureFrame, MelodyCaptureTransitionKind

// Visual projection (native port consumable)
MelodyChordTreeViewMode { Minimal, Neighborhood, Full }
MelodyGraphEdge, MelodyGraphNodeKind { SubRoot, Command }
MelodyChordTreeProjection
IMelodyGraphCatalog
MelodyChordTreeProjector.Project(stack, catalog, viewMode)
```

Planets implement `IMelodyGraphCatalog` from intent-catalog / hotkeys trie when ready.

---

## Discoverability stack (target)

| Layer | When | Surface |
|-------|------|---------|
| Muscle memory | chord mode | none |
| Visual Chord Tree | chord mode | in-session graph/list (this ADR) |
| `c:` palette | Ctrl+Q peel | slug / Help catalog (ADR-0015) |

Same melody catalog; three projections.

---

## Example (illustrative)

```text
Binding:  cascade_chord = Ctrl+K
Trie:     root → [t: Test] → [ra: run-all, rb: run-branch]
Wire:     Ctrl+K  M-t  r  a
Stack:    root → test (consumed "ra" resolves cmd.test-run-all)
UI:       Neighborhood shows { ra, rb } after entering Test; ghost "r" while typing
```

Expressible today as **Mixed** melody steps without trie; trie + sub-root unlock **shared prefixes** and graph navigation.

---

## Non-goals (until accepted)

- Second capture engine or `SubRoot` binding kind
- Platform graph renderer
- Mandatory trie in v1 CIDE ship (`PureByNote` slug remains default)

---

## Acceptance checklist (when un-deferring)

- [ ] Catalog SSOT: trie / `MelodyGroup` prefix nodes
- [ ] CIDE chord stack uses `MelodyCaptureStack` contract
- [ ] Neighborhood projection wired in Glass/CIDE chord HUD
- [ ] Conformance vectors for prefix resolve (optional)
- [ ] Status → Accepted; cross-link ADR-0015 § discoverability

---

## References

- [GUIDERS-ADR-0015 §7 Melody articulation](GUIDERS-ADR-0015-invocation-mechanics-slash-melody-binding.md)
- [GUIDERS-ADR-0017 `cascade_chord`](GUIDERS-ADR-0017-binding-catalog-family.md)
