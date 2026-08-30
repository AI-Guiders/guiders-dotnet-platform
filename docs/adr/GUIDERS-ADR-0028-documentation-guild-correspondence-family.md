# GUIDERS-ADR-0028: Documentation guild — Correspondence family map

| | |
|---|---|
| **Status** | Accepted |
| **Date** | 2026-08-30 |
| **Tags** | #guiders #documentation #correspondence #guild #map |
| **Relates to** | GUIDERS-ADR-0027 · GUIDERS-ADR-0025 · CIDE ADR-0156 · CDP WorkspaceCorrespondence |

> **Note:** This ADR is a **guild map** (migration SSOT). It may be folded into ADR-0027/0025 later; kept for history of the v0.24 split.

---

## Context

Doc↔code mechanics lived in **two monoliths**:

| Source | Location |
|--------|----------|
| CDP | `Cdp.ScriptableIde.WorkspaceCorrespondence` (~880 lines) |
| CIDE | `DocReverseAnchorResolver` + `WorkspaceCorrespondenceResolver` |

Platform v0.22–v0.23 shipped **`Documentation.Anchors`**, **`LinkCheck`**, **`LinkMutate`**, **`Reports`** (MdLinker, glossary). **Correspondence** (forward ADR map + reverse md scan) remained planet-local.

Operator rule: **mechanics in platform guilds**; CDP/CIDE/roslyn-mcp = façades only.

---

## Decision

### Documentation guild (target)

```text
Documentation.*
├── Anchors          Family:doc wire resolve (ADR-0027)
├── LinkCheck        md dry-resolve (--check)
├── LinkMutate       structured axis patch (--apply-rename)
├── Reports          generated vocabulary tables
└── Correspondence.*  doc↔code CRS (this ADR)
    ├── Core         IR: ForwardDoc, ReverseAnchor, Result, paths
    ├── Workspace    forward: workspace.toml → feature + ADR map
    ├── Reverse      reverse scan: bracket / backtick / md link / line range
    └── Resolve      compose TryResolve + BuildContext (ADR-0156 JSON)
```

### Dependency DAG

```text
Notations.Bracket (optional; code F: brackets in reverse scan)

Documentation.Correspondence.Core
    ↑
Documentation.Correspondence.Reverse
Documentation.Correspondence.Workspace  (+ Tomlyn)
    ↑
Documentation.Correspondence.Resolve
    ↑
Cdp.ScriptableIde · CIDE CRS · tools
```

### Not in Correspondence.*

| Concern | Package |
|---------|---------|
| `Family:doc` symbol catalog | `Language.CSharp.Symbols` |
| `Family:doc` anchor lint/patch | `LinkCheck` / `LinkMutate` |
| ReaderId tables | `Reports` |
| Roslyn rename / F:M:L attach | `Language.CSharp.*` |
| CRS UI (MFD timeline) | CIDE / Glass (view only) |

### Planet façades

| Planet | After v0.24 |
|--------|-------------|
| **CDP** | `WorkspaceCorrespondence` → forward to `Correspondence.Resolve` + `IdeReport` adapter |
| **CIDE** | `DocReverseAnchorResolver` → forward to `Correspondence.Reverse` (mapper to `CodeAnchor`) |
| **Glass** | unchanged view; feed uses platform via CIDE/CDP |

---

## Migration (v0.24)

| Phase | Deliverable |
|-------|-------------|
| P0 | This map + `Correspondence.Core/Reverse/Workspace/Resolve` packages |
| P1 | CDP façade; platform tests (CIDE oracle: bracket in ADR body) |
| P2 | CIDE thin forwarders; deprecate duplicate scan bodies |
| P3 | Optional: merge ADR-0028 into ADR-0027 appendix |

---

## Consequences

- One reverse-scan SSOT; no drift between CDP and CIDE regex paths.
- New scan heuristic = change `Correspondence.Reverse` only.
- This ADR can be **archived** once guild map lives in a stable index — content preserved in git history.
