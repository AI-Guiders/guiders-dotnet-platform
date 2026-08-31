# Guiders Platform — Architecture Hub

| | |
|---|---|
| **Status** | Living hub (synthesizes ADRs; normative detail stays in linked ADRs) |
| **Version** | Platform packages **v0.30.0** |
| **Date** | 2026-08-31 |
| **Audience** | Integrators, planet maintainers, federation allies |
| **Formats** | This file (MD) · companion [GUIDERS-PLATFORM-ARCHITECTURE-HUB.docx](./GUIDERS-PLATFORM-ARCHITECTURE-HUB.docx) · [Русский](./GUIDERS-PLATFORM-ARCHITECTURE-HUB.ru.md) |

**Start elsewhere if you need:** [Federation Constitution](./GUIDERS-FEDERATION-CONSTITUTION.md) (why & how to join) · [GUIDERS-ADR-0006](./adr/GUIDERS-ADR-0006-confederation-charter.md) (normative charter) · [README](../README.md) (build & NuGet)

---

## Table of contents

1. [Executive summary](#1-executive-summary)
2. [Federation principles](#2-federation-principles)
3. [Confederation model](#3-confederation-model)
4. [AI Era stack — three pillars](#4-ai-era-stack--three-pillars)
5. [Platform boundary](#5-platform-boundary)
6. [Architecture layers](#6-architecture-layers)
7. [Package families & capabilities](#7-package-families--capabilities)
8. [Command plane — deep dive](#8-command-plane--deep-dive)
9. [Invocation mechanics](#9-invocation-mechanics)
10. [Discoverability — Visual Command Tree](#10-discoverability--visual-command-tree)
11. [Agent ingress — MCPlane](#11-agent-ingress--mcplane)
12. [Cockpit & routing](#12-cockpit--routing)
13. [Supporting families](#13-supporting-families)
14. [Conformance hyperlane](#14-conformance-hyperlane)
15. [Quarry waves & roadmap](#15-quarry-waves--roadmap)
16. [How to join the federation](#16-how-to-join-the-federation)
17. [Reference missions & consumers](#17-reference-missions--consumers)
18. [ADR index](#18-adr-index)
19. [Glossary](#19-glossary)

---

## 1. Executive summary

**Guiders Platform** (`guiders-platform`) is the federation's **headless mechanics layer**: testable NuGet packages (`AIGuiders.Platform.*`) that unify command semantics, notation IR, cockpit contracts, documentation guild tools, navigation scenes, and agent response envelopes — without owning product UI, domain models, or MCP wire.

**In one sentence (federation):** *Sovereign product repos, shared protocols on NuGet — integrate without annexation, embed without joining our product line.*

The platform answers:

| Question | Owner |
|----------|-------|
| What command exists? How resolve a slash path? | **CommandPlane** |
| How parse keyboard/command wire into IR? | **Notations** |
| How merge layered catalogs? | **Combinations** + **Sources** |
| What does the agent see back (pulse, next)? | **MCPlane** |
| What channel snapshot shape? | **Cockpit.*** |
| How navigate a repo bounded for agents? | **Navigation** |
| How link docs ↔ code? | **Documentation.Correspondence** |

Products (**planets**) answer: UI, execution hosts, catalog **content**, domain logic, release cadence.

---

## 2. Federation principles

These principles are normative in [GUIDERS-ADR-0006](./adr/GUIDERS-ADR-0006-confederation-charter.md) and expanded in the [Federation Constitution](./GUIDERS-FEDERATION-CONSTITUTION.md).

### 2.1 Prime protocol (non-negotiable)

**Forbidden**

- Lifting product domain into platform «because it's used twice» without a Core extraction ADR.
- One planet's runtime as the only gateway to federation contracts.
- «Easy integrate» README without version pin, contract, and a reference route.
- Identity overwrite — one product claiming to be universal SSOT for all.

**Required**

- Semver on public contracts; breaking change = major + migration note.
- ADR for new hyperlane or moved boundary.
- Second consumer before calling a contract **stable**.

### 2.2 Sovereignty rules

1. **Products keep repos** — no monorepo annexation.
2. **Domain stays on the planet** — Forge IOP, CDP buffer plane, Glass projection, DashSpec dashboards.
3. **Shared kit is protocol, not colony** — packages are federated contracts with independent CI.
4. **Native per ecosystem** — TS, Kotlin, PHP ports implement the same IR; .NET quarry is embassy, not gate.

### 2.3 Open Core

Conformance is not paywalled. MIT packages, public ADRs. Fork adapters — not fork contract semantics in secret.

### 2.4 Planets are not federation SSOT

Experimental behavior on a planet (e.g. CDP Citizen tools) is **informative**, not normative. Federation cites IR, schemas, conformance vectors — not product-specific wire.

### 2.5 Adoption alliances (real, not decorative)

A **real alliance** = explicit pact on a hyperlane:

| Element | Meaning |
|---------|---------|
| **What** | Named hyperlane + semver spec tag |
| **Who adopts** | Which planet pins quarry vs ports vectors natively |
| **How to contribute** | Issues/PRs to spec or reference quarry |
| **Conformance** | Testable join — pass vectors, document adapter ADR |

Automation: [`AdoptionReport`](../tools/AdoptionReport) → [`ADOPTION-ALLIANCE.generated.md`](./ADOPTION-ALLIANCE.generated.md).

---

## 3. Confederation model

```
                    ┌─────────────────────────────────────────┐
                    │         AI Guiders Federation            │
                    │  protocols · ADR signage · conformance   │
                    └─────────────────────────────────────────┘
           NuGet / schema / MCP                    hyperlanes
    ┌──────────┬──────────┬──────────┬──────────┬──────────┐
    │ Platform │ UI Core  │  Core    │ Plugin   │  Notes   │
    │ (this)   │          │  organs  │  Host    │  (KB)    │
    └────┬─────┴────┬─────┴────┬─────┴────┬─────┴────┬─────┘
         │          │          │          │          │
    ┌────┴────┐ ┌───┴───┐ ┌────┴────┐ ┌───┴───┐      │
    │  Forge  │ │ CIDE  │ │  CDP    │ │ Glass │  ... │
    │ embassy │ │quarry │ │ habitat │ │ proj  │      │
    └─────────┘ └───────┘ └─────────┘ └───────┘      │
         │          │          │                      │
    DashSpec ───────┴──────────┴── (CommandPlane pin) ─┘
```

| Concept | Meaning |
|---------|---------|
| **Planet** | Sovereign repo — Forge, CIDE, DashSpec, CDP, your SaaS |
| **Federation** | Cross-repo contracts, semver packages, ADR signage |
| **Hyperlane** | Versioned protocol (NuGet, schema, MCP surface) |
| **Embassy** | Reference consumer proving the lane (Forge — not capital) |
| **Signage** | ADRs, conformance specs, stable test ids |
| **Prime protocol** | Do not break a planet's domain for integrator convenience |

**Rejected metaphors:** single city, empire, monolith.

---

## 4. AI Era stack — three pillars

The product center is **not** one planet eating others. Three traditions compose the stack; each has a home, wired by protocol:

| Pillar | Question | Home |
|--------|----------|------|
| **Aviation** | Who flies/monitors; what system layers aligned? | Platform `Cockpit.*` + CIDE/Glass ([ADR-0007](./adr/GUIDERS-ADR-0007-aviation-mental-model.md)) |
| **Agent Env** | Where does the agent live and remember? | **CDP** (`cdp-mcp`) — sovereign planet |
| **CASE** | What are we steering; what changes if we ship? | Platform workbench + conformance ([ADR-0023](./adr/GUIDERS-ADR-0023-case-workbench-heritage.md)) |

Platform ships **roads** (neutral IR, MCPlane tiers, conformance). Planets dogfood and pin packages at their pace.

---

## 5. Platform boundary

From [GUIDERS-ADR-0001](./adr/GUIDERS-ADR-0001-platform-boundary.md):

| Layer | Platform owns | Products own |
|-------|---------------|--------------|
| **Contracts** | interfaces, DTOs, event records | — |
| **Mechanics** | resolver, merge, fold, routing envelopes | — |
| **Catalog content** | — | TOML, JSON, DB, Forge plugins |
| **UI / host** | — | Blazor, WPF, JS, MCP wire |
| **Execute shape** | `CommandOutcome`, registry contracts | handlers, HTTP, MCP kernel |

**Dependency rule:** no WPF/Avalonia refs in platform. Monolith `AIGuiders.Platform.Cockpit` 0.1.0 is deprecated — use split `Cockpit.*` packages.

**Not in platform scope:** DashSpec product domain, Avalonia UI forks, Citizen organ handlers (stay in cdp-mcp).

---

## 6. Architecture layers

### 6.1 Two ingress planes

```
  Human                          Agent
  ─────                          ─────
  Slash / CCL ──┐                MCP CallTool ──┐
  Melody chord ─┤                                │
  Hotkey ───────┼──► CommandPlane ◄──────────────┤
  Palette c: ───┘         │                      │
                          ▼                      ▼
                     MCPlane              Pulse / next[] / tiers
                          │
                     Cockpit.* ◄── Routing (IIntentOrgan)
```

| Plane | Question | Key packages |
|-------|----------|--------------|
| **CommandPlane** | What command? How resolve path? | `CommandPlane.*`, `Notations.*` |
| **MCPlane** | What does agent see back? | `MCPlane`, `Abstractions` |
| **Cockpit** | Channel snapshot shape? | `Cockpit.DataBus`, `Channels` |
| **Routing** | Where dispatch intent? | `Routing` |

### 6.2 Dependency cake (platform internal)

```
                    ┌─ Surfaces (planets) ─┐
                    │  JS · WPF · Blazor   │
                    └──────────┬───────────┘
                               │ adapters
┌──────────────────────────────┴──────────────────────────────┐
│ CommandPlane.Slash · Melody · Binding · Sources             │
│ Notations.Keyboard · Command · Argument · Bracket           │
│ Combinations · Sources · Configurations                     │
│ Documentation · Navigation · LanguageIntelligence         │
│ Conformance · MCPlane · Utilities.Adoption                  │
└──────────────────────────────┬──────────────────────────────┘
                               │
                    CommandPlane (core) · Cockpit.*
                               │
                    Abstractions · Routing
```

---

## 7. Package families & capabilities

**87 projects** in `src/` · published as `AIGuiders.Platform.*` on [nuget.org](https://www.nuget.org/packages?q=AIGuiders.Platform).

### 7.1 Foundation

| Package | Capabilities |
|---------|--------------|
| `Abstractions` | `IntentOutcome`, `RoutedIntent`, `PulseFormat` (default ~240 chars truncation) |
| `Routing` | `IIntentOrgan<TRoute,TOutcome>`, `DispatchCallOverride`, route refusal helpers |

### 7.2 Sources & Combinations

| Package | Capabilities |
|---------|--------------|
| `Sources` | Generic `ISource<T>` transport abstraction |
| `Sources.File` / `.Toml` | File + TOML transport |
| `Catalog` | `CatalogIndex<TKey,TEntry>`, `ICatalogProfile`, merge policies ([ADR-0041](./adr/GUIDERS-ADR-0041-catalog-kernel-profiles.md)) |
| `Combinations` | `Combinator<T>`, `OrderedCombination.Fold`, `CombinationSemantics` |
| `Combinations.Workspace` | `FieldOverlay` — overlay non-null wins |
| `Combinations.Catalog` | Meta → `ShipFirst` merge in CommandPlane.Catalog |
| `Combinations.Binding` | Meta → `OverlayWins` merge in CommandPlane.Binding |
| `Combinations.All` | Meta-bundle |

**Operator rule:** **Sources** = transport only; **Combinations** = ordered fold + named policies ([ADR-0030](./adr/GUIDERS-ADR-0030-combinations-family.md)).

### 7.3 Notations (wire → IR)

| Package | Capabilities |
|---------|--------------|
| `Notations` | Shared primitives (`NotationKvPair`, list split) |
| `Notations.Keyboard.*` | `NormalizedKeySequence` — KeyGesture, Vim, Neovim, Emacs wires |
| `Notations.Command.*` | `NormalizedCommandLine`, slash/console body tokenize |
| `Notations.Argument.*` | Kv, positional, CLI flags, delimited (colon) tails |
| `Notations.Bracket` | Bracket notation branch ([ADR-0026](./adr/GUIDERS-ADR-0026-notations-bracket-branch.md)) |
| `InputNotation.*` | **Legacy alias** → `Notations.Keyboard.*` (obsolete forwards) |

Platform ships **reference quarry** (.NET parsers). Planets **port vectors** to native stacks (Forge JS, VS Code extension, etc.).

### 7.4 CommandPlane

| Package | Capabilities |
|---------|--------------|
| `CommandPlane` | GoF `IPlatformCommand<T>`, `PlatformCommandRegistry`, `ICommandContext` (hub) |
| `CommandPlane.Catalog` | Command catalog IR + index facade ([ADR-0039](./adr/GUIDERS-ADR-0039-command-catalog-family.md), [ADR-0041](./adr/GUIDERS-ADR-0041-catalog-kernel-profiles.md)) |
| `CommandPlane.ArgSuggestions` | Federated arg suggestion broker + planet provider registry |
| `CommandPlane.Constructors` | Value constructor registry, session, navigator, locale input ([ADR-0035](./adr/GUIDERS-ADR-0035-slash-value-constructors.md)) |
| `CommandPlane.PrefixArmed` | PAC profiles + coordinator — surface-agnostic ([ADR-0038](./adr/GUIDERS-ADR-0038-prefix-armed-completion.md)) |
| `CommandPlane.PrefixArmed.Locale` | Optional locale date PAC profile ([ADR-0037](./adr/GUIDERS-ADR-0037-slash-locale-typed-value-input.md)) |
| `CommandPlane.Slash` | `SlashLineResolver`, completion, ArgTail, slash guidance projector (consumes core catalog) |
| `CommandPlane.Melody` | Melody descriptors, line profile, policy, chord tree projection |
| `CommandPlane.Binding` | Hotkey catalog, gesture normalize, layered merge |
| `CommandPlane.Catalog.Sources.*` | Json, Toml, Xml, File, Database transports → Core |
| `CommandPlane.Catalog.Sources` | Meta-bundle all formats |

### 7.5 Cockpit

| Package | Capabilities |
|---------|--------------|
| `Cockpit.Abstractions` | CCU, DAL, channel, CDS, compositor contracts |
| `Cockpit.DataBus` | `IDataBus`, build/test/debug/git/ide events |
| `Cockpit.Channels` | IdeHealth/EnvReady DTOs + CCU kits |
| `Cockpit.Cds` / `Composition` | Routing/compositor DTOs |
| `Cockpit.Transport` | `IngressEvent`, `BoundedIngressBus` |
| `Cockpit.Ids` | IDS overlay search seam |

Glass WPF = **projection** of snapshots; does not own CCU mechanics.

### 7.6 Documentation guild

| Package | Capabilities |
|---------|--------------|
| `Documentation.Anchors` | Family:doc wire resolve |
| `Documentation.LinkCheck` | md dry-resolve (`--check`) |
| `Documentation.LinkMutate` | Structured axis patch (`--apply-rename`) |
| `Documentation.Reports` | Generated vocabulary tables |
| `Documentation.Correspondence.*` | Forward ADR map + reverse md scan ([ADR-0028](./adr/GUIDERS-ADR-0028-documentation-guild-correspondence-family.md)) |

### 7.7 Navigation

| Package | Capabilities |
|---------|--------------|
| `Navigation` | `navigation_scene/v1` — nodes, edges, caps |
| `Navigation.Policy` | Presets, kind filters, profile caps |
| `Navigation.Code` | Roslyn wire parser + scene builder ([ADR-0033](./adr/GUIDERS-ADR-0033-navigation-family-semantic-scenes.md)) |

Hosts (CDP SemanticMap, CIDE Skia) = projectors, not SSOT.

### 7.8 Language intelligence

| Package | Capabilities |
|---------|--------------|
| `LanguageIntelligence` | Anchor/Locus/TextEdit IR, resolve tiers |
| `LanguageIntelligence.Adapters.Roslyn` | Roslyn adapter |
| `Language.CSharp.*` / `Language.Xml.Anchors` | Symbol/anchor wires |

### 7.9 Configurations

| Package | Capabilities |
|---------|--------------|
| `Configurations.Project` / `.Workspace` | Layered config compose |
| `Configurations.*.Sources` | Source transports for config layers |

### 7.10 Conformance & utilities

| Package | Capabilities |
|---------|--------------|
| `Conformance.Schemas` / `.Policies` / `.Navigation` | Obligation specs, policy-as-code |
| `MCPlane` | Agent response envelope, detail tiers, `next[]` hints |
| `Utilities.Adoption` | Planet pin scanner → alliance report |

---

## 8. Command plane — deep dive

Pattern: [GUIDERS-ADR-0009 — Catalog · Registry · Command · Surface](./adr/GUIDERS-ADR-0009-command-surface-pattern.md).

```
  Catalog                    Registry                 Command
  (discoverability)          (executor lookup)        (one effect)
       │                          │                        │
  CommandDescriptor    PlatformCommandRegistry   IPlatformCommand
  CommandCatalogIndex         EditorCommandRegistry     PlatformCommand
  capabilities.commands[]   Forge CommandCatalog
       │                          │                        │
       └──────── path / id ───────┴──── commandId ────────┘
                                    ▲
              Surfaces: slash · CCL · palette · hotkey · MCP
```

| Pattern | Question | Platform SSOT | Does NOT |
|---------|----------|-----------------|----------|
| **Catalog** | What user sees; how find by path? | Index, resolver, visitor | Execute, buffer edits |
| **Registry** | By `commandId` — which executor? | `PlatformCommandRegistry<T>` | Autocomplete UI |
| **Command** | One effect — one `Execute` | `IPlatformCommand<T>` | Parse slash string |
| **Surface** | Where human invoked? | — (product) | Own business logic |

**Wire rule:** catalog entry carries `CommandId` → surface resolves path (catalog) → registry → command.

### 8.1 Catalog merge flow

```
Forge capabilities overlay ──┐
CIDE intent-catalog.toml ──┼──► CommandCatalogIndex.Merge ──► SlashLineResolver
DashSpec embedded TOML ────┤                                      │
Product DB delegate ───────┘                                      ▼
                                                          completion + guidance
```

Merge policies ([ADR-0030](./adr/GUIDERS-ADR-0030-combinations-family.md)):

| Domain | Policy | Collision rule |
|--------|--------|----------------|
| Slash catalog | ShipFirst | TryAdd — ship wins |
| Binding catalog | OverlayWins | overlay overwrites key |
| Workspace fields | FieldOverlay | overlay non-null wins |

### 8.2 Minimal third-party embed

```csharp
var catalog = CommandCatalogComposer.Build(
    CommandSources.FromFile("commands.toml"),
    RegistryCatalogBuilder.ToCommandSource(myRegistry));
// Your execute endpoint, your UI — federation does not own the wire.
```

### 8.3 Arg completion modes

From [ADR-0012](./adr/GUIDERS-ADR-0012-arg-picker-completion.md) and [ADR-0035](./adr/GUIDERS-ADR-0035-slash-value-constructors.md):

| Mode | When | User experience |
|------|------|-----------------|
| `Picker` | Closed enum / preset values | Tab to insert wire token |
| `Constructor` | Structured typed values (date, range) | Guided segment tree |
| `FreeText` | Escape hatch | Type wire per `ArgHint` |
| `Ready` | Line complete | Execute |

**Value constructors** form a **composite tree** — Range → Date(from) → Year/Month/Day. Free text always available as sibling.

---

## 9. Invocation mechanics

Three distinct input mechanics ([ADR-0015](./adr/GUIDERS-ADR-0015-invocation-mechanics-slash-melody-binding.md)):

| Mechanic | User input | Platform package |
|----------|------------|------------------|
| **Slash** | `/docs adr open` | `CommandPlane.Slash` |
| **Melody** | `<Ctrl+K>` `b` `s` (+ optional tail) | `CommandPlane.Melody` |
| **Binding** | `Ctrl+Q` → `commandId` | `CommandPlane.Binding` |

**Not a fourth mechanic:** palette prefix **`c:`** = discoverability peel (browse melody catalog) — not melody execution.

### 9.1 Musical metaphor

| Musical | Invocation |
|---------|------------|
| **Note** | Single key after chord root |
| **Chord** | Simultaneous keys, or chord root gesture |
| **Melody** | Sequential line after root |
| **Articulation** | ByNote vs ByChord per step |
| **Score on the wall** | `c:` in palette |

One `commandId`; mechanics are how you **play** it.

### 9.2 InvocationEngage glossary

Planet cues before mechanics ([ADR-0036](./adr/GUIDERS-ADR-0036-invocation-engage-glossary.md)):

| Term | Meaning |
|------|---------|
| **Sigil** | Text engage cue |
| **DiscoverabilityPrefix** | `c:` in palette |
| **ChordRoot** | Gesture that arms melody lane |

Platform resolve starts after strip/peel/tunnel — engage is not a Core type.

---

## 10. Discoverability — Visual Command Tree

[GUIDERS-ADR-0024](./adr/GUIDERS-ADR-0024-visual-command-tree-capture-stack.md) generalizes melody **Visual Chord Tree** to all engages.

| Engage | Capture state | Projector |
|--------|---------------|-----------|
| Melody chord | `MelodyCaptureStack` | `MelodyChordTreeProjector` |
| Slash / CCL | typed line + mode | `SlashVisualCommandTreeProjector` |
| Constructor | `SlashConstructorSession` | slash projector (`EngageKind = Constructor`) |

**Shared DTO:** `VisualCommandTreeProjection` — breadcrumb, placeholder, next hops, view mode (Minimal / Neighborhood / Full).

**Discoverability stack:**

| Layer | When | Surface |
|-------|------|---------|
| Muscle memory | expert | none |
| Visual Command Tree | in-session capture | trail + table + guidance |
| Catalog peel | out-of-band | `c:`, Ctrl+K palette |

DashSpec CCL today renders **Neighborhood** implicitly (trail + `SlashInputGuidance` + suggestion table).

---

## 11. Agent ingress — MCPlane

[GUIDERS-ADR-0020](./adr/GUIDERS-ADR-0020-mcplane-agent-ingress.md) — sibling plane to CommandPlane, not inside it.

| Plane | Question |
|-------|----------|
| **CommandPlane** | What command? How resolve? |
| **MCPlane** | What does agent **see** back? How expand? What's **next**? |

| Capability | Notes |
|------------|-------|
| Agent response envelope | `IntentOutcome`, pulse, reason slots |
| Detail tiers | `pulse` (default) · `slim` · `full` — explicit opt-in |
| `next[]` hints | Suggestions only — not execution |
| Pulse truncation | `PulseFormat` (~240 chars default) |
| Catalog projection | Agent slice from `ICatalogVisitor` |

MCP tool implementation stays in **product**. MCPlane holds response contract + projection rules.

---

## 12. Cockpit & routing

### 12.1 Cockpit

Headless channel/CCU contracts for aviation mental model ([ADR-0007](./adr/GUIDERS-ADR-0007-aviation-mental-model.md)):

- **DataBus** — build, test, debug, git, ide host events
- **Channels** — IdeHealth, EnvReady fold kits
- **Transport** — bounded ingress bus for agent/human events

Consumers: CIDE, cdp-mcp (`cdp_ide_health`), Glass channel bind.

### 12.2 Routing

`IIntentOrgan<TRoute,TOutcome>` — neutral intent dispatch seam. Citizen organs in cdp-mcp implement; platform defines contract only.

---

## 13. Supporting families

### 13.1 Configurations

Layered project/workspace config with source transports — used by navigation presets, workspace overlays.

### 13.2 Language & anchors

XML/C# anchor resolution for CSX lift, doc correspondence, navigation wire parsing ([ADR-0034](./adr/GUIDERS-ADR-0034-csx-lift-navigation-config-xml-anchors.md)).

### 13.3 Policy-as-readable-code

Overlay profiles as TOML/JSON specs ([ADR-0031](./adr/GUIDERS-ADR-0031-policy-as-readable-code-overlay-profiles.md)) — consumed by Conformance family.

---

## 14. Conformance hyperlane

Bootstrap specs in [`docs/conformance/`](./conformance/README.md); target sibling repo `aiguiders-conformance` ([ADR-0019](./adr/GUIDERS-ADR-0019-conformance-hyperlane-monorepo.md)).

| Spec area | Examples |
|-----------|----------|
| Slash | `slash-line-resolve`, `slash-arg-completion` |
| Notation | `command-slash`, `argument-kv`, `invocation-parity` |
| MCPlane | `pulse-default`, `next-hints` |
| Navigation | `code-explore-scene` |
| Policies | `slash-ship-first`, `binding-overlay-wins`, `workspace-field-overlay` |

**Join pattern:** adopt schema → implement native adapter → pass vectors → document in product ADR.

---

## 15. Quarry waves & roadmap

From [GUIDERS-ADR-0010](./adr/GUIDERS-ADR-0010-platform-mechanics.md) and [GUIDERS-ROADMAP](./GUIDERS-ROADMAP.md):

| Wave | Scope | Status |
|------|-------|--------|
| **W1** | Cockpit DataBus / IdeHealth | ✓ shipped |
| **W2** | CommandPlane descriptors, ArgTail, index, resolver | ✓ shipped |
| **W2b** | Editor `IPlatformCommand` + registry | ✓ shipped |
| **W2c** | `ICatalogVisitor`, registry catalog builder | ✓ shipped |
| **W2e** | InputNotation → Notations, Melody | ✓ shipped |
| **W2f** | CommandPlane / Slash split | ✓ shipped |
| **W2x** | Sources, Combinations, Documentation, Navigation, VCT, Constructors | ✓ in progress / shipped |
| **W2d** | Forge domain → `IPlatformCommand<ForgeCommandContext>` | planned |
| **W3+** | Desk latch, full Correspondence native ports | planned |

Living backlog: [GUIDERS-ROADMAP.md](./GUIDERS-ROADMAP.md) · friction: [GUIDERS-pain-inventory.md](./GUIDERS-pain-inventory.md).

---

## 16. How to join the federation

Voluntary, testable ([Constitution § How to join](./GUIDERS-FEDERATION-CONSTITUTION.md#how-to-join-voluntary-testable)):

1. **Adopt** relevant hyperlane package or schema.
2. **Implement** native adapter in your stack (no mandatory UI framework).
3. **Pass** conformance — contract tests, journey smoke, semver pin.
4. **Document** wiring in your product ADR; link platform ADRs.

No requirement to use DOI paths, Forge, or our product line. Flat paths and your own `commandId` space are valid.

### 16.1 Layer responsibilities for integrators

| Layer | Federation ships | Planet implements natively |
|-------|------------------|----------------------------|
| Contract | IR, schemas, `commandId`, catalog shape | — |
| Signage | conformance specs | — |
| Reference quarry | .NET packages (embassy) | may pin as-is |
| Wire → IR | spec + reference parser | port to TS/Kotlin/… |
| IR → input | — | key match, OS shortcuts |
| Surface | — | WPF, Blazor, extension host |

---

## 17. Reference missions & consumers

| Planet | Role | Platform hyperlanes |
|--------|------|---------------------|
| **agent-forge** | Embassy — MCP, capabilities, plugin host | CommandPlane, MCPlane, Notations port |
| **cascade-ide** | Quarry → distill mechanics | Slash, Melody, Binding, Cockpit, Notations |
| **cdp-mcp** | Agent habitat | Cockpit, Routing, Navigation (informative dogfood) |
| **Glass** | WPF cockpit projection | Cockpit channels |
| **dash-spec** | Dashboard CCL adapter | CommandPlane.Slash, constructors |
| **Third party** | Any stack | Pin NuGet or port conformance vectors |

Sibling repos (not in platform monorepo):

| Repo | Role |
|------|------|
| `guiders-ui-platform` | Tokens, Agent AX, UI adapters |
| `guiders-core` | Shared backend organs |
| `guiders-plugin-host` | ALC plugin transport |
| `agent-notes` | Operator KB, handoff canon |

---

## 18. ADR index

| ADR | Topic |
|-----|-------|
| [0001](./adr/GUIDERS-ADR-0001-platform-boundary.md) | Platform boundary |
| [0003](./adr/GUIDERS-ADR-0003-platform-ssot-quarry.md) | SSOT quarry map |
| [0006](./adr/GUIDERS-ADR-0006-confederation-charter.md) | Confederation charter |
| [0007](./adr/GUIDERS-ADR-0007-aviation-mental-model.md) | Aviation mental model |
| [0009](./adr/GUIDERS-ADR-0009-command-surface-pattern.md) | Catalog · Registry · Command · Surface |
| [0010](./adr/GUIDERS-ADR-0010-platform-mechanics.md) | Platform mechanics |
| [0012](./adr/GUIDERS-ADR-0012-arg-picker-completion.md) | Arg picker completion |
| [0015](./adr/GUIDERS-ADR-0015-invocation-mechanics-slash-melody-binding.md) | Slash · Melody · Binding |
| [0020](./adr/GUIDERS-ADR-0020-mcplane-agent-ingress.md) | MCPlane (draft) |
| [0021](./adr/GUIDERS-ADR-0021-notations-quarry-family.md) | Notations quarry |
| [0024](./adr/GUIDERS-ADR-0024-visual-command-tree-capture-stack.md) | Visual Command Tree |
| [0030](./adr/GUIDERS-ADR-0030-combinations-family.md) | Combinations family |
| [0033](./adr/GUIDERS-ADR-0033-navigation-family-semantic-scenes.md) | Navigation scenes |
| [0035](./adr/GUIDERS-ADR-0035-slash-value-constructors.md) | Value constructors |
| [0036](./adr/GUIDERS-ADR-0036-invocation-engage-glossary.md) | InvocationEngage glossary |

Full list: [`docs/adr/`](./adr/).

---

## 19. Glossary

| Term | Definition |
|------|------------|
| **Mechanic** | Headless testable unit: contract + implementation + identity + context |
| **Hyperlane** | Versioned federation protocol (NuGet, schema, MCP surface) |
| **Planet** | Sovereign product repo |
| **Embassy** | Reference consumer, not capital |
| **Quarry** | Extract mechanics from legacy product code into platform |
| **ArgTail** | Slash arg phase policy (picker, constructor, free text) |
| **commandId** | Stable executor key in registry |
| **Visual Command Tree** | Headless capture-stack projection for discoverability |
| **Conformance vector** | Testable spec instance proving hyperlane compatibility |

---

## Document maintenance

| Trigger | Action |
|---------|--------|
| New package family | Update §7 + ADR index |
| Major semver | Update version header |
| New hyperlane | Update §6 + Constitution cross-link |
| Quarterly | Regenerate `ADOPTION-ALLIANCE.generated.md`; verify docx export |

Regenerate DOCX:

```bash
pandoc docs/GUIDERS-PLATFORM-ARCHITECTURE-HUB.md \
  -o docs/GUIDERS-PLATFORM-ARCHITECTURE-HUB.docx \
  --toc --toc-depth=3 \
  -V lang=ru-RU
```

---

*Unified planets: one mechanics layer, many atmospheres.*
