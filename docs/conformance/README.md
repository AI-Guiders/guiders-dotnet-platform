# Federation conformance hyperlane

Machine-readable **vectors + JSON Schema + determinism rules** for platform mechanics.  
Product ship catalogs (`forge.repo.*`, CIDE intents) are **never** conformance input.

**Target home (next step):** sibling repo [`aiguiders-conformance`](https://github.com/AI-Guiders/aiguiders-conformance) — see [GUIDERS-ADR-0019](../adr/GUIDERS-ADR-0019-conformance-hyperlane-monorepo.md).

**Today:** specs live here while reference harnesses ship in `guiders-platform` NuGet. Forge (JS) will pin tagged releases from the conformance repo once extracted.

---

## Why

| Problem | Conformance answer |
|---------|-------------------|
| N native ports drift | One spec file → vitest / xunit / kotlin from same vectors |
| Vectors tied to .NET repo | Sibling monorepo; platform is one consumer |
| Mechanics change with features | Spec semver; rare bumps |

---

## Layout (current → target)

### Current (`guiders-platform`)

```text
docs/conformance/
├── README.md
├── RULES.md
├── slash-arg-completion-v1.{spec,schema}.json
└── … (growing)

tests/.../Fixtures/          ← embedded for .NET CI
src/.../slash/               ← copied into NuGet content
src/.../Conformance/         ← reference harness (.NET only)
```

### Target (`aiguiders-conformance` monorepo)

```text
aiguiders-conformance/
├── README.md
├── RULES.md
├── package.json             ← optional: @aiguiders/conformance
├── schemas/
│   ├── slash-arg-completion-v1.schema.json
│   ├── slash-line-resolve-v1.schema.json
│   ├── binding-catalog-v1.schema.json
│   ├── melody-line-v1.schema.json
│   ├── command-catalog-wire-v1.schema.json
│   └── notation-quarry-v1.schema.json
├── slash/
│   ├── arg-completion-v1.spec.json
│   └── line-resolve-v1.spec.json
├── binding/
│   └── catalog-v1.spec.json
├── melody/
│   └── line-policy-v1.spec.json
├── catalog/
│   └── command-wire-v1.spec.json
├── notation/
│   ├── neovim-kbd-v1.spec.json
│   └── emacs-kbd-v1.spec.json
└── harness/                 ← optional later: language-agnostic CLI
    └── README.md
```

Implementations **do not** live in this repo — only contracts.

---

## Spec backlog

| Spec | Surface | Status | Source tests today | Forge relevance |
|------|---------|--------|-------------------|-----------------|
| `slash/arg-completion-v1` | picker + path steps + guidance | **shipped** | `SlashConformanceTests` | `forge-slash-resolve.js` peel |
| `slash/line-resolve-v1` | path + argTail + runnable | planned | `CommandPlaneTests` LineResolver | execute / suggest routing |
| `slash/catalog-merge-v1` | overlay wins longest prefix | planned | `CatalogIndex_longest_prefix_and_merge` | capabilities merge |
| `binding/catalog-v1` | wire → gesture, overlay, chord root | planned | `BindingCatalogTests` | hotkeys (later) |
| `melody/line-policy-v1` | profile, articulation, normalize | planned | `MelodyLinePolicyTests` | palette `c:` discoverability |
| `catalog/command-wire-v1` | TOML/JSON → descriptor IR | planned | `CommandSourceTests` | `/commands/complete` catalog load |
| `notation/command-slash-v1` | slash body tokenize | planned | `NotationsTests` | — |
| `notation/argument-kv-v1` | kv tail → slots | planned | `NotationsTests` | — |
| `notation/invocation-parity-v1` | slash vs console → same path | planned | `NotationsTests` | execute routing |
| `mcplane/pulse-default-v1` | agent envelope pulse | planned | — | Forge `/capabilities` |

**Out of scope:** execute handlers, MCP wire, plugin host layout, product picker HTTP, UI popover.

---

## Pin contract

Consumers pin **git tag** or **npm** `@aiguiders/conformance@1.x`:

```text
guiders-platform CI    → checkout aiguiders-conformance@v1.0.0 → embed → SlashSpecConformance
agent-forge CI         → @aiguiders/conformance@1.0.0 → vitest against same JSON
```

Platform semver (`AIGuiders.Platform.*`) and conformance semver are **independent**.  
Breaking vector → conformance major + migration note; platform may lag one release.

---

## Determinism

See [RULES.md](RULES.md). All specs must be:

- JSON (+ Schema)
- deterministic (no clock, locale, network in vectors)
- fixture ids only (`fixture.*`, not product `commandId`s)

---

## Workflow

1. **Add vectors** in platform repo (until extraction) or directly in `guiders-conformance`.
2. **Reference harness** must pass in `guiders-platform` CI.
3. **Native port** (Forge JS) adds thin vitest — same spec path.
4. **Extract** when slash + binding specs cover Forge's first port slice.

### Naming (align with NuGet)

| Ecosystem | Prefix | Example |
|-----------|--------|---------|
| NuGet | `AIGuiders.*` | `AIGuiders.Platform.CommandPlane.Slash` |
| npm scope | `@aiguiders/*` | `@aiguiders/conformance`, `@aiguiders/command-plane-slash` |
| GitHub org | `AI-Guiders` | `AI-Guiders/aiguiders-conformance` |
| ADR / federation docs | `GUIDERS-*` | charter signage (product name «Guiders Federation») |

---

## Related ADRs

- [GUIDERS-ADR-0018](../adr/GUIDERS-ADR-0018-slash-conformance-vectors.md) — slash arg-completion v1
- [GUIDERS-ADR-0019](../adr/GUIDERS-ADR-0019-conformance-hyperlane-monorepo.md) — sibling repo plan
- [GUIDERS-ADR-0012](../adr/GUIDERS-ADR-0012-arg-picker-completion.md) — slash mechanics
- [GUIDERS-ADR-0016](../adr/GUIDERS-ADR-0016-input-notation-quarry-family.md) — notation quarry
