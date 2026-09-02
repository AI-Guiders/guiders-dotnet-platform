# GUIDERS-ADR-0027: MdLinker — doc anchor check on Bracket + LanguageIntelligence

| | |
|---|---|
| **Status** | Accepted |
| **Date** | 2026-08-30 |
| **Tags** | #guiders #utilities #documentation #anchor #bracket #mdlinker #drift |
| **Relates to** | GUIDERS-ADR-0022 · GUIDERS-ADR-0025 · GUIDERS-ADR-0026 · CIDE ADR-0128 · CIDE ADR-0186 · G-008 |

---

## Context

Federation docs (ADR narrative, README tables, constitution sketches) **rot faster than code** — expected for prose, painful for **API vocabulary** (`NormalizedArgTail`, `WireClass`, package names).

Alternatives considered in operator review (2026-08-30):

| Approach | Verdict |
|----------|---------|
| Hand-edited markdown tables | rots; G-008 |
| XMLDoc only | API summaries in `.cs`; no ADR/README inline refs; `cref` does not cover catalog wire |
| Separate «md compiler» + regex `{{symbol:…}}` | duplicates bracket grammar; third wire alphabet |
| Generated glossary only (`*.generated.md`) | good for tables; not enough for inline ADR refs |
| **Anchor wire in markdown + dry-resolve** | reuses 0128 / 0186 / platform Bracket + LI |

CDP and CIDE already treat **bracket wire** as the federation pointer:

```text
[F:File.cs; M:Member]                    Family:code  → Locus (re-resolve)
[Family:navigation; Command:open; …]     Family:navigation
[FRG:pilot/issues/7]                     Forge family (CIDE 0159)
```

**MdLinker** is not a new bracket language. It is a **Utilities** tool that scans markdown for existing bracket envelopes and runs **dry-resolve** — CI exit code on broken anchors (doc-time `CS0246`).

Same hyperlane pattern as [GUIDERS-ADR-0022](GUIDERS-ADR-0022-utilities-adoption-report.md):

```text
SOURCE (markdown files)  →  IR (anchor wires)  →  CHECK / optional EMIT
```

---

## Decision (proposed)

### 1. MdLinker consumes existing parsers — no parallel regex grammar

```text
docs/**/*.md  (+ optional README)
        │
        │  locate `[` … `]` candidates (BracketEnvelopeScan — Notations.Bracket)
        ▼
Notations.Bracket.BracketReader(profile)
        ▼
NormalizedBracketWire
        ▼
LanguageIntelligence.Anchors  (family dispatch → IAnchorResolver)
        ▼
--check  → exit 1 on unresolved
--write  → optional linkified md (defer v2)
```

**Rejected:** MdLinker-owned `{{symbol:Type.Member}}` token language.

### 2. Two anchor families in docs (v1 + v1.1)

| Family | Example wire | Resolver | When |
|--------|--------------|----------|------|
| **code** (reuse CIDE) | `[F:src/…/ArgumentReaders.cs; M:Kv]` | Roslyn / compilation | file-known refs; compact axes |
| **doc** (platform) | see §2.1 | symbol bind in platform sln | ADR inline type / member refs |

`Family:doc` uses the **same** `BracketNotationProfile` machinery as [ADR-0026](GUIDERS-ADR-0026-notations-bracket-branch.md) — profile id `bracket.doc-symbol`, its own axis vocabulary.

Catalog external wire (`tail_wire_class` in TOML) **stays catalog field**; mapping to `ReaderId` lives in generated glossary ([ADR-0022](GUIDERS-ADR-0022-utilities-adoption-report.md) pattern) unless later promoted to `CatalogField:tail_wire_class` on `Family:doc`.

#### 2.1 `Family:doc` axis canon (decided)

**Platform doc axes are not required to mirror CIDE one-letter aliases** (`F:`/`M:`/`L:`). Those exist for tight agent/chat wires ([CIDE 0186](https://github.com/AI-Guiders/cascade-ide/blob/main/docs/adr/0186-anchor-families-navigation.md) canon→alias). **MdLinker / `Family:doc` prioritizes readable names** in ADR prose; profile declares allowed axis keys — no global one-char law.

| Axis (canon) | Role | Example |
|--------------|------|---------|
| `Family` | dispatch | `doc` |
| `Package` | NuGet / namespace hint (optional) | `Notations.Argument` |
| `Type` | CLR type name (short or qualified) | `NormalizedArguments` |
| `Member` | field, property, const on `Type` | `Kv`, `ReaderId` |

Examples:

```text
[Family:doc; Type:NormalizedArguments]
[Family:doc; Package:Notations.Argument; Type:ArgumentReaders; Member:Kv]
[Family:doc; Package:Notations.Argument; Type:ArgumentNotationProfile; Member:ReaderId]
```

**Rejected for doc profile:** forcing `T:`/`M:` shorthands or reusing code-family aliases — avoids confusion with `[F:…; M:…]` and keeps ADR wires self-documenting.

Optional v1.1 axes (same profile, extend `BracketAxisValuePlan`): `CatalogField`, `Reader` (catalog→internal id rows). Still **full words**, not single-char.

Parse: `BracketNotationProfile` with `ListSeparator`;`, `KvSign`:` — axis keys are opaque strings matched by profile + resolver, not hardcoded in `BracketReader`.

### 3. Package / tool map

| Layer | Artifact | Responsibility |
|-------|----------|----------------|
| **Scan** | `Notations.Bracket` (`BracketEnvelopeScan`) | depth-aware `[ … ]` in prose/md |
| **Lex** | `Notations.Bracket` (`BracketReader`, profiles) | wire → `NormalizedBracketWire` |
| **Wire IR** | `LanguageIntelligence.Anchors` (`BracketAnchorWire`, `BracketAnchorSpan`) | axis aliases, classify, format |
| **Doc resolve** | `Documentation.Anchors` (`DocSymbolAnchorResolver`) | `Family:doc` dry-resolve |
| **C# syntax resolve** | `Language.CSharp.Anchors` (`CSharpBracketAnchorResolve`) | F/M/L/S → syntax range |
| **C# symbol catalog** | `Language.CSharp.Symbols` (`RoslynDocSymbolCatalog`) | doc anchor symbol index |
| **Link check** | `Documentation.LinkCheck` (`DocAnchorChecker`) | markdown scan + resolve |
| **Link mutate** | `Documentation.LinkMutate` (`DocAnchorRenamer`) | `--apply-rename` on Type/Member axes |
| **Vocabulary** | `Documentation.Reports` + `tools/NotationGlossaryReport` | `NOTATION-VOCABULARY.generated.md` |
| **CLI** | `tools/MdLinker` | `--check` / `--apply-rename` |
| **Meta** | `AIGuiders.Platform.Execution.Utilities.DocLink` (v1.1 pack) | contracts if reused outside CLI |
| **Conformance** | `notation/bracket-doc-symbol` | wire vectors → resolve ok (planned) |
| **Planet façade** | `Cdp.ScriptableIde.BracketLocate` | thin forwarder; not SSOT |

MdLinker v1 refs **Notations.Bracket + Documentation.* + Language.CSharp.Symbols** (à la carte) — not `ScriptableIde`.

CLI is **not** a NuGet product in v1 — dogfood like `AdoptionReport`.

### 4. Relationship to XMLDoc and generated md

| Layer | Role |
|-------|------|
| **XMLDoc + `cref`** | IntelliSense / API browser on public members |
| **Anchor in ADR** | compile-check inline pointer to symbol |
| **`*.generated.md`** | vocabulary tables (ReaderId, packages, catalog aliases) — still useful |

MdLinker does **not** replace XMLDoc or AdoptionReport; it closes the **inline ref drift** gap.

### 5. CI (v1)

```bash
dotnet run --project tools/MdLinker -- --check docs/ README.md
dotnet run --project tools/NotationGlossaryReport -- --write docs/NOTATION-VOCABULARY.generated.md
# after Language.CSharp rename (or manual symbol rename):
dotnet run --project tools/MdLinker -- --apply-rename OldMember NewMember --kind member docs/
```

Same drift gate as ADOPTION-ALLIANCE: fail if unresolved anchors after renames.

Pilot scope: this ADR + `Notations.Argument.*` symbols after **ReaderId** rename (v0.21). Inline refs:

- [Family:doc; Package:Notations.Argument; Type:NormalizedArguments]
- [Family:doc; Package:Notations.Argument; Type:ArgumentReaders; Member:Kv]
- [Family:doc; Package:Notations.Argument; Type:ArgumentNotationProfile; Member:ReaderId]

---

## Phases

| Phase | Deliverable |
|-------|-------------|
| **P0** | This ADR accepted; ReaderId rename in code (v0.21) |
| **P1** | `BracketNotationProfile` `bracket.doc-symbol`; stub `DocSymbolAnchorResolver`; `tools/MdLinker --check` on one ADR |
| **P2** | `Language.CSharp.Symbols`; CI on `docs/adr/`; `NotationGlossaryReport` + drift gate |
| **P3** | Conformance `notation/bracket-doc-symbol`; `Documentation.LinkMutate --apply-rename`; migrate ADR-0021 refs |
| **P4** | optional `--write` linkify for GitHub |

**Out of scope v1:** full markdown AST compiler; Pandoc plugin; planet-specific anchor families beyond `doc` + reuse `code`.

---

## Open questions (review)

1. ~~**`Family:doc` axis canon**~~ — **decided §2.1:** full-word axes (`Type`, `Member`, `Package`); no required one-char aliases.
2. ~~**Scan / mechanics split**~~ — **decided §3:** `Notations.Bracket`; wire IR in **LI.Anchors**; doc in **Documentation.***; C# in **Language.CSharp.***; planets = façades only.
3. ~~**Packaging**~~ — **decided:** CLI-only v1 (`tools/MdLinker`); `Utilities.DocLink` deferred to v1.1.
4. ~~**Generated glossary**~~ — **decided + shipped v0.23:** `NOTATION-VOCABULARY.generated.md` via `NotationGlossaryReport`; anchors remain primary in ADR prose.

---

## Consequences

- ADR authors use bracket wires for symbol refs; plain `` `OldName` `` for moved types becomes lint failure in CI.
- Rename type/const → update anchors or MdLinker breaks (desired).
- CIDE/CDP anchor UX and platform doc check share one wire grammar over time.

---

## References

- CIDE [0128](https://github.com/AI-Guiders/cascade-ide/blob/main/docs/adr/0128-intercom-attachment-anchors-and-code-references.md) — AttachmentAnchor / code bracket
- CIDE [0186](https://github.com/AI-Guiders/cascade-ide/blob/main/docs/adr/0186-anchor-families-navigation.md) — `Family:` dispatch
- Platform [0025](GUIDERS-ADR-0025-language-intelligence-boundary.md) — `LanguageIntelligence.Anchors`
- Platform [0026](GUIDERS-ADR-0026-notations-bracket-branch.md) — `BracketReader`
