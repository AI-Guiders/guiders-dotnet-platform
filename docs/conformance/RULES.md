# Conformance determinism rules (normative)

Machine-readable vectors in `*.spec.json` are the **SSOT** for cross-ecosystem slash mechanics. Implementations (`.NET` reference quarry, JS port, …) MUST pass the same vectors; product command catalogs are **not** part of conformance.

## Scope

| In conformance | Out of conformance |
|----------------|-------------------|
| Path-step peel, static picker filter, dynamic `picker:<id>` via **stubs** | Product `commandId` / ship catalog |
| `SlashCompletionItem` shape + sort order | HTTP suggest transport, ACL, DB |
| `SlashInputGuidance` mode / hint / placeholder | Surface popover chrome |

Fixture catalogs use `fixture.*` command ids only.

## String comparison

| Field | Rule |
|-------|------|
| `body`, `insertText`, `slashPath`, `help`, `hint`, `pickValue` | **Ordinal** (case-sensitive) |
| `stepSegments` set | **OrdinalIgnoreCase** per segment; order ignored (sorted before compare) |
| `breadcrumbContains`, `placeholderContains` | **OrdinalIgnoreCase** substring |
| `mode`, `kind` | **OrdinalIgnoreCase** enum name |

## Suggestion item order

When `expect.suggestions.items` is used, actual items MUST match **exact count and order** after platform sort (`SlashCompletionSort`: `slashPath` sort key, `OrdinalIgnoreCase`).

Tie-break: stable order from catalog / picker choice declaration order in the spec fixture.

## Dynamic pickers

`pickerStubs` in the spec define `ISlashPickerChoiceSource` responses. Stub filter for partial arg tail:

- `choice.value` contains `partial` (OrdinalIgnoreCase), **or**
- `choice.label` contains `partial` (OrdinalIgnoreCase)

Platform `SlashArgCompletion` applies a second filter (`value` prefix, `label`/`hint` contains). Vectors are authored for the **combined** behavior.

## Guidance

`expect.guidance` checks only fields present in the vector. Omitted fields are not asserted.

## Versioning

- `version` in spec = document format major.
- Breaking vector or schema change → new spec file (`slash-arg-completion-v2.spec.json`) or new `tier`.
- Products pin platform semver; conformance pack version is independent of product catalog.

## Harness

Any test generator MUST:

1. Load spec JSON (+ JSON Schema validate when tooling available).
2. Build catalog from `catalogs[<name>]`.
3. Build picker source from `pickerStubs`.
4. Run `SlashStepCompletion` / `SlashCompletion` equivalent.
5. Compare with rules above.

Reference: `SlashSpecConformance` in `AIGuiders.Platform.CommandPlane.Slash`.
