# GUIDERS-ADR-0061: Language Resolver Center (LRC) — federation IDE verbs

| | |
|---|---|
| **Status** | Accepted |
| **Date** | 2026-09-02 |
| **Tags** | #guiders #federation #language-intelligence #lrc #fsharp #roslyn #fcs #gdl #lsp #ide |
| **Related** | [GUIDERS-ADR-0025](./GUIDERS-ADR-0025-language-intelligence-boundary.md) · [GUIDERS-ADR-0059](./GUIDERS-ADR-0059-gdl-hyperlane.md) · [GUIDERS-ADR-0048](./GUIDERS-ADR-0048-authoring-quarry-family.md) · [GUIDERS-FSHARP-ADR-0002](https://github.com/AI-Guiders/guiders-fsharp/blob/main/docs/adr/GUIDERS-FSHARP-ADR-0002-model-guild-fsharp-ownership.md) · [GUIDERS-FSHARP-ADR-0003](https://github.com/AI-Guiders/guiders-fsharp/blob/main/docs/adr/GUIDERS-FSHARP-ADR-0003-model-extraction-matrix.md) · [CDP-ADR-0208](https://github.com/AI-Guiders/cdp-mcp/blob/main/docs/adr/CDP-ADR-0208-language-resolver-center-cdp-host.md) |

## Context

Federation hosts edit many languages in one workspace:

| Family | Examples | Parse / validate SSOT today |
|--------|----------|----------------------------|
| GPL | C#, F#, TS, PS | Roslyn, FCS, tsserver/LSP — **planet-local** (CDP) |
| GDL | `*.deck.gdl`, `*.catalog.gdl`, … | `Platform.Modeling.Gdl.*` (`guiders-fsharp`) |
| Planet DSL | `.dashspec`, … | sovereign repos |

**Bare IDE verbs** — `get_diagnostics`, `get_document_symbols`, `go_to_definition`, `find_usages`, … — must return **one typed contract** regardless of backend. Today CDP (`IdeLanguageTools.Dispatch`) implements a flat `if language` switch with **no F# slot**, **no GDL slot**, and **per-language JSON shapes**.

[GUIDERS-ADR-0025](./GUIDERS-ADR-0025-language-intelligence-boundary.md) defines **`LanguageIntelligence.*`** for **edit intelligence** (Anchor, Locus, Sniper, buffer mutate tiers). LRC is the **sibling concern**: **workspace language service** (LSP-shaped diagnostics / outline / navigation). Same federation road; different verbs.

[GUIDERS-ADR-0059](./GUIDERS-ADR-0059-gdl-hyperlane.md) §9 requires one **GDL language service host** — LRC is that host’s federation contract, not a CDP-only implementation detail.

**Constitution:** federation sells **roads** ([GUIDERS-FEDERATION-CONSTITUTION](../GUIDERS-FEDERATION-CONSTITUTION.md)). LRC kernel + resolver **must not** be SSOT inside a single planet (`cdp-mcp`). CDP is **first dogfood host** ([CDP-ADR-0208](https://github.com/AI-Guiders/cdp-mcp/blob/main/docs/adr/CDP-ADR-0208-language-resolver-center-cdp-host.md)).

## Decision

### 1. Language Resolver Center (LRC)

Replace per-host language switches with a **single federation gateway**:

```text
(file path, project hint) ──► LanguageResolverCenter
                                    │
              ┌─────────────────────┼─────────────────────┐
              ▼                     ▼                     ▼
      Adapter.Roslyn        Adapter.Fcs           Adapter.Gdl
       .cs / .csproj          .fs / .fsproj        *.{quarry}.gdl
              │                     │                     │
              └──────────► Platform.Modeling.Language ◄───┘
                           (F# kernel — verb envelopes)
```

**Rule:** one resolver registry, many backends. Hosts (CDP, CIDE, DashSpec LSP, Studio) **reference** `Platform.Execution.Language`; they do not fork envelopes.

### 2. Package model (normative)

Follow [GUIDERS-FSHARP-ADR-0002](https://github.com/AI-Guiders/guiders-fsharp/blob/main/docs/adr/GUIDERS-FSHARP-ADR-0002-model-guild-fsharp-ownership.md) **Modeling / Execution** split:

```text
guiders-fsharp (Modeling)
  AIGuiders.Platform.Modeling.Language              F# — kernel + verb result envelopes
  AIGuiders.Platform.Modeling.Language.Adapters.Fcs F# — FCS backend
  AIGuiders.Platform.Modeling.Language.Adapters.Gdl F# — GDL → Platform.Modeling.Gdl.*

guiders-platform (Execution)
  AIGuiders.Platform.Execution.Language                    C# — LanguageResolverCenter, ILanguageBackend registry
  AIGuiders.Platform.Execution.Language.Adapters.Roslyn    C# — C# / MSBuild workspace
  AIGuiders.Platform.Execution.Language.Adapters.TypeScript C# — tsserver / worker bridge
  … future LSP preset adapters …
```

**Dependency rule:** `Execution.Language.*` → `Modeling.Language`. **Do not** fork `LanguageDiagnostic` / `LanguageSymbol` in CDP, CIDE, or planets.

**Do not** copy `GdlFragment` or GPL AST into `Modeling.Language` — adapters **project** into kernel envelopes only.

#### 2.1 Kernel (shared — all languages)

```fsharp
namespace AIGuiders.Platform.Modeling.Language

[<CLIMutable>] type SourceSpan = { Path: string; Line: int; Column: int; EndLine: int; EndColumn: int }
type Severity = | Error | Warning | Info | Hint

[<CLIMutable>] type LanguageDiagnostic =
    { Id: string; Severity: Severity; Message: string; Span: SourceSpan; Tags: string[]; Language: string }

[<CLIMutable>] type LanguageSymbol =
    { Name: string; Kind: string; Span: SourceSpan; Container: string; Children: LanguageSymbol[] }

[<CLIMutable>] type LanguageNavigation =
    { Definition: SourceSpan; Declarations: SourceSpan[] }

[<CLIMutable>] type DiagnosticsResult = { Diagnostics: LanguageDiagnostic[] }
[<CLIMutable>] type DocumentSymbolsResult = { Root: LanguageSymbol }
```

JSON at host wire (MCP, LSP bridge) serializes these types — **not** a parallel JSON-schema SSOT.

#### 2.2 Gateway seam (C#)

```csharp
public interface ILanguageBackend {
    string LanguageId { get; }
    bool CanHandle(string path, ProjectHint hint);
    Task<DiagnosticsResult> GetDiagnosticsAsync(LanguageRequest req, CancellationToken ct);
    Task<DocumentSymbolsResult> GetDocumentSymbolsAsync(LanguageRequest req, CancellationToken ct);
    Task<LanguageNavigation?> GoToDefinitionAsync(LanguageRequest req, CancellationToken ct);
}
```

`LanguageResolverCenter` resolves backend from path + registers capabilities. **Per-file resolution** wins over session default.

### 3. Language catalog (federation)

| `LanguageId` | Adapter package | Extensions / signal |
|--------------|-----------------|---------------------|
| `csharp` | `Adapters.Roslyn` | `.cs`, `.csproj`, `.sln` (C#-primary) |
| `fsharp` | `Adapters.Fcs` | `.fs`, `.fsproj`, F# slnx entries |
| `typescript` | `Adapters.TypeScript` | `.ts`, `.tsx`, `.js`, `tsconfig` |
| `gdl` | `Adapters.Gdl` | `*.{quarry}.gdl`, `.gdlproj` |
| `powershell` | LSP preset (host) | `.ps1` |
| python, delphi, … | LSP presets | unchanged at host |

**Refuse** wrong pairings: `.fs` + `language=csharp`, `.deck.gdl` + Roslyn.

### 4. GDL backend (Authoring Guild)

GDL is a **first-class** `gdl` language in LRC — not a CDP special case.

| Item | Owner |
|------|-------|
| Grammar / AST / validation | **Authoring Guild** — `Platform.Modeling.Gdl.*` |
| `GdlBackend` adapter | `Platform.Modeling.Language.Adapters.Gdl` |
| IDE verbs | same LRC envelopes as GPL |

```text
get_diagnostics on foo.deck.gdl
  → Adapters.Gdl
  → Platform.Modeling.Gdl.Parse.Deck + Validation
  → DiagnosticsResult (Language="gdl")
```

**Non-Turing-complete** GDL constrains runtime eval — **not** IDE tooling depth. Sniper / fix / peel on declare files follow the same phased ladder as FCS ([CDP-ADR-0208](https://github.com/AI-Guiders/cdp-mcp/blob/main/docs/adr/CDP-ADR-0208-language-resolver-center-cdp-host.md) P8–P10 for habitat wiring).

### 5. Relationship to LanguageIntelligence ([0025](./GUIDERS-ADR-0025-language-intelligence-boundary.md))

| Concern | Guild / package | Verbs |
|---------|-----------------|-------|
| **LRC** | `Platform.Execution.Language` + `Modeling.Language` | diagnostics, symbols, goto, usages |
| **LanguageIntelligence** | `LanguageIntelligence.*` | Anchor, Locus, Sniper, buffer edit tiers |

- **Shared later:** `SourceSpan`, Roslyn adapter process, resolve tiers — adapters may consolidate in Phase E; **do not** block LRC on LI Phase 2.
- **`Modeling.Gdl.Language`** (matrix §4.3) — neutral **edit** shapes (Locus, TextEdit); complements LRC navigation envelopes. Same repo family; distinct packages.

### 6. Host responsibilities (planets)

| Host | Owns | References |
|------|------|------------|
| **CDP** | MCP wire, DocumentStore, tenant, `cdp_*` catalog | `Platform.Execution.Language` |
| **CIDE / cascade-ide** | buffer, sniper UI, CSX | LRC + LI |
| **DashSpec LSP** | `.dashspec` planet grammar | LRC optional; planet parser separate |
| **guiders-platform** | Roslyn test workspaces | `Adapters.Roslyn` smoke |

Planets **reference** federation packages — never fork LRC kernel.

### 7. Wire format

Hosts serialize `Modeling.Language` CLIMutable types at their boundary (CDP → MCP JSON). **OpenAPI / hand-written JSON schema is not SSOT.**

## Migration phases (federation)

```text
F0  ADR-0061 + package scaffold (Modeling.Language kernel in guiders-fsharp)
F1  Platform.Execution.Language — resolver + registry (guiders-platform)
F2  Adapters.Fcs + Adapters.Roslyn → kernel parity (diagnostics + symbols)
F3  Adapters.TypeScript + Adapters.Gdl (deck/catalog pilots)
F4  CDP cutover — IdeLanguageTools → Platform.Execution.Language ([CDP-ADR-0208](https://github.com/AI-Guiders/cdp-mcp/blob/main/docs/adr/CDP-ADR-0208-language-resolver-center-cdp-host.md))
F5  GDL goto / usages; FSharp.Analyzers in diagnostic stream
F6  LI adapter consolidation (optional); GDL sniper axis with LanguageIntelligence
```

Sibling wire: extend `eng/Guiders.Modeling.props` with `UseGuidersModelingLanguage=true`.

## Consequences

- LRC is **federation-level** — any planet can embed IDE verbs without importing all of CDP.
- GDL tooling rides the same road as C#/F# ([0059](./GUIDERS-ADR-0059-gdl-hyperlane.md) §9 satisfied by LRC + `Adapters.Gdl`).
- CDP remains Agent Env planet — **hosts** LRC first, does not own the contract.
- Platform waves (Authoring parse, IR absorption) **feed** `Adapters.Gdl` but are not blocked on CDP.

## Non-goals (F0–F3)

- Merging LRC into `LanguageIntelligence.Core` as one mega-guild.
- Copying quarry ASTs into `Modeling.Language`.
- NuGet publish (sibling `ProjectReference` until platform arch debt window closes).
- Planet DSL grammars (`.dashspec`) in federation LRC v1.

## References

- [GUIDERS-ADR-0025](./GUIDERS-ADR-0025-language-intelligence-boundary.md) — Anchor / Sniper / edit tiers
- [GUIDERS-ADR-0059](./GUIDERS-ADR-0059-gdl-hyperlane.md) — GDL hyperlane + tooling §9
- [GUIDERS-FSHARP-ADR-0003](https://github.com/AI-Guiders/guiders-fsharp/blob/main/docs/adr/GUIDERS-FSHARP-ADR-0003-model-extraction-matrix.md) — `IR.Language` → `Modeling.Gdl.Language`
- [CDP-ADR-0208](https://github.com/AI-Guiders/cdp-mcp/blob/main/docs/adr/CDP-ADR-0208-language-resolver-center-cdp-host.md) — CDP first host profile
