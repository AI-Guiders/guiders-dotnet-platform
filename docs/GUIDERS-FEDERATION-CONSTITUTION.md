# Guiders Federation Constitution

**Status:** accepted (2026-08-29)  
**Level:** federation hub — **why** and **how to join**; normative detail stays in ADRs  
**Charter (normative):** [GUIDERS-ADR-0006](adr/GUIDERS-ADR-0006-confederation-charter.md)

---

## In one sentence

**Sovereign product repos, shared protocols on NuGet** — integrate without annexation, embed without joining our product line.

---

## Welcome to all

Any team, any stack, any planet — not only AI Guiders products — may:

1. **Adopt** a federation hyperlane (`AIGuiders.Platform.*`, future `AIGuiders.UI.*`, …) from [nuget.org](https://www.nuget.org/packages?q=AIGuiders.Platform).
2. **Keep** your repo, UI, domain model, and release cadence.
3. **Wire** a native adapter (catalog source, registry host, surface).
4. **Ship** conformance at your pace; no merge into a monorepo, no runtime lock-in.

Federation sells **roads and signage**, not citizenship in a single app.

**Friction log:** [GUIDERS pain inventory](GUIDERS-pain-inventory.md) — why hyperlanes exist (notation stitch, agent ingress, «инженер заебался» tier).  
**Roadmap:** [GUIDERS-ROADMAP.md](GUIDERS-ROADMAP.md) — living backlog across waves.  
**Fleet thesis:** [PLATFORM-FLEET-THESIS.md](PLATFORM-FLEET-THESIS.md) — AI Era stack: Aviation + Agent Env + CASE (draft).

### AI Era stack — three pillars, neighbors not war

The product center is **not** one planet eating others. Three traditions compose the candy AI Era needs; each has a **home planet or hyperlane**, wired by protocol — not merger:

| Pillar | What it gives | Question answered | Home |
|--------|---------------|-------------------|------|
| **Aviation** | CRM **PM/PF** pair + **incident investigation** (layers, Just Culture) | *Who flies/monitors; what system layers aligned?* | Platform `Cockpit.*` + CIDE/Glass (ADR-0007) |
| **Agent Env** | Agent habitat — memory, gates, packs, journal; mutate habitat outside raw chat context | *Where does the agent live and remember?* | **CDP** (`cdp-mcp`) — sovereign planet |
| **CASE** | SE **Vision / Decision Environment** — estate topology, multi-view workbench, trace, review-before-commit | *What are we steering, and what changes if we ship?* | Platform workbench + conformance (ADR-0023) |

```text
     Human + agent pair (Aviation)     Vision / Decision (CASE)
              \                            /
               \   hyperlanes (platform)   /
                \   MCPlane · Cockpit ·   /
                 \  Notations · vectors  /
                  \                      /
                   Agent Env (CDP) — memory & habitat
```

**Neighbors collaborate:** Platform ships **roads** (neutral IR, conformance, MCPlane tiers). CDP dogfoods Agent Env and **pins** platform packages — its Citizen/buffer wire stays on CDP. CIDE/Glass dogfood **human embassy** (aviation attention over CASE estate views). Forge, ANPM, third parties adopt hyperlanes at their pace. **Informative ≠ normative** (see § Planets are not federation SSOT). Compete on product; integrate on protocol.

---

**Already normative** in [GUIDERS-ADR-0006 §2](adr/GUIDERS-ADR-0006-confederation-charter.md) (*Native per ecosystem*), [GUIDERS-ADR-0016 non-goals](adr/GUIDERS-ADR-0016-input-notation-quarry-family.md), [GUIDERS-ADR-0015 non-goals](adr/GUIDERS-ADR-0015-invocation-mechanics-slash-melody-binding.md) — promoted here so Constitution readers see it without ADR archaeology.

| Layer | Federation ships | Planet implements **natively** |
|-------|------------------|--------------------------------|
| **Contract** | IR, schemas, `commandId`, catalog shape | — |
| **Signage** | `aiguiders-conformance` repo (`*.spec.json`, schemas, RULES); npm `@aiguiders/conformance` — [ADR-0019](adr/GUIDERS-ADR-0019-conformance-hyperlane-monorepo.md); bootstrap copy in `docs/conformance/` | — |
| **Reference quarry** | `.NET` packages (`Notations.*`, `InputNotation.*` legacy alias, `CommandPlane.*`) — **embassy, not gate** | may pin NuGet as-is |
| **Wire → IR** | spec + reference parser | **port** to TS (VS Code), Kotlin (JetBrains), PHP, … |
| **IR → input** | — | key match, `KeyBinding`, `keydown`, OS shortcuts |
| **Surface** | — | WPF, Avalonia, Blazor, extension host |

**Rule:** Platform does **not** ship a universal binding runtime. `CommandPlane.Binding` (when it exists) = headless **catalog** (gesture → `commandId`, merge ship/user) — not `IKeyListener` for every UI stack.

**Join pattern:** adopt schema/spec → implement native adapter → pass conformance. VS Code takes a JS port; JetBrains takes Kotlin; PHP takes PHP. Same IR, same vectors, different atmosphere.

---

## Confederation model

| Concept | Meaning |
|---------|---------|
| **Planet** | Sovereign repo — Forge, CIDE, DashSpec, your SaaS, your game, your IDE |
| **Federation** | Cross-repo contracts, semver packages, ADR signage |
| **Hyperlane** | Versioned protocol you can depend on (NuGet, schema, MCP surface) |
| **Embassy** | Reference consumer that proves the lane works (Forge is embassy, not capital) |
| **Prime protocol** | Do not break a planet's domain for integrator convenience |

**Rejected:** one city, one empire, one repo to rule them all. See [GUIDERS-ADR-0006 §1](adr/GUIDERS-ADR-0006-confederation-charter.md).

### Planets are not federation SSOT

Any **planet** (Forge, CIDE, DashSpec, CDP, Glass, …) may ship **early** or **experimental** behavior. That does **not** make its wire, tools, or ADRs normative for federation.

| | Federation | Planet (e.g. CDP) |
|---|------------|-------------------|
| **Normative** | IR, schemas, conformance vectors, neutral tier names | — |
| **Informative** | — | lived dogfood, product ADRs, tool docs |
| **Embassy** | reference .NET quarry (optional pin) | may implement first; others port vectors |

**CDP** (`cdp-mcp`) is a **sovereign experimental habitat** — buffer plane, Citizen, Meta tools stay on the planet. Federation cites CDP only as *informative* precedent, never as gate. Prefer **Forge** (open embed) and **CIDE** (slash/melody) as embassy examples in new ADRs.

---

## What the federation owns vs what planets own

| | Federation (platform) | Planet (product) |
|---|------------------------|------------------|
| **Mechanics** | resolve, merge, index, routing envelopes, ArgTail policy | — |
| **Catalog content** | — | TOML, JSON, DB, embedded resources, capabilities |
| **Execution** | GoF command contracts | registry host, `IPlatformCommand<TContext>`, MCP/kernel |
| **Surface** | — | Blazor, WPF, JS, TUI, agent wire |
| **Context** | `ICommandContext` shape | product payload (`ForgeCommandContext`, …) |

Pattern stack for commands: [GUIDERS-ADR-0009 — Catalog · Registry · Command · Surface](adr/GUIDERS-ADR-0009-command-surface-pattern.md).

### Invocation glossary (Slash · Melody · Binding)

Keyboard-as-instrument canon ([GUIDERS-ADR-0015](adr/GUIDERS-ADR-0015-invocation-mechanics-slash-melody-binding.md)):

| Term | Meaning | Not |
|------|---------|-----|
| **Chord** | Simultaneous keys, or **chord root** gesture (e.g. `<Ctrl+K>`) | — |
| **Melody** | Sequential line after root — e.g. `<Ctrl+K>` `b` `s` (+ optional tail) | palette text mode |
| **Articulation** | Per-step play style inside a melody — **ByNote** (single key) or **ByChord** (gesture as one step) | not a fourth mechanic |
| **Profile** | Line policy — **PureByNote** (default), **PureByChord**, or **Mixed** (explicit hybrid) | not a step type |
| **Input notation** | Quarry: Vim + KeyGesture → `NormalizedKeySequence` (`InputNotationParser`; target **`Notations.Keyboard.*`** — [ADR-0021](adr/GUIDERS-ADR-0021-notations-quarry-family.md)) | UI render / key match stay product |
| **Command / argument notation** | Quarry: slash + console wire → `NormalizedCommandLine` + `NormalizedArgTail` (`Notations.Command.*`, `Notations.Argument.*` — [ADR-0021](adr/GUIDERS-ADR-0021-notations-quarry-family.md)) | path resolve / execute stay CommandPlane |
| **Binding** | Direct hotkey → `commandId` or surface opener (`Ctrl+Q`, chord root assignment) | slug/tail parser |
| **`c:`** | Palette **discoverability** prefix (Ctrl+Q) — browse melody catalog, Help | **not** the melody mechanic |
| **InvocationEngage** | Planet cue before mechanics — **Sigil** (text), **DiscoverabilityPrefix** (`c:`), **ChordRoot** (gesture) | not a platform Core type ([ADR-0036](adr/GUIDERS-ADR-0036-invocation-engage-glossary.md)) |

One `commandId`; mechanics are how you **play** it; `c:` is the **score on the wall**. Engage is how the planet **arms** the lane; platform resolve starts after strip/peel/tunnel.

---

## Hyperlanes (living protocols)

| Hyperlane | Package / home | Role | Reference mission |
|-----------|----------------|------|-------------------|
| **Command plane** | `CommandPlane` (core) · `.Slash` · `.Sources` · `.Melody` · `.Binding` · `.Binding.Sources` | Registry hub; slash/melody/binding mechanics à la carte (ADR-0015, ADR-0017) | Forge, CIDE, DashSpec |
| **Agent ingress (MCPlane)** | `Abstractions` today → `MCPlane` (draft) | Pulse/default, `next[]`, detail tiers, catalog projection — **not** MCP wire ([ADR-0020](adr/GUIDERS-ADR-0020-mcplane-agent-ingress.md)) | Forge MCP (embassy), any agent host |
| **Notations** | `Notations` (core) · `.Quarry` · `.Keyboard.*` · `.Command.*` · `.Argument.*` · `.All` — supersedes `InputNotation.*` naming ([ADR-0021](adr/GUIDERS-ADR-0021-notations-quarry-family.md)); `InputNotation.*` ships until alias wave | Wire → IR quarry; keyboard + command-line + arg tails; native port per stack | CIDE, Forge slash JS port |
| **Input notation** (legacy label) | `InputNotation.*` → migrate to `Notations.Keyboard.*` ([ADR-0016](adr/GUIDERS-ADR-0016-input-notation-quarry-family.md)); alias metapackages ship in v0.12+ | Same keyboard IR; CIDE pin migration pending | CIDE (.NET) |
| **Intent / routing** | `AIGuiders.Platform.Routing`, `Abstractions` | Intent organ **contracts** (neutral) | planets that embed organs |
| **Cockpit** | `AIGuiders.Platform.Cockpit.*` | Channels, DataBus, CCU contracts | CIDE, Glass (CDP may pin; not SSOT) |
| **UI semantics** | `guiders-ui-platform` (`AIGuiders.UI.*`) | Tokens, Agent AX, adapters | Forge View (embassy) |
| **Plugin transport** | `guiders-plugin-host` | ALC, staging, manifest | Forge plugins |

New planets add rows. Federation does not absorb the planet.

**First open embed proof (2026-08):** CommandPlane on NuGet — catalog from code, JSON/TOML/XML, DB delegate, embedded `commands.toml`, registry visitor; products merge sources into one `SlashCatalogIndex`. See [GUIDERS-ADR-0013](adr/GUIDERS-ADR-0013-command-catalog-sources.md), [GUIDERS-ADR-0014](adr/GUIDERS-ADR-0014-registry-catalog-visitor.md).

Minimal third-party embed:

```csharp
var catalog = SlashCatalogComposer.Build(
    CommandSources.FromFile("commands.toml"),
    RegistryCatalogBuilder.ToCommandSource(myRegistry));
```

Your execute endpoint, your UI — federation does not own the wire.

---

## How to join (voluntary, testable)

1. **Adopt** the relevant hyperlane package or schema.
2. **Implement** a native adapter in your stack (no mandatory UI framework).
3. **Pass** conformance — contract tests, journey smoke, semver pin (checklist: [GUIDERS-ADR-0006 vNext](adr/GUIDERS-ADR-0006-confederation-charter.md)).
4. **Document** wiring in your product ADR; link platform ADRs, do not fork semantics silently.

No requirement to use DOI paths, Forge, or our product line. Flat paths and your own `commandId` space are valid.

### Adoption alliances (real, not decorative)

MIT (and peers) enable take-without-return **legally** — they do not create **cooperation** by themselves. Fake alliances: logo slides, «partner ecosystem», no shared spec or issue path. **Real alliance** = explicit pact on a hyperlane:

| Pact element | What it means |
|--------------|----------------|
| **What** | Named hyperlane + semver spec tag (e.g. `slash-arg-completion`, `notation/command-slash`) |
| **Who adopts** | Which planet pins quarry vs ports vectors natively |
| **How to contribute** | Issues, bugs, PRs to spec or reference quarry — expected, not guaranteed SDLC in one throat |
| **No warranty (read honestly)** | Shield from **litigation** and impossible solo-SDLC at scale — **not** «we ignore you»; alliance = shared maintenance **intent** without enterprise indemnity in LICENSE |
| **Conformance** | Testable join — pass vectors, document adapter ADR |

Federation is structured for this: confederation charter, embassy planets (Forge, CIDE), conformance repo — **alliance mechanics**, not a single vendor support desk. Planets may add bilateral pacts (e.g. Forge + DashSpec on CommandPlane); platform docs record **who** ships which port.

**Automation (v1):** [`AdoptionReport`](../tools/AdoptionReport) scans `docs/adoption/planets.json` + NuGet pins → [`ADOPTION-ALLIANCE.generated.md`](../ADOPTION-ALLIANCE.generated.md). See [GUIDERS-ADR-0022](adr/GUIDERS-ADR-0022-utilities-adoption-report.md).

See [GUIDERS pain inventory — G-008](GUIDERS-pain-inventory.md#g-008).

---

## Prime protocol (non-negotiable)

**Forbidden**

- Lifting product domain into platform «because it's used twice» without a Core extraction ADR.
- One planet's runtime as the only gateway to federation contracts.
- «Easy integrate» README without version pin, contract, and a reference route.

**Required**

- Semver on public contracts; breaking change = major + migration note.
- ADR for new hyperlane or moved boundary.
- Second consumer before calling a contract **stable** (UI platform vNext).

Full list: [GUIDERS-ADR-0006 §5](adr/GUIDERS-ADR-0006-confederation-charter.md).

---

## Open Core

Conformance is not paywalled. MIT packages, public ADRs, fork adapters — not fork contract semantics in secret. Ethical use: [AI Guiders licensing](https://github.com/AI-Guiders/licensing/blob/main/docs/ethical-use.md).

---

## Signage — where to read next

| Start here | Doc |
|------------|-----|
| **Architecture hub (full map)** | [EN](GUIDERS-PLATFORM-ARCHITECTURE-HUB.md) · [EN DOCX](GUIDERS-PLATFORM-ARCHITECTURE-HUB.docx) · [RU](GUIDERS-PLATFORM-ARCHITECTURE-HUB.ru.md) · [RU DOCX](GUIDERS-PLATFORM-ARCHITECTURE-HUB.ru.docx) |
| Platform boundary | [GUIDERS-ADR-0001](adr/GUIDERS-ADR-0001-platform-boundary.md) |
| Confederation charter (normative) | [GUIDERS-ADR-0006](adr/GUIDERS-ADR-0006-confederation-charter.md) |
| Platform mechanics quarry | [GUIDERS-ADR-0010](adr/GUIDERS-ADR-0010-platform-mechanics.md) |
| Command plane quarry | [GUIDERS-ADR-0003](adr/GUIDERS-ADR-0003-platform-ssot-quarry.md) |
| Invocation mechanics (Slash · Melody · Binding) | [GUIDERS-ADR-0015](adr/GUIDERS-ADR-0015-invocation-mechanics-slash-melody-binding.md) |
| Binding catalog family | [GUIDERS-ADR-0017](adr/GUIDERS-ADR-0017-binding-catalog-family.md) |
| Input notation quarry + native ports | [GUIDERS-ADR-0016](adr/GUIDERS-ADR-0016-input-notation-quarry-family.md) |
| Slash conformance vectors | [GUIDERS-ADR-0018](adr/GUIDERS-ADR-0018-slash-conformance-vectors.md) |
| Conformance sibling monorepo | [GUIDERS-ADR-0019](adr/GUIDERS-ADR-0019-conformance-hyperlane-monorepo.md) |
| MCPlane (agent ingress draft) | [GUIDERS-ADR-0020](adr/GUIDERS-ADR-0020-mcplane-agent-ingress.md) |
| Aviation mental model | [GUIDERS-ADR-0007](adr/GUIDERS-ADR-0007-aviation-mental-model.md) |
| Platform workbench (CASE heritage, draft) | [GUIDERS-ADR-0023](adr/GUIDERS-ADR-0023-case-workbench-heritage.md) · [fleet thesis](PLATFORM-FLEET-THESIS.md) |
| All ADRs | [docs/adr/](adr/) |

Planet-level constitutions (local law) remain in each repo — e.g. CIDE [ADR 0100](https://github.com/AI-Guiders/cascade-ide/blob/develop/docs/adr/0100-project-constitution.md).

---

## What this document is not

- Not a replacement for [GUIDERS-ADR-0006](adr/GUIDERS-ADR-0006-confederation-charter.md) — that ADR stays the charter.
- Not governance of trademarks or org politics.
- Not a promise to unify all OSS — scope is **Guiders federation protocols** only.

---

*Unified planets: one mechanics layer, many atmospheres. Welcome to all.*
