# GUIDERS-ADR-0062: IDE Solution Session — graph, lifecycle, orchestrator

| | |
|---|---|
| **Status** | Accepted · In progress |
| **Implementation** | Phase 1b–2: `Execution.Ide.Session`, `FederationSessionRuntime`; Σ_π / timeline ports open |
| **Date** | 2026-09-02 |
| **Tags** | #guiders #federation #ide #solution #lifecycle #workspace #fsharp #csharp #typescript #lrc #first-class |
| **Related** | [GUIDERS-ADR-0025](./GUIDERS-ADR-0025-language-intelligence-boundary.md) · [GUIDERS-ADR-0059](./GUIDERS-ADR-0059-gdl-hyperlane.md) · [GUIDERS-ADR-0048](./GUIDERS-ADR-0048-authoring-quarry-family.md) · [GUIDERS-FSHARP-ADR-0004](https://github.com/AI-Guiders/guiders-fsharp/blob/main/docs/adr/GUIDERS-FSHARP-ADR-0004-ide-session-modeling-ownership.md) · [ide-session-axioms-v0](https://github.com/AI-Guiders/guiders-fsharp/blob/main/docs/math/ide-session-axioms-v0.md) · [CDP-ADR-0208](https://github.com/AI-Guiders/cdp-mcp/blob/main/docs/adr/CDP-ADR-0208-language-resolver-center-cdp-host.md) · [Constitution](../GUIDERS-FEDERATION-CONSTITUTION.md) |

## Context

Federation hosts edit **multi-language solutions** in one IDE session: `*.sln` / `*.slnx` with `csproj`, `fsproj`, `tsconfig` / `package.json`, GDL manifests, and planet DSLs.

Today there is **no federation SSOT** for the solution/session model:

| Piece today | Problem |
|-------------|---------|
| `DotNetWorkspace.Core` (guiders-core) | Tactical C# graph parser — **not** `Platform.Modeling.*`, no lifecycle |
| Roslyn `MsBuildWorkspace` | Implicit C#-only solution SSOT; MSBuild in-process |
| F# Sdk phased loader + `IdeWorkspaceWarm` | Planet-local warm hooks; F# not first-class |
| LRC ([0061](./GUIDERS-ADR-0061-language-resolver-center.md)) | Verb gateway only — receives `SolutionOrProjectPath` as **string hint** |
| CDP `SessionContext` | Planet session DTO — not domain model |
| Out-of-process probes / workers (legacy) | **Bypass orchestrator** — ad-hoc sidecars, not graph-attributed capabilities |

**User requirement (normative intent):** every GPL/GDL language is **first-class** in the same solution graph. Adding a language is **adding a project-kind branch** — not a new dispatch rail, warm hook, or planet-local sidecar that **bypasses** the session graph.

**Mental model (operator):**

```text
Solution (graph of projects) ── has Lifecycle (orchestrated)
  ├── C#Project   (csproj)  ── Lifecycle ── CompilerServices @ DesignTime
  ├── F#Project   (fsproj)  ── Lifecycle ── CompilerServices @ DesignTime
  ├── NodeProject (tsconfig)── Lifecycle ── CompilerServices @ DesignTime
  └── GdlProject  (gdlproj) ── Lifecycle ── CompilerServices @ DesignTime
```

Solution lifecycle is the **coordinator**; project lifecycles are **peers** with the same phase vocabulary.

[LRC](./GUIDERS-ADR-0061-language-resolver-center.md) remains the **verb gateway** for diagnostics / symbols / navigation. It **consumes** design-time compiler services from the session orchestrator ([0062](./GUIDERS-ADR-0062-ide-solution-session-orchestrator.md)) — it does **not** own solution graph or lifecycle.

[GDL `gdlproj`](./GUIDERS-ADR-0051-authoring-project-abstraction.md) is a **parallel** authoring-project abstraction (declare-time documents). It **may** appear as a project node in the same session graph; it is **not** merged into dotnet MSBuild IR.

## Decision

### 1. Federation SSOT: `Platform.Modeling.Ide.Session` (F#)

Solution/session domain lives in **Modeling (F#)** per [GUIDERS-FSHARP-ADR-0002](https://github.com/AI-Guiders/guiders-fsharp/blob/main/docs/adr/GUIDERS-FSHARP-ADR-0002-model-guild-fsharp-ownership.md) §13.

```text
guiders-fsharp
  AIGuiders.Platform.Modeling.Ide.Session     F# — graph IR, lifecycle algebra, orchestration policy
  AIGuiders.Platform.Modeling.Ide.Session.Ports F# — parser/host port traits (slnx, tsconfig, gdlproj)

guiders-platform
  AIGuiders.Platform.Execution.Ide.Session      C# — session host, orchestrator runtime, adapter wiring
```

**Dependency rule:** `Execution.Ide.Session` → `Modeling.Ide.Session`. Planets (CDP, CIDE) → `Execution.Ide.Session`. **No** second solution IR in C# or planets.

### 2. Core IR (normative shapes)

```fsharp
type LifecyclePhase =
    | Unloaded
    | DesignTime
    | CompileTime
    | RunTime
    | TestTime

type ProjectKind =
    | DotNet of DotNetProject   // CSharp | FSharp — csproj/fsproj
    | Node of NodeProject       // package.json / tsconfig anchor
    | Gdl of GdlProject         // gdlproj — reuses Modeling.Gdl.Project identity
    | Planet of PlanetProject   // sovereign DSL anchor (dashspec, …)

type ProjectNode =
    { Id: ProjectId
      Kind: ProjectKind
      AbsolutePath: string
      Phase: LifecyclePhase
      Capabilities: CapabilityNode list }

type SolutionGraph =
    { AnchorPath: string
      Projects: ProjectNode list
      FileOwnership: Map<string, ProjectId>
      Edges: SessionEdge list }

type SolutionSession =
    { Graph: SolutionGraph
      Phase: LifecyclePhase
      Policy: SessionPolicy }
```

**Rule:** a new language is a **new `ProjectKind` case** + **capability subtree** under that project — not a new IDE subsystem.

#### 2.1 Capability graph (attributes on nodes)

The session model is a **typed graph**, not a flat project list. A project node **hosts** capability nodes; capabilities carry **execution topology** and other attributes. The orchestrator **routes** verbs to a capability handle based on graph attributes + policy — it does not hard-code in-process vs out-of-process per language.

```text
SolutionSession
  └── ProjectNode (fsproj)
        ├── Capability: CompilerServices     { execution: InProcess, phase: DesignTime }
        ├── Capability: StaticAnalysis       { execution: Adaptive, … }
        ├── Capability: Build                { execution: SubprocessTool, phase: CompileTime }
        └── Capability: TestDiscovery        { execution: OutOfProcess, phase: TestTime }
```

**Normative node kinds (extensible):**

| Capability | Typical phase | Default `ExecutionTopology` |
|------------|---------------|-------------------------------|
| `CompilerServices` | DesignTime | `InProcess` (Roslyn, FCS, warmed TS) |
| `StaticAnalysis` | DesignTime / CompileTime | `Adaptive` |
| `Build` | CompileTime | `SubprocessTool` (`dotnet`, `npm`) |
| `TestDiscovery` / `TestRun` | TestTime | `OutOfProcess` or `SubprocessTool` |
| `LspBridge` | DesignTime | `OutOfProcess` (Python, Delphi presets) |

```fsharp
type ExecutionTopology =
    | InProcess
    | OutOfProcess
    | SubprocessTool      // short-lived CLI, not a language host
    | Adaptive            // orchestrator picks from predicates below

type CapabilityAttributes =
    { Topology: ExecutionTopology
      Warmth: WarmthHint              // Cold | Warm | Hot
      CostTier: CostTier              // Interactive | Standard | Heavy
      Scope: CapabilityScope          // File | Project | Solution
      Predicate: AdaptiveRule list }  // when Adaptive

type AdaptiveRule =
    | WhenProjectFileCountBelow of int -> ExecutionTopology
    | WhenAlreadyWarm -> ExecutionTopology
    | WhenFullSolutionScan -> ExecutionTopology
    | WhenElapsedBudgetExceeds of TimeSpan -> ExecutionTopology
```

**Example (operator intent):** full-solution static analysis on a large repo → `OutOfProcess` + `Heavy`; same analysis on a warmed small project → `InProcess`. **One capability**, two materializations — chosen by orchestrator from **graph attributes + session state**, not a forked dispatch path.

**Edges** (`SessionEdge`) express dependencies the orchestrator respects:

| Edge | Meaning |
|------|---------|
| `requires` | capability B needs A materialized first (e.g. Build requires DesignTime context) |
| `invalidates` | A advancing invalidates B (e.g. CompileTime invalidates warmed DesignTime caches) |
| `feeds` | output of A is input to B (build artifacts → refreshed sources) |

Attributes MAY attach to **projects**, **capabilities**, or **edges** — prefer the **most specific** node (capability > project > solution).

```fsharp
type SessionEdge =
    { From: GraphNodeId
      To: GraphNodeId
      Kind: SessionEdgeKind
      Attributes: Map<string, string> }
```

Planets and adapters **declare** capabilities and default attributes in Modeling; orchestrator **materializes** handles. **Forbidden:** ad-hoc out-of-process workers that are not registered as graph capabilities (legacy probe pattern).

### 3. Lifecycle semantics

| Phase | Meaning | Typical triggers |
|-------|---------|------------------|
| `Unloaded` | Node known in graph; no compiler host materialized | graph parse only |
| `DesignTime` | IntelliSense, diagnostics, symbols, navigation, rename (when supported) | `cdp_open`, first verb on project file |
| `CompileTime` | Build graph, emit, generated sources on disk | `cdp_build`, explicit compile |
| `RunTime` | Launch / debug session | `cdp_run`, debugger attach |
| `TestTime` | Test discovery / execution | `cdp_test` |

**Normative:**

- **DesignTime MUST NOT** require full `dotnet build` / `tsc --build` by default.
- **CompileTime** MAY call `dotnet` / `npm` / MSBuild **as subprocess tools** — that is **not** out-of-process language services; it is the **build phase**, separate from compiler service host.
- **Solution `Phase`** is the **ceiling** for project phases (orchestrator may advance all projects or a subset per policy).

```text
cdp_open(sln)
  → parse graph (Unloaded)
  → SolutionOrchestrator.Enter(DesignTime)
      → foreach project (or lazy-on-demand): materialize capabilities for DesignTime
      → topology from capability attributes (default InProcess for CompilerServices)

cdp_build
  → SolutionOrchestrator.Enter(CompileTime)
      → invoke build tools; refresh generated inputs
      → return to DesignTime (or stay CompileTime per policy)
```

### 4. Orchestrator (Execution — thin)

`SolutionSessionOrchestrator` (C#) is the **only** place that:

- loads / unloads **capability handles** (not ad-hoc hosts)
- resolves `ExecutionTopology` from capability attributes + `SessionPolicy` + warmth
- advances lifecycle phases (with validation)
- applies **SessionPolicy** (eager vs lazy design-time, retain vs evict on close)
- exposes **handles** to LRC, LanguageIntelligence, build/test channels

```text
                    ┌─────────────────────────────────────┐
  cdp_open / verbs  │  Platform.Execution.Ide.Session     │
                    │  SolutionSessionOrchestrator        │
                    └──────────────┬──────────────────────┘
                                   │
         ┌─────────────────────────┼─────────────────────────┐
         ▼                         ▼                         ▼
  Capability: CompilerServices   …                    Capability: StaticAnalysis
  (InProcess default)                                  (Adaptive → In|Out)
         │                         │                         │
         └────────────► Modeling.Ide.Session ◄────────────────┘
                        (graph + lifecycle + attributes)
```

**Orchestrator API (sketch):**

```csharp
public interface ISolutionSessionOrchestrator
{
    SolutionSession Open(string anchorPath, SessionOpenOptions options);
    Task EnterPhaseAsync(LifecyclePhase phase, PhaseScope scope, CancellationToken ct);
    Task<ICapabilityHandle> EnsureCapabilityAsync(ProjectId project, CapabilityId cap, CancellationToken ct);
    Task EvictAsync(ProjectId project, LifecyclePhase downTo);
    void Close();
}
```

**Load / unload policy (normative defaults):**

| Policy | Behavior |
|--------|----------|
| `DesignTimeLazy` (default) | Materialize compiler services on first verb touching project |
| `DesignTimeEager` | On `Enter(DesignTime)`, warm all projects in graph |
| `EvictOnClose` | Drop all handles on session close |
| `RetainWarm` | Keep design-time caches until memory pressure / explicit evict |

Planets **configure** policy; they **do not** implement per-language warm branches.

### 5. Capability handles and compiler services

```fsharp
type ICapabilityHandle =
    abstract member CapabilityId: CapabilityId
    abstract member ProjectId: ProjectId
    abstract member Topology: ExecutionTopology
    abstract member Phase: LifecyclePhase
    abstract member Dispose: unit -> unit

type ICompilerServices =
    inherit ICapabilityHandle
    abstract member LanguageId: string
    abstract member GetDiagnostics: LanguageRequest -> Task<DiagnosticsResult>
    abstract member GetDocumentSymbols: LanguageRequest -> Task<DocumentSymbolsResult>
    abstract member GoToDefinition: LanguageRequest -> Task<LanguageNavigation option>
```

**Normative:**

- **Default** for `CompilerServices` on GPL languages (C#, F#, TS): `ExecutionTopology.InProcess`.
- **Out-of-process is allowed** when declared as a **capability attribute** on the graph (e.g. heavy static analysis, LSP bridge, full-solution scan). Orchestrator materializes the handle; adapters **project the same** `Modeling.Language` envelopes regardless of topology.
- **Forbidden:** capability implementations that **bypass** the session graph (legacy per-call probes, planet-local workers not registered on the project node).
- `SubprocessTool` (`dotnet build`, `npm test`) is **not** the same as `OutOfProcess` language host — short-lived tools on **CompileTime** / **TestTime** edges only.

LRC ([0061](./GUIDERS-ADR-0061-language-resolver-center.md)) becomes:

```text
LanguageResolverCenter
  → resolve ProjectId from file path (session graph)
  → orchestrator.EnsureCapability(projectId, CompilerServices)
  → handle (InProcess or OutOfProcess per attributes) → Modeling.Language envelopes
```

### 6. Graph parsing — ports, not SSOT

| Anchor | Port owner | Feeds |
|--------|------------|-------|
| `.sln` / `.slnx` | `DotNetWorkspace.Core` → **port** | `DotNet` project nodes |
| `.csproj` / `.fsproj` | Sdk project file reader | single node + ownership |
| `tsconfig.json` / `package.json` | Node project port | `Node` project node |
| `.gdlproj` | `Modeling.Gdl.Project` | `Gdl` project node |

`AIGuiders.DotNetWorkspace.Core` **remains** a parser/util package in guiders-core. It **must not** own session lifecycle or orchestration. Long-term it is a **dependency of** `Modeling.Ide.Session.Ports.DotNet`.

### 7. Relationship to sibling federation concerns

| Concern | Owner | Uses session how |
|---------|-------|------------------|
| **LRC** verbs | `Execution.Language` | `EnsureCapability(CompilerServices)` → handle |
| **LanguageIntelligence** ([0025](./GUIDERS-ADR-0025-language-intelligence-boundary.md)) | `LanguageIntelligence.*` | same project ownership + spans |
| **Build / test** | `Execution` build channels | `Enter(CompileTime)` / `Enter(TestTime)` |
| **Cockpit DataBus** | `Modeling.Cockpit.DataBus` | events keyed by solution/project id ([FSHARP-ADR-0002](https://github.com/AI-Guiders/guiders-fsharp/blob/main/docs/adr/GUIDERS-FSHARP-ADR-0002-model-guild-fsharp-ownership.md) §15) |
| **GDL authoring** | `Modeling.Gdl.*` | `Gdl` branch in graph — not a fork of sln IR |

### 8. First-class language rule

A language is **first-class** when:

1. It has a `ProjectKind` case (or documented `Planet` profile).
2. It declares a `CompilerServices` capability (default `InProcess`; other capabilities optional).
3. `file → project` ownership comes from **session graph**, not walk-up hacks in adapters.
4. LRC (or LI) routes through **orchestrator.EnsureCapability**, not planet `if language`.

**Adding F#** should have been step (2)–(4) on the **same graph** — not a separate LRC warm rail.

### 9. Migration phases

```text
Phase 0 (this ADR)     Normative model + package names; stop new planet warm hooks
Phase 1                Modeling.Ide.Session IR + sln/slnx/fsproj/csproj graph port
Phase 2                Execution orchestrator; CDP cdp_open → orchestrator.Open
Phase 3                LRC + Roslyn adapters → orchestrator.EnsureDesignTime
Phase 4                Retire IdeWorkspaceWarm, probe, MSBuild-as-SSOT for C# design-time
Phase 5                Node + gdlproj nodes; TS first-class in same graph
```

**Shim rule:** `DotNetWorkspace.Core`, `SessionContext`, and existing LRC entry points MAY delegate to orchestrator for one release cycle. Planets MUST NOT fork graph IR.

### 10. CDP responsibilities after migration

| CDP owns | CDP does not own |
|----------|------------------|
| MCP wire, tenant, `DocumentStore` | Solution graph IR |
| Calling `ISolutionSessionOrchestrator` on `cdp_open` | Per-language warm (`IdeWorkspaceWarm`) |
| Session policy knobs (lazy/eager) | `ProjectContextPhase` as public API |

## Consequences

- **F# first-class** means graph + lifecycle parity with C#, not a special diagnostics pipeline.
- New languages are **cheap** — new `ProjectKind` + capability subtree with attributes.
- **One orchestrator** routes all topologies; no competing bypass pipelines.
- Graph attributes encode **when** to go out-of-process (heavy, cold, full scan) vs in-process (interactive, warm, small) — policy is data on the graph, not scattered `if` in planets.
- `Platform.Modeling.Ide.Session` becomes the right home for DU-heavy lifecycle algebra.
- LRC stays focused — verb envelopes only ([0061](./GUIDERS-ADR-0061-language-resolver-center.md)).
- `DotNetWorkspace.Core` shrinks to parser port; tactical phased loader moves under F# `DotNet` design-time materialization.

## Non-goals

- Merging GDL `gdlproj` IR into dotnet MSBuild project model
- Mandating out-of-process for all GPL design-time compiler services (default remains `InProcess`)
- Replacing external LSP for languages without in-process adapters — they register as `LspBridge` capability with `OutOfProcess`
- Rewriting Roslyn or FCS — adapters only
- Planet-specific solution SSOT (CDP, CIDE)
