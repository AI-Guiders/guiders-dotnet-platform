# GUIDERS-ADR-0035: Slash value constructors (guided arg assembly)

| | |
|---|---|
| **Status** | Accepted |
| **Date** | 2026-08-31 |
| **Tags** | #guiders #commandplane #slash #picker #constructor #dashspec |
| **Relates to** | GUIDERS-ADR-0009 · GUIDERS-ADR-0010 · GUIDERS-ADR-0011 · GUIDERS-ADR-0012 · GUIDERS-ADR-0018 · DASHSPEC-ADR-0043 · DASHSPEC-ADR-0044 |

## Context

[GUIDERS-ADR-0012](GUIDERS-ADR-0012-arg-picker-completion.md) covers **closed** arg pickers (`picker:enum:*`, `picker:<id>` + `ICommandPickerChoiceSource`). Surfaces enter `ArgInputMode.Picker` and the user chooses a finished wire token.

Many commands need **typed values** that are not a small enum:

- calendar date / date range (`2026-08-01..2026-08-31`)
- durations, numeric bounds, structured tails

Today the fallback is `ArgInputMode.FreeText` with an `ArgHint` (“type YYYY-MM-DD…”). That forces the user to remember product wire grammar and locale-agnostic formats. DashSpec date filters illustrate the gap: presets (`today`, `last-week`) appear in the picker, but range construction is documented only in hints while the executor already accepts `from..to`.

We need a **third arg-step mechanic** between picker and free text: **value constructor** — guided assembly of a canonical wire value with human-friendly display.

Constructors form a **tree**, not a flat step list: composite nodes (Range, Date) contain child constructors; only **leaf** nodes expose pickable segments (Year, Month, Day). The arg phase before entering a constructor offers **Free text** and root constructors side by side (presets remain instant picker values).

Related: ephemeral slash UI state (draft tail, highlight targets) MUST stay isolated from product page render trees ([DASHSPEC-ADR-0043](https://github.com/AI-Guiders/dash-spec/blob/main/design/DASHSPEC-ADR-0043-filter-command-palette.md) `CommandSession` pattern). Constructor drafts belong to the same **surface session** layer, not committed command context.

## Decision

### 0. Arg phase before constructor (entry menu)

When the command path is complete and `ArgTail` allows construction, the user sees an **arg entry menu** — not only enum presets:

```text
Arg entry (ArgInputMode.Picker + escape hatches)
├── today, last-week, …          ← preset Value rows (instant wire)
├── Range                          ← root Constructor row → enters tree
└── (implicit) Free text           ← ArgInputMode.FreeText; user types wire directly
```

| Row | Mode after accept | Result |
|-----|-------------------|--------|
| Preset | `Ready` | wire token immediately |
| Root constructor (e.g. Range) | `Constructor` | navigate constructor tree |
| Free text | `FreeText` | no constructor; `ArgHint` + manual typing |

**Rule:** Free text is always available as sibling escape hatch — constructors are optional structured path, not a cage.

### 0.1 Constructor hierarchy (composite tree)

Constructors are **composable**. A composite node delegates to child constructors; leaves expose ordered segments.

Example — date range (DashSpec):

```text
Range                              ← composite (root)
├── Date (from)                    ← composite (child slot)
│   ├── Year                       ← leaf segment
│   ├── Month                      ← leaf segment
│   └── Day                        ← leaf segment
├── ..                             ← separator (auto, not a pick row)
└── Date (to)                      ← composite (child slot)
    ├── Year
    ├── Month
    └── Day
```

Navigation = **cursor in tree**: `(nodePath[], leafStepIndex)`. Accepting a leaf segment advances within the leaf; completing a leaf advances to the next sibling slot or separator; completing the root emits wire.

Display builds depth-first: `31.08.2026 .. 15.09.2026`. Wire builds in parallel: `2026-08-31..2026-09-15`.

Same `Date` constructor is **reused** under Range (from/to), under `date_single` (scalar), and under grain variants (`Year → Month` only) — product defines the tree; platform walks it.

### 1. Pattern: Constructor sits on ArgTail

Extend the CommandPlane arg-tail vocabulary:

| Wire | Kind | Meaning |
|------|------|---------|
| `picker:enum:<id>` | Picker | closed list (ADR-0012) ✓ |
| `picker:<id>` | Picker | dynamic list via `ICommandPickerChoiceSource` ✓ |
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
| **Platform** | `ArgInputMode.Constructor`, constructor session, **tree navigation**, leaf step suggestions, wire emission, conformance vectors |
| **Product** | Constructor **catalog** (composite + leaf defs), reusable child constructors (`date`, `date_range`), display locale |
| **Surface** | Popover/table chrome, accept-key, breadcrumb rendering — unchanged (ADR-0009) |

```text
Catalog descriptor
  argTail = picker+constructor:date_preset+date_range
  argPickerChoices = [today, last-week, …]
  constructors = [{ id: date_range, label: "Выбрать период…", … }]
        │
        ▼
SlashStepCompletion / SlashArgCompletion
  ├─ Arg entry: presets + root constructors + FreeText escape
  └─ Constructor row accepted → ArgConstructorSession (tree cursor)
        │
        ▼
ValueConstructorNavigator (platform)
  composite node → enter child slot
  leaf node → pick segment (year / month / day)
        │
        ▼
ArgTail on context → Registry.TryExecute (unchanged)
```

**Rule:** constructor emits the **same wire string** the command would accept from free text. No second executor path.

### 3. `SlashInputMode` at arg step

| Mode | When | User action |
|------|------|-------------|
| `Picker` | arg entry menu | preset or root constructor row |
| `FreeText` | user skipped constructors | type wire manually (`ArgHint`) |
| `Constructor` | inside tree | pick next **leaf** segment at cursor |
| `Ready` | `TryEmitWire` succeeded | Enter executes |

Range is a **composite constructor**, not a separate input mode.

Guidance while in `Constructor`:

- `Breadcrumb` — tree path + partial display (`… › from › 31.08. › to ›`)
- `Placeholder` — current leaf segment (`Год`, `Месяц`, `День`) or child slot (`Дата (с)`)
- `NextStepHint` — constructor-specific hint

### 4. Product contract: constructor catalog (composite tree)

Two node shapes in one registry:

```csharp
public interface IValueConstructorRegistry
{
    bool TryGet(string constructorId, out ConstructorDefinition definition);
}

public abstract record ConstructorDefinition(string Id, string? Label);

/// <summary>Ordered leaf segments (Year, Month, Day).</summary>
public sealed record LeafConstructorDefinition(
    string Id,
    string? Label,
    IReadOnlyList<ConstructorSegmentDefinition> Segments)
    : ConstructorDefinition(Id, Label);

/// <summary>Child slots + separators (Range).</summary>
public sealed record CompositeConstructorDefinition(
    string Id,
    string? Label,
    IReadOnlyList<ConstructorSlotDefinition> Slots)
    : ConstructorDefinition(Id, Label);

public sealed record ConstructorSlotDefinition(
    string SlotId,
    string ConstructorId,
    string? Label,
    string? SeparatorBefore = null);  // e.g. ".." inserted before this slot in wire/display
```

Platform tree walk:

```csharp
public interface IValueConstructorNavigator
{
    IReadOnlyList<ArgCompletionItem> GetSuggestions(
        ArgConstructorDraft draft,
        string partial);

    bool TryAdvance(ArgConstructorDraft draft, string pickedSegment, out ArgConstructorDraft next);

    bool TryEmitWire(ArgConstructorDraft draft, out string wireValue, out string? error);
}
```

Product registers **definitions**; platform owns cursor + navigation. Product MAY supply custom navigator per id for exotic grammars.

Descriptor block:

```json
{
  "constructors": [
    {
      "id": "date",
      "segments": ["year", "month", "day"],
      "displayFormat": "dd.MM.yyyy",
      "wireFormat": "yyyy-MM-dd"
    },
    {
      "id": "date_range",
      "label": "Период…",
      "slots": [
        { "slot": "from", "constructor": "date", "label": "Дата (с)" },
        { "slot": "to", "constructor": "date", "label": "Дата (по)", "separatorBefore": ".." }
      ],
      "wireFormat": "{from}..{to}"
    }
  ]
}
```

Shared `date` leaf — reused under range, single-date, and grain-truncated variants (drop trailing segments in a derived leaf def).

### 5. Display format ≠ wire format

| Facet | Purpose | Example |
|-------|---------|---------|
| **Display** | what user sees while constructing | `31.08.2026 .. 15.09.2026` (`dd.MM.yyyy`) |
| **Wire** | canonical `ArgTail` for `Execute` | `2026-08-31..2026-09-15` |

Platform stores both in draft:

```csharp
public sealed class ArgConstructorDraft
{
    public string RootConstructorId { get; init; }
    public IReadOnlyList<string> NodePath { get; set; }  // e.g. ["from"] → ["to"]
    public int SegmentIndex { get; set; }                  // index within current leaf
    public string DisplayBuffer { get; set; }
    public string WireBuffer { get; set; }
    public IReadOnlyDictionary<string, string> CompletedSlots { get; set; }  // slotId → wire
}
```

Surfaces render `DisplayBuffer` in the input / breadcrumb; completion rows show localized labels. **`TryEmitWire`** is the only gate to `ArgInputMode.Ready`.

Configurable `displayFormat` is a **product/catalog** concern (BCP 47 / .NET format string). Platform passes it through descriptors; default step labels are product-localized.

Per-segment zero-padding (e.g. ISO month/day) is declared on `ConstructorSegmentDefinition` via optional `WireMinWidth` / `DisplayMinWidth`. Platform applies padding when emitting wire/display; no domain-specific segment ids in navigator code.

### 6. Virtual picker rows

Extend `CommandPickerChoice` (non-breaking):

```csharp
public sealed class CommandPickerChoice
{
    public string Value { get; init; }
    public string? Label { get; init; }
    public string? Hint { get; init; }
    public CommandPickerChoiceKind Kind { get; init; } = CommandPickerChoiceKind.Value;
}

public enum CommandPickerChoiceKind
{
    Value,        // commits wire token (ADR-0012)
    Constructor,  // opens ArgConstructorSession; Value = constructor id
}
```

`SlashArgCompletion` merges static choices + virtual constructor rows from descriptor. Filter partial against `Label`/`Hint` only for `Value` rows; constructor rows always visible at tail unless filtered by label.

### 7. Session isolation (`ArgConstructorSession`)

Constructor state is **ephemeral surface state** (scoped per host circuit / editor session):

- active constructor id + draft
- `DraftChanged` / `StepChanged` events
- cleared on Execute, Escape, or surface deactivate

Must **not** trigger product page/data re-render (same boundary as slash highlight session). Surfaces subscribe; heavy views do not.

Orchestration entry:

```csharp
SlashCompletionResult GetResult(
    CommandCatalogIndex catalog,
    string body,
    ICommandPickerChoiceSource? pickerSource,
    IValueConstructorRegistry? constructors,
    ArgConstructorSession? constructorSession);
```

When `constructorSession.IsActive`, platform skips path/picker phases and delegates to `GetStepSuggestions`.

### 8. Completion items in constructor phase

Reuse `ArgCompletionItem` with `Kind = ConstructorStep` (new) or `Picker`:

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
- **CommandDescriptor** / route entry gains optional `Constructors[]`
- **DashSpec** first adopter: date range constructor ([DASHSPEC-ADR-0044](https://github.com/AI-Guiders/dash-spec/blob/main/design/DASHSPEC-ADR-0044-date-filter-value-constructor.md))
- Forge / CIDE: follow with revision-range, path pickers, etc.
- ADR-0012 non-goal “free-text coaching” remains; constructors are the structured alternative

## Quarry wave

| Wave | Scope |
|------|-------|
| **W1 platform** | ADR + types (`ArgConstructorDraft`, `ISlashValueConstructor`, registry), `ArgInputMode.Constructor`, virtual picker rows, unit tests |
| **W1a conformance** | `slash-value-constructor.spec.json` + `SlashSpecConformance` hook |
| **W2 DashSpec** | `DateRangeValueConstructor`, composite `picker+constructor` on date filters, CCL UX |
| **W3 session lift** | Generic `SlashCommandSession<TProjection>` + constructor session merge (optional refactor) |
| **W4 Forge/CIDE** | product constructors; JS complete API parity |

## Anti-patterns

- Constructor that calls `Execute` directly — must emit wire, then surface commits through registry
- Duplicating wire parsing in constructor **and** command — command parser stays SSOT
- Constructor steps that re-render product data views on each keystroke
- Hard-coding `dd.MM.yyyy` in platform — display format is catalog/product
- Flattening a composite tree into one `steps[]` array in descriptors — use slots + reusable leaf defs
- Fake picker rows that insert invalid partial wire (`2026-08-` without completion gate)

## Prior art

| Doc | Relevance |
|-----|-----------|
| ADR-0012 | picker baseline; FreeText gap this ADR closes |
| ADR-0011 | step completion orchestration |
| ADR-0009 | surface vs command boundary |
| DASHSPEC-ADR-0043 §3 | date wire grammar (`today`, `YYYY-MM`, `from..to`) |
| DashSpec `DashboardCommandSession` | ephemeral surface state prototype for session isolation |
