# GUIDERS-ADR-0021: Notations quarry family (Keyboard · Command · Argument)

| | |
|---|---|
| **Status** | Draft |
| **Date** | 2026-08-30 |
| **Tags** | #guiders #notations #quarry #inputnotation #slash #commandplane #conformance |
| **Relates to** | GUIDERS-ADR-0015 · GUIDERS-ADR-0016 · GUIDERS-ADR-0018 · GUIDERS-ADR-0019 · GUIDERS-ADR-0020 |

## Context

Federation already treats **wire alphabets** as quarryable siblings of **mechanics**:

| Today | Wire examples | IR (proto) | Consumer |
|-------|---------------|------------|----------|
| `InputNotation.*` | `<C-k>`, `Ctrl+K`, `C-x` | `NormalizedKeySequence` | Melody, Binding |
| `CommandPlane.Slash` (inline) | `/docs adr open` | `CanonicalPath` + `ArgTail` string | Slash catalog, completion |
| Product console | `buffer open doc=README.md`, `@intent …` | ad hoc parsers | CDP habitat (in-house) |

[GUIDERS-ADR-0016](GUIDERS-ADR-0016-input-notation-quarry-family.md) named **InputNotation** as a sibling guild. Slash path parsing lives inside **CommandPlane.Slash** (`SlashLineResolver`). Argument tails are policy strings (`ArgTail`, `picker:…`, `tail_wire_class`) without a shared IR package.

Operators asked: if Vim and KeyGesture are two **notations** for the same keyboard IR, why not treat **slash** and **console** as two **notations** for the same command-line IR?

**Working umbrella:** **`Notations.*`** — one quarry family, three branches:

```text
Notations
├── Keyboard.*     ← today InputNotation.* (rename target)
├── Command.*      ← path / verb wire → NormalizedCommandLine
└── Argument.*     ← tail / params wire → NormalizedArgTail
```

**Mechanics** (Slash, Melody, Binding) and **planes** (CommandPlane, MCPlane) **consume** Notations; they do not own wire parsers.

## Decision (proposed)

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
| **Argument** | How are **params** written after the name? | `NormalizedArgTail` | slash remainder, `key=value`, JSON (optional) |

**Compose at resolve time:**

```text
NormalizedInvocation = NormalizedCommandLine + NormalizedArgTail?
       │
       └──► SlashCatalogIndex / registry  ──► commandId  ──► Execute
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

Notations.Argument.Core     NormalizedArgTail { Raw, Slots?, WireClass? }
Notations.Argument.Slash    space-separated tail + picker token passthrough
Notations.Argument.Kv       `key=value` rest-of-line (console parity)
Notations.Argument.Json     (defer) schema-shaped args for MCP symmetry

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

Product-specific prefixes (`/`, `@intent`, tool name) are **surface policy** — strip before reader or live in product adapter, not federation SSOT.

**CDP Citizen / `@frame` wire:** in-house only ([GUIDERS-ADR-0020](GUIDERS-ADR-0020-mcplane-agent-ingress.md)); not a Notations package.

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
| **W2i** | Conformance vectors under `notation/keyboard-*`, `notation/command-slash-v1`, `notation/argument-kv-v1` ([ADR-0019](GUIDERS-ADR-0019-conformance-hyperlane-monorepo.md)) |
| **Later** | Obsolete `InputNotation` package IDs after CIDE/Forge pin Notations |

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

### 9. Conformance (future)

| Spec | Proves |
|------|--------|
| `notation/keyboard-vim-v1` | Vim wire → `NormalizedKeySequence` (move from platform embed) |
| `notation/keyboard-keygesture-v1` | `Ctrl+K` wire ≡ Vim subset where defined |
| `notation/command-slash-v1` | path tokenization + longest-prefix body |
| `notation/argument-kv-v1` | `key=value` pairs → slots |
| `notation/invocation-parity-v1` | slash + kv readers → same `commandId` for fixture catalog |

## Non-goals

- Renaming packages in this wave (ADR only).
- Replacing CommandPlane mechanics or MCPlane.
- Normative CDP `@intent` / Citizen frame grammar.
- Universal MCP JSON Schema → Notations (defer).

## Consequences

- One mental model: **Notation = wire alphabet**, **Mechanic = how user invokes**, **Plane = federation contract layer**.
- Forge JS slash port targets **Notations.Command/Argument** specs, not `CommandPlane.Slash` internals.
- Constitution hyperlane row evolves: `Notations.*` supersedes `InputNotation.*` label when packages ship.

## Open questions

1. **Single `Notations.Core` reader interface** vs three branch-specific interfaces?
2. **`Console` reader:** path tokens only, or also accept single-token tool names (`buffer`)?
3. **Argument.Json:** ship with MCPlane conformance or stay product-local?
4. **Obsoletion timeline** for `InputNotation` NuGet IDs?

## References

- [GUIDERS-ADR-0016 InputNotation quarry](GUIDERS-ADR-0016-input-notation-quarry-family.md)
- [GUIDERS-ADR-0015 invocation mechanics](GUIDERS-ADR-0015-invocation-mechanics-slash-melody-binding.md)
- [GUIDERS-ADR-0018 slash conformance vectors](GUIDERS-ADR-0018-slash-conformance-vectors.md)
- `SlashLineResolver`, `InputNotationParser` — reference quarry seeds
