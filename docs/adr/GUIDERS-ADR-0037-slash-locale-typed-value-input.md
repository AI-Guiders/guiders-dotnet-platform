# GUIDERS-ADR-0037: Slash locale typed value input

| | |
|---|---|
| **Status** | Accepted |
| **Date** | 2026-08-31 |
| **Tags** | #guiders #commandplane #slash #constructor #locale #i18n |
| **Relates to** | GUIDERS-ADR-0012 · GUIDERS-ADR-0035 · GUIDERS-ADR-0036 · GUIDERS-ADR-0038 · DASHSPEC-ADR-0044 · DASHSPEC-ADR-0045 |

## Context

[GUIDERS-ADR-0035](GUIDERS-ADR-0035-slash-value-constructors.md) ships guided value constructors (picker rows → segment tree → wire). Users still face two gaps:

1. **Path vs value** — Tab completes command path; typing after path should continue in **locale value** form, not wire grammar (`2026-08` vs `31.08.2026`).
2. **Culture ownership** — Platform must not impose a locale. The ambient culture (OS, browser, product chrome) defines how users type and read dates. Platform normalizes to **canonical wire** for storage and execution.

Wire-prefix disambiguation (`2026-` = month? week? day?) is the wrong layer. Ambiguity is removed by **separating input format from display format from wire format**.

## Decision

### 1. Three layers (culture is ambient, not platform-owned)

| Layer | Owner | Example |
|-------|-------|---------|
| **Input** | Ambient culture (`ISlashCultureAmbient`) | `31.08.2026` (ru-RU), `8/31/2026` (en-US) |
| **Display** | Product/surface (unlimited) | breadcrumb `31.08.2026`, chip `авг 2026`, heatmap axis |
| **Wire / storage** | Product executor SSOT | `2026-08-31`, `2026-08`, `2026-W26` |

**Rule:** Platform reads culture from host injection; default fallback is `CultureInfo.CurrentCulture`. Conformance vectors use **fixed culture** (`ru-RU`) for determinism.

### 2. Mixed mode (Tab + type)

```text
Path phase (Tab)          Value phase (locale stream)
────────────────          ───────────────────────────
select                    31.08.2026
filter            [Tab]     ↑ locale lexer
usage_date                  → canonical wire at commit
```

- **Tab / Ctrl+Space** — path segments and picker/constructor rows.
- **Typing** — locale date lexer via PAC profile ([GUIDERS-ADR-0038](GUIDERS-ADR-0038-prefix-armed-completion.md)); auto-arms constructor or reaches `Ready` when wire-complete.
- **Enter** — executes canonical wire (unchanged executor path).

### 3. Platform components

| Type | Role |
|------|------|
| `ISlashCultureAmbient` | Host-provided ambient culture (not hardcoded) |
| `SlashLocaleInputProfile` | Derived from `CultureInfo` — field order, separators |
| `SlashLocaleDateParser` | Locale partial/complete date + range parse |
| `SlashLocaleDisplayFormatter` | Canonical segments → display string |
| `SlashLocaleDatePrefixArmProfile` | PAC adapter for locale dates ([GUIDERS-ADR-0038](GUIDERS-ADR-0038-prefix-armed-completion.md)) |
| `SlashCompletionOptions` | Registry + culture + PAC profiles for `SlashCompletion.GetResult` |

### 4. `SlashInputMode.TypedInput`

New mode when path is complete and user is typing a locale value before constructor is armed or wire is ready:

- Placeholder from `SlashLocaleInputProfile` (culture short-date pattern).
- Hint: product `ArgHint` or generic “type date in locale format”.
- Suggestions: next segment values from `ISlashConstructorSegmentProvider` when constructor armed.

Constructor mode (`SlashInputMode.Constructor`) remains for explicit picker entry and in-tree navigation.

### 5. Session rules

- Constructor session **survives** arg-tail extension (typing); cancelled only on path change or Escape.
- `SlashConstructorSession.GetCompletionResult(partial)` passes typed suffix to segment provider.
- `TrySyncFromLocalePartial` bulk-fills segments when locale parse matches constructor depth.

### 6. Product responsibilities

- Register constructor catalog (`SlashValueConstructorRegistry`).
- Implement `ISlashConstructorSegmentProvider` (or use locale-aware default).
- Wire `ISlashCultureAmbient` from host (spec chrome, `RequestLocalization`, etc.).
- Executor accepts emitted wire — **no second parser**.

### 7. Conformance

New family `slash-value-constructor.spec.json` — fixed `culture: "ru-RU"`, vectors for parse, arm, emit. No clock/network.

## Consequences

- ADR-0035 §5 display≠wire extended with locale input profile.
- DashSpec: `DashboardFilterContext.Culture`, CCL does not cancel constructor on arg-tail keystrokes.
- ADR-0044 free-text wire hints deprecated in UI; locale input is primary path.

## Quarry wave

| Wave | Scope |
|------|-------|
| W1 | ADR + locale types + parser + PAC date profile + platform tests |
| W2 | DashSpec CCL wiring + segment provider culture + tests |
| W3 | Conformance pack + descriptor `arg_constructors` parsing |
