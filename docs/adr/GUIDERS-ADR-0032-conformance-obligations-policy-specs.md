# GUIDERS-ADR-0032: Conformance obligations — policy specs + formal proofs

| | |
|---|---|
| **Status** | Accepted |
| **Date** | 2026-08-30 |
| **Tags** | #guiders #conformance #combinations #policy #z3 #obligations |
| **Relates to** | GUIDERS-ADR-0031 · GUIDERS-ADR-0030 · GUIDERS-ADR-0018 · GUIDERS-ADR-0019 |

---

## Context

ADR-0031 made overlay **policies readable in code** (`OverlayProfile`, named `CombinationSemantics`). ADR-0018/0019 established the conformance hyperlane (vectors + JSON Schema) but ADRs alone are not machine-checkable obligations for federation ports.

Operator ask: **policy-as-contract** — same overlay semantics provable across JSON/TOML specs, harness, and optional formal methods — without reviving legacy .NET Code Contracts.

---

## Decision

### Obligations index

`docs/conformance/obligations.index.yaml` maps **obligation id → ADR → spec path or proof tool**. CI walks the index via `tools/ContractOracle index`.

### Policy spec surface

New schema `policy-spec.schema.json`:

| Field | Role |
|-------|------|
| `kind` | always `policy` |
| `policy` | stable id (`slash.ship-first`, `workspace.field-overlay`, …) |
| `semantics` | `CombinationSemantics` enum |
| `vectors[]` | baseline / overlay / expect fixtures |

Specs live under `docs/conformance/policies/`. **Multi-format:** same vectors may ship as `.json` and `.toml` (TOML normalized to JSON for schema + harness).

### Harness

| Component | Role |
|-----------|------|
| `AIGuiders.Platform.Conformance.Policies` | load specs, run registered combinators |
| `tools/ContractOracle` | `verify --spec`, `index --root` |
| `ConformanceSchemaValidator.ValidatePolicyJson` | JSON Schema gate |

Policy ids bind to platform combinators (`SlashCombinators.ShipFirst`, etc.) — specs test **behavior**, not re-implement merge.

### Formal proofs (CI-only)

`tools/CombinationsProof` + **Microsoft.Z3** proves abstract **ShipFirst** invariants (baseline wins on collision; overlay fills missing keys). Not runtime dependency.

Proof obligations referenced from index (`kind: proof`); executed in CI separately from vector harness.

### Out of scope

- Reviving **Code Contracts** (deprecated; no .NET Core+ checker/rewriter).
- Runtime Z3 or contract injection — proofs and vectors are build/CI gates only.

---

## Consequences

- Federation ports can pin policy specs + obligations index independently of NuGet semver.
- New overlay policy **must** add: readable profile (ADR-0031) + policy spec vectors + obligations index row.
- TOML policy specs use Tomlyn → JSON bridge; schema remains JSON Schema SSOT.

---

## References

- `docs/conformance/obligations.index.yaml`
- `docs/conformance/policies/`
- `src/AIGuiders.Platform.Conformance.Policies/`
- `tools/ContractOracle/`, `tools/CombinationsProof/`
