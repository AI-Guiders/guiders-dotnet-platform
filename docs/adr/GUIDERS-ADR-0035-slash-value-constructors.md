# GUIDERS-ADR-0035: Slash value constructors (guided arg assembly)

| | |
|---|---|
| **Status** | Accepted |
| **Date** | 2026-08-31 |
| **Tags** | #guiders #commandplane #slash #picker #constructor #dashspec |
| **Relates to** | GUIDERS-ADR-0009 · GUIDERS-ADR-0010 · GUIDERS-ADR-0011 · GUIDERS-ADR-0012 · GUIDERS-ADR-0018 · DASHSPEC-ADR-0043 · DASHSPEC-ADR-0044 |

## Context

[GUIDERS-ADR-0012](GUIDERS-ADR-0012-arg-picker-completion.md) covers **closed** arg pickers (`picker:enum:*`, `picker:<id>` + `ISlashPickerChoiceSource`). Surfaces enter `SlashInputMode.Picker` and the user chooses a finished wire token.

Many commands need **typed values** that are not a small enum:

- calendar date / date range (`2026-08-01..2026-08-31`)
- durations, numeric bounds, structured tails

Today the fallback is `SlashInputMode.FreeText` with an `ArgHint` (“type YYYY-MM-DD…”). That forces the user to remember product wire grammar and locale-agnostic formats. DashSpec date filters illustrate the gap: presets (`today`, `last-week`) appear in the picker, but range construction is documented only in hints while the executor already accepts `from..to`.

We need a **third arg-step mechanic** between picker and free text: **value constructor** — stepwise assembly of a canonical wire value with human-friendly display.

Related: ephemeral slash UI state (draft tail, highlight targets) MUST stay isolated from product page render trees ([DASHSPEC-ADR-0043](https://github.com/AI-Guiders/dash-spec/blob/main/design/DASHSPEC-ADR-0043-filter-command-palette.md) `CommandSession` pattern). Constructor drafts belong to the same **surface session** layer, not committed command context.

## Decision

### 1. Pattern: Constructor sits on ArgTail

Extend the CommandPlane arg-tail vocabulary:

| Wire | Kind | Meaning |
|------|------|---------|
| `picker:enum:<id>` | Picker | closed list (ADR-0012) ✓ |
| `picker:<id>` | Picker | dynamic list via `ISlashPickerChoiceSource` ✓ |
| **`constructor:<id>`** | **Constructor** | guided multi-step assembly |
| **`picker+constructor:…`** | **Composite** | presets in picker + virtual “build…” rows |

Composite form (recommended for date filters):

```text
argTail = picker+constructor:date_preset+date_range
```

Static presets remain `ArgPickerChoices`. Platform injects **virtual constructor entries** (e.g. “Выбрать период…”) alongside enum rows. Accepting a virtual row switches mode; accepting a preset commits immediately (unchanged ADR-0012 behaviour).

### 2. Platform owns mechanics; product owns semantics

| Layer | Owns |
|-------|------|
| **Platform** | `SlashInputMode.Constructor`, constructor session, step orchestration, step suggestions API, wire emission contract, conformance vectors |
| **Product** | `ISlashValueConstructor` implementations, descriptor `constructor` block, display locale / step sequences |
| **Surface** | Popover/table chrome, accept-key, breadcrumb rendering — unchanged (ADR-0009) |

```text
Catalog descriptor
  argTail = picker+constructor:date_preset+date_range
  argPickerChoices = [today, last-week, …]
  constructors = [{ id: date_range, label: "Выбрать период…", … }]
        │
        ▼
SlashStepCompletion / SlashArgCompletion
  ├─ Picker phase → SlashInputMode.Picker
  └─ virtual row accepted → SlashConstructorSession
        │
        ▼
ISlashValueConstructor (product)
  GetStepSuggestions(draft, stepIndex, partial)
  TryEmitWire(draft, out wireValue)
        │
        ▼
ArgTail on context → Registry.TryExecute (unchanged)
```

**Rule:** constructor emits the **same wire string** the command would accept from free text. No second executor path.

### 3. `SlashInputMode.Constructor`

Add to `SlashInputMode` ([ADR-0012 §5](GUIDERS-ADR-0012-arg-picker-completion.md)):

| Mode | When | User action |
|------|------|-------------|
| `Constructor` | `SlashConstructorSession` active | pick next step segment (year → month → day → …) |
| `ConstructorRange` | optional sub-kind in guidance | between range endpoints, show `..` separator step |

Guidance fields:

- `Breadcrumb` — includes partial display buffer (`31.08.2026 .. 15.09.`)
- `Placeholder` — next step label (`Год`, `Месяц`, `День`, `Конец периода`)
- `NextStepHint` — constructor-specific hint

### 4. Product contract: `ISlashValueConstructor`

```csharp
public interface ISlashValueConstructor
{
    string ConstructorId { get; }
    SlashConstructorKind Kind { get; }

    IReadOnlyList<SlashConstructorStepDefinition> Steps { get; }

    IReadOnlyList<SlashCompletionItem> GetStepSuggestions(
        SlashConstructorDraft draft,
        int stepIndex,
        string partial);

    bool TryEmitWire(SlashConstructorDraft draft, out string wireValue, out string? error);
}

public enum SlashConstructorKind
{
    Scalar,
    Range,      // two sub-values + separator token
    Collection, // future: [a, b, c]
}
```

Platform registry:

```csharp
public interface ISlashValueConstructorRegistry
{
    bool TryGet(string constructorId, out ISlashValueConstructor constructor);
}
```

Descriptor block (JSON/TOML/XML via existing catalog sources):

```json
{
  "constructors": [
    {
      "id": "date_range",
      "label": "Выбрать период…",
      "kind": "range",
      "displayFormat": "dd.MM.yyyy",
      "wireFormat": "yyyy-MM-dd..yyyy-MM-dd",
      "separator": "..",
      "steps": ["year", "month", "day", "separator", "year", "month", "day"]
    }
  ]
}
```

`steps` MAY be omitted — product constructor supplies default sequence. Platform only requires ordered step indices and a draft bag.

### 5. Display format ≠ wire format

| Facet | Purpose | Example |
|-------|---------|---------|
| **Display** | what user sees while constructing | `31.08.2026 .. 15.09.2026` (`dd.MM.yyyy`) |
| **Wire** | canonical `ArgTail` for `Execute` | `2026-08-31..2026-09-15` |

Platform stores both in draft:

```csharp
public sealed class SlashConstructorDraft
{
    public string ConstructorId { get; init; }
    public int StepIndex { get; set; }
    public string DisplayBuffer { get; set; }  // human segments + separators
    public string WireBuffer { get; set; }     // canonical partial
    public IReadOnlyList<string> CompletedSteps { get; set; }
}
```

Surfaces render `DisplayBuffer` in the input / breadcrumb; completion rows show localized labels. **`TryEmitWire`** is the only gate to `SlashInputMode.Ready`.

Configurable `displayFormat` is a **product/catalog** concern (BCP 47 / .NET format string). Platform passes it through descriptors; default step labels are product-localized.

### 6. Virtual picker rows

Extend `SlashPickerChoice` (non-breaking):

```csharp
public sealed class SlashPickerChoice
{
    public string Value { get; init; }
    public string? Label { get; init; }
    public string? Hint { get; init; }
    public SlashPickerChoiceKind Kind { get; init; } = SlashPickerChoiceKind.Value;
}

public enum SlashPickerChoiceKind
{
    Value,        // commits wire token (ADR-0012)
    Constructor,  // opens SlashConstructorSession; Value = constructor id
}
```

`SlashArgCompletion` merges static choices + virtual constructor rows from descriptor. Filter partial against `Label`/`Hint` only for `Value` rows; constructor rows always visible at tail unless filtered by label.

### 7. Session isolation (`SlashConstructorSession`)

Constructor state is **ephemeral surface state** (scoped per host circuit / editor session):

- active constructor id + draft
- `DraftChanged` / `StepChanged` events
- cleared on Execute, Escape, or surface deactivate

Must **not** trigger product page/data re-render (same boundary as slash highlight session). Surfaces subscribe; heavy views do not.

Orchestration entry:

```csharp
SlashCompletionResult GetResult(
    SlashCatalogIndex catalog,
    string body,
    ISlashPickerChoiceSource? pickerSource,
    ISlashValueConstructorRegistry? constructors,
    SlashConstructorSession? constructorSession);
```

When `constructorSession.IsActive`, platform skips path/picker phases and delegates to `GetStepSuggestions`.

### 8. Completion items in constructor phase

Reuse `SlashCompletionItem` with `Kind = ConstructorStep` (new) or `Picker`:

- `InsertText` — append display segment + trailing space when step completes
- `PickValue` — wire segment fragment
- `StepSegment` — e.g. `2026`, `08`, `31`

Accept policy: same as picker (Tab / Ctrl+Space surface policy). Partial filter applies within current step only (e.g. typing `20` narrows years).

### 9. Conformance ([ADR-0018](GUIDERS-ADR-0018-slash-conformance-vectors.md))

New vector family `slash-value-constructor.spec.json`:

- picker lists presets + virtual constructor row
- step sequences yield expected `DisplayBuffer` / `wireValue`
- invalid calendar combinations rejected at step or emit gate
- Escape / back navigates steps (optional W2)

Platform CI runs vectors headless; DashSpec/Forge adapters add product-specific constructors to the same schema.

## Non-goals

- Platform date/calendar widgets (Blazor/Avalonia popover UI)
- Replacing locale-aware **committed** filter widgets on the dashboard toolbar
- Natural-language dates (“last Friday”) — NL belongs outside constructor mechanics
- Agent MCP auto-stepping constructors in v1 (agents SHOULD emit wire tokens directly; constructor is human-primary UX per FORGE-ADR-0025)
- Breaking existing `picker:enum:*` descriptors

## Consequences

- **CommandPlane.Slash** gains constructor session + registry + extended `SlashInputMode`
- **SlashCommandDescriptor** / route entry gains optional `Constructors[]`
- **DashSpec** first adopter: date range constructor ([DASHSPEC-ADR-0044](https://github.com/AI-Guiders/dash-spec/blob/main/design/DASHSPEC-ADR-0044-date-filter-value-constructor.md))
- Forge / CIDE: follow with revision-range, path pickers, etc.
- ADR-0012 non-goal “free-text coaching” remains; constructors are the structured alternative

## Quarry wave

| Wave | Scope |
|------|-------|
| **W1 platform** | ADR + types (`SlashConstructorDraft`, `ISlashValueConstructor`, registry), `SlashInputMode.Constructor`, virtual picker rows, unit tests |
| **W1a conformance** | `slash-value-constructor.spec.json` + `SlashSpecConformance` hook |
| **W2 DashSpec** | `DateRangeValueConstructor`, composite `picker+constructor` on date filters, CCL UX |
| **W3 session lift** | Generic `SlashCommandSession<TProjection>` + constructor session merge (optional refactor) |
| **W4 Forge/CIDE** | product constructors; JS complete API parity |

## Anti-patterns

- Constructor that calls `Execute` directly — must emit wire, then surface commits through registry
- Duplicating wire parsing in constructor **and** command — command parser stays SSOT
- Constructor steps that re-render product data views on each keystroke
- Hard-coding `dd.MM.yyyy` in platform — display format is catalog/product
- Fake picker rows that insert invalid partial wire (`2026-08-` without completion gate)

## Prior art

| Doc | Relevance |
|-----|-----------|
| ADR-0012 | picker baseline; FreeText gap this ADR closes |
| ADR-0011 | step completion orchestration |
| ADR-0009 | surface vs command boundary |
| DASHSPEC-ADR-0043 §3 | date wire grammar (`today`, `YYYY-MM`, `from..to`) |
| DashSpec `DashboardCommandSession` | ephemeral surface state prototype for session isolation |
