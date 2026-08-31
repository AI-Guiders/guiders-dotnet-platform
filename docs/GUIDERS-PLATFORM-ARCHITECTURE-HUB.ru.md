# Guiders Platform — архитектурный hub

| | |
|---|---|
| **Статус** | Живой hub (синтез ADR; нормативная детализация — в связанных ADR) |
| **Версия** | Пакеты платформы **v0.30.0** |
| **Дата** | 2026-08-31 |
| **Аудитория** | Интеграторы, мейнтейнеры планет, союзники федерации |
| **Форматы** | Этот файл (MD) · [DOCX](./GUIDERS-PLATFORM-ARCHITECTURE-HUB.ru.docx) · [English](./GUIDERS-PLATFORM-ARCHITECTURE-HUB.md) |

**См. также:** [Конституция федерации](./GUIDERS-FEDERATION-CONSTITUTION.md) (зачем и как присоединиться) · [GUIDERS-ADR-0006](./adr/GUIDERS-ADR-0006-confederation-charter.md) (нормативный charter) · [README](../README.md) (сборка и NuGet)

---

## Содержание

1. [Краткое резюме](#1-краткое-резюме)
2. [Принципы федерации](#2-принципы-федерации)
3. [Модель конфедерации](#3-модель-конфедерации)
4. [Стек AI Era — три опоры](#4-стек-ai-era--три-опоры)
5. [Граница платформы](#5-граница-платформы)
6. [Архитектурные слои](#6-архитектурные-слои)
7. [Семейства пакетов и возможности](#7-семейства-пакетов-и-возможности)
8. [Command plane — углублённо](#8-command-plane--углублённо)
9. [Механики invocation](#9-механики-invocation)
10. [Discoverability — Visual Command Tree](#10-discoverability--visual-command-tree)
11. [Agent ingress — MCPlane](#11-agent-ingress--mcplane)
12. [Cockpit и routing](#12-cockpit-и-routing)
13. [Вспомогательные семейства](#13-вспомогательные-семейства)
14. [Conformance hyperlane](#14-conformance-hyperlane)
15. [Quarry waves и roadmap](#15-quarry-waves-и-roadmap)
16. [Как присоединиться к федерации](#16-как-присоединиться-к-федерации)
17. [Reference missions и потребители](#17-reference-missions-и-потребители)
18. [Индекс ADR](#18-индекс-adr)
19. [Глоссарий](#19-глоссарий)

---

## 1. Краткое резюме

**Guiders Platform** (`guiders-platform`) — **headless-слой механики** федерации: тестируемые NuGet-пакеты (`AIGuiders.Platform.*`), которые унифицируют семантику команд, notation IR, контракты cockpit, инструменты documentation guild, navigation-сцены и agent response envelopes — **без** владения product UI, доменными моделями и MCP wire.

**Одной фразой (федерация):** *Суверенные product-репозитории, общие протоколы на NuGet — интеграция без аннексии, embed без вступления в нашу продуктовую линейку.*

Платформа отвечает на вопросы:

| Вопрос | Владелец |
|--------|----------|
| Какая команда существует? Как резолвить slash path? | **CommandPlane** |
| Как распарсить keyboard/command wire в IR? | **Notations** |
| Как смержить слоистые каталоги? | **Combinations** + **Sources** |
| Что агент видит обратно (pulse, next)? | **MCPlane** |
| Какова форма channel snapshot? | **Cockpit.*** |
| Как ограниченно навигировать репо для агентов? | **Navigation** |
| Как связать docs ↔ code? | **Documentation.Correspondence** |

Продукты (**планеты**) отвечают за: UI, execution hosts, **содержимое** каталогов, доменную логику, release cadence.

---

## 2. Принципы федерации

Нормативно зафиксированы в [GUIDERS-ADR-0006](./adr/GUIDERS-ADR-0006-confederation-charter.md) и развёрнуты в [Конституции федерации](./GUIDERS-FEDERATION-CONSTITUTION.md).

### 2.1 Prime protocol (не обсуждается)

**Запрещено**

- Поднимать product domain в platform «потому что используется дважды» без ADR на Core extraction.
- Делать runtime одной планеты единственным шлюзом к контрактам федерации.
- README «легко интегрируйтесь» без pin версии, контракта и reference route.
- Identity overwrite — один продукт претендует на универсальный SSOT для всех.

**Обязательно**

- Semver на публичных контрактах; breaking change = major + migration note.
- ADR на новый hyperlane или сдвинутую границу.
- Второй consumer перед объявлением контракта **stable**.

### 2.2 Правила суверенитета

1. **Продукты сохраняют репозитории** — без аннексии в monorepo.
2. **Домен остаётся на планете** — Forge IOP, CDP buffer plane, Glass projection, DashSpec dashboards.
3. **Общий kit — протокол, не колония** — пакеты как federated contracts с независимым CI.
4. **Native per ecosystem** — TS, Kotlin, PHP портируют тот же IR; .NET quarry — embassy, не gate.

### 2.3 Open Core

Conformance не за paywall. MIT-пакеты, публичные ADR. Форкай адаптеры — не форкай семантику контрактов втихую.

### 2.4 Планеты — не SSOT федерации

Экспериментальное поведение на планете (например, CDP Citizen tools) — **informative**, не normative. Федерация цитирует IR, schemas, conformance vectors — не product-specific wire.

### 2.5 Adoption alliances (настоящие, не декоративные)

**Настоящий alliance** = явный pact на hyperlane:

| Элемент | Смысл |
|---------|-------|
| **What** | Именованный hyperlane + semver spec tag |
| **Who adopts** | Какая планета pin'ит quarry vs портирует vectors natively |
| **How to contribute** | Issues/PRs в spec или reference quarry |
| **Conformance** | Тестируемое присоединение — pass vectors, adapter ADR |

Автоматизация: [`AdoptionReport`](../tools/AdoptionReport) → [`ADOPTION-ALLIANCE.generated.md`](./ADOPTION-ALLIANCE.generated.md).

---

## 3. Модель конфедерации

```
                    ┌─────────────────────────────────────────┐
                    │         AI Guiders Federation            │
                    │  protocols · ADR signage · conformance   │
                    └─────────────────────────────────────────┘
           NuGet / schema / MCP                    hyperlanes
    ┌──────────┬──────────┬──────────┬──────────┬──────────┐
    │ Platform │ UI Core  │  Core    │ Plugin   │  Notes   │
    │ (здесь)  │          │  organs  │  Host    │  (KB)    │
    └────┬─────┴────┬─────┴────┬─────┴────┬─────┴────┬─────┘
         │          │          │          │          │
    ┌────┴────┐ ┌───┴───┐ ┌────┴────┐ ┌───┴───┐      │
    │  Forge  │ │ CIDE  │ │  CDP    │ │ Glass │  ... │
    │ embassy │ │quarry │ │ habitat │ │ proj  │      │
    └─────────┘ └───────┘ └─────────┘ └───────┘      │
         │          │          │                      │
    DashSpec ───────┴──────────┴── (CommandPlane pin) ─┘
```

| Понятие | Смысл |
|---------|-------|
| **Planet** | Суверенный repo — Forge, CIDE, DashSpec, CDP, твой SaaS |
| **Federation** | Cross-repo контракты, semver-пакеты, ADR signage |
| **Hyperlane** | Версионированный протокол (NuGet, schema, MCP surface) |
| **Embassy** | Reference consumer, доказывающий lane (Forge — не capital) |
| **Signage** | ADR, conformance specs, stable test ids |
| **Prime protocol** | Не ломать domain планеты ради удобства интегратора |

**Отвергнутые метафоры:** один город, империя, monolith.

---

## 4. Стек AI Era — три опоры

Центр продукта — **не** одна планета, поглощающая остальные. Три традиции составляют стек; у каждой свой home, связь через протокол:

| Опора | Вопрос | Home |
|-------|--------|------|
| **Aviation** | Кто летит/мониторит; какие слои системы выровнены? | Platform `Cockpit.*` + CIDE/Glass ([ADR-0007](./adr/GUIDERS-ADR-0007-aviation-mental-model.md)) |
| **Agent Env** | Где агент живёт и помнит? | **CDP** (`cdp-mcp`) — суверенная планета |
| **CASE** | Чем управляем; что изменится при ship? | Platform workbench + conformance ([ADR-0023](./adr/GUIDERS-ADR-0023-case-workbench-heritage.md)) |

Platform шипит **дороги** (neutral IR, MCPlane tiers, conformance). Планеты dogfood'ят и pin'ят пакеты в своём темпе.

---

## 5. Граница платформы

По [GUIDERS-ADR-0001](./adr/GUIDERS-ADR-0001-platform-boundary.md):

| Слой | Platform | Продукты |
|------|----------|----------|
| **Contracts** | interfaces, DTOs, event records | — |
| **Mechanics** | resolver, merge, fold, routing envelopes | — |
| **Catalog content** | — | TOML, JSON, DB, Forge plugins |
| **UI / host** | — | Blazor, WPF, JS, MCP wire |
| **Execute shape** | `CommandOutcome`, registry contracts | handlers, HTTP, MCP kernel |

**Правило зависимостей:** никаких WPF/Avalonia refs в platform. Monolith `AIGuiders.Platform.Cockpit` 0.1.0 deprecated — используй split `Cockpit.*`.

**Вне scope platform:** домен DashSpec, Avalonia UI forks, Citizen organ handlers (остаются в cdp-mcp).

---

## 6. Архитектурные слои

### 6.1 Две ingress-плоскости

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

| Плоскость | Вопрос | Ключевые пакеты |
|-----------|--------|-----------------|
| **CommandPlane** | Какая команда? Как резолвить path? | `CommandPlane.*`, `Notations.*` |
| **MCPlane** | Что агент видит обратно? | `MCPlane`, `Abstractions` |
| **Cockpit** | Форма channel snapshot? | `Cockpit.DataBus`, `Channels` |
| **Routing** | Куда dispatch intent? | `Routing` |

### 6.2 Dependency cake (внутри platform)

```
                    ┌─ Surfaces (планеты) ─┐
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

## 7. Семейства пакетов и возможности

**87 проектов** в `src/` · публикуются как `AIGuiders.Platform.*` на [nuget.org](https://www.nuget.org/packages?q=AIGuiders.Platform).

### 7.1 Foundation

| Пакет | Возможности |
|-------|-------------|
| `Abstractions` | `IntentOutcome`, `RoutedIntent`, `PulseFormat` (truncate ~240 символов по умолчанию) |
| `Routing` | `IIntentOrgan<TRoute,TOutcome>`, `DispatchCallOverride`, route refusal helpers |

### 7.2 Sources & Combinations

| Пакет | Возможности |
|-------|-------------|
| `Sources` | Generic `ISource<T>` transport abstraction |
| `Sources.File` / `.Toml` | File + TOML transport |
| `Catalog` | `CatalogIndex<TKey,TEntry>`, `ICatalogProfile`, merge policies ([ADR-0041](./adr/GUIDERS-ADR-0041-catalog-kernel-profiles.md)) |
| `Combinations` | `Combinator<T>`, `OrderedCombination.Fold`, `CombinationSemantics` |
| `Combinations.Workspace` | `FieldOverlay` — overlay non-null wins |
| `Combinations.Catalog` | Meta → `ShipFirst` merge в CommandPlane.Catalog |
| `Combinations.Binding` | Meta → `OverlayWins` merge в CommandPlane.Binding |
| `Combinations.All` | Meta-bundle |

**Operator rule:** **Sources** = только transport; **Combinations** = ordered fold + named policies ([ADR-0030](./adr/GUIDERS-ADR-0030-combinations-family.md)).

### 7.3 IntermediateRepresentation (neutral IR)

| Пакет | Возможности |
|-------|-------------|
| `IntermediateRepresentation.Argument` | `ArgumentNotationProfile`, slots, `NormalizedArguments` |
| `IntermediateRepresentation.Keyboard` | `NormalizedKeySequence` + step records |
| `IntermediateRepresentation.Invocation` | `NormalizedCommandLine` |
| `IntermediateRepresentation.Bracket` | Bracket wire IR ([ADR-0026](./adr/GUIDERS-ADR-0026-notations-bracket-branch.md)) |
| `IntermediateRepresentation.Command` | Command catalog descriptors + route rows |
| `IntermediateRepresentation.Binding` | Binding descriptors + entries |
| `IntermediateRepresentation.Melody` | Melody descriptor + line/step IR |
| `IntermediateRepresentation.Agent` | Agent envelope: `DetailTier`, `NextHint`, `AgentResponseEnvelope` |
| `IntermediateRepresentation.Language` | `Locus`, `TextEdit`, `BracketAnchorSpan`, `SniperScope`, … |

См. [ADR-0042](./adr/GUIDERS-ADR-0042-intermediate-representation-family.md). **Notations** парсят wire → IR; **CommandPlane** guilds — механики.

### 7.4 Notations (wire → IR parsers)

| Пакет | Возможности |
|-------|-------------|
| `Notations` | Shared primitives (`NotationKvPair`, list split) |
| `Notations.Keyboard.*` | `IKeyboardNotationReader` — KeyGesture, Vim, Neovim, Emacs wires |
| `Notations.Command.*` | Slash/console body tokenize, `InvocationNotation` helpers |
| `Notations.Argument.*` | Kv, positional, CLI flags, delimited parsers (`ArgumentNotation.All`) |
| `Notations.Bracket` | `BracketReader`, `IBracketNotationReader` |
| `InputNotation.*` | **Legacy alias** → `Notations.Keyboard.*` (obsolete forwards) |

Platform шипит **reference quarry** (.NET parsers). Планеты **портят vectors** в native stacks (Forge JS, VS Code extension и т.д.).

### 7.5 CommandPlane

| Пакет | Возможности |
|-------|-------------|
| `CommandPlane` | GoF `IPlatformCommand<T>`, `PlatformCommandRegistry`, `ICommandContext` (hub) |
| `CommandPlane.Catalog` | Catalog index, sources, merge facade — IR в `IntermediateRepresentation.Command` ([ADR-0039](./adr/GUIDERS-ADR-0039-command-catalog-family.md), [ADR-0041](./adr/GUIDERS-ADR-0041-catalog-kernel-profiles.md), [ADR-0042](./adr/GUIDERS-ADR-0042-intermediate-representation-family.md)) |
| `CommandPlane.ArgSuggestions` | Federated arg suggestion broker + planet provider registry |
| `CommandPlane.Constructors` | Value constructor registry, session, navigator, locale input ([ADR-0035](./adr/GUIDERS-ADR-0035-slash-value-constructors.md)) |
| `CommandPlane.PrefixArmed` | PAC profiles + coordinator — кросс-поверхностная механика ([ADR-0038](./adr/GUIDERS-ADR-0038-prefix-armed-completion.md)) |
| `CommandPlane.PrefixArmed.Locale` | Опциональный locale date PAC profile ([ADR-0037](./adr/GUIDERS-ADR-0037-slash-locale-typed-value-input.md)) |
| `CommandPlane.Slash` | `SlashLineResolver`, completion, ArgTail, slash guidance projector (consumes core catalog) |
| `CommandPlane.Melody` | Melody descriptors, line profile, policy, chord tree projection |
| `CommandPlane.Binding` | Hotkey catalog, gesture normalize, layered merge |
| `CommandPlane.Catalog.Sources.*` | Json, Toml, Xml, File, Database transports → Core |
| `CommandPlane.Catalog.Sources` | Meta-bundle всех форматов |

### 7.5 Cockpit

| Пакет | Возможности |
|-------|-------------|
| `Cockpit.Abstractions` | CCU, DAL, channel, CDS, compositor contracts |
| `Cockpit.DataBus` | `IDataBus`, build/test/debug/git/ide events |
| `Cockpit.Channels` | IdeHealth/EnvReady DTOs + CCU kits |
| `Cockpit.Cds` / `Composition` | Routing/compositor DTOs |
| `Cockpit.Transport` | `IngressEvent`, `BoundedIngressBus` |
| `Cockpit.Ids` | IDS overlay search seam |

Glass WPF = **projection** snapshots; не владеет CCU mechanics.

### 7.6 Documentation guild

| Пакет | Возможности |
|-------|-------------|
| `Documentation.Anchors` | Family:doc wire resolve |
| `Documentation.LinkCheck` | md dry-resolve (`--check`) |
| `Documentation.LinkMutate` | Structured axis patch (`--apply-rename`) |
| `Documentation.Reports` | Generated vocabulary tables |
| `Documentation.Correspondence.*` | Forward ADR map + reverse md scan ([ADR-0028](./adr/GUIDERS-ADR-0028-documentation-guild-correspondence-family.md)) |

### 7.7 Navigation

| Пакет | Возможности |
|-------|-------------|
| `Navigation` | `navigation_scene/v1` — nodes, edges, caps |
| `Navigation.Policy` | Presets, kind filters, profile caps |
| `Navigation.Code` | Roslyn wire parser + scene builder ([ADR-0033](./adr/GUIDERS-ADR-0033-navigation-family-semantic-scenes.md)) |

Hosts (CDP SemanticMap, CIDE Skia) = projectors, не SSOT.

### 7.8 Language intelligence

| Пакет | Возможности |
|-------|-------------|
| `LanguageIntelligence` | Anchor/Locus/TextEdit IR, resolve tiers |
| `LanguageIntelligence.Adapters.Roslyn` | Roslyn adapter |
| `Language.CSharp.*` / `Language.Xml.Anchors` | Symbol/anchor wires |

### 7.9 Configurations

| Пакет | Возможности |
|-------|-------------|
| `Configurations.Project` / `.Workspace` | Layered config compose |
| `Configurations.*.Sources` | Source transports для config layers |

### 7.10 Conformance & utilities

| Пакет | Возможности |
|-------|-------------|
| `Conformance.Schemas` / `.Policies` / `.Navigation` | Obligation specs, policy-as-code |
| `MCPlane` | Agent response envelope, detail tiers, `next[]` hints |
| `Utilities.Adoption` | Planet pin scanner → alliance report |

---

## 8. Command plane — углублённо

Паттерн: [GUIDERS-ADR-0009 — Catalog · Registry · Command · Surface](./adr/GUIDERS-ADR-0009-command-surface-pattern.md).

```
  Catalog                    Registry                 Command
  (discoverability)          (executor lookup)        (один эффект)
       │                          │                        │
  CommandDescriptor    PlatformCommandRegistry   IPlatformCommand
  CommandCatalogIndex         EditorCommandRegistry     PlatformCommand
  capabilities.commands[]   Forge CommandCatalog
       │                          │                        │
       └──────── path / id ───────┴──── commandId ────────┘
                                    ▲
              Surfaces: slash · CCL · palette · hotkey · MCP
```

| Паттерн | Вопрос | Platform SSOT | НЕ делает |
|---------|--------|---------------|-----------|
| **Catalog** | Что видит пользователь; как найти по path? | Index, resolver, visitor | Execute, buffer edits |
| **Registry** | По `commandId` — какой executor? | `PlatformCommandRegistry<T>` | Autocomplete UI |
| **Command** | Один эффект — один `Execute` | `IPlatformCommand<T>` | Parse slash string |
| **Surface** | Откуда человек вызвал? | — (product) | Собственную business logic |

**Wire rule:** catalog entry несёт `CommandId` → surface резолвит path (catalog) → registry → command.

### 8.1 Catalog merge flow

```
Forge capabilities overlay ──┐
CIDE intent-catalog.toml ──┼──► CommandCatalogIndex.Merge ──► SlashLineResolver
DashSpec embedded TOML ────┤                                      │
Product DB delegate ───────┘                                      ▼
                                                          completion + guidance
```

Merge policies ([ADR-0030](./adr/GUIDERS-ADR-0030-combinations-family.md)):

| Домен | Policy | Правило коллизий |
|-------|--------|------------------|
| Slash catalog | ShipFirst | TryAdd — ship wins |
| Binding catalog | OverlayWins | overlay перезаписывает key |
| Workspace fields | FieldOverlay | overlay non-null wins |

### 8.2 Минимальный third-party embed

```csharp
var catalog = CommandCatalogComposer.Build(
    CommandSources.FromFile("commands.toml"),
    RegistryCatalogBuilder.ToCommandSource(myRegistry));
// Твой execute endpoint, твой UI — federation не владеет wire.
```

### 8.3 Режимы arg completion

По [ADR-0012](./adr/GUIDERS-ADR-0012-arg-picker-completion.md) и [ADR-0035](./adr/GUIDERS-ADR-0035-slash-value-constructors.md):

| Mode | Когда | UX |
|------|-------|-----|
| `Picker` | Closed enum / preset values | Tab → wire token |
| `Constructor` | Structured typed values (date, range) | Guided segment tree |
| `FreeText` | Escape hatch | Ввод wire по `ArgHint` |
| `Ready` | Строка complete | Execute |

**Value constructors** образуют **composite tree** — Range → Date(from) → Year/Month/Day. Free text всегда доступен как sibling.

---

## 9. Механики invocation

Три distinct input mechanics ([ADR-0015](./adr/GUIDERS-ADR-0015-invocation-mechanics-slash-melody-binding.md)):

| Механика | Ввод пользователя | Platform package |
|----------|-------------------|------------------|
| **Slash** | `/docs adr open` | `CommandPlane.Slash` |
| **Melody** | `<Ctrl+K>` `b` `s` (+ optional tail) | `CommandPlane.Melody` |
| **Binding** | `Ctrl+Q` → `commandId` | `CommandPlane.Binding` |

**Не четвёртая механика:** palette prefix **`c:`** = discoverability peel (browse melody catalog) — не melody execution.

### 9.1 Музыкальная метафора

| Музыка | Invocation |
|--------|------------|
| **Note** | Одна клавиша после chord root |
| **Chord** | Одновременные клавиши или chord root gesture |
| **Melody** | Последовательная линия после root |
| **Articulation** | ByNote vs ByChord на шаг |
| **Score on the wall** | `c:` в palette |

Один `commandId`; mechanics — как ты **играешь** команду.

### 9.2 InvocationEngage glossary

Planet cues до mechanics ([ADR-0036](./adr/GUIDERS-ADR-0036-invocation-engage-glossary.md)):

| Термин | Смысл |
|--------|-------|
| **Sigil** | Text engage cue |
| **DiscoverabilityPrefix** | `c:` в palette |
| **ChordRoot** | Gesture, который arms melody lane |

Platform resolve начинается после strip/peel/tunnel — engage не Core type.

---

## 10. Discoverability — Visual Command Tree

[GUIDERS-ADR-0024](./adr/GUIDERS-ADR-0024-visual-command-tree-capture-stack.md) обобщает melody **Visual Chord Tree** на все engages.

| Engage | Capture state | Projector |
|--------|---------------|-----------|
| Melody chord | `MelodyCaptureStack` | `MelodyChordTreeProjector` |
| Slash / CCL | typed line + mode | `SlashVisualCommandTreeProjector` |
| Constructor | `SlashConstructorSession` | slash projector (`EngageKind = Constructor`) |

**Shared DTO:** `VisualCommandTreeProjection` — breadcrumb, placeholder, next hops, view mode (Minimal / Neighborhood / Full).

**Discoverability stack:**

| Слой | Когда | Surface |
|------|-------|---------|
| Muscle memory | expert | none |
| Visual Command Tree | in-session capture | trail + table + guidance |
| Catalog peel | out-of-band | `c:`, Ctrl+K palette |

DashSpec CCL сегодня рендерит **Neighborhood** неявно (trail + `SlashInputGuidance` + suggestion table).

---

## 11. Agent ingress — MCPlane

[GUIDERS-ADR-0020](./adr/GUIDERS-ADR-0020-mcplane-agent-ingress.md) — sibling plane к CommandPlane, не внутри него.

| Plane | Вопрос |
|-------|--------|
| **CommandPlane** | Какая команда? Как резолвить? |
| **MCPlane** | Что агент **видит** обратно? Как expand? Что **next**? |

| Capability | Заметки |
|------------|---------|
| Agent response envelope | `IntentOutcome`, pulse, reason slots |
| Detail tiers | `pulse` (default) · `slim` · `full` — explicit opt-in |
| `next[]` hints | Только suggestions — не execution |
| Pulse truncation | `PulseFormat` (~240 chars default) |
| Catalog projection | Agent slice из `ICatalogVisitor` |

MCP tool implementation остаётся в **product**. MCPlane держит response contract + projection rules.

---

## 12. Cockpit и routing

### 12.1 Cockpit

Headless channel/CCU contracts для aviation mental model ([ADR-0007](./adr/GUIDERS-ADR-0007-aviation-mental-model.md)):

- **DataBus** — build, test, debug, git, ide host events
- **Channels** — IdeHealth, EnvReady fold kits
- **Transport** — bounded ingress bus для agent/human events

Consumers: CIDE, cdp-mcp (`cdp_ide_health`), Glass channel bind.

### 12.2 Routing

`IIntentOrgan<TRoute,TOutcome>` — neutral intent dispatch seam. Citizen organs в cdp-mcp implement; platform определяет только contract.

---

## 13. Вспомогательные семейства

### 13.1 Configurations

Layered project/workspace config с source transports — navigation presets, workspace overlays.

### 13.2 Language & anchors

XML/C# anchor resolution для CSX lift, doc correspondence, navigation wire parsing ([ADR-0034](./adr/GUIDERS-ADR-0034-csx-lift-navigation-config-xml-anchors.md)).

### 13.3 Policy-as-readable-code

Overlay profiles как TOML/JSON specs ([ADR-0031](./adr/GUIDERS-ADR-0031-policy-as-readable-code-overlay-profiles.md)) — consumed Conformance family.

---

## 14. Conformance hyperlane

Bootstrap specs в [`docs/conformance/`](./conformance/README.md); target sibling repo `aiguiders-conformance` ([ADR-0019](./adr/GUIDERS-ADR-0019-conformance-hyperlane-monorepo.md)).

| Область spec | Примеры |
|--------------|---------|
| Slash | `slash-line-resolve`, `slash-arg-completion` |
| Notation | `command-slash`, `argument-kv`, `invocation-parity` |
| MCPlane | `pulse-default`, `next-hints` |
| Navigation | `code-explore-scene` |
| Policies | `slash-ship-first`, `binding-overlay-wins`, `workspace-field-overlay` |

**Join pattern:** adopt schema → native adapter → pass vectors → product ADR.

---

## 15. Quarry waves и roadmap

По [GUIDERS-ADR-0010](./adr/GUIDERS-ADR-0010-platform-mechanics.md) и [GUIDERS-ROADMAP](./GUIDERS-ROADMAP.md):

| Wave | Scope | Статус |
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

Живой backlog: [GUIDERS-ROADMAP.md](./GUIDERS-ROADMAP.md) · friction: [GUIDERS-pain-inventory.md](./GUIDERS-pain-inventory.md).

---

## 16. Как присоединиться к федерации

Добровольно, тестируемо ([Constitution § How to join](./GUIDERS-FEDERATION-CONSTITUTION.md#how-to-join-voluntary-testable)):

1. **Adopt** relevant hyperlane package или schema.
2. **Implement** native adapter в своём stack (без mandatory UI framework).
3. **Pass** conformance — contract tests, journey smoke, semver pin.
4. **Document** wiring в product ADR; link platform ADRs.

Не требуется DOI paths, Forge или наша product line. Flat paths и свой `commandId` space — valid.

### 16.1 Слои для integrators

| Слой | Federation шипит | Планета реализует natively |
|------|-------------------|----------------------------|
| Contract | IR, schemas, `commandId`, catalog shape | — |
| Signage | conformance specs | — |
| Reference quarry | .NET packages (embassy) | may pin as-is |
| Wire → IR | spec + reference parser | port в TS/Kotlin/… |
| IR → input | — | key match, OS shortcuts |
| Surface | — | WPF, Blazor, extension host |

---

## 17. Reference missions и потребители

| Планета | Роль | Platform hyperlanes |
|---------|------|---------------------|
| **agent-forge** | Embassy — MCP, capabilities, plugin host | CommandPlane, MCPlane, Notations port |
| **cascade-ide** | Quarry → distill mechanics | Slash, Melody, Binding, Cockpit, Notations |
| **cdp-mcp** | Agent habitat | Cockpit, Routing, Navigation (informative dogfood) |
| **Glass** | WPF cockpit projection | Cockpit channels |
| **dash-spec** | Dashboard CCL adapter | CommandPlane.Slash, constructors |
| **Third party** | Любой stack | Pin NuGet или port conformance vectors |

Sibling repos (не в platform monorepo):

| Repo | Роль |
|------|------|
| `guiders-ui-platform` | Tokens, Agent AX, UI adapters |
| `guiders-core` | Shared backend organs |
| `guiders-plugin-host` | ALC plugin transport |
| `agent-notes` | Operator KB, handoff canon |

---

## 18. Индекс ADR

| ADR | Тема |
|-----|------|
| [0001](./adr/GUIDERS-ADR-0001-platform-boundary.md) | Граница platform |
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

Полный список: [`docs/adr/`](./adr/).

---

## 19. Глоссарий

| Термин | Определение |
|--------|-------------|
| **Mechanic** | Headless testable unit: contract + implementation + identity + context |
| **Hyperlane** | Версионированный federation protocol (NuGet, schema, MCP surface) |
| **Planet** | Суверенный product repo |
| **Embassy** | Reference consumer, не capital |
| **Quarry** | Извлечение mechanics из legacy product code в platform |
| **ArgTail** | Slash arg phase policy (picker, constructor, free text) |
| **commandId** | Stable executor key в registry |
| **Visual Command Tree** | Headless capture-stack projection для discoverability |
| **Conformance vector** | Testable spec instance, доказывающий hyperlane compatibility |

---

## Поддержка документа

| Триггер | Действие |
|---------|----------|
| Новое семейство пакетов | Обновить §7 + индекс ADR |
| Major semver | Обновить версию в шапке |
| Новый hyperlane | Обновить §6 + cross-link в Constitution |
| Ежеквартально | Regenerate `ADOPTION-ALLIANCE.generated.md`; проверить docx export |

Regenerate DOCX (RU):

```bash
pandoc docs/GUIDERS-PLATFORM-ARCHITECTURE-HUB.ru.md \
  -o docs/GUIDERS-PLATFORM-ARCHITECTURE-HUB.ru.docx \
  --toc --toc-depth=3 \
  -V lang=ru-RU
```

Regenerate DOCX (EN):

```bash
pandoc docs/GUIDERS-PLATFORM-ARCHITECTURE-HUB.md \
  -o docs/GUIDERS-PLATFORM-ARCHITECTURE-HUB.docx \
  --toc --toc-depth=3 \
  -V lang=en-US
```

---

*Unified planets: one mechanics layer, many atmospheres.*

*Единые планеты: один слой механики, много атмосфер.*
