# GUIDERS-ADR-0024: Visual Command Tree · capture-stack projection

| | |
|---|---|
| **Status** | **Accepted** (headless projection shipped; native graph ports deferred) |
| **Date** | 2026-08-30 (generalized 2026-08-31) |
| **Tags** | #guiders #discoverability #slash #melody #ccl #capture-stack |
| **Relates to** | [GUIDERS-ADR-0012](GUIDERS-ADR-0012-arg-picker-completion.md) · [GUIDERS-ADR-0015](GUIDERS-ADR-0015-invocation-mechanics-slash-melody-binding.md) · [GUIDERS-ADR-0035](GUIDERS-ADR-0035-slash-value-constructors.md) · DASHSPEC-ADR-0043 |

> **Name history:** *Visual Chord Tree* = melody/chord engage of the same pattern. Platform types use **Visual Command Tree**; melody APIs keep `MelodyChordTree*` aliases.

---

## Context

Complex systems need discoverability without abandoning keyboard-first flow. Operators must always see:

1. **Where am I** — breadcrumb / capture stack  
2. **What is next** — bounded next hops, not an infinite trie  
3. **How to complete** — placeholder + hint aligned with input mode  

Melody chord mode ([ADR-0015](GUIDERS-ADR-0015-invocation-mechanics-slash-melody-binding.md)) and slash/CCL ([ADR-0012](GUIDERS-ADR-0012-arg-picker-completion.md)) are **different engages** on the same catalog graph. They MUST share one headless projection contract so CIDE, DashSpec, and Forge render the same semantics.

**Cost concern:** full trie exploration is for onboarding; daily use projects **Minimal** or **Neighborhood** slices only.

---

## Decision

### 1. One platform projection — many engages

| Engage | Capture state | Catalog source | Platform projector |
|--------|---------------|----------------|-------------------|
| **Melody chord** | `MelodyCaptureStack` | `IMelodyGraphCatalog` (trie) | `MelodyChordTreeProjector` → shared core |
| **Slash / CCL** | typed line + mode | `SlashCompletionResult` | `SlashVisualCommandTreeProjector` |
| **Constructor** | `SlashConstructorSession` | segment provider | slash projector (`EngageKind = Constructor`) |

Native surfaces (Glass, Blazor, WPF) consume **`VisualCommandTreeProjection`** only — no product-specific peel logic.

### 2. Shared types (`AIGuiders.Platform.CommandPlane`)

```csharp
VisualCommandTreeEngageKind { MelodyChord, SlashLine, Constructor }
VisualCommandTreeViewMode { Minimal, Neighborhood, Full }
VisualCommandTreeFrame, VisualCommandTreeEdge, VisualCommandTreeProjection
IVisualCommandTreeCatalog
VisualCommandTreeProjector.ProjectCapture(...)
SlashVisualCommandTreeProjector.Project(SlashCompletionResult, ...)
```

Melody namespace keeps **`MelodyChordTree*`** as thin adapters (no breaking rename in v0.18.x).

### 3. View modes (planet policy)

| Mode | Shows | Default for |
|------|-------|-------------|
| **Minimal** | breadcrumb + top *n* hops | low-cost HUD |
| **Neighborhood** | current node children (+ filter by partial) | daily CCL / chord |
| **Full** | extended slice (`ExtendedGraph`) | onboarding / Ctrl+K exploration |

### 4. Discoverability stack (all engages)

| Layer | When | Surface |
|-------|------|---------|
| Muscle memory | expert | none |
| **Visual Command Tree** | in-session capture | trail + table + guidance badge |
| Catalog peel | out-of-band | `c:` (melody), Ctrl+K palette |

Same catalog SSOT; three projections ([ADR-0015](GUIDERS-ADR-0015-invocation-mechanics-slash-melody-binding.md)).

### 5. Slash mapping rules

`SlashVisualCommandTreeProjector` maps:

| `SlashCompletionItemKind` | `VisualCommandTreeNodeKind` |
|---------------------------|----------------------------|
| `Segment` | `Segment` |
| `Picker` | `Picker` |
| `ConstructorEntry` | `ConstructorEntry` |
| `ConstructorStep` | `ConstructorStep` |

`SlashInputGuidance` → `BreadcrumbDisplay`, `Placeholder`, `NextStepHint`, `InputMode`.

DashSpec trail/guidance/suggestion table **are** the Neighborhood render today; products MAY adopt `VisualCommandTreeProjection` JSON for agent parity.

---

## Deferred (native port)

- Graph layout / Skia / WPF chord HUD wiring (CIDE)
- Melody trie SSOT (`MelodyGroup` prefix metadata) — capture runtime on planets
- HTTP `visual_tree` field on `/commands/complete` (Forge)

---

## Acceptance

- [x] Shared `VisualCommandTree*` types in CommandPlane
- [x] `SlashVisualCommandTreeProjector` for slash/CCL
- [x] `MelodyChordTreeProjector` delegates to shared core
- [ ] Native renderers consume projection DTO (CIDE/Glass)
- [ ] Forge complete API exposes optional projection payload

---

## References

- [GUIDERS-ADR-0012 § Input guidance](GUIDERS-ADR-0012-arg-picker-completion.md)
- [GUIDERS-ADR-0015 § discoverability](GUIDERS-ADR-0015-invocation-mechanics-slash-melody-binding.md)
- [DASHSPEC-ADR-0043](https://github.com/AI-Guiders/dash-spec/blob/main/design/DASHSPEC-ADR-0043-filter-command-palette.md) — DashSpec CCL adapter
