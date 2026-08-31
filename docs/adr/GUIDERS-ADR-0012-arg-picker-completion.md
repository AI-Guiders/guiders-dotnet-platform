# GUIDERS-ADR-0012: Slash arg picker completion

| | |
|---|---|
| **Status** | Accepted |
| **Date** | 2026-08-29 |
| **Relates to** | GUIDERS-ADR-0009 · GUIDERS-ADR-0011 · GUIDERS-ADR-0035 · DASHSPEC-ADR-0043 |

## Context

ADR-0011 ships path-step completion (domain → object → intent). Descriptor wire already had `ArgTail = picker:…` and `ArgPickerChoices`, but arg-step UI was product-local (Forge `forge-slash-resolve.js`). That duplicates peel/filter logic and blocks headless/agent parity for closed enumerations.

## Decision

### 1. Platform owns arg-step completion

| Layer | Owns |
|-------|------|
| **Platform** | `SlashArgCompletion`, static `ArgPickerChoices` filter, dynamic `ISlashPickerChoiceSource` hook |
| **Product** | Descriptor values (`ArgPickerChoices`, `picker:<id>`), adapter implementing `ISlashPickerChoiceSource` |
| **Surface** | Popover chrome, debounce, accept-key — unchanged (ADR-0011) |

`SlashStepCompletion.GetSuggestions(catalog, body, pickerSource?)` tries arg completion **before** path segments when the typed body resolves to a catalog row with non-`none` arg tail.

### 2. Static closed enum

```csharp
new SlashCommandDescriptor {
    Path = "format mode",
    CommandId = "editor.format.mode",
    ArgTail = "picker:enum:text_mode",
    ArgPickerChoices = SlashPickerChoices.FromLabels(("md", "Markdown"), ("html", "HTML")),
};
```

Platform filters choices by arg-tail partial. Completion items use `Kind = Picker`, `PickValue`, `InsertText = "/format mode md"`.

Helpers: `SlashPickerChoices.FromValues`, `FromLabels`, `FromEnum<TEnum>()`.

### 3. Dynamic picker

When `ArgTail = picker:<id>` and **no** static `ArgPickerChoices`, platform calls:

```csharp
public interface ISlashPickerChoiceSource {
    IReadOnlyList<SlashPickerChoice> GetChoices(string pickerId, string partial);
}
```

Product examples: `picker:dash.field.app` → distinct values; Forge may keep HTTP suggest internally but SHOULD converge on the same interface for C# hosts.

### 4. Wire / HTTP

`SlashCompletionItem` gains `Kind` + `PickValue`. Existing JSON fields remain; surfaces MAY render picker rows differently when `kind = picker`.

`SlashRouteEntry` carries `ArgTail`, `ArgPickerChoices`, and `ArgHint` from descriptor.

### 5. Input guidance (`SlashInputGuidance`)

`SlashCompletion.GetResult(catalog, body, pickerSource?)` returns items **and** guidance:

| `SlashInputMode` | When | Placeholder / hint |
|------------------|------|---------------------|
| `Path` | completing command path | `Next: <segment>` |
| `Picker` | closed / dynamic picker | pick or filter |
| `FreeText` | `required` without picker | `Type value (free text)` or `ArgHint` |
| `Optional` | `optional` arg tail | Enter without arg |
| `Ready` | `IsRunnable` | Press Enter to run |

Surfaces show `Breadcrumb` (`/select › date › today`) + mode badge; no product-specific peel.

## Non-goals

- Platform UI widgets (popover layout stays product)
- Replacing Forge remote suggest HTTP in this wave
- Free-text `required` arg coaching (no fake picker)

## Consequences

- CommandPlane **0.4.3+**
- DashSpec date presets → static `ArgPickerChoices`; field filters → `ISlashPickerChoiceSource`
- Forge JS picker peel can shrink over time to thin adapter over same completion API

## Quarry wave

| Wave | Scope |
|------|-------|
| **W4 platform** ✓ | `SlashArgCompletion`, `ISlashPickerChoiceSource`, route refactor, tests |
| **W5 products** | DashSpec picker wiring; Forge converge JS → `/commands/complete` picker rows |
| **W5a conformance** ✓ | `slash-arg-completion.spec.json` + schema + `SlashSpecConformance` ([ADR-0018](GUIDERS-ADR-0018-slash-conformance-vectors.md)) |
| **W5b Forge JS** | vitest harness on same spec |
