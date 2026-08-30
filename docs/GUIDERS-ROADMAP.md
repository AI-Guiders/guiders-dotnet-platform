# Guiders Federation — master roadmap (living)

**Status:** living · **Date:** 2026-08-30  
**Sources:** operator chat 2026-08-29/30, [pain inventory](GUIDERS-pain-inventory.md), ADR-0015–0022, [ANPM pain](https://github.com/AI-Guiders/agent-nuget-pm/blob/main/docs/ANPM-pain-inventory.md)

Legend: ✅ done · 🚧 in progress · 📋 planned · ⏸ defer · 👤 operator

---

## Done this arc (docs + tooling)

| Item | Artifact |
|------|----------|
| ✅ snupkg paired push (federation repos) | `push-artifacts.sh`, N-001 |
| ✅ ANPM pain inventory | `agent-nuget-pm/docs/ANPM-pain-inventory.md` |
| ✅ MCPlane ADR (draft), CDP decoupled | ADR-0020, Constitution § Planets |
| ✅ Notations ADR (draft) | ADR-0021 |
| ✅ GUIDERS pain inventory + alliance / license pains | G-001–G-008 |
| ✅ Adoption alliance automation | ADR-0022, `Utilities.Adoption.*`, `ADOPTION-ALLIANCE.generated.md` |
| ✅ Slash arg-completion conformance | ADR-0018, `slash-arg-completion-v1` |
| ✅ CommandPlane registry visitor + Sources | ADR-0013/0014, v0.10 |

---

## Wave 1 — Stitch layer v1 (current)

**Goal:** wire → IR packages + first notation conformance; alliance CI gate.

| # | Deliverable | Maps to | Status |
|---|-------------|---------|--------|
| 1.1 | `Notations` Core IR (`NormalizedArgTail`, wire types) | G-003, ADR-0021 W2g | ✅ |
| 1.2 | `Notations.Command.Slash` — body tokenize | ADR-0021 | ✅ |
| 1.3 | `Notations.Argument.Kv` + `Notations.Command.Console` | G-011 | ✅ |
| 1.4 | `CommandPlane.Slash` delegates tokenize → Notations | ADR-0021 W2h | ✅ |
| 1.5 | `invocation-parity-v1` vectors (slash vs kv → same path) | G-011 | 🚧 (unit tests; spec JSON next) |
| 1.6 | CI: `ADOPTION-ALLIANCE.generated.md` drift check | G-008 | ✅ |
| 1.7 | Conformance README: notation + mcplane backlog rows | G-010 | 🚧 |

---

## Wave 2 — Conformance extraction

| # | Deliverable | Status |
|---|-------------|--------|
| 2.1 | `slash/line-resolve-v1` spec + harness | 📋 |
| 2.2 | `notation/command-slash-v1`, `argument-kv-v1` specs | 📋 |
| 2.3 | Repo `aiguiders-conformance` extract (ADR-0019) | 📋 |
| 2.4 | Forge JS vitest pin `@aiguiders/conformance` | 📋 |

---

## Wave 3 — MCPlane quarry

| # | Deliverable | Maps to | Status |
|---|-------------|---------|--------|
| 3.1 | `MCPlane.Abstractions` package (from `Abstractions` seeds) | G-020 | 📋 |
| 3.2 | `mcplane/pulse-default-v1`, `next-hints-v1` vectors | G-020 | 📋 |
| 3.3 | `agent-catalog-projection-v1` | G-021 | 📋 |
| 3.4 | Accept ADR-0020 after first conformance tag | | ⏸ |

---

## Wave 4 — Notations rename + keyboard

| # | Deliverable | Status |
|---|-------------|--------|
| 4.1 | `InputNotation` → `Notations.Keyboard.*` aliases | 📋 |
| 4.2 | `Argument.Cli` (System.CommandLine quarry) | G-004, defer v2 |
| 4.3 | `Argument.Delimited` (colon wire_class) | 📋 |

---

## Wave 5 — Platform naming & docs

| # | Deliverable | Status |
|---|-------------|--------|
| 5.1 | Desk → **Attention** (ADR / README) | 📋 |
| 5.2 | Aviation vs MCPlane package prefix decision | ⏸ |
| 5.3 | Supported embed surface doc (warranty / legal) | G-006b, G-007 | 📋 |

---

## ANPM / NuGet (parallel track — 👤 + ops)

| # | Deliverable | Pain | Status |
|---|-------------|------|--------|
| N.1 | Co-owner `AIGuiders` on ~46 packages | N-011 | 👤 in-progress |
| N.2 | TP `user: AIGuiders` in release workflows | N-020 | 📋 |
| N.3 | Repair failed symbols (e.g. Cdp.Core 0.4.22) | N-002 | 📋 |
| N.4 | ANPM L3a: `registry.toml`, owners drift | proposed | 📋 |

---

## Explicit defer

- Citizen / `@frame` wire in federation (planet only)
- MCP JSON → `Argument.Json` (MCPlane projection first)
- PowerShell `Argument.PowerShell` package
- `aiguiders-conformance` npm until Wave 2 extract

---

## How to refresh

```bash
# Adoption alliance table
dotnet run --project tools/AdoptionReport -- --write docs/ADOPTION-ALLIANCE.generated.md

# Tests
dotnet test -c Release
```

Related: [Constitution](GUIDERS-FEDERATION-CONSTITUTION.md) · [conformance README](conformance/README.md)
