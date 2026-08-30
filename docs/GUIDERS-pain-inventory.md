# GUIDERS pain inventory

| | |
|---|---|
| **Status** | Living doc |
| **Date** | 2026-08-30 |
| **Relates to** | [Constitution](GUIDERS-FEDERATION-CONSTITUTION.md), [ADR-0015](adr/GUIDERS-ADR-0015-invocation-mechanics-slash-melody-binding.md), [ADR-0019](adr/GUIDERS-ADR-0019-conformance-hyperlane-monorepo.md), [ADR-0020](adr/GUIDERS-ADR-0020-mcplane-agent-ingress.md), [ADR-0021](adr/GUIDERS-ADR-0021-notations-quarry-family.md) |
| **North star** | Friction insight → artifact; не «ещё один monorepo», а **hyperlanes** (CommandPlane · Notations · MCPlane · conformance) чтобы планеты стыковались без аннексии |

По образцу [FORGE-pain-inventory](https://github.com/AI-Guiders/agent-forge/blob/main/design/FORGE-pain-inventory.md) и [ANPM-pain-inventory](https://github.com/AI-Guiders/agent-nuget-pm/blob/main/docs/ANPM-pain-inventory.md): шишки federation platform — пока боль живая, потом ADR, packages, vectors.

## Как пользоваться

Одна строка = одна боль. Колонки:

| Колонка | Смысл |
|---------|--------|
| **ID** | `G-xxx` для ссылок из ADR/issue |
| **Боль** | Симптом своими словами |
| **Кто** | human / agent / ops / all |
| **Откуда** | pilot, ADR draft, operator chat, industry |
| **GUIDERS-ответ** | hyperlane + конкретика или «planet only» |
| **Статус** | open / in-progress / resolved / wont-fix / upstream |

**Приоритет:** боль бьёт по **стыку планет**, повторяется на каждом embed, или объясняет agent/human friction на границе surfaces → текущая wave; остальное — backlog.

**Фильтр фич** (из charter + ADR):

1. **Mechanics ≠ planet domain** — federation не тащит buffer plane, Citizen, product MCP tools.
2. **Wire → IR → conformance** — не «ещё один парсер в каждом repo».
3. **Planets are not SSOT** — dogfood на CDP/Forge допустим; normative только vectors + neutral types.
4. **Embassy, not gate** — .NET quarry; JS/Kotlin portят spec.

**Мем-канон:** *«инженер заебался»* — когда парсеры есть 40 лет, а IR между human / IDE / agent — нет. См. **G-001**, **G-003**.

---

## Сводка по категориям

| Категория | Типичная жалоба | Лидеры боли |
|-----------|-----------------|-------------|
| **Missing stitch layer** | slash, CLI, MCP, hotkey — разные dialects, один effect | every IDE, every agent host |
| **Rocket pulls foundation** | хочешь agent parity → тащишь notation, catalog, envelope | federation build-out 2026-08 |
| **Blame the model** | «агент недетерминирован» | скрывает fragmented invoke/observe |
| **NIH vs MIT on disk** | `getopt`/LSP/OpenAPI есть, adopt не бесплатен | integrators, platform teams |
| **License friction** | GPL/viral, AS-IS misread, closed fork on MIT, **fake alliances** | legal + platform teams |
| **Planet as empire** | один experimental habitat становится «каноном» | in-house tools leaking into NuGet docs |

---

## Pain inventory

### Foundation — почему вообще Guiders platform

| ID | Боль | Кто | Откуда | GUIDERS-ответ | Статус |
|----|------|-----|--------|---------------|--------|
| G-001 | **Ракету просто так не построишь** — agent parity / federation embed тянет слои ниже: notation, catalog, conformance, ingress envelope; индустрия не продала стык, каждый продукт копирует resolve | all | operator chat 2026-08-30; ADR-0020/0021 wave | Hyperlane stack: **Notations** (wire→IR) · **CommandPlane** (mechanics) · **MCPlane** (observe) · **aiguiders-conformance** (vectors); à la carte NuGet | in-progress |
| G-002 | Агентов ругают за «недетерминированную хрень», хотя корень — **мир из кусков без единых правил** invoke/observe; так страдали люди до агентов (slash vs palette vs CLI vs docs) | human+agent | operator chat 2026-08-30 | Один `commandId`, many surfaces ([ADR-0009](adr/GUIDERS-ADR-0009-command-surface-pattern.md)); MCPlane pulse/default; conformance вместо «надеемся на промпт» | in-progress |
| G-003 | **Инженер заебался:** getopt/slash/keymaps/OpenAPI/LSP — по отдельности; **нет нейтрального IR** между human notation, IDE binding и MCP JSON; «должны были сделать 100500 лет назад» | human | operator chat 2026-08-30; industry archaeology | **Notations.*** quarry family ([ADR-0021](adr/GUIDERS-ADR-0021-notations-quarry-family.md)); native ports по vectors, не копипаста C# | in-progress |
| G-004 | NIH при MIT на диске: `System.CommandLine`/GNU vectors есть, но adopt ≠ `NormalizedArgTail` для slash-catalog; каждый пишет `Split(' ')` | human+ops | ADR-0021 §11; pilot embeds | v1 owned lexers (slash/kv); v2 **Argument.Cli** as quarry wrapper; spec semver — adopt дешевле rewrite | open |
| G-005 | Experimental **planet** (habitat MCP, in-house wire) **утекает в federation** как normative → третьи планеты не приходят | human+agent | MCPlane/Notations drafts 2026-08 | [Constitution § Planets are not SSOT](GUIDERS-FEDERATION-CONSTITUTION.md#planets-are-not-federation-ssot); embassy = Forge/CIDE; CDP informative only | in-progress |

### Licensing & cooperation

| ID | Боль | Кто | Откуда | GUIDERS-ответ | Статус |
|----|------|-----|--------|---------------|--------|
| G-006 | Эпоха MIT/открытых лицензий дала **шанс кооперации как никогда** — но GPL и «заразные» лицензии снова строят барьеры; боль сохранения открытости понятна, побочный эффект — **плохой adopt**, clean-room, обход вместо pin | human+ops | operator chat 2026-08-30; Neovim/Emacs quarry ([ADR-0016](adr/GUIDERS-ADR-0016-input-notation-quarry-family.md)) | **Conformance-first:** vectors + neutral IR (MIT); quarry = behavior port, не GPL blob в NuGet; license review per package; Constitution: public ADRs, fork adapters not fork semantics in secret | open |
| G-006a | Permissive (MIT) **разрешает закрытый форк без отдачи** — кооперация на уровне spec, не на уровне «все в одном commons»; мотивация upstream слабая, если value ушло в proprietary adapter | human+ops | operator chat 2026-08-30 | Hyperlane = **roads/signage** (spec semver), не monorepo empire; conformance tag + embassy proof; ethical-use / attribution в charter, не copyleft в core packages | open |
| G-006b | **AS-IS / no warranty** читают как «нельзя зависеть»; чаще это **отказ от судебных тяжб** и нереального solo-SDLC на всех пользователей сразу — не отказ от issues/bugs/contrib | ops+human | operator chat 2026-08-30 | **Adoption alliance** ([Constitution § Adoption alliances](GUIDERS-FEDERATION-CONSTITUTION.md#adoption-alliances-real-not-decorative)): supported embed surface + semver spec; issues/PR welcome; warranty stays in LICENSE, **cooperation** — в пакте | open |
| G-007 | Enterprise/legal блокирует dependency на «не тот» license → команда пишет свой `Split(' ')` вместо embassy quarry | ops | integrator pilots | À la carte packages; minimal deps in v1 Notations; document **what** is pinned vs **what** is ported; `@aiguiders/conformance` без runtime GPL | open |
| G-008 | **Рисованные альянсы** (логотипы, «ecosystem») без договорённости **кто как адоптит** MIT-штуку → снова обход и closed fork | human+ops | operator chat 2026-08-30 | Confederation = настоящий слой: hyperlane + conformance tag + embassy + **`AdoptionReport`** → `ADOPTION-ALLIANCE.generated.md` ([ADR-0022](adr/GUIDERS-ADR-0022-utilities-adoption-report.md)); CI drift gate | in-progress |

### G-008 {#g-008}

Anchor for Constitution § Adoption alliances. Alliance ≠ shared repo; alliance = shared **spec semver**, visible adopters, issue path, optional reference quarry.

### Invocation & notation (surface stitch)

| ID | Боль | Кто | Откуда | GUIDERS-ответ | Статус |
|----|------|-----|--------|---------------|--------|
| G-010 | Slash resolve/logic копируется в Forge JS, CIDE, тестах — нет одного spec | agent+human | ADR-0018; CommandPlaneTests-only harness | `slash-*-v1` specs + conformance repo ([ADR-0019](adr/GUIDERS-ADR-0019-conformance-hyperlane-monorepo.md)) | in-progress |
| G-011 | Console `key=value` и slash `/path tail` — два resolve, один `commandId` | agent | CDP-style meta vs slash catalog | **Notations.Command.*** + **Argument.Kv** + `invocation-parity-v1` vector ([ADR-0021](adr/GUIDERS-ADR-0021-notations-quarry-family.md)) | in-progress |
| G-012 | Keyboard wire: Vim vs KeyGesture vs Emacs — три алфавита, один chord | human | CIDE hotkeys + melody | **Notations.Keyboard.*** ([ADR-0016](adr/GUIDERS-ADR-0016-input-notation-quarry-family.md) → 0021 rename) | in-progress |

### Agent ingress (observe path)

| ID | Боль | Кто | Откуда | GUIDERS-ответ | Статус |
|----|------|-----|--------|---------------|--------|
| G-020 | MCP `CallTool` возвращает JSON wall — контекст агента горит | agent | agent-era pilots | **MCPlane**: pulse default, `next[]`, detail tiers ([ADR-0020](adr/GUIDERS-ADR-0020-mcplane-agent-ingress.md)) | in-progress |
| G-021 | Tool docs в Meta/README ≠ slash `commandId` — drift | agent+human | multi-surface products | `commandId` parity + `agent-catalog-projection-v1`; Forge [FORGE-ADR-0025](https://github.com/AI-Guiders/agent-forge/blob/master/design/FORGE-ADR-0025-human-command-parity.md) | open |

---

## Шаблон новой строки

```markdown
| G-0xx | … | human/agent | откуда | hyperlane + ADR | open |
```

При закрытии: статус **resolved** + ссылка на commit/ADR/PR в комментарии к строке или follow-up issue.
