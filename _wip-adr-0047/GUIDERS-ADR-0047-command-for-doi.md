# GUIDERS-ADR-0047: Command authoring — DOI-first, typed IR

| | |
|---|---|
| **Status** | Accepted (grammar evolves — implementation wave 2026-09-01) |
| **Level** | **Federation hyperlane** — not planet-local DX |
| **Date** | 2026-08-31 |
| **Tags** | #guiders #federation #commandplane #catalog #doi #ir #guild #dx |
| **Related** | GUIDERS-ADR-0006 · GUIDERS-ADR-0009 · GUIDERS-ADR-0021 · GUIDERS-ADR-0042 · GUIDERS-ADR-0045 · GUIDERS-ADR-0046 · [GUIDERS-ADR-0048](../_wip-adr-0048/GUIDERS-ADR-0048-authoring-quarry-family.md) · ADR-0154 |

## Context

`CommandCatalogWire` (today `CommandDescriptor`) and `CatalogRouteEntry` carry **DOI** and slash grammar as raw `string` fields. Planets duplicate vocabulary in builders, path helpers, parsers, and executors.

**Human–agent DX parity:** federation declares X once on the typed spine; planets reference instance `.catalog` files, never re-stringify. **Автор `.catalog` явно объявляет нотацию** для каждого вида wire в файле — читатель (человек, агент, LSP) знает, **в каком алфавите** записаны gestures, melody-slug и line templates, без догадок и без молчаливых дефолтов парсера.

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
| `channels` / `channels table` | `end channels` |
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
| `scope` | **где** команда видна (`dashboard`, …) — [ADR-0044](GUIDERS-ADR-0044-command-catalog-scope.md) | product area |
| `surfaces` | **где в UI** видна команда — federation tag (`slash`, `console`, `ccl.filter`, …) → IR `CommandDescriptor.Surfaces` | mechanics (`binding`, `melody`); MCP; planet prefix `dash-*` |
| `bindings` | **keyboard mechanic** — gesture → `commandId` ([ADR-0015](GUIDERS-ADR-0015-invocation-mechanics-slash-melody-binding.md)) | federation surface |
| `melodies` | **keyboard mechanic** — slug-lane после chord root → `commandId` | federation surface; palette `c:` = discoverability only |
| `mcp` / `projections` | **agent projection** — `CallTool` schema ([ADR-0009](GUIDERS-ADR-0009-command-surface-pattern.md) MCP row) | human invoker surface |
| `channels` | **планета:** surface/sub + `planet-id` + **`grammar` block** | surface id как grammar id (`slash` ≠ `command-slash`) |
| `notation` | **алфавит записи** wire в этом файле → `Notations.*` ([ADR-0021](GUIDERS-ADR-0021-notations-quarry-family.md)); автор **обязан** объявить — читатель знает, чего ждать | UI surface; молчаливый reader |
| `helps` | **верхний уровень** — соответствие *entity* + *field* → *text* (`helps table`) | inline prose в `commands` |
| `expand` | recipe runtime; `fills` — какие variables подставляет | — |
| `defaults` | **явные** дефолты: `variable.kind`, `command.scope`, `command.surfaces`, `grammar.keyboard.*`, `binding.chord-root` | молчаливый parser default |

Семейство **верхний уровень kv/table**: `variables`, `helps`, `profiles`, **`phrases`**, `defaults`, `commands`, `bindings`, `melodies`, `mcp`, `executors` — см. §4.1.

#### 4.1 Authoring surfaces (DSL sections) — где ещё kv/table

> **Омоним:** здесь *surface* = **секция файла** (variables, helps, commands…). **Federation surfaces** (invoker: slash, console, …) — отдельная ось; см. блок **Federation surfaces** ниже.

Один принцип: **декларация матрицей** — `variables`, **`helps table`**, `profiles`, **`phrases table`**, **`commands table`**. Dotted kv / `pattern` block — sugar → row.

| Surface | v0 | Содержимое | Зачем table |
|---------|-----|------------|-------------|
| **`variables`** | ✅ | phrase-слоты `{filter}`… | словарь слотов |
| **`helps`** | ✅ | *target* + *field* → *text* | entity/object ↔ копирайт |
| **`profiles`** | ✅ | arg-меню, bundle | preset/constructor rows |
| **`phrases`** | ✅ | *name* → *phrase* | slash templates |
| **`defaults`** | ✅ | `command.*`, **`variable.kind`** — явные дефолты поверхностей | без молчаливого kind |
| **`commands`** | ✅ | phrase, profile, expand, fills… | **главная матрица** каталога |
| **`bindings`** | 📋 | gesture → `commandId` | keyboard Binding mechanic |
| **`melodies`** | 📋 | slug → `commandId` (после chord root) | keyboard Melody mechanic |
| **`mcp`** | 📋 | `command` → tool exposure / schema | agent projection (не surface) |
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

**Federation surfaces** — **где** в UI человек вызывает команду (slash-bar, filter CCL, palette). **Не** notation ([ADR-0021](GUIDERS-ADR-0021-notations-quarry-family.md)): surface = монтирование invoker; notation = **алфавит записи** wire (как Vim vs Emacs для клавиш, `command-slash` vs `command-console` для текста).

**Пять осей (не смешивать):**

| Ось | Вопрос | Пример | В `.catalog` |
|-----|--------|--------|--------------|
| **`scope`** | где видна команда? | `dashboard` | `command.scope` |
| **`surfaces`** | какой UI invoker? | `slash.bar`, `ccl.filter` | `command.surfaces`, `channels` |
| **`grammar`** | **какой string-grammar** у wire на этом sub? | `command-slash` + `argument-kv` | `channels` → `grammar` block; `grammar.keyboard.*` in `defaults` |
| **mechanics** | клавиатурная механика без line UI? | Binding, Melody | `bindings`, `melodies` |
| **projections** | агент? | MCP tool | `mcp` |

`binding` / `melody` — **mechanics** ([ADR-0015](GUIDERS-ADR-0015-invocation-mechanics-slash-melody-binding.md)), не surfaces. `mcp` — **projection**, не surface.

**Surface families** (UI invoker, без префикса планеты):

| Family | Смысл | Engage | v0 |
|--------|-------|--------|-----|
| `slash` | typed path line (`/` sigil) | Sigil | ✅ |
| `console` | REPL / filter bar (sigil planet) | Sigil | ✅ |
| `palette` | command palette peel | DiscoverabilityPrefix | ✅ |
| `ccl` | contextual command line | Sigil | ✅ |

**Grammar registry** (federation id → `Notations.*` + `docs/grammar/notation/`; SSOT code: `NotationGrammarRegistry`):

| Branch | Federation id (v0) | Пакет | Пример wire |
|--------|-------------------|-------|-------------|
| Command | `command-slash` | `Notations.Command.Slash` | `/filter date today` |
| Command | `command-console` | `Notations.Command.Console` | `select filter usage_date=today` |
| Argument | `argument-slash` | `Notations.Argument.Slash` | tail после path, пробелы |
| Argument | `argument-kv` | `Notations.Argument.Kv` | `filter=usage_date value=today` |
| Keyboard | `keyboard-vim` | `Notations.Keyboard.Vim` | `<C-k>`, `j` `k` |
| Keyboard | `keyboard-emacs` | `Notations.Keyboard.Emacs` | `C-x C-s` |
| Keyboard | `keyboard-key-gesture` | `Notations.Keyboard.KeyGesture` | `Ctrl+K`, `Ctrl+Shift+D` |

Melody slug-lane — тот же **Keyboard** branch; articulation by-note — planet policy ([ADR-0015](GUIDERS-ADR-0015-invocation-mechanics-slash-melody-binding.md) §7).

#### Автор объявляет grammar (contract для читателя)

`.catalog` — не только матрица команд, но и **легенда алфавитов**: автор спеки фиксирует, **каким языком записан каждый wire** в этом файле. Читатель (ревьюер, агент, codegen, LSP) **не угадывает** — смотрит объявление.

| Что в файле | Где автор объявляет | Federation id (пример) | Читатель понимает |
|-------------|---------------------|------------------------|-------------------|
| phrase / line на slash-sub | `channels` → `grammar.command` / `grammar.argument` | `command-slash`, `argument-slash` | path + tail как в Slash vs Console |
| line на ccl / console sub | то же в `channels` | `command-console`, `argument-kv` | `select filter …` vs `/filter …` |
| `bindings` gesture column | `defaults` → `grammar.keyboard.binding` | `keyboard-key-gesture` \| `keyboard-vim` | `Ctrl+Shift+D` vs `<C-S-d>` |
| `melodies` slug column | `defaults` → `grammar.keyboard.melody` | `keyboard-key-gesture` \| `keyboard-vim` | буквы `fd` vs vim-style шаги |
| `binding.chord-root` | `defaults` | *(в grammar `grammar.keyboard.binding`)* | как записан engage-жест |

**Правила:**

1. **Нет объявления — нет parse.** Подключён line-sub без `grammar` block (`command` + `argument`) → compile error. Есть `bindings` / `melodies` без `grammar.keyboard.*` → compile error.
2. **Одна спека — один контракт на секцию.** Все строки `bindings` в одной нотации; все `melodies` — в одной (override per-table — 📋 v1).
3. **Surface id ≠ notation id.** `surfaces` говорит «где в UI»; notation говорит «как **записано** в таблице».
4. **Tooling обязан показывать контракт:** LSP / `authoring validate` — summary в начале файла или hover: *«bindings: keyboard-vim; melodies: keyboard-key-gesture; ccl.filter: command-console + argument-kv»*.
5. **Сказал vim — пиши vim. Wire must match declared notation** — иначе **compile error** (не warning, не auto-fix). Parser **не** угадывает и **не** нормализует молча.

| Объявлено | Допустимый wire в ячейке | ❌ compile error |
|----------|--------------------------|-----------------|
| `grammar.keyboard.binding = keyboard-vim` | `<C-S-d>`, `j`, `k` | `Ctrl+Shift+D`, `Ctrl+K` |
| `grammar.keyboard.binding = keyboard-key-gesture` | `Ctrl+K`, `Ctrl+Shift+D` | `<C-k>`, `C-x C-s` |
| `grammar.keyboard.melody = keyboard-vim` | vim slug steps | bare `fd` без vim grammar (если не by-note profile) |
| `channels` … `grammar.command = command-slash` | phrases как slash path segments | console-style `select filter …` in phrase без desugar |
| `channels` … `grammar.argument = argument-kv` | `key=value` tails в profile wire | slash space-tail |

Диагностика (пример): `grammar-wire-mismatch: bindings row 2 — declared keyboard-vim, cell looks like KeyGesture ('Ctrl+Shift+D')`. Conformance: тот же reader, что runtime ([ADR-0021](GUIDERS-ADR-0021-notations-quarry-family.md) §9 `notation/key-gesture`, `notation/neovim-kbd`) — **authoring validate = те же векторы**, не отдельная эвристика.

Пример: planet пишет мелодии в Vim-стиле, binding — KeyGesture:

```text
defaults
  grammar.keyboard.binding = keyboard-key-gesture
  grammar.keyboard.melody  = keyboard-vim
  binding.chord-root        = Ctrl+K
end defaults

melodies table
  | slug   | command     |
  | <C-f>d | filter.date |
end melodies
```

Другой автор в том же federation id читает `grammar.keyboard.melody = keyboard-vim` и знает: slug-колонка — **Vim wire**, не «просто две буквы».

**Surface ≠ notation:** один `ccl.filter` может писать **`command-console` + `argument-kv`** (DashSpec `>` bar); другой planet — **`command-slash` + `argument-slash`**. Имена `slash` / `console` в **surfaces** — про UI; `command-slash` / `command-console` — про **грамматику строки**.

**`channels`** — wiring sub **и** явный `grammar` block:

```text
channels
  slash
    bar = toolbar-slash
    grammar
      command = command-slash
      argument = argument-slash
    end grammar
  ccl
    filter = filter-ccl
    grammar
      command = command-console
      argument = argument-kv
    end grammar
  console
    filter = filter-bar
    grammar
      command = command-console
      argument = argument-kv
    end grammar
  palette = command-palette
end channels
```

Форма B (table): колонки `surface`, `sub`, `planet-id`, `grammar.command`, `grammar.argument`.

Подключён line-sub **без** `grammar` block → **compile error** (никакого «ccl молча = console»).

**`defaults`** — явные дефолты каталога **и нотации для keyboard-секций** (авторский контракт для читателя):

```text
defaults
  grammar.keyboard.binding = keyboard-key-gesture
  grammar.keyboard.melody  = keyboard-key-gesture
  binding.chord-root        = Ctrl+K
end defaults
```

`bindings` / `melodies` парсят gesture и slug в wire формате, заданном `grammar.keyboard.*`. Смена на `keyboard-vim` — те же строки таблицы, другой reader ([ADR-0021](GUIDERS-ADR-0021-notations-quarry-family.md) §9).

**Подповерхности (sub):** federation tag `family.sub` в `command.surfaces` (`ccl.filter`). Имя **sub** = ключ в `channels`.

**`bindings`** — таблица жестов ([ADR-0017](GUIDERS-ADR-0017-binding-catalog-family.md) wire; gesture в `grammar.keyboard.binding`):

```text
bindings table
  | gesture      | command     | role        |
  | Ctrl+Shift+D | filter.date | execute     |
  | Ctrl+K       | —           | chord-root  |
end bindings
```

`role = chord-root` — engage для melody capture (не melody step). Остальные строки → прямой `commandId`.

**`melodies`** — slug-lane после chord root (не palette `c:` — тот только discoverability):

```text
melodies table
  | slug | command     |
  | fd   | filter.date |
  | fr   | filter.field |
end melodies
```

**`mcp`** — agent projection (отдельная секция; opt-in per command):

```text
mcp table
  | command     | expose |
  | filter.date | yes    |
  | host.show   | yes    |
end mcp
```

Codegen → `mcp-tools.json` + `inputSchema` из той же строки `commands` (fills/profile). Команда без строки в `mcp` → tool не эмитится (human-only).

**Не путать:** `scope` = где; `surfaces` = UI invoker; `notation` = **алфавит записи** (Vim/Emacs/Slash-path — разные id); mechanics/projections — соседние секции, тот же `commandId`.

**`defaults`** (полный блок — после `channels`):

```text
defaults
  variable.kind     = phrase-slot
  command.scope     = dashboard
  command.surfaces  = slash, palette, console.filter, ccl.filter
  grammar.keyboard.binding = keyboard-key-gesture
  grammar.keyboard.melody  = keyboard-key-gesture
  binding.chord-root        = Ctrl+K
end defaults
```

| Ключ | Применяется к | v0 значение |
|------|---------------|-------------|
| `variable.kind` | строки `variables` без `= kind` / пустая колонка `kind` | `phrase-slot` |
| `command.scope` | все строки `commands` | `dashboard` |
| `command.surfaces` | все строки `commands` | federation family или `family.sub` |
| `grammar.keyboard.binding` | `bindings` gesture column | `keyboard-key-gesture` \| `keyboard-vim` \| … |
| `grammar.keyboard.melody` | `melodies` slug column | `keyboard-key-gesture` \| … |
| `binding.chord-root` | melody capture engage | gesture wire в grammar.keyboard.binding |

**`channels` table (форма B):**

```text
channels table
  | surface | sub    | planet-id        | grammar.command | grammar.argument |
  | slash   | bar    | toolbar-slash    | command-slash    | argument-slash    |
  | console | filter | filter-bar       | command-console  | argument-kv       |
  | ccl     | filter | filter-ccl       | command-console  | argument-kv       |
  | palette |        | command-palette  |                  |                   |
end channels
```

Пустой `sub` = default sub family. Family **не** в `channels` → invoker не монтируется. `palette` — peel, line notation не требуется.

Codegen → `CommandDescriptor.Surfaces` (flat tags). Host adapter: surface tag → UI; notation pair → `Notations.*` readers. Legacy `dash-slash` / `editor-ccl` → `channels` + federation tags.

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
catalog → import → channels? → defaults → variables → helps → profiles → phrases → commands table → bindings? → melodies? → mcp? → executors?
```

v0 ship: line surfaces + `notation.*` ✅; `bindings` / `melodies` / `mcp` 📋 (grammar reserved, same file). Parser sugar: kv → table rows; `pattern … end pattern` → `phrases`; `command … end command` → `commands`.

#### Полный пример: `dash.catalog` (DashSpec)

Читать сверху вниз: **`defaults`** → словари → **`commands table`**.

```text
# ─────────────────────────────────────────────────────────────
# DashSpec.Host/Catalog/dash.catalog
# Federation grammar v0 — planet content only
# ─────────────────────────────────────────────────────────────

catalog dash

import <grain/date-filter>

channels
  slash
    bar = toolbar-slash
    grammar
      command = command-slash
      argument = argument-slash
    end grammar
  console
    filter = filter-bar
    grammar
      command = command-console
      argument = argument-kv
    end grammar
  ccl
    filter = filter-ccl
    grammar
      command = command-console
      argument = argument-kv
    end grammar
  palette = command-palette
end channels

defaults
  variable.kind      = phrase-slot
  command.scope      = dashboard
  command.surfaces   = slash.bar, palette, console.filter, ccl.filter
  grammar.keyboard.binding = keyboard-key-gesture
  grammar.keyboard.melody  = keyboard-key-gesture
  binding.chord-root        = Ctrl+K
end defaults

# ── Словарь phrase-слотов (kind из defaults, если не указан) ─
variables
  filter
  report
  page
  card
  view
  surface
end variables

# variables table
#   | name   | kind        |   ← kind опционален при defaults.variable.kind
#   | filter |             |
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

Как **`helps`**: один блок на файл → `CatalogVariableIndex`. **Kind** — из строки, иначе из **`defaults.variable.kind`** (обязательно объявить в `defaults`, не молчаливый parser default).

**Форма A — имена (dash v0):**

```text
defaults
  variable.kind = phrase-slot
end defaults

variables
  filter
  report
end variables
```

**Форма B — явный kind (override defaults):**

```text
variables
  filter = phrase-slot
  report = phrase-slot
end variables
```

**Форма C — table:**

```text
variables table
  | name   | kind        |
  | filter |             |
  | report | phrase-slot |
end variables
```

Пустая ячейка / строка без `=` → merge `defaults.variable.kind`.

| `kind` (v0) | Значение |
|-------------|----------|
| `phrase-slot` | плейсхолдер в `{…}` внутри `phrase` |

Parser: `{foo}` в `phrase` без строки в `variables` → **compile error**. Отсутствует `defaults.variable.kind` и нет kind на строке → **compile error**. Codegen → `{Planet}Vocabulary.g.cs`.

Подписи для слотов — в **`helps table`**: `variable filter` + `label` (не в `variables`).

| Где | Синтаксис | Соответствие |
|-----|-----------|--------------|
| `channels` | nested / table | surface/sub + `planet-id` + **`grammar` block** |
| `defaults` | kv | `variable.kind`, `command.scope`, `command.surfaces`, `grammar.keyboard.*`, `binding.chord-root` |
| `bindings` | **table** | gesture → `commandId` (+ `chord-root`) |
| `melodies` | **table** | slug → `commandId` |
| `mcp` | **table** | `command` → agent expose |
| `variables` | table / kv / name-only | *name* → *kind* (merge из `defaults.variable.kind`) |
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

`commands table` **не содержит** prose — только phrase/profile/expand (+ merge из `defaults` для scope/surfaces). Копирайт — в `helps`.

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
| `surfaces` | нет | override `defaults.command.surfaces` (federation id) |

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
