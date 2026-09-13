# GUIDERS-ADR-0021: Notations quarry family (Keyboard · Command · Argument)

| | |
|---|---|
| **Status** | **Accepted** (v1 Final — see §12) |
| **Date** | 2026-08-30 |
| **Tags** | #guiders #notations #quarry #inputnotation #slash #commandplane #conformance |
| **Relates to** | GUIDERS-ADR-0015 · GUIDERS-ADR-0016 · GUIDERS-ADR-0018 · GUIDERS-ADR-0019 · GUIDERS-ADR-0020 · [Constitution § Planets are not SSOT](../GUIDERS-FEDERATION-CONSTITUTION.md#planets-are-not-federation-ssot) |

## Context

Federation already treats **wire alphabets** as quarryable siblings of **mechanics**:

| Today | Wire examples | IR (proto) | Consumer |
|-------|---------------|------------|----------|
| `InputNotation.*` | `<C-k>`, `Ctrl+K`, `C-x` | `NormalizedKeySequence` | Melody, Binding |
| `CommandPlane.Slash` (inline) | `/docs adr open` | `CanonicalPath` + `ArgTail` string | Slash catalog, completion |
| Product console | `buffer open doc=README.md` | ad hoc parsers | sovereign planets |

[GUIDERS-ADR-0016](GUIDERS-ADR-0016-input-notation-quarry-family.md) named **InputNotation** as a sibling guild. Slash path parsing lives inside **CommandPlane.Slash** (`SlashLineResolver`). Argument tails are policy strings (`ArgTail`, `picker:…`, `tail_wire_class`) without a shared IR package.

Operators asked: if Vim and KeyGesture are two **notations** for the same keyboard IR, why not treat **slash** and **console** as two **notations** for the same command-line IR?

**Working umbrella:** **`Notations.*`** — one quarry family, five branches:

```text
Notations
├── Keyboard.*        ← today InputNotation.* (rename target)
├── Command.*         ← path / verb wire → NormalizedCommandLine
├── Argument.*        ← wire + profile → NormalizedArguments (`Notations.Argument`, `Argument.All`)
├── Bracket.*         ← paired delimiters `<…>` `[…]` → NormalizedBracketWire ([ADR-0026](GUIDERS-ADR-0026-notations-bracket-branch.md))
└── Presentation.*    ← topology / layout wires → PresentationTopology ([ADR-0058](GUIDERS-ADR-0058-presentation-topology-ir.md))
```

**Mechanics** (Slash, Melody, Binding) and **planes** (CommandPlane, MCPlane) **consume** Notations; they do not own wire parsers.

**Planet boundary:** no planet wire (CDP Citizen, Forge-only quirks, CIDE palette) is federation SSOT. Conformance vectors and neutral IR are SSOT; planets are early implementers at most.

## Decision

### 1. Notations is a sibling hyperlane guild

```text
                         Federation platform
    ┌────────────────────┬────────────────────┬────────────────────┐
    │  Notations.*       │  CommandPlane.*    │  MCPlane           │
    │  wire → IR         │  catalog/registry  │  agent envelope    │
    │  quarry + specs    │  mechanics         │  pulse / next[]    │
    └─────────┬──────────┴─────────┬──────────┴─────────┬──────────┘
              │                    │                      │
              └────────────────────┴──────────────────────┘
                          product surfaces + MCP hosts
```

Same constitution rule as InputNotation: **reference quarry on NuGet; native port per stack** (TS for Forge slash, Kotlin, …).

### 2. Three branches, one pattern

Each branch follows the ADR-0016 axes:

```text
WIRE FORMAT(s)  →  IR (branch Core)  →  MECHANIC / product
```

| Branch | Question | Core IR | Example wires |
|--------|----------|---------|---------------|
| **Keyboard** | How is a key/gesture written? | `NormalizedKeySequence` | Vim, Neovim, Emacs, KeyGesture |
| **Command** | How is a command **named** in text? | `NormalizedCommandLine` | Slash path, console path, flat verb |
| **Argument** | How are **params** written after the name? | `NormalizedArguments` + `ArgumentNotationProfile` | slash remainder, `key=value`, JSON (optional) |
| **Bracket** | How is a **paired-delimiter** payload written? | `NormalizedBracketWire` + `BracketNotationProfile` | `[F:…;M:…]`, `<C-k>` (profile-specific) |

**Compose at resolve time:**

```text
NormalizedInvocation = NormalizedCommandLine + NormalizedArguments?
       │
       └──► CommandCatalogIndex / registry  ──► commandId  ──► Execute
```

### 3. Package map (target)

NuGet prefix: **`AIGuiders.Platform.Notations.*`**

```text
Notations.Core              shared: INotationReader<T>, spec loader hooks
Notations.Quarry            shared lexer/normalizer/conformance helpers

Notations.Keyboard.Core     NormalizedKeySequence, IKeyboardNotationReader
Notations.Keyboard.Vim
Notations.Keyboard.Neovim
Notations.Keyboard.Emacs
Notations.Keyboard.KeyGesture
Notations.Keyboard.All      facade (optional)

Notations.Command.Core      NormalizedCommandLine { Path, PathSegments[] }
Notations.Command.Slash     `/domain/object/intent` body (no leading `/` policy in reader)
Notations.Command.Console   neutral path tokens (no `@intent` — product extension)

Notations.Argument            ArgumentNotationProfile, ArgumentSlot, NormalizedArguments
Notations.Argument.All        ArgumentNotation.Parse (profile → IR)
Notations.Argument.Slash    space-separated tail + picker token passthrough
Notations.Argument.Kv       `key=value` rest-of-line (console parity)
Notations.Argument.Positional ordered tokens (v1)
Notations.Argument.Delimited colon/csv via wire_class (v1)
Notations.Argument.Cli      (v2) POSIX/GNU-like flags via System.CommandLine quarry
Notations.Argument.PowerShell (defer) `-Name Value` grammar
Notations.Argument.Json     (defer) schema-shaped args for MCP symmetry

Notations.Bracket           BracketNotationProfile, BracketAxis, NormalizedBracketWire ([ADR-0026](GUIDERS-ADR-0026-notations-bracket-branch.md))
Notations.Bracket.All       optional meta-bundle; planet profiles via conformance specs

Notations.All               optional meta-bundle
```

**Namespace mirror:** `AIGuiders.Platform.Notations.Keyboard.Vim`, `…Command.Slash`, etc.

### 4. Mechanics consume; they do not parse wire

| Mechanic | Notation consumers | Discovery key (unchanged) |
|----------|-------------------|---------------------------|
| **Slash** | `Command.Slash` + `Argument.Slash` | slash **path** |
| **Melody** | `Keyboard.*` (steps) + `Argument.*` (tail via `wire_class`) | melody **slug** |
| **Binding** | `Keyboard.KeyGesture` (or Vim) | **gesture** → `commandId` |

[GUIDERS-ADR-0015](GUIDERS-ADR-0015-invocation-mechanics-slash-melody-binding.md) stands: Slash/Melody/Binding are **not** notations. Palette `c:` stays discoverability only.

### 5. Slash ≡ console (same IR, different readers)

| Wire | Example | Reader | Same IR |
|------|---------|--------|---------|
| Slash | `/buffer open README.md` | `Notations.Command.Slash` + `Argument.Slash` | path `buffer/open`, tail `README.md` |
| Console (neutral) | `buffer open doc=README.md` | `Notations.Command.Console` + `Argument.Kv` | same path + structured slots |

Product-specific prefixes (`/`, `@mention`, tool name) are **surface policy** — strip before reader or live in product adapter, not federation SSOT. Umbrella glossary: **InvocationEngage** → **Sigil** ([GUIDERS-ADR-0036](GUIDERS-ADR-0036-invocation-engage-glossary.md)).

**Planet in-house wires** (e.g. experimental habitat frames, product-only sigils) stay on that planet — not Notations packages.

### 6. MCP boundary

MCP `CallTool(name, jsonArgs)` is **not** Command notation on v1:

- Tool name → `commandId` map = CommandPlane + MCPlane catalog projection.
- JSON args = schema-driven; optional future `Notations.Argument.Json` if conformance needs it.

Do not block Slash/Console quarry on MCP JSON grammar.

### 7. Migration from InputNotation

| Phase | Action |
|-------|--------|
| **Now** | Ship this ADR; keep publishing `InputNotation.*` |
| **W2g** | Add `Notations.*` packages; **type-forward** or duplicate-publish aliases `InputNotation → Notations.Keyboard` |
| **W2h** | Move `SlashLineResolver` body/tokenize into `Notations.Command.Slash`; CommandPlane.Slash calls Notations |
| **W2i** | Conformance vectors under `notation/*` ([ADR-0019](GUIDERS-ADR-0019-conformance-hyperlane-monorepo.md)) | **Shipped** — `docs/conformance/notation/*` + embedded fixtures |
| **Later** | Remove `InputNotation` NuGet IDs after CIDE/Forge pin Notations (obsoletion **shipped**; sunset date TBD — see [migration doc](../migration/inputnotation-to-notations-v1.md)) |

[GUIDERS-ADR-0016](GUIDERS-ADR-0016-input-notation-quarry-family.md) remains accepted for **keyboard quarry semantics**; **package naming target** moves to this ADR.

### 8. Core IR shapes (sketch)

```csharp
// Notations.Keyboard.Core — unchanged from InputNotation Core
record NormalizedKeySequence(/* steps */);

// Notations.Command.Core
record NormalizedCommandLine(
    string CanonicalPath,
    IReadOnlyList<string> PathSegments);

// Notations.Argument.Core
record NormalizedArgTail(
    string? Raw,
    IReadOnlyDictionary<string, string>? Slots,
    string? WireClass);
```

`SlashLineResolver.TryResolveBody` becomes the reference implementation seed for `Command.Slash` + `Argument.Slash` composed lookup against catalog (catalog stays CommandPlane).

### 9. Conformance (v1 shipped)

| Spec | Proves | Status |
|------|--------|--------|
| `notation/neovim-kbd` | Neovim wire → `NormalizedKeySequence` | **shipped** — `QuarryNotationConformanceTests` |
| `notation/emacs-kbd` | Emacs wire → `NormalizedKeySequence` | **shipped** |
| `notation/key-gesture` | KeyGesture / hotkeys.toml → IR; Vim subset parity | **shipped** |
| `notation/command-slash` | path tokenization + longest-prefix body | **shipped** — `NotationConformanceTests` |
| `notation/argument-kv` | `key=value` pairs → slots | **shipped** |
| `notation/argument-positional` | ordered tokens after path | **shipped** |
| `notation/argument-delimited` | `wire_class=colon` → slots | **shipped** |
| `notation/argument-cli` | POSIX/GNU flags quarry | **shipped** (v1 subset; full quarry v2) |
| `notation/invocation-parity` | slash + kv readers → same path | **shipped** |
| `notation/argument-cli-v1` | (alias) | see `argument-cli` |
| `notation/argument-json` | MCP JSON args | **defer** |
| `notation/keyboard-vim-v1` | Vim document chords | **covered** by Neovim + `VimChordNotationParser` unit tests |
| `notation/presentation-topology` | deck topology wire → `PresentationTopology` | **shipped** — `PresentationTopologyConformanceTests` |

### 10. Notation families inventory

Wire families operators named in the wild — and federation stance.  
**Command** = how the verb/path is written; **Argument** = everything after the path is resolved.

#### Command (path / verb)

| Family | Example | Federation | Notes |
|--------|---------|------------|-------|
| **Positional path** | `git remote add` | **v1** (`Command.Console`) | Subcommand chains → `PathSegments[]` |
| **Slash path** | `/docs adr open` | **v1** (`Command.Slash`) | Leading `/` = surface policy |
| **Sigil path** | `:w`, `!help`, `@intent …` | product strip ([ADR-0036](GUIDERS-ADR-0036-invocation-engage-glossary.md) **Sigil**) | After strip → Console or Slash IR |
| **Dotted path** | `module.sub.command` | defer | Optional `Command.Dotted` if catalog uses it |
| **Tool name** | `cdp_buffer`, `example.exe` | projection | MCP / Win `.exe` = surface; maps to `commandId` |
| **REST path** | `/api/v1/users/123` | defer | HTTP surface, not slash catalog |
| **Palette fuzzy** | `fmt ins` | product | Discovery UI, not linear wire |

#### Argument (tail / params)

| Family | Example | Federation | Notes |
|--------|---------|------------|-------|
| **Raw / space tail** | `open README.md` | **v1** (`Argument.Slash`) | Remainder string + catalog `ArgTail` policy |
| **Positional args** | `arg1 arg2` | **v1** (`Argument.Positional`) | Ordered tokens after path |
| **Key=value** | `doc=README.md op=scene` | **v1** (`Argument.Kv`) | Console / agent meta parity |
| **Colon-delimited** | `arg1:arg2:arg3` | **v1** (`wire_class`) | `Argument.Delimited` + `WireClass=colon` |
| **GNU/POSIX-like flags** | `-h`, `--out=file`, `-abc` | **v2** (`Argument.Cli`) | See §11 — quarry, not v1 blocker |
| **Windows `/switch`** | `program /S /P` | **v2** | Often merged into `Argument.Cli` subset or product |
| **PowerShell params** | `-Name Value`, `-Name:Value` | **defer** (`Argument.PowerShell`) | Heavy grammar; see §11 |
| **JSON object** | `{ "op": "scene" }` | defer | MCP / schema; MCPlane projection |
| **Shell meta** | `\|`, `&&`, `>` | product | `Notations.Shell` out of scope |
| **CSV / `;` query** | `a=1;b=2` | defer | `WireClass` extension |

#### Operator examples mapped

| Wire | Command reader | Argument reader |
|------|----------------|-----------------|
| `example.exe -exampleArg` | tool name (surface) | `Argument.Cli` (v2) |
| `example.exe exampleArg` | tool name | `Argument.Positional` |
| `/example exampleArg` | `Command.Slash` | `Argument.Slash` / Positional |
| `/example arg1:arg2:…` | `Command.Slash` | `Argument.Delimited` (`colon`) |
| `buffer open doc=README.md` | `Command.Console` | `Argument.Kv` |

### 11. Parser quarry (POSIX · PowerShell · System.CommandLine)

Keyboard notation had **no** standalone NuGet SSOT (parsers live inside Neovim/Emacs). **Command-line argument** notation is better served — but mature packages are **CLI app frameworks**, not federation **wire → `NormalizedArgTail`** adapters.

| Layer | What exists | Federation use |
|-------|-------------|----------------|
| **SSOT behavior** | POSIX/SUS `getopt`, GNU `getopt_long`, BSD variants, PowerShell language spec | Conformance vectors + docs — not one true binary |
| **.NET reference** | [`System.CommandLine`](https://www.nuget.org/packages/System.CommandLine) (Microsoft; `dotnet` CLI stack) | **Quarry for modern `-` / `--` tokenization** into slots |
| **Alternates** | `CommandLineParser`, `McMaster.Extensions.CommandLineUtils`, `Mono.Options` | App builders; do **not** multi-pin — pick one quarry or own subset |
| **PowerShell** | `System.Management.Automation` parser | **Too heavy** for `Notations` core; optional product dep or **native port only** (JS/Kotlin never pull PS SDK) |
| **Kv / slash tail** | Small lexers (`SlashLineResolver`, planet consoles) | **Own v1** — not worth a NuGet framework |

**Decision:**

1. **v1 ship without** `System.CommandLine` dependency — `Argument.Slash`, `Argument.Kv`, `Argument.Positional`, `Argument.Delimited` are small owned quarries + spec JSON.
2. **`Notations.Argument.Cli` (v2)** — thin adapter over **System.CommandLine** token model (or ported subset of its test vectors):
   - Input: `string[]` or tail string **after** `commandId` / path is known.
   - Output: `NormalizedArgTail` with `Slots` + positional remainder + `UnparsedTokens` for completion.
   - **Not** a hosted `RootCommand` per federation consumer — ephemeral parse against **descriptor-supplied** option schema (`CommandDescriptor` / capabilities arg schema).
3. **POSIX vs GNU:** federation documents **tier tables** (like Vim v1/v2 in ADR-0016): v1 = `System.CommandLine`-aligned modern CLI; v2 = GNU edge cases (`--opt=value`, clustered shorts) from upstream vectors where license permits.
4. **PowerShell:** defer dedicated package; planets that need PS wire implement **native port** or optional `Notations.Argument.PowerShell` with **explicit** dependency on PowerShell SDK (not in meta `Notations.All`).
5. **Windows `/switch`:** do not invent a third grammar in v1 — map product rules in adapter; conformance only where CIDE/Forge need parity.

```text
v1 (owned, no heavy deps)          v2 (quarry)                 defer
─────────────────────────          ─────────────                 ─────
Command.Slash / Console            Argument.Cli                  Argument.PowerShell
Argument.Slash / Kv / Positional     ← System.CommandLine          Argument.Json
Argument.Delimited (wire_class)      (descriptor-driven parse)     Shell / REST
```

**Rule (same as InputNotation):** Platform ships **IR + spec + reference quarry**; VS Code / Forge JS **port vectors**, they do not embed `System.CommandLine`.

Updated package map (Argument branch):

```text
Notations.Argument.Positional   argv-style tokens
Notations.Argument.Delimited    colon/csv wire_class
Notations.Argument.Cli            (v2) System.CommandLine quarry → NormalizedArgTail
Notations.Argument.PowerShell     (defer) optional heavy package
```


- Renaming packages in this wave (ADR only).
- Replacing CommandPlane mechanics or MCPlane.
- Normative **planet** grammars (`@intent`, Citizen frames, buffer Meta tools).
- Universal MCP JSON Schema → Notations (defer).
- Pulling **PowerShell SDK** into default `Notations.All` bundle.
- Shipping a full **CLI app host** inside Notations (use System.CommandLine in products; Notations only parses tail → IR).

### 12. Implementation status (v1 Final)

| Deliverable | Status | Notes |
|-------------|--------|-------|
| `Notations.Keyboard.*` packages + `InputNotation` aliases | **Shipped** | Roadmap W2e, Wave 4 |
| `Notations.Command.Slash` + `Command.Console` | **Shipped** | `SlashLineResolver` delegates tokenize |
| `Notations.Argument.*` (Kv, Positional, Delimited, All) | **Shipped** | v1 owned quarries |
| `Notations.Bracket` | **Shipped** | ADR-0026 Phase 0+ |
| `IR.Argument` / `IR.Invocation` split | **Shipped** | ADR-0042 |
| Conformance hyperlane (`notation/*` vectors) | **Shipped (v1)** | `docs/conformance/notation/*` + `NotationConformanceTests` / `QuarryNotationConformanceTests` |
| `Argument.Cli` (POSIX/GNU-like flags quarry) | **Shipped (v1 subset)** | Owned lexer in `Notations.Argument.Cli`; no `System.CommandLine` package dependency; full descriptor-driven quarry remains v2 |
| `Argument.Json` / `Argument.PowerShell` | **Deferred** | §10 inventory |
| `InputNotation.*` package obsoletion | **Shipped** | Assembly + type `[Obsolete]`; packages still publish; removal sunset **TBD** — [migration doc](../migration/inputnotation-to-notations-v1.md) |
| Native ports (Forge TS slash, …) | **Planned** | spec-first; not blocking v1 |
| `Notations.Presentation.Topology` | **Shipped** | ADR-0058 v1; `TopologyNotation.Parse` + `notation/presentation-topology` vectors |
| `Notations.Shell` (pipe / `&&` / redirect meta) | **Deferred** | §10 — product shell hosts, not federation wire IR |
| `Authoring.Display.Binding` (`.display.gdl`) | **Deferred** | Wave 2; v1 runtime profile via `Execution.Presentation.Binding` table stub |

### 12.1 Addendum — Presentation.Topology branch (ADR-0058 closure)

| Branch | Question | Core IR | Example wires |
|--------|----------|---------|---------------|
| **Presentation.Topology** | How are logical display **hosts** declared? | `PresentationTopology` + `LogicalDisplayHost` | `(MFD)(F)`, `(F/P/M)`, `single` |

Package: `AIGuiders.Platform.Notations.Presentation.Topology` — parse wire → IR. Physical monitor binding is **`DisplayBindingProfile`** (deployment SSOT, not notation).

**Explicit defer (unchanged):** `Argument.Json`, `Argument.PowerShell`, `Notations.Shell` — see §10 Argument inventory.

**Authoring layer (GDL):** declare-time **GDL** (Guiders Declarative Language) — **`Authoring.*`** ([GUIDERS-ADR-0048](./GUIDERS-ADR-0048-authoring-quarry-family.md) Accepted, name [0059](./GUIDERS-ADR-0059-gdl-hyperlane.md)). Quarries: `.catalog`, `.deck`, `.display` (proposed), `.cockpit.logic` (proposed). Codegen **emits** tier-D wire consumed by `Notations.Command.*` / `Notations.Argument.*` at resolve time. **Do not** place authoring parsers under `Notations.*`.

### 13. Resolved decisions (post-v0 → v1 Final)

| # | Question | Decision |
|---|----------|----------|
| 1 | Single `Notations.Core` reader interface vs branch-specific? | **Keep branch-specific interfaces** (`IKeyboardNotationReader`, command/argument readers per branch). Shared hooks stay in `Notations` / quarry packages; no forced universal `INotationReader<T>` in v1. |
| 2 | `Console` reader: path tokens only, or tool names too? | **Notations = wire projections** to shared invocation IR (see **Projection model** below). **`Command.Console`** parses multi-token path + argument tail only; a single token is a **one-segment path**, not registry/tool-name lookup. **Surface projections** (MCP tool names, `.exe`, `@intent` / sigil strip, catalog `commandId` aliases) live in **CommandPlane / MCPlane**, not in Notations. |
| 3 | `Argument.Json` with MCPlane conformance or product-local? | **Defer** `Notations.Argument.Json`; MCP JSON args remain schema-driven via MCPlane / catalog projection until a conformance gate needs a shared wire IR. |
| 4 | `InputNotation` NuGet obsoletion timeline? | **Obsoletion shipped** (warnings + migration doc); **package removal sunset TBD** after CIDE/Forge pin `Notations.*`. |
| 5 | `Argument.Cli`: pin `System.CommandLine` vs own lexer? | **Own lexer** for v1 subset (`CliArgumentNotation`); align vectors with System.CommandLine semantics without taking a NuGet dependency. Descriptor-driven full quarry stays v2. |
| 6 | Which `notation/*` vectors in next hyperlane gate? | **v1 gate complete** per §9 table; future gates add GNU edge cases, `argument-json`, and native-port parity only when a planet declares need. |

### 13.1 Projection model (normative)

**Operator rule (2026-09-13):** treat **Slash, Console, KeyGesture, Bracket, Presentation.Topology, Argument.\*** as **projections** of the same canonical invocation — not independent grammars per surface.

| Canonical semantics | Slash wire | Console wire (flavor-dependent) |
|--------------------|------------|----------------------------------|
| `git plan verify` | `/git plan verify` | `git plan --verify` or `git -p -v` (profile + `Argument.Cli` / Kv) |

```text
Surface projection (CommandPlane / MCPlane)  →  strip aliases, map toolName → commandId
Notation projection (Notations.*)            →  wire → NormalizedCommandLine / NormalizedArguments / …
CommandPlane resolve                         →  catalog longest-prefix, runnable policy
```

- **Keyboard / KeyGesture:** projection → `NormalizedKeySequence`; **Binding** maps chord → command (mechanic, not notation).
- **MCP JSON args:** surface projection + schema validation — **not** `Notations.Argument.Json` in v1 (§13 row 3).
- **Conformance:** vectors assert **wire → IR** for a named projection; catalog phrase slots come from GDL emit, not duplicated in notation specs.

## Consequences

- One mental model: **Notation = wire projection → IR**, **Mechanic = how user invokes**, **Plane = federation contract layer** (surface projection + resolve).
- Forge JS slash port targets **Notations.Command/Argument** specs, not `CommandPlane.Slash` internals.
- **`SlashLineResolver` boundary:** slash **wire tokenize** lives in `Modeling.Notations.Command` (`SlashCommandNotation`); **catalog longest-prefix resolution** stays in `CommandPlane.Slash` (`SlashLineResolver`). Notations owns alphabet → IR; CommandPlane owns registry lookup and runnable policy.
- Constitution hyperlane row evolves: `Notations.*` supersedes `InputNotation.*` label when packages ship.
- Root pains: [GUIDERS pain inventory](../GUIDERS-pain-inventory.md) **G-001**, **G-003**, **G-011**.

## References

- [GUIDERS-ADR-0016 InputNotation quarry](GUIDERS-ADR-0016-input-notation-quarry-family.md)
- [GUIDERS-ADR-0015 invocation mechanics](GUIDERS-ADR-0015-invocation-mechanics-slash-melody-binding.md)
- [GUIDERS-ADR-0018 slash conformance vectors](GUIDERS-ADR-0018-slash-conformance-vectors.md)
- `SlashLineResolver`, `InputNotationParser` — reference quarry seeds
- [System.CommandLine](https://www.nuget.org/packages/System.CommandLine) — v2 quarry candidate for `Argument.Cli` (not v1 dependency)
- [GUIDERS-ADR-0048 authoring quarry](./GUIDERS-ADR-0048-authoring-quarry-family.md) — **`Authoring.*`** guild; declare → IR → emit
- [GUIDERS-ADR-0047 command catalog authoring](./GUIDERS-ADR-0047-command-for-doi.md) — `.catalog` branch under Authoring
