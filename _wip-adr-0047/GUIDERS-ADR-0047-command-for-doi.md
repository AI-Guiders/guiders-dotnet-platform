# GUIDERS-ADR-0047: Command authoring — DOI-first, typed IR

| | |
|---|---|
| **Status** | Proposed |
| **Level** | **Federation hyperlane** — not planet-local DX |
| **Date** | 2026-08-31 |
| **Tags** | #guiders #federation #commandplane #catalog #doi #ir #guild #dx |
| **Related** | GUIDERS-ADR-0006 · GUIDERS-ADR-0009 · GUIDERS-ADR-0021 · GUIDERS-ADR-0042 · GUIDERS-ADR-0045 · GUIDERS-ADR-0046 · [GUIDERS-ADR-0048](../_wip-adr-0048/GUIDERS-ADR-0048-authoring-quarry-family.md) · ADR-0154 |

## Context

`CommandCatalogWire` (today `CommandDescriptor`) and `CatalogRouteEntry` carry **DOI** and slash grammar as raw `string` fields. Planets duplicate vocabulary in builders, path helpers, parsers, and executors.

**Human–agent DX parity:** federation declares X once on the typed spine; planets reference instance `.catalog` files, never re-stringify.

**String obsession is tech debt.** Strings only at notation import/export. Inside the spine — typed IR ([ADR-0042](GUIDERS-ADR-0042-intermediate-representation-family.md)).

## Decision

### 1. Federation scope

Planets ship `<planet>.catalog` **content** only. Grammar, parser, CodeGen, emit CLI — federation **`Authoring.*`** guild ([GUIDERS-ADR-0048](../_wip-adr-0048/GUIDERS-ADR-0048-authoring-quarry-family.md)); wire resolve stays **`Notations.*`** ([ADR-0021](GUIDERS-ADR-0021-notations-quarry-family.md)).

### 2. Three axes

```text
DOI      CommandDoi           → CommandId (derived)
Phrase   InvocationPhrase     slash/CCL line the human types (not a filesystem path)
Execute  PlatformCommand<T>
```

### 3. Packages

```text
IR.Command                      CommandDoi · InvocationPhrase · CommandCatalogEntry · ArgTailProfile
Authoring.Command.Catalog       .catalog grammar + parser  (guild: Authoring.*)
Authoring.Command.Bundles       federation stdlib bundles (grain/date-filter, …)
CommandPlane.Catalog.CodeGen    Roslyn → {Planet}Catalog.g.cs
CommandPlane.Catalog            expand, assembly, emit CLI host
Planets                         <planet>.catalog + Execute partials
```

Guild map: [GUIDERS-ADR-0048](../_wip-adr-0048/GUIDERS-ADR-0048-authoring-quarry-family.md). **Not** `Notations.Command.Catalog` — Notations = wire-in only.

### 4. `.catalog` grammar — block syntax

**Block syntax** (v0). Comments `# …` allowed.

**Разделители блоков — DashSpec parity:** `keyword … end keyword`, **без `{ }`**. Как в `.dashspec` (`variables … end variables`, `commands … end commands`, `heatmap … end heatmap`); brace diagram blocks в DashSpec уже сняты (`CardParser`: *brace diagram blocks removed*).

| Открытие | Закрытие |
|----------|----------|
| `variables` / `variables table` | `end variables` |
| `helps` / `helps table` | `end helps` |
| `profiles` / `profiles table` | `end profiles` |
| `phrases` / `phrases table` | `end phrases` |
| `defaults` | `end defaults` |
| `commands` / `commands table` | `end commands` |
| `executors` | `end executors` |
| `command filter.date` (sugar → row в `commands`) | `end command` |
| `pattern filter-by-name` (sugar → `phrases`) | `end pattern` |

`{filter}` / `{surface}` в **phrase** — phrase-slot interpolation, **не** block delimiter (как `{max}` в legend string в dashspec).

`catalog dash` — однострочный заголовок домена (как `@tab analytics`), не блок.

#### Слова в DSL (короткий словарь)

| Слово | Что это на человеческом | Не путать с |
|-------|-------------------------|-------------|
| `catalog` | планета / домен (`dash` → все id начинаются с `dash.`) | — |
| `commands` | **верхний уровень** — матрица каталога (table); одна строка = одна команда | nested `command … end command` |
| `command filter.date` | id строки в `commands` (колонка `command`); wire = `dash.filter.date` | class name |
| `variables` | **верхний уровень** — словарь `{filter}`… (kv или table); только структура | constructor slot (ADR-0035) |
| `phrase` | **что человек набирает** в slash после `/`; `{имя}` только из `variables` | filesystem path |
| `phrases` | **верхний уровень** — соответствие *name* → *slash template* (`phrases table`) | runtime `InvocationPhrase` |
| `pattern` | sugar → row в `phrases` (`pattern name … end pattern`) | C# pattern |
| `profiles` | **верхний уровень** — arg-меню (kv/table); ссылка из `commands` | wire `ArgTail` |
| `profile date-value` | в `commands` — колонка `profile` → имя из `profiles` | inline nested `profile … end profile` |
| `arg` | именованный **arg-хвост** после phrase (`value` — не phrase-variable) | `{filter}` в phrase |
| `import <…>` | подключить federation bundle в scope файла (стандартная библиотека) | C# `using` |
| `use` | устаревает в inline-блоках; bundle → `profiles … bundle` (kv/table) | `import` |
| `preset` / `constructor` | пункты arg-меню (instant / guided) | wire id `date_today` |
| `picker for-slot` | динамический picker по variable из `variables` | `picker:dash-field.*` |
| `scope` | где команда видна (dashboard, editor, …) | — |
| `channels` | откуда вызывают (slash-bar, palette, mcp) | phrase |
| `helps` | **верхний уровень** — соответствие *entity* + *field* → *text* (`helps table`) | inline prose в `commands` |
| `expand` | recipe runtime; `fills` — какие variables подставляет | — |

Слова **`path`**, **`invoker-tag`** — **не используем** (v0). Семейство **верхний уровень kv/table**: `variables`, `helps`, `profiles`, **`phrases`** (pattern↔phrase), `defaults`, `commands`, `executors` — см. §4.1.

#### 4.1 Authoring surfaces — где ещё kv/table

Один принцип: **декларация матрицей** — `variables`, **`helps table`**, `profiles`, **`phrases table`**, **`commands table`**. Dotted kv / `pattern` block — sugar → row.

| Surface | v0 | Содержимое | Зачем table |
|---------|-----|------------|-------------|
| **`variables`** | ✅ | phrase-слоты `{filter}`… | словарь слотов |
| **`helps`** | ✅ | *target* + *field* → *text* | entity/object ↔ копирайт |
| **`profiles`** | ✅ | arg-меню, bundle | preset/constructor rows |
| **`phrases`** | ✅ | *name* → *phrase* | slash templates |
| **`defaults`** | ✅ | `scope`, `channels` для всех команд | dash: одно и то же в каждой строке |
| **`commands`** | ✅ | phrase, profile, expand, fills… | **главная матрица** каталога |
| **`executors`** | 📋 | `filter.date` → `SelectDateFilterCommand` | optional override convention |
| **`imports`** | — | `import <…>` | мало строк, список ок |

**`phrases`** — соответствие *name* → *phrase* (как `variables`: *name* → *kind*). Канон — **`phrases table`**:

```text
phrases table
  | name           | phrase                   |
  | filter-by-name | select filter {filter}   |
  | pick-report    | select report {report}   |
  | pick-page      | select page {page}       |
  | pick-view      | view {card} {view}       |
end phrases

# kv sugar (desugar → rows выше)
phrases
  filter-by-name = "select filter {filter}"
  pick-report    = "select report {report}"
end phrases

# pattern block sugar (desugar → phrases)
pattern filter-by-name
  phrase select filter {filter}
end pattern
```

В **`commands`**: колонка `phrase` = **name** из `phrases`; `phrase-inline` = inline template без строки в `phrases` (ровно одна из двух).

**`helps`** — то же соответствие *entity* + *field* → *text* (как `variables`: *name* → *kind*):

```text
helps table
  | target              | field   | text                              |
  | command filter.date | summary | Установить date-фильтр на toolbar |
  | command filter.date | arg     | select filter … <дата>            |
  | variable filter     | label   | Имя фильтра на toolbar            |
end helps

# kv sugar (desugar → rows выше)
helps
  command filter.date.summary = "Установить date-фильтр на toolbar"
  variable filter.label       = "Имя фильтра на toolbar"
end helps
```

`target` = entity: `command <id>` (строка из `commands`) или `variable <name>` (из `variables`). Копирайт **не** в `commands` / `variables` — только здесь.

**`defaults`** (scope/channels для всех строк `commands`; per-row override — опциональные колонки `scope`, `channels`):

```text
defaults
  command.scope    = dashboard
  command.channels = dash-slash, dash-palette, dash-ccl
end defaults
```

**`commands`** — главная матрица каталога:

```text
commands table
  | command       | phrase         | phrase-inline       | profile     | expand                 | fills          |
  | filter.date   | filter-by-name |                     | date-value  | toolbar-filters date   | filter         |
  | filter.field  | filter-by-name |                     | field-value | toolbar-filters field  | filter         |
  | report.select | pick-report    |                     | —           | report-catalog         | report         |
  | page.select   | pick-page      |                     | —           | report-pages           | page           |
  | card.view     | pick-view      |                     | —           | card-views             | card, view     |
  | host.show     |                | show host {surface} | —           | host-surfaces          | surface        |
end commands
```

Profile `—` = нет arg-хвоста. Parser: `command filter.date … end command` → row (sugar).

**`executors`** (escape hatch поверх convention):

```text
executors
  filter.date = SelectDateFilterCommand
end executors
```

**Не выносим** в table (остаётся как есть):

| Что | Почему |
|-----|--------|
| `catalog dash` | заголовок домена |
| `import <…>` | 1–3 строки, не матрица |
| expand **recipe impl** | C# (`DashExpandRecipes`), не authoring |
| constructor **tree** | C# registry (ADR-0035) |

**Порядок файла (target):**

```text
catalog → import → variables → helps → profiles → phrases → defaults → commands table → executors?
```

v0 ship: все surfaces ✅ кроме `executors`. Parser sugar: kv → table rows; `pattern … end pattern` → `phrases`; `command … end command` → `commands`.

#### Полный пример: `dash.catalog` (DashSpec)

Читать сверху вниз: словари → `defaults` → **`commands table`**.

```text
# ─────────────────────────────────────────────────────────────
# DashSpec.Host/Catalog/dash.catalog
# Federation grammar v0 — planet content only
# ─────────────────────────────────────────────────────────────

catalog dash

import <grain/date-filter>

# ── Словарь phrase-слотов (верхний уровень) ──────────────────
variables
  filter  = phrase-slot
  report  = phrase-slot
  page    = phrase-slot
  card    = phrase-slot
  view    = phrase-slot
  surface = phrase-slot
end variables

# variables table
#   | name    | kind        |
#   | filter  | phrase-slot |
#   …
# end variables

# ── Копирайт: entity + field → text ───────────────────────────
helps table
  | target              | field   | text                                        |
  | command filter.date | summary | Установить date-фильтр на toolbar           |
  | command filter.date | arg     | select filter … <дата>                      |
  | command filter.field| summary | Установить field-фильтр на toolbar          |
  | command report.select | summary | Переключить отчёт                         |
  | command page.select | summary | Переключить страницу отчёта                 |
  | command card.view   | summary | Переключить вид карточки                    |
  | command host.show   | summary | Показать host surface                       |
  | variable filter     | label   | Имя фильтра на toolbar                      |
  | variable report     | label   | Идентификатор отчёта                        |
  | variable page       | label   | Страница отчёта                             |
  | variable card       | label   | Карточка dashboard                          |
  | variable view       | label   | Вид карточки                                  |
  | variable surface    | label   | Host surface (панель / режим)               |
end helps

# ── Arg profiles (верхний уровень) ───────────────────────────
profiles
  date-value  = bundle date-filter
  field-value.value.picker-for-slot = filter
end profiles

# ── Slash templates: name → phrase ───────────────────────────
phrases table
  | name           | phrase                   |
  | filter-by-name | select filter {filter}   |
  | pick-report    | select report {report}   |
  | pick-page      | select page {page}       |
  | pick-view      | view {card} {view}       |
end phrases

defaults
  command.scope    = dashboard
  command.channels = dash-slash, dash-palette, dash-ccl
end defaults

commands table
  | command       | phrase         | phrase-inline       | profile     | expand                 | fills          |
  | filter.date   | filter-by-name |                     | date-value  | toolbar-filters date   | filter         |
  | filter.field  | filter-by-name |                     | field-value | toolbar-filters field  | filter         |
  | report.select | pick-report    |                     | —           | report-catalog         | report         |
  | page.select   | pick-page      |                     | —           | report-pages           | page           |
  | card.view     | pick-view      |                     | —           | card-views             | card, view     |
  | host.show     |                | show host {surface} | —           | host-surfaces          | surface        |
end commands
```

#### Что получается из одной строки `commands` (например `filter.date`)

```text
| filter.date | filter-by-name | | date-value | toolbar-filters date | filter |
```

| Выход | Значение |
|-------|----------|
| **CommandId** (agent, registry) | `dash.filter.date` |
| **Phrase** (human slash) | `select filter {filter}` + arg `value` (дата) |
| **Пример ввода** | `select filter usage_date today` |
| **MCP tool** | `dash.filter.date` + `{ "filter": "usage_date", "value": "today" }` |
| **Expand** | recipe `toolbar-filters date` → N строк каталога: `… usage_date`, `… app_date`, … |
| **Arg profile** | `profiles`: `date-value = bundle date-filter` |
| **Wire emit** (tier D, не authoring) | `argTail = picker+constructor:+date_today+date_week+…` |
| **C# executor** | `SelectDateFilterCommand` (sourcegen map по convention) |

`expand toolbar-filters date` = «для каждого date-фильтра на toolbar подставь имя в `{filter}`». Реализация recipe — в C# планеты (`DashExpandRecipes`), не в DSL.

#### `variables` — верхний уровень (kv или table)

Как **`helps`**: один блок на файл, две поверхности синтаксиса → один `CatalogVariableIndex`.

**Форма A — key-value:**

```text
variables
  filter  = phrase-slot
  report  = phrase-slot
end variables
```

**Форма B — table:**

```text
variables table
  | name   | kind        |
  | filter | phrase-slot |
  | report | phrase-slot |
end variables
```

| `kind` (v0) | Значение |
|-------------|----------|
| `phrase-slot` | плейсхолдер в `{…}` внутри `phrase` (default) |

Parser: `{foo}` в `phrase` без строки в `variables` → **compile error**. Codegen → `{Planet}Vocabulary.g.cs`.

Подписи для слотов — в **`helps table`**: `variable filter` + `label` (не в `variables`).

| Где | Синтаксис | Соответствие |
|-----|-----------|--------------|
| `variables` | table / kv | *name* → *kind* |
| `helps` | **table** / kv sugar | *target* + *field* → *text* |
| `phrases` | **table** / kv sugar | *name* → *phrase* |
| `profiles` | table / kv | *profile* → arg-menu rows |
| `commands` | **table** | *command* → wiring |

#### `helps` — верхний уровень (table)

**Структура** (`commands`, `variables`…) и **копирайт** разделены. Соответствие: *entity* + *field* → *text*. Канон — **`helps table`**; dotted kv — sugar.

**Форма A — table** (v0 default):

```text
helps table
  | target              | field   | text                              |
  | command filter.date | summary | Установить date-фильтр на toolbar |
  | command filter.date | arg     | select filter … <дата>            |
  | variable filter     | label   | Имя фильтра на toolbar            |
end helps
```

| Колонка | Значение |
|---------|----------|
| `target` | entity: `command <id>` или `variable <name>` |
| `field` | роль текста (см. ниже) |
| `text` | prose для UI / MCP |

`command filter.date` → `dash.filter.date` (домен из `catalog`). `variable filter` → слот из `variables`.

| `field` | → IR |
|---------|------|
| `summary` | `CommandCatalogEntry.Help` |
| `arg` | `ArgHint` |
| `detail` | long tooltip (v1) |
| `agent` | MCP description |
| `label` | variable / schema label |

**Форма B — kv** (desugar → rows; ключ `<target>.<field>` или `<target> <field>`):

```text
helps
  command filter.date.summary = "Установить date-фильтр на toolbar"
  command filter.date.arg      = "select filter … <дата>"
  variable filter.label        = "Имя фильтра на toolbar"
end helps
```

Parser сводит обе формы в `CatalogHelpIndex` → merge при codegen в `CommandCatalogEntry`. Отсутствующий `helps` для объявленной команды — **warning** (v0) / **error** (v1). i18n overlay (`helps.ru.catalog`) — отдельный файл, тот же ключевое пространство (defer v1).

`commands table` **не содержит** prose — только phrase/profile/expand (+ merge из `defaults` для scope/channels). Копирайт — в `helps`.

#### `commands` — верхний уровень (table)

Одна строка → один `CommandCatalogEntry`. Колонки (v0):

| Колонка | Обязательна | Значение |
|---------|-------------|----------|
| `command` | да | id без домена (`filter.date` → `dash.filter.date`) |
| `phrase` | * | ссылка на имя из `phrases` |
| `phrase-inline` | * | inline slash template (если нет строки в `phrases`) |
| `profile` | нет | имя из `profiles`; `—` = без arg-хвоста |
| `expand` | нет | recipe id + опциональный arg (`toolbar-filters date`) |
| `fills` | нет | variables, которые заполняет recipe |
| `scope` | нет | override `defaults.command.scope` |
| `channels` | нет | override `defaults.command.channels` |

\* ровно одна из `phrase` / `phrase-inline` непуста.

Sugar (desugar → row):

```text
command filter.date
  phrase filter-by-name
  profile date-value
  expand toolbar-filters date fills filter
end command
```

#### `profiles` — верхний уровень (kv или table)

Arg-меню после phrase — как **`variables`** / **`helps`**: не nested `profile … end profile` с `arg` внутри.

**Bundle (обычный случай):**

```text
import <grain/date-filter>

profiles
  date-value = bundle date-filter
end profiles
```

**Форма A — key-value (явное меню):**

```text
profiles
  date-value.value.preset      = today
  date-value.value.constructor = week
  date-value.value.constructor = month-week
  date-value.value.constructor = month
  date-value.value.constructor = quarter
  date-value.value.constructor = range
  date-value.value.free-text   = *

  field-value.value.picker-for-slot = filter
end profiles
```

**Форма B — table** (то, на что «просится» явное меню):

```text
profiles table
  | profile     | arg   | entry           | ref        |
  | date-value  | value | preset          | today      |
  | date-value  | value | constructor     | week       |
  | date-value  | value | constructor     | month-week |
  | date-value  | value | constructor     | month      |
  | date-value  | value | constructor     | quarter    |
  | date-value  | value | constructor     | range      |
  | date-value  | value | free-text       | *          |
  | field-value | value | picker-for-slot | filter     |
end profiles
```

| `entry` | Значение |
|---------|----------|
| `preset` | instant wire (`today`) |
| `constructor` | root constructor id (`week`, `range`, …) |
| `free-text` | `*` = escape hatch всегда доступен |
| `picker-for-slot` | dynamic picker по variable |
| `bundle` | целиком federation bundle (одна строка на profile) |

Порядок строк в table = порядок в slash arg-меню. Parser → `CatalogProfileIndex` → `ArgTailProfile` → wire emit.

В **`commands`**: колонка `profile` → имя из `profiles`.

**`expand … fills`** связывает recipe с variable(s): какие плейсхолдеры recipe заполняет на runtime. Federation recipe registry дублирует контракт для conformance; при однозначном recipe `fills` можно опустить (dash: `report-catalog` → `report`).

#### Inline phrase (без `phrases`)

Допустимо в колонке `phrase-inline` (одна строка, без записи в `phrases`):

```text
| host.show | | show host {surface} | — | host-surfaces | surface |
```

#### Wire export (TOML tier D)

Поле **`path`** в TOML — legacy имя для **phrase** при экспорте. В `.catalog` слова `path` нет.


### 5. CodeGen — both

| Tool | Emits |
|------|--------|
| **Roslyn source generator** | `{Planet}Catalog.g.cs`, `{Planet}Vocabulary.g.cs`, executor map |
| **`dotnet catalog emit`** | `mcp-tools.json`, `catalog.wire.toml`, conformance snippets (CI / ops) |

Same parser, multiple backends.

### 6. Expand recipes — hybrid, no SID/CID until needed

Recipe ids — **kebab-case** plain strings in v0:

```text
expand toolbar-filters date
expand report-catalog
```

Planet registers implementation for ids used in its `.catalog`. Federation maintains a **recommended id list** + conformance for cross-planet recipes; no separate SID/CID type system until a proven multi-tenant need. Planet-private ids use prefix `planet.<planet>.` (e.g. `planet.dash.custom`).

### 7. Wire DTO — rename now

`CommandDescriptor` → **`CommandCatalogWire`**. SSOT = `CommandCatalogEntry`. One-way rename in IR.Command + consumers in the same wave (no alias type).

### 8. `ArgTailProfile` — structured authoring, wire is emit-only

**Problem:** строка `tail picker+constructor:+date_today+…` — это **tier-D wire** ([ADR-0012](GUIDERS-ADR-0012-arg-picker-completion.md), [ADR-0035](GUIDERS-ADR-0035-slash-value-constructors.md)). В `.catalog` её **не пишем**: она нечитаема и дублирует три разных слоя, которые сегодня размазаны по C# (`ArgTail`, `ArgConstructorBindings`, `DateConstructorCatalog.Register`).

**Rule:** `.catalog` описывает **что видит человек в arg-меню**; codegen эмитит `ArgTailProfile` → `CommandCatalogWire.ArgTail` + constructor bindings + MCP `inputSchema`.

#### Три слоя (не схлопывать)

| Слой | Кто владеет | Пример |
|------|-------------|--------|
| **Arg menu** (preset / root constructor / free-text) | `profiles` kv/table | `preset today`, `constructor range` |
| **Constructor tree** (сегменты, wire/display patterns) | C# registry планеты | `DateConstructorCatalog` ([ADR-0035](GUIDERS-ADR-0035-slash-value-constructors.md) §4) |
| **Wire `argTail`** | emit only | `picker+constructor:+date_today+date_range` |

DSL **выбирает** root constructors из registry; **не переопределяет** leaf-сегменты (`year` / `month` / `day`).

#### Federation bundle library — `import <…>` + `profiles`

Стандартная библиотека federation — **не копируется** в `dash.catalog`. Планета: `import` + одна строка в `profiles`.

| Verb | Scope | Пример |
|------|-------|--------|
| **`import <path>`** | файл `.catalog` | `import <value/email>` |
| **`import <path> as name`** | файл (alias) | `import <grain/date-filter> as dash-date` |
| **`profiles … bundle`** | верхний уровень | `date-value = bundle date-filter` |

Угловые скобки = **внешний federation module** (не phrase, не filesystem path).

**Таксономия путей** (рекомендуемый префикс первого сегмента):

| Prefix | Что внутри | Пример |
|--------|------------|--------|
| `value/` | универсальные агрегаты (дата, email, url, phone, …) | `<value/date-grain>`, `<value/email>` |
| `grain/` | продуктовые пресеты поверх `value/*` | `<grain/date-filter>` (= date-grain для toolbar) |
| `planet.<id>/` | private bundle планеты (редко) | `<planet.dash/custom-range>` |

`grain/date-filter` в federation **extends** `value/date-grain` (тот же constructor registry, фиксированное arg-меню).

**Planet file:**

```text
import <grain/date-filter>

profiles
  date-value = bundle date-filter
end profiles
```

**Federation source** (`CommandPlane.Catalog.Bundles/grain/date-filter.catalogbundle` — не в репо планеты) — тот же IR, что разворачивается в table:

```text
profiles table
  | profile    | arg   | entry       | ref        |
  | date-filter| value | preset      | today      |
  | date-filter| value | constructor | week       |
  | date-filter| value | constructor | month-week |
  | date-filter| value | constructor | month      |
  | date-filter| value | constructor | quarter    |
  | date-filter| value | constructor | range      |
  | date-filter| value | free-text   | *          |
end profiles
```

Планета: `date-value = bundle date-filter` — alias на эти строки.

Default import name = **последний сегмент пути** (`<grain/date-filter>` → `date-filter`). `as` — если нужен другой local name или коллизия.

Federation vocabulary map (DSL имя → constructor id) — внутри bundle `value/date-grain`:

| DSL | Constructor id | Kind |
|-----|----------------|------|
| `today` | `date_today` | preset (instant wire) |
| `week` | `date_week` | root |
| `month-week` | `date_month_week` | root |
| `month` | `date_month` | root |
| `quarter` | `date_quarter` | root |
| `range` | `date_range` | root |

Labels («Сегодня», «Период…») — из bundle defaults; override в `helps` или extra rows в `profiles table` (v1).

Явное меню без bundle — **`profiles table`** (см. §4); nested `profile … end profile` с `arg` внутри **не используем**.

`picker-for-slot filter` → emit `picker:dash-field.{filter}` (см. `DashboardCommandCatalogExpander.FieldFilter`).

#### Command reference

```text
| filter.date | filter-by-name | | date-value | toolbar-filters date | filter |
```

`ArgTailProfile` в IR ссылается на `IR.Argument` (`ArgumentNotationProfile`, picker ids, constructor bindings) — тот же spine, что slash completion.

### 9. CommandId migration — hard cut, no aliases

Registry keys **only** new DOI-derived ids. No dual-register, no wire alias period. Slash **phrases** stay stable (`select filter …`) — users unaffected. DashSpec migration table is a one-time break.

### 10. Executor binding — convention (proposed)

Default: row `filter.date` → `SelectDateFilterCommand` via sourcegen map (`DashCatalog.Executors.g.cs`). Optional `executors` kv для escape hatch.

*Pending operator confirmation.*

### 11. Projections wave 1 (proposed)

Roslyn C# + `mcp-tools.json` + `catalog.wire.toml` + grammar conformance vectors.

*Pending operator confirmation.*

### 12. Human–agent parity

One `.catalog` row → phrase (human) + CommandId + MCP schema (agent). No duplicate manifests.

## Examples (IRL)

### Command class — Execute only

```csharp
internal sealed partial class SelectDateFilterCommand
    : PlatformCommand<DashboardFilterContext>, ICatalogDescribed
{
    public static CommandCatalogEntry Catalog => DashCatalog.Filter.Date;
    public override CommandDoi Doi => Catalog.Doi;

    protected override CommandOutcome Execute(DashboardFilterContext context) =>
        context.ApplyDate(context.BoundSlot(DashVocabulary.Slots.Filter), …);
}
```

### MCP (generated)

```json
{
  "name": "dash.filter.date",
  "inputSchema": {
    "properties": { "filter": { "type": "string" }, "value": { "type": "string" } },
    "required": ["filter", "value"]
  }
}
```

## Resolved (operator)

| # | Topic | Decision |
|---|--------|----------|
| 1 | Grammar | DashSpec **`end keyword`**; top-level tables: **`variables`**, **`helps table`**, **`phrases table`**, **`profiles`**, **`defaults`**, **`commands table`**; kv / `pattern` sugar; **`import <…>`**; **`expand … fills`**; no nested `profile … end profile` / `tail` wire |
| 2 | CodeGen | Roslyn + `dotnet catalog emit` |
| 3 | Expand | Hybrid dotted ids; **no SID/CID** until proven need |
| 4 | Wire DTO | Rename **`CommandCatalogWire`** now |
| 5 | Arg tail | **Full `IR.Argument` link** this wave |
| 6 | CommandId | **Hard cut**, no alias period |

## Open (operator)

| # | Topic | Proposal |
|---|--------|----------|
| 7 | Executor binding | Convention + sourcegen map; optional `executor` override |
| 8 | Emit wave 1 | C# + MCP + wire TOML + conformance |

## Reference: DashSpec CommandId migration (hard cut)

| Command | New CommandId | Current |
|---------|---------------|---------|
| Show host | `dash.host.show` | `dash.show.surface` |
| Date filter | `dash.filter.date` | `dash.select.filter.date` |
| Field filter | `dash.filter.field` | `dash.select.filter.{name}` |
| Report | `dash.report.select` | `dash.select.report` |
| Page | `dash.page.select` | `dash.select.page` |
| View | `dash.card.view` | `dash.view.card` |

## Non-goals

- Filesystem path semantics in `.catalog`
- FluentValidation-style authoring
- Planet-local grammar forks
- SID/CID recipe type system (v0)

## Consumers

| Layer | Action |
|-------|--------|
| **IR.Command** | `CommandCatalogEntry`, `InvocationPhrase`, rename `CommandCatalogWire` |
| **IR.Argument** | linked `ArgTailProfile` |
| **Authoring.Command.Catalog** | `.catalog` parser (`phrases`, `commands table`, …) |
| **CommandPlane.Catalog.CodeGen** | Roslyn + emit CLI |
| **DashSpec / Forge / CIDE** | `<planet>.catalog` instance |
