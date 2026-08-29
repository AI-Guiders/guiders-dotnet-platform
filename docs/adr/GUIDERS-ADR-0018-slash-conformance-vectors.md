# GUIDERS-ADR-0018: Slash conformance vectors (machine-readable)

| | |
|---|---|
| **Status** | Accepted |
| **Date** | 2026-08-29 |
| **Relates to** | GUIDERS-ADR-0012 · GUIDERS-ADR-0016 · Federation Constitution |

## Context

Per-ecosystem native ports (JS for Forge slash, Kotlin, …) need a **single SSOT** for mechanics — not N copies of Xunit assertions. InputNotation already uses `*.spec.json` + `QuarrySpecConformance`; slash arg/path completion had logic only in `CommandPlaneTests.cs`.

## Decision

### 1. Conformance pack (platform fixtures only)

| Artifact | Role |
|----------|------|
| `docs/conformance/slash-arg-completion-v1.spec.json` | Vectors + fixture catalogs (`fixture.*` command ids) |
| `docs/conformance/slash-arg-completion-v1.schema.json` | JSON Schema for spec documents |
| `docs/conformance/RULES.md` | Determinism rules (ordinal compare, sort order, stub filter) |
| `SlashSpecConformance` in `CommandPlane.Slash` | Reference harness (.NET) |
| NuGet content `slash/slash-arg-completion-v1.spec.json` | Same file shipped with package |

**Not in scope:** product ship catalogs (Forge `/repo`, CIDE `/build`, …).

### 2. Vector shape

Each vector: `catalog` ref → `body` typed line → `expect.suggestions` and/or `expect.guidance`.

Dynamic pickers use `pickerStubs` in the spec (not live HTTP). Product suggest adapters are tested in planet CI separately.

### 3. Harness contract

Any port MUST:

1. Load spec JSON.
2. Build catalog from `catalogs[<name>]`.
3. Wire `pickerStubs` as `PickerChoiceSource`.
4. Run headless completion equivalent to `SlashStepCompletion` / `SlashCompletion`.
5. Assert per `RULES.md`.

Test generators (xunit, vitest, …) are thin wrappers over the same spec file.

### 4. Wave linkage (ADR-0012)

| Wave | Scope |
|------|-------|
| **W4** ✓ | Platform `SlashArgCompletion`, `ISlashPickerChoiceSource` |
| **W5a** ✓ | Machine-readable slash conformance pack + .NET harness |
| **W5b** | Forge JS port consuming same spec (vitest) |

## Consequences

- Platform **0.10.0+**
- New vectors → edit spec + all ports' CI; breaking shape → `v2` spec or schema major
- After phase E, canonical path → `aiguiders-conformance` monorepo ([ADR-0019](GUIDERS-ADR-0019-conformance-hyperlane-monorepo.md)); bootstrap copy remains until extraction.
- Quarry notation specs should gain JSON Schema in conformance repo (same pattern)
