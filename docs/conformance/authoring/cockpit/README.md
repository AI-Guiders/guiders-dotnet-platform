# Authoring conformance — `.cockpit.logic`

Conformance fixtures for federation cockpit annunciation ([GUIDERS-ADR-0057](../../adr/GUIDERS-ADR-0057-cockpit-logic-authoring-quarry.md)).

**Not** planet domain rules — see [0053](../../adr/GUIDERS-ADR-0053-planet-responsibilities.md).

## Spec vectors

| Path | Covers |
|------|--------|
| `dark-cockpit.spec.json` | Dark Cockpit principle ([0007](../../adr/GUIDERS-ADR-0007-aviation-mental-model.md)): nominal → EICAS hidden |

## Fixtures (normative sketches, parser TBD)

| Path | Role |
|------|------|
| `tests/.../federation-dark-cockpit.cockpit.logic` | Federation importable grain |
| `tests/.../dashspec-studio.cockpit.logic` | Planet example (`need-commit` on git+verification facts) |

## Running tests

Parser not implemented — vectors are **signage** for future `Authoring.Cockpit.Logic` + `Platform.Cockpit.Rules` harness.

When implemented:

```bash
dotnet test tests/AIGuiders.Platform.Authoring.Tests -c Release --filter CockpitLogic
```
