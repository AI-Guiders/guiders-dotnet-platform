# GUIDERS-ADR-0055: Surface WPF guild + deck authoring (write meaning · gen rest)

| | |
|---|---|
| **Status** | Proposed |
| **Level** | **Federation surface hyperlane** — planet-side UI, not platform mechanics |
| **Date** | 2026-09-01 |
| **Tags** | #guiders #federation #wpf #surface #cockpit #authoring #codegen #deck #dx |
| **Related** | [0001](./GUIDERS-ADR-0001-platform-boundary.md) · [0048](./GUIDERS-ADR-0048-authoring-quarry-family.md) · [0047](./GUIDERS-ADR-0047-command-for-doi.md) · [0054](./GUIDERS-ADR-0054-phrase-slot-completion.md) · CIDE [0021](https://github.com/AI-Guiders/cascade-ide/blob/develop/docs/adr/0021-pfd-mfd-cockpit-attention-model.md) · [0053](./GUIDERS-ADR-0053-planet-responsibilities.md) |

## Context

Windows desktop planets are converging on **WPF** with shared **Dark Cockpit / Attention Zones** language:

| Planet | Host |
|--------|------|
| Glass | `CDP.GlassCockpit.Windows` + `CascadeIDE.GlassCore` |
| DashSpec Studio | planned WPF + WebView2 Report Preview |
| DBA Studio | planned WPF REPL-forward |

**Platform** ([0001](./GUIDERS-ADR-0001-platform-boundary.md)) stays **headless** — no WPF/Avalonia refs in `AIGuiders.Platform.*`. Cockpit **contracts** (CDS, channel DTOs, DataBus) remain platform; **chrome** is planet-side.

**Catalog precedent:** `.catalog` declare → `Authoring.Command.Catalog` IR → `CatalogCatalogEmitter` → `{Planet}Catalog.g.cs` ([0047](./GUIDERS-ADR-0047-command-for-doi.md), [0054](./GUIDERS-ADR-0054-phrase-slot-completion.md)). Planets ship **meaning**; federation/tooling **generates** wire maps, phrase slots, partial classes.

**Presentation today:** Glass already resolves topology strings `(P)(F)(M)` → column defs via `GlassPresentationLayout` + `PresentationSurfaceWire` in **GlassCore** — but layout SSOT is scattered (settings.toml, workspace.toml, latch JSON). Studio/DBA would re-copy WPF grid glue without a shared **deck** spine.

**Operator DX thesis:** *Write the meaning. Gen the rest.* — engineers declare attention zones, presets, and surface routing once; codegen emits WPF column recipes, zone visibility flags, registry hooks, and optional XAML skeletons.

## Decision

### 1. Surface WPF guild (package line)

Introduce federation **Surface WPF guild** — **sibling to platform**, not inside it:

```text
AIGuiders.Platform.*           headless mechanics (CommandPlane, Cockpit.*, Authoring.*)
AIGuiders.Surface.Wpf.*        WPF adapters, UiKit, deck emit targets
Planets                        Glass, dash-spec-studio, dba-studio — content + Execute partials
```

**Seed:** peel `CDP.GlassCockpit.Windows/UiKit` + shared projectors when **second WPF consumer** ships (DashSpec Studio or DBA Studio bootstrap).

| Package (draft) | Owns |
|-----------------|------|
| `Surface.Wpf.Abstractions` | `IDeckHost`, zone slot ids, attach/detach contracts |
| `Surface.Wpf.UiKit` | Dark Cockpit ResourceDictionary, dock chrome primitives |
| `Surface.Wpf.WebView2` | Blazor/Hybrid host shell (Report Preview) |
| `Surface.Wpf.CodeGen` | Roslyn emit from deck IR → grid/columns/flags partials |
| `Authoring.Deck` | `.deck` grammar + parser → typed IR |

**Dependency rule:** `Surface.Wpf.*` may reference `Platform.Cockpit.Abstractions` + `Platform.CommandPlane` adapters; **never** planet domain (DashSpec, SQL, IDE organs).

### 2. Deck authoring — second quarry (parallel to `.catalog`)

New declare-time artifact: **`<planet>.deck`** (name TBD; `.presentation` acceptable if aligned with Glass `presentation` latch).

**Meaning layer (human writes):**

```text
deck dashspec-studio

preset report-author
  topology (MFD)(F)
  forward report-preview
  mfd spec-tree | resolve
  mfd data-lab repl
  eicas when alerts
end preset

preset data-probe
  topology (MFD)(F)
  forward repl
  mfd spec-tree
end preset

zones
  report-preview = forward
  repl             = forward
  spec-tree        = mfd
end zones
end deck
```

**Block syntax:** DashSpec parity — `keyword … end keyword`, `#` comments ([0047](./GUIDERS-ADR-0047-command-for-doi.md) §4).

**IR spine (typed, not strings in product code):**

| IR type | Role |
|---------|------|
| `DeckDocument` | planet id, presets[], zone map |
| `AttentionPreset` | topology wire, forward zone id, mfd slots[], eicas policy |
| `ZoneBinding` | zone id → channel / view type / DataTemplate key |
| `TopologyWire` | reuse or embed `PresentationSurfaceWire` from GlassCore |

**Emit boundary (gen the rest):**

```text
.deck  →  Authoring.Deck parser  →  DeckDocument IR
       →  Surface.Wpf.CodeGen     →  DashSpecStudioDeck.g.cs
                                   →  WpfMainGridColumns recipe (like Glass)
                                   →  preset switch / visibility flags
                                   →  optional XAML fragment stubs (DataTemplate keys)
```

Planets implement **only** zone content (views, ViewModels) keyed by generated `ZoneIds.*` constants — not hand-maintained column math.

### 3. Split: platform semantics vs WPF projection

| Layer | Guild | Example |
|-------|-------|---------|
| **Semantics** | `Platform.Cockpit.*` | CDS snapshot, EICAS severity enum, channel ids |
| **Meaning** | `Authoring.Deck` | which zone is forward for preset `report-author` |
| **Mechanics** | `GlassCore` / shared peel | topology parse `(MFD)(F)` → flags (existing) |
| **Projection** | `Surface.Wpf.*` | `ApplyColumnDefinitions`, UiKit brushes, WebView2 host |

CIDE [0021](https://github.com/AI-Guiders/cascade-ide/blob/develop/docs/adr/0021-pfd-mfd-cockpit-attention-model.md) remains **normative semantics**; `.deck` is **declare-time binding** of semantics to a planet's surfaces.

### 4. Integration with `.catalog`

Deck and catalog are **orthogonal SSOTs**, same authoring pipeline shape:

| File | Declares | Emits |
|------|----------|-------|
| `<planet>.catalog` | commands, phrases, profiles | `Catalog.g.cs`, phrase slots |
| `<planet>.deck` | presets, zones, topology | `Deck.g.cs`, grid recipes |

Cross-link optional: `deck` preset may reference `catalog` surface ids for command bar placement — via IR refs, not copy-paste.

CLI shape (mirror `dotnet catalog emit`):

```bash
dotnet deck emit --project DashSpec.Studio.Wpf --deck dashspec-studio.deck
```

### 5. Migration / reuse Glass

1. **Extract** topology parse + column math from `CascadeIDE.GlassCore` into shared neutral assembly (`Surface.Presentation.Core` or keep GlassCore as v0 SSOT with `Surface.Wpf` referencing it).
2. Glass `settings.toml` `topology` remains **runtime override**; `.deck` presets are **ship defaults**.
3. Glass may adopt `.deck` later; not blocking Studio v0.

## Consequences

- DashSpec Studio / DBA Studio bootstrap WPF with **preset constants** from day one — no forked `(P)(F)(M)` parsers per repo.
- Second consumer validates Surface WPF guild before calling packages **stable**.
- Agents read `.deck` + `.catalog` as compact meaning — same as human authors.

## Non-goals (v0)

- Avalonia projector in Surface guild (Glass Avalonia stays legacy; mechanics already in GlassCore).
- Blazor planet UI in `Surface.Wpf.*` (DashSpec **Host** stays separate; WebView2 hosts Host/RCL only).
- Auto-generating ViewModel **business logic** — only wiring, ids, grid recipes, template keys.
- Merging deck IR into `Platform.Cockpit` (keeps platform UI-free).

## Open questions

1. **File extension:** `.deck` vs `.presentation` vs block inside `workspace.toml`?
2. **Neutral assembly name:** extend `GlassCore` vs `AIGuiders.Surface.Presentation.Core`?
3. **XAML emit depth:** constants-only v0 vs partial ResourceDictionary merge v1?

## Reference missions

| Planet | First preset | Forward zone |
|--------|--------------|--------------|
| `dash-spec-studio` | `report-author` | `report-preview` |
| `dba-studio` | `dba-ops` | `repl` |
| Glass | existing topology strings | migrate to deck optional |

## Worked example (target end state)

Author edits `dashspec-studio.deck` + `dash.catalog`. CI:

```text
dotnet catalog emit  →  DashCatalog.g.cs
dotnet deck emit     →  DashSpecStudioDeck.g.cs
dotnet build         →  WPF host; zones bind by ZoneIds.ReportPreview
```

Human/agent changes **topology for data-probe preset** in one file — no hunt through MainWindow.xaml.cs.
