# GUIDERS-ADR-0019: Conformance hyperlane (sibling monorepo)

| | |
|---|---|
| **Status** | Accepted |
| **Date** | 2026-08-29 |
| **Relates to** | GUIDERS-ADR-0018 · GUIDERS-ADR-0006 · Federation Constitution · Forge slash |

## Context

[GUIDERS-ADR-0018](GUIDERS-ADR-0018-slash-conformance-vectors.md) ships slash arg-completion vectors in `guiders-platform/docs/conformance`. That works for bootstrap (.NET reference harness + single PR with mechanics).

**Forge** is the first non-.NET consumer (`forge-slash-resolve.js` → JS port + vitest). Charter [GUIDERS-ADR-0006](GUIDERS-ADR-0006-confederation-charter.md) already places federation conformance **cross-repo**. Keeping vectors only under `docs/` in the implementation repo couples contracts to NuGet release cadence and forces clone of full platform for port CI.

Vectors change **relatively rarely** (new mechanics / edge cases), so a dedicated repo is low churn and high clarity.

## Decision

### 1. Target: `aiguiders-conformance` sibling monorepo

| Lives in `aiguiders-conformance` | Stays in product / platform repos |
|--------------------------------|----------------------------------|
| `*.spec.json`, `*.schema.json`, `RULES.md` | `SlashSpecConformance`, `QuarrySpecConformance` (reference harness) |
| Fixture catalogs (`fixture.*` command ids) | Ship catalogs, suggest HTTP, execute |
| Optional future: language-agnostic `harness run` CLI | Forge UI, CIDE WPF, vitest/xunit wrappers |

Monorepo layout: [conformance README](../conformance/README.md).

### 2. Sequence (not big-bang)

| Phase | Action |
|-------|--------|
| **A** (now) | Accumulate specs in platform; .NET harness proves reference quarry |
| **B** | Cover Forge-critical slice: slash (arg + line resolve + merge), catalog wire |
| **C** | Binding + melody specs (CIDE parity; Forge hotkeys later) |
| **D** | Move notation quarry specs from `InputNotation.*` into conformance repo |
| **E** | Create `AI-Guiders/aiguiders-conformance`; platform + Forge pin tags; remove duplicate `docs/conformance` canonical copy (keep redirect README) |

### 3. Versioning

- Conformance repo: **semver tags** (`v1.0.0`, `v1.1.0`).
- Breaking vector or schema → major + migration note in `CHANGELOG.md`.
- `guiders-platform` CI: checkout pinned conformance tag → embed for `EmbeddedResource` tests.
- `agent-forge` CI: same tag → vitest imports JSON from npm/git submodule.

Independent from `AIGuiders.Platform.*` package version.

### 4. Consumer contract

Any port MUST:

1. Pin a conformance release.
2. Implement headless mechanics (or call reference library).
3. Pass all vectors for surfaces it claims to support (Forge may declare `slash` only initially).

Failing CI = port drift, not «platform changed secretly».

### 5. What we are not doing

- Putting .NET harness **source** in conformance repo (stays platform).
- Conformance for product-specific commands or picker backends.
- Blocking platform releases on Forge port green (Forge pins; platform must pass reference harness).

## Consequences

- [conformance README](../conformance/README.md) documents backlog and pin model.
- Next implementation waves: `slash/line-resolve-v1`, `binding/catalog-v1`, Forge vitest (ADR-0012 W5b).
- After phase E, Constitution «Signage» row points to `guiders-conformance` repo, not `docs/conformance`.

## Non-goals (this ADR)

- Creating the GitHub repo (phase E).
- npm publish mechanics (optional; git tag pin is sufficient for v1).
