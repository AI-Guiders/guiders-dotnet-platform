# GUIDERS-ADR-0060: Platform.Execution.* — Phase D kickoff (Cockpit vertical)

| | |
|---|---|
| **Status** | Accepted (2026-09-02) |
| **Tags** | #guiders #platform #execution #modeling #cockpit #phase-d |
| **Related** | [GUIDERS-FSHARP-ADR-0002](https://github.com/AI-Guiders/guiders-fsharp/blob/main/docs/adr/GUIDERS-FSHARP-ADR-0002-model-guild-fsharp-ownership.md) · [GUIDERS-FSHARP-ADR-0003](https://github.com/AI-Guiders/guiders-fsharp/blob/main/docs/adr/GUIDERS-FSHARP-ADR-0003-model-extraction-matrix.md) |

## Decision

1. **Cockpit runtime packages** rename to `AIGuiders.Platform.Execution.Cockpit.*` (no flat `Platform.Cockpit.*` for new work).
2. **DataBus event schema** SSOT = `AIGuiders.Platform.Modeling.Cockpit.DataBus` (F#, `guiders-fsharp`). C# `Execution.Cockpit.DataBus` = `IDataBus` + `InMemoryDataBus` only.
3. **Sibling repo wire:** `eng/Guiders.Modeling.props` + `ProjectReference` to `../guiders-fsharp` when `UseGuidersModelingCockpitDataBus=true`.
4. **No duplicate shapes** in Execution — CCU fold algebra (`BuildStateFold`) lives in Modeling; Channels call F# directly.

## Package map (Cockpit, shipped)

| Execution (C#) | Modeling (F#) |
|----------------|---------------|
| `Execution.Cockpit.Abstractions` | — (seams only) |
| `Execution.Cockpit.DataBus` | `Modeling.Cockpit.DataBus` |
| `Execution.Cockpit.Channels` | consumes Modeling events + fold |
| `Execution.Cockpit.Cds` | `Modeling.Cockpit.Cds` (next) |
| `Execution.Cockpit.Composition` | scene DTOs → Modeling later |
| `Execution.Cockpit.Transport` | — |
| `Execution.Cockpit.Ids` | — |

## Consequences

- Products referencing `AIGuiders.Platform.Cockpit.*` must migrate to `Execution.Cockpit.*` + `Modeling.Cockpit.*` event types.
- Remaining 100+ flat `Platform.*` packages follow [ADR-0003 matrix](https://github.com/AI-Guiders/guiders-fsharp/blob/main/docs/adr/GUIDERS-FSHARP-ADR-0003-model-extraction-matrix.md) in subsequent waves.
