# Notations.Bracket migration checklist

Canonical plan: [GUIDERS-ADR-0026](../adr/GUIDERS-ADR-0026-notations-bracket-branch.md).

## Two-pass parse

```text
Pass 1 — Bracket envelope:  [  key:value ; key:value  ]
Pass 2 — Axis value class:  command.path | argument.colon | line.range | bracket.nested
```

Examples:

| Wire fragment | Pass 1 | Pass 2 |
|---------------|--------|--------|
| `FRG:pilot/issues/7` | axis FRG | `command.path` → repo/issues/N |
| `F:src/Foo.cs` | axis F | `command.path` (file) |
| `S:for:2` | axis S, value `for:2` | `argument.colon` → kind + index |
| `L:12-34` | axis L | `line.range` |

`:` at envelope = axis KV delimiter. `:` inside `S:for:2` = **Argument colon** micro-grammar (not `=`).

## Contract fields

See ADR-0026. Planet tables: `BracketAxisValuePlans.CdpCode`, `ForgeFrgCompound`.

## Phase 1

```
BracketReader pass 1 → BracketAxis[]
Optional pass 2 via BracketAxisValuePlan + Notations.Command/Argument readers
```
