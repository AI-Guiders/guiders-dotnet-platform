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
| `TopologyWire` | opaque wire in deck IR; parsed by `Notations.Presentation.Topology` → `NormalizedPresentationSurface` |

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
| **Mechanics** | `Notations.Presentation.*` + Surface peel | topology wire → IR → column/host flags |
| **Projection** | `Surface.Wpf.*` | `ApplyColumnDefinitions`, UiKit brushes, WebView2 host |

CIDE [0021](https://github.com/AI-Guiders/cascade-ide/blob/develop/docs/adr/0021-pfd-mfd-cockpit-attention-model.md) remains **normative semantics**; `.deck` is **declare-time binding** of semantics to a planet's surfaces.

### 3.1 Topology wire → **Notations** (not planet-local parser)

Strings like `(P)(F)(M)`, `(MFD)(F)`, `(F/P/M)` are **one notation alphabet** in the cockpit family — not ad-hoc Glass strings. Per [0021](./GUIDERS-ADR-0021-notations-quarry-family.md) quarry pattern:

```text
WIRE (topology string)  →  IR (PresentationSurfacePack, flags)  →  MECHANIC (grid columns, host windows)
         │                          │                                    │
   Notations.Presentation.*    platform / Surface.Core          Surface.Wpf.* / GlassCore peel
```

| Layer | Guild | Owns |
|-------|-------|------|
| **Meaning** | `Authoring.Deck` | *which* topology wire a preset uses (`topology (MFD)(F)`) |
| **Wire → IR** | **`Notations.Presentation.*`** (new branch) | parse `(P)(F)(M)` → typed IR — extract from `PresentationSurfaceWire` / `PresentationParser` (GlassCore today) |
| **IR → UI** | `Surface.Wpf.*` | column recipes, visibility, multi-window host spawn |

**Why Notations:** same split as slash — `.catalog` **declares** phrase; `Notations.Command.Slash` **parses** wire at resolve. `.deck` **declares** topology line; `Notations.Presentation.Topology` **parses** it. Planets must not ship private `(P)(F)(M)` parsers.

**Package sketch (target):**

```text
Notations.Presentation.Core     NormalizedPresentationSurface, slot roles (P/F/M/OneOf)
Notations.Presentation.Topology (P)(F)(M) · (MFD)(F) · surface-stack wire ([0021] §1.2)
Notations.Presentation.All    facade + conformance vectors
```

Bracket branch ([0026](./GUIDERS-ADR-0026-notations-bracket-branch.md)) stays for **payload** delimiters; presentation topology is **layout grouping**, not arg tail — separate branch, may share lexer helpers only.

**Emit path:** `deck emit` calls Notations reader on each preset's `topology` line → embeds resolved IR constants in `Deck.g.cs` (or references normalized wire hash → flags table generated once).

**Single-screen = first class (server deployment):** DashSpec Studio is expected to run on **Windows Server** (RDS, jump box, shared VM) — often **one physical display**. One-screen layout is **not** a degraded/mobile fallback; it must be **the same cockpit semantics** as multi-monitor (Forward/PFD/MFD/EICAS), only projection differs.

| Profile | Topology wire (examples) | Projection |
|---------|--------------------------|------------|
| **Cockpit** (multi-display) | `(P)(F)(M)`, `(MFD)(F)` + satellite host | dedicated TopLevels per zone |
| **Server / single surface** | `(F/P/M)` OneOf, `(MFD/F)` merged | **one TopLevel** — XOR pages or docked MFD; Forward glass full width when active |

Reuse GlassCore precedent: `OperatorReviewFlightTopology = "(F/P/M)"` — all scan channels in **one** TopLevel ([GlassPresentationLayout](https://github.com/AI-Guiders/cascade-ide/blob/develop/CascadeIDE.GlassCore/Presentation/GlassPresentationLayout.cs)). `Notations.Presentation` MUST parse **both** families with equal conformance coverage — no «second-class» IR for single-screen.

**Deck presets (DashSpec Studio):**

```text
preset report-author          # cockpit — multi-monitor when available
  topology (MFD)(F)
  …
end preset

preset report-author-server   # default on server / RDS — first class
  topology (F/P/M)
  forward report-preview
  mfd spec-tree | data-lab | resolve
  eicas when alerts
end preset
```

**Invariants:**

- Report Preview on single screen = **full-width forward glass**, not thumbnail strip.
- MFD zones = tabs / bottom dock / OneOf — **not** «open second window» (often blocked or awkward on RDS).
- Runtime detects display count → **select preset** or adapt IR projection; **do not** fork ViewModels per layout tier.
- WPF on Server: plan for RDS session (WebView2 GPU, input latency) — deployment note, not a separate product.

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

### 5. Guild-owned standard (not `workspace.toml` inheritance)

**Surface WPF guild defines its own SSOT** — grammar, file extension, IR, emit CLI, preset vocabulary. Planets **adopt the guild standard**; they do not embed deck blocks inside CIDE `workspace.toml` or Glass `settings.toml` as the long-term home.

| Artifact | Role in evolution |
|----------|-------------------|
| **`<planet>.deck`** | **Guild SSOT** — attention presets, zones, topology meaning |
| **`workspace.toml` / `settings.toml` presentation** | **Legacy / transitional** — today hosts topology in Glass/CIDE; may be **rewritten or replaced** as `.deck` + runtime profile wins |
| **Runtime user overlay** | Still allowed (last-write operator preference) — but **loads from deck-emitted schema**, not ad-hoc TOML dialects per product |

**Principle:** do not design `.deck` as «yet another block in workspace.toml». The guild standard is **first-class** (like `.catalog`), so new planets (Studio, DBA Studio) start on `.deck` without inheriting CIDE repo-layout coupling.

**Migration / reuse Glass:**

1. **Extract** topology parse from `CascadeIDE.GlassCore` → **`Notations.Presentation.Topology`** (platform); GlassCore becomes consumer, not owner.
2. Glass **may** bridge: read `settings.toml` topology → map to deck preset until Glass migrates; bridge is **adapter**, not SSOT.
3. When guild standard stabilizes, `workspace.toml` presentation keys become **deprecated** in favor of `.deck` + generated runtime manifest — exact cutover per planet, not big-bang federation mandate in v0.

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

1. **File extension:** `.deck` (guild default) vs `.presentation` — **not** nested in `workspace.toml` as permanent SSOT.
2. **Neutral assembly name:** `Surface.Presentation.Core` (IR→columns) vs keep column math in Glass peel until second consumer?
3. **Notations.Presentation:** track as amendment to [0021](./GUIDERS-ADR-0021-notations-quarry-family.md) §2 branch table when package lands?
4. **XAML emit depth:** constants-only v0 vs partial ResourceDictionary merge v1?
5. **`workspace.toml` sunset:** adapter period length for Glass/CIDE vs greenfield `.deck`-only for Studio/DBA?
6. **Server/RDS:** WebView2 + GPU policy matrix for Report Preview on Windows Server?

## Reference missions

| Planet | First preset | Forward zone | Server preset |
|--------|--------------|--------------|---------------|
| `dash-spec-studio` | `report-author` | `report-preview` | `report-author-server` `(F/P/M)` |
| `dba-studio` | `dba-ops` | `repl` | `dba-ops-server` (TBD) |
| Glass | existing topology strings | migrate to deck optional | `(F/P/M)` already sealed |

## Worked example (target end state)

Author edits `dashspec-studio.deck` + `dash.catalog`. CI:

```text
dotnet catalog emit  →  DashCatalog.g.cs
dotnet deck emit     →  DashSpecStudioDeck.g.cs
dotnet build         →  WPF host; zones bind by ZoneIds.ReportPreview
```

Human/agent changes **topology for data-probe preset** in one file — no hunt through MainWindow.xaml.cs.
