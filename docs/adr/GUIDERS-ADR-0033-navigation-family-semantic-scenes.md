# GUIDERS-ADR-0033: Navigation family — bounded semantic scenes

| | |
|---|---|
| **Status** | Accepted |
| **Date** | 2026-08-30 |
| **Tags** | #guiders #navigation #semantic-map #agents #conformance |
| **Relates to** | GUIDERS-ADR-0032 · GUIDERS-ADR-0031 · GUIDERS-ADR-0025 · GUIDERS-ADR-0019 |

---

## Context

Agents need **repo navigation** without dumping the whole graph into context. CDP/CIDE already ship **SemanticMap** (`roslyn_get_workspace_navigation_context`, CSX `SemanticMap.Explore`) — bounded related/subgraph peel with presets and kind caps.

Platform had anchors and correspondence pieces but **no Navigation family** — mechanics lived only in `cascade-ide` / `roslyn-mcp` / `cdp-mcp`.

Operator ask: lift navigation as **federation mechanics** (like Sources / Combinations / Conformance policies) with machine-readable scene contract.

---

## Decision

### Package family

```text
Navigation                 anchor, mode, node, edge, scene (navigation_scene/v1)
Navigation.Policy          presets, kind filters, NavigationProfile caps
Navigation.Code            Roslyn wire parser + scene builder (v0.29)
Navigation.Docs            planned — doc reverse hops (Correspondence)
Navigation.Workspace       planned — workspace.toml layer compose
Navigation.All             meta-bundle (later)
```

**Hosts remain adapters:** CDP `SemanticMap`, CIDE Skia, Forge TS — projectors over the same scene schema, not SSOT.

### Scene schema

Normative wire: `navigation_scene/v1` — nodes, edges, caps applied, summary string.  
Roslyn MCP JSON (`mode=related`, `items[]`) is an **input dialect** parsed by `NavigationWireParser`.

### Presets (shared with roslyn-mcp)

Bundled ids aligned with Cascade IDE / `BundledWorkspaceNavigationPresets`:

| Preset | Role |
|--------|------|
| `explore_default` | exclude `project_peer` |
| `peers_only` | structural peers only |
| `structure_only` | partial / xaml / directory |
| `no_namespace_noise` | exclude loose directory/namespace |
| `tests_and_peers` | tests + peers |

Kind caps (related mode): `same_directory≤4`, `same_namespace≤4`, `project_peer≤3`.

### Conformance

| Artifact | Role |
|----------|------|
| `navigation-spec.schema.json` | spec shape |
| `navigation/code-explore-scene.spec.json` | vectors (wire + profile → expect) |
| `AIGuiders.Platform.Conformance.Navigation` | harness |
| `tools/NavigationOracle` | CI verify + obligations index |

### Out of scope (v0.29)

- Full Roslyn MSBuild provider in platform (stays in `roslyn-mcp`)
- Skia / Glass presentation
- `Navigation.Docs` / `Navigation.Workspace` providers
- Usages merge (`WithUsages`) — profile flag reserved

---

## Consequences

- CDP SemanticMap should map output to `navigation_scene/v1` (follow-up in cdp-mcp).
- New navigation behavior requires preset/scene vectors + obligations row.
- `workspace.adr.max_related` and `NavigationProfile.MaxRelated` stay aligned conceptually.

---

## References

- `src/AIGuiders.Platform.Navigation*/`
- `docs/conformance/navigation/`
- `tools/NavigationOracle/`
- roslyn-mcp `GetWorkspaceNavigationContext`
