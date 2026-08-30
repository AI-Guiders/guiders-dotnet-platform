# GUIDERS-ADR-0034: CSX lift slice — navigation scene, workspace/project config, XML anchors

| | |
|---|---|
| **Status** | Accepted |
| **Date** | 2026-08-30 |
| **Tags** | #guiders #cdp #csx #navigation #configurations |
| **Relates to** | GUIDERS-ADR-0033 · GUIDERS-ADR-0029 · GUIDERS-ADR-0025 |

---

## Context

CSX/ScriptableIDE wires MCP tool calls through fluent facades. Three host-local mechanics remained after v0.29.1:

1. **SemanticMap** returned Roslyn wire / `IdeReport`, not `navigation_scene/v1`
2. **ExploreCorr** + **project.toml** used ad hoc Tomlyn models beside `Configurations.Workspace`
3. **BracketXmlResolve** duplicated XML anchor resolve (~500 LOC) while C# resolve was already federated

---

## Decision

### Navigation.Code in CSX

- `NavigationSceneJson` serializes `navigation_scene/v1`
- `NavigationProfile.FromExplore` merges preset + MCP kind overrides
- `Cdp.ScriptableIde.NavigationSceneBridge` + `SemanticMapFacade` emit scene JSON (`GetNavigationSceneAsync`, default `GetAsync`)
- `WithUsages()` composite still returns `IdeReport` (usage merge projector stays host-local)

### Workspace + project configuration

- `WorkspaceExploreCorrSettings` in `WorkspaceDocument`; policy in `WorkspaceExploreCorrPolicy`
- `WorkspaceSources.ResolveExploreCorrMode` loads cascade TOML
- `Configurations.Project` + `ProjectSources` for `.cdp/project.toml` (field merge via `ProjectDocument.MergeOver`)
- `ExploreCorrPolicy` / `CdpProjectToml` → thin CDP façades

### XML anchors

- `Language.Xml.Anchors.XmlBracketAnchorResolve` — SSOT for `X:`/`A:`/`K:Element`
- `Cdp.ScriptableIde.BracketXmlResolve` — compatibility façade (mirror C# pattern)

---

## Consequences

- CDP `SemanticMap.cs` should follow same bridge (follow-up)
- New workspace.toml sections require `WorkspaceDocument` + overlay update
- XML mutate hosts consume platform package, not local parser

---
