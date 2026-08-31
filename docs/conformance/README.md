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
├── schemas/                 ← JSON Schema (normative shape)
│   ├── conformance-common.schema.json
│   ├── policy-spec.schema.json
│   ├── navigation-spec.schema.json
│   ├── slash-arg-completion.schema.json
│   ├── slash-line-resolve.schema.json
│   ├── notation-spec.schema.json
│   ├── notation-quarry.schema.json
│   ├── mcplane-pulse-default.schema.json
│   ├── mcplane-next-hints.schema.json
│   └── command-catalog-wire.schema.json
├── *.spec.json              ← vectors (validate against schemas/)
├── obligations.index.yaml   ← ADR → spec / proof obligations (ADR-0032)
├── policies/                ← overlay policy vectors (JSON + TOML)
├── navigation/              ← explore scene vectors (Navigation family)
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
│   ├── slash-arg-completion.schema.json
│   ├── slash-line-resolve.schema.json
│   ├── binding-catalog.schema.json
│   ├── melody-line.schema.json
│   ├── command-catalog-wire.schema.json
│   └── notation-quarry.schema.json
├── slash/
│   ├── arg-completion.spec.json
│   └── line-resolve.spec.json
├── binding/
│   └── catalog.spec.json
├── melody/
│   └── line-policy.spec.json
├── catalog/
│   └── command-wire.spec.json
├── notation/
│   ├── neovim-kbd.spec.json
│   └── emacs-kbd.spec.json
└── harness/                 ← optional later: language-agnostic CLI
    └── README.md
```

Implementations **do not** live in this repo — only contracts.

---

## Spec backlog

| Spec | Surface | Status | Source tests today | Forge relevance |
|------|---------|--------|-------------------|-----------------|
| `slash/arg-completion` | picker + path steps + guidance | **shipped** | `SlashConformanceTests` | `forge-slash-resolve.js` peel |
| `slash/line-resolve` | path + argTail + runnable | **shipped** | `SlashLineResolveConformanceTests` | execute / suggest routing |
| `slash/catalog-merge` | overlay wins longest prefix | planned | `CatalogIndex_longest_prefix_and_merge` | capabilities merge |
| `binding/catalog` | wire → gesture, overlay, chord root | planned | `BindingCatalogTests` | hotkeys (later) |
| `melody/line-policy` | profile, articulation, normalize | planned | `MelodyLinePolicyTests` | palette `c:` discoverability |
| `catalog/command-wire` | TOML/JSON → descriptor IR | planned | `CommandSourceTests` | `/commands/complete` catalog load |
| `notation/command-console` | console path + kv split | **shipped** | `NotationConformanceTests` | CDP meta parity |
| `notation/argument-positional` | ordered tail tokens | **shipped** | `NotationConformanceTests` | slash remainder |
| `notation/argument-cli` | POSIX/GNU flags quarry | **shipped** | `NotationConformanceTests` | `example.exe -h` |
| `notation/quarry-oracle` | optional neovim/emacs subprocess audit | **shipped** | `tools/QuarryOracle` | clean-room quarry |
| `combinations/slash-ship-first` | baseline path wins on collision | **shipped** | `ContractOracle` + `Conformance.Policies` | user slash overlay |
| `combinations/binding-overlay-wins` | overlay gesture wins on key | **shipped** | `ContractOracle` | hotkey override |
| `combinations/workspace-field-overlay` | ADR field overlay + section replace | **shipped** | `ContractOracle` (JSON + TOML) | workspace.toml hub |
| `combinations/proof-ship-first` | Z3 abstract ShipFirst invariants | **shipped** | `tools/CombinationsProof` | CI-only proof |
| `navigation/code-explore-scene` | Roslyn wire → bounded scene + presets | **shipped** | `NavigationOracle` + `Navigation.Code` | SemanticMap / agents |
| `notation/command-slash` | slash body tokenize | **shipped** | `NotationConformanceTests` | — |
| `notation/argument-kv` | kv tail → slots | **shipped** | `NotationConformanceTests` | — |
| `notation/argument-delimited` | colon-delimited tail → slots | **shipped** | `NotationConformanceTests` | — |
| `notation/invocation-parity` | slash vs console → same path | **shipped** | `NotationConformanceTests` | execute routing |
| `notation/neovim-kbd` | wire → IR | **shipped** | `QuarryNotationConformanceTests` | — |
| `notation/emacs-kbd` | wire → IR | **shipped** | `QuarryNotationConformanceTests` | — |
| `notation/key-gesture` | KeyGesture / hotkeys.toml → IR | **shipped** | `QuarryNotationConformanceTests` | CIDE hotkeys.toml |
| `language-intelligence/line-range` | line range parse + text delete | planned (ADR-0025 P1) | `EditorSurfaceTests` (quarry) | Forge/CIDE buffer |
| `notation/bracket-cdp-square-kv` | CDP wire → axes + nested | **shipped** | `BracketConformanceTests` | CSX, sniper, peek |
| `notation/bracket-angle-opaque` | `<…>` opaque inner | planned (ADR-0026 P1) | `QuarryBracketTokenParser` (quarry) | keyboard oracle |
| `language-intelligence/anchor-resolve` | normalized wire → locus + tier | planned (ADR-0025 P2) | — | CDP sniper |
| `mcplane/pulse-default` | agent envelope pulse | **shipped** | `McPlaneConformanceTests` | Forge `/capabilities` |
| `mcplane/next-hints` | `next[]` shape | **shipped** | `McPlaneConformanceTests` | agent follow-ups |

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
- [GUIDERS-ADR-0033](../adr/GUIDERS-ADR-0033-navigation-family-semantic-scenes.md) — Navigation family + explore scenes
- [GUIDERS-ADR-0032](../adr/GUIDERS-ADR-0032-conformance-obligations-policy-specs.md) — obligations index + policy specs + Z3 proofs
- [GUIDERS-ADR-0012](../adr/GUIDERS-ADR-0012-arg-picker-completion.md) — slash mechanics
- [GUIDERS-ADR-0016](../adr/GUIDERS-ADR-0016-input-notation-quarry-family.md) — notation quarry
