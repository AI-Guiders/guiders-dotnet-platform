# GUIDERS-ADR-0016: InputNotation quarry + package family

| | |
|---|---|
| **Status** | Accepted |
| **Date** | 2026-08-29 |
| **Tags** | #guiders #inputnotation #quarry #cide #melody #binding |
| **Related** | GUIDERS-ADR-0003 · GUIDERS-ADR-0015 · CIDE ADR-0060 · CIDE ChordNotation |

## Context

Keyboard wire appears in product configs in **different alphabets**:

| Wire | Example | Typical source |
|------|---------|----------------|
| Vim-doc | `<C-k> s p` | CIDE intent-catalog, vim help |
| KeyGesture | `Ctrl+K s p` | hotkeys.toml, WPF/VS |
| Emacs kbd | `C-x C-f`, `M-RET` | emacs keymaps (future) |

Parsers for Vim/Emacs notation are **older than most frameworks** — but they live **inside editors**, not as standalone .NET NuGet packages:

| SSOT | Location |
|------|----------|
| Vim/Neovim key-notation | Neovim `src/nvim/keycodes.c` (`replace_termcodes`, `find_special_key`); Lua API `vim.keycode()` |
| Emacs kbd | Emacs `key-parse` / `(kbd "…")` in `lisp/keymap.el` |
| KeyGesture (Ctrl+K) | de-facto IDE convention; JS libs (kilid, keymap) — **different token grammar** |

**Today:** monolithic `AIGuiders.Platform.InputNotation` with a small CIDE quarry (`VimChordNotationParser` + Eto.Parse grammar + `KeyGestureChordSyntax`). Covers platform needs; **not** full Neovim/Emacs coverage.

## Decision

### 1. InputNotation is a **sibling guild** (not nested under CommandPlane)

```text
InputNotation (Core)     ← IR + IInputNotationReader contract
    ├── .KeyGesture      ← Ctrl+K, Cmd+Shift+P
    ├── .Vim             ← CIDE quarry (Eto.Parse interim)
    ├── .Neovim          ← :help key-notation quarry (v1)
    ├── .Emacs           ← kbd / key-parse quarry (v1)
    ├── .Quarry          ← shared lexer, normalizer, spec conformance
    └── InputNotation.All ← optional meta-bundle
```

`CommandPlane.Melody` and `CommandPlane.Binding` **consume** InputNotation packages; they do not own wire parsers.

### 2. Quarry over reinvent

**Do not** grow bespoke PEG grammars to replicate 30 years of editor edge cases.

**Do** port or quote lexer behavior from canonical sources into C# adapters that emit platform IR:

| Package | Quarry SSOT | Platform output |
|---------|-------------|-----------------|
| `InputNotation.Vim` | Neovim `keycodes.c` semantics (subset by version table) | `NormalizedKeySequence` |
| `InputNotation.Emacs` | Emacs `key-parse` semantics (subset) | `NormalizedKeySequence` |
| `InputNotation.KeyGesture` | CIDE `KeyGestureChordSyntax` + VS/WPF conventions | `NormalizedKeySequence` |

**IR (Core):** `NormalizedKeySequence`, `NormalizedChordStep`, `NormalizedPlainKeyStep`, `ChordModifierKeys`, `ChordSemanticNormalizer`, `IInputNotationReader`.

**Melody note tokens** (`b`, `s`) are **not** a notation family — move `TryParseMelodyNoteStep` to `CommandPlane.Melody`.

### 3. Two axes (same pattern as Sources)

```text
WIRE FORMAT          IR (Core)              CONSUMERS
Json/Toml/Xml   →    SlashCommandDescriptor → Slash
Vim/KeyGesture  →    NormalizedKeySequence → Melody, Binding
```

Similar-looking wires (`<C-x> <C-f>` vs `<C-k> s p`) share bracket **lexicon** but differ in **special-key tables** and unbracketed forms (`C-x`) — separate quarry tracks, one IR.

### 4. Versioned subset tables

Each quarry package documents supported wire vs deferred:

| Tier | Vim quarry (example) | Deferred |
|------|----------------------|----------|
| **v1** (now) | `<C-M-S-…>`, plain tokens, CIDE chord+melody lines | mouse, `<C-Left>`, terminal codes |
| **v2** | named keys `RET`, `Space`, function keys | `<C-LeftMouse>`, script-id |
| **v3** | align with Neovim `vim.keycode` test vectors | full `K_SPECIAL` byte encoding |

Conformance = port tests from Neovim/Emacs upstream vectors where license permits; CIDE `ChordNotationSemanticTests` for cross-surface equivalence (Vim wire ≡ KeyGesture wire → same IR).

### 5. Eto.Parse

Interim only for v1 CIDE subset. **Target:** replace with quarry lexer from `keycodes.c` / `key-parse` — Eto.Parse is a parser toolkit, not vim SSOT.

## Package map (target)

> **Naming:** package IDs move to `Notations.Keyboard.*` per [GUIDERS-ADR-0021](GUIDERS-ADR-0021-notations-quarry-family.md). Table below is the interim `InputNotation` layout until alias wave.

| Package | Role |
|---------|------|
| `InputNotation` | Core IR + `IInputNotationReader` |
| `InputNotation.KeyGesture` | `KeyGestureChordSyntax`, `Ctrl+K` wire |
| `InputNotation.Quarry` | Shared quarry pattern: lexer, normalizer, spec JSON |
| `InputNotation.Vim` | Vim-doc wire; CIDE quarry (Eto.Parse interim) |
| `InputNotation.Neovim` | Neovim key-notation wire; quarry v1 + spec |
| `InputNotation.Emacs` | Emacs kbd wire; quarry v1 + spec |
| `InputNotation.All` | `InputNotationParser` facade (optional) |

## Embed

| Planet | Pin |
|--------|-----|
| CIDE | `.Vim` + `.KeyGesture` |
| Forge / DashSpec | `.KeyGesture` or none |
| Melody only | `.Vim` or `.KeyGesture` per catalog wire |
| Lazy | meta `InputNotation` |

## Non-goals

- Shipping Neovim/Emacs binaries or GPL code blobs in NuGet
- Matching every mapping corner case before a product needs it
- UI key matching / `KeyBinding` — stay in product (renderer, `KeyGestureChordMatching`)
- A **universal platform binding runtime** — native port per ecosystem; .NET packages are reference quarry only ([Constitution — Native ports](../GUIDERS-FEDERATION-CONSTITUTION.md#native-ports-not-platform-bindings))

## Consequences

- Split monolithic `InputNotation` like `CommandPlane.Sources` (separate wave)
- New notation = new sibling package implementing `IInputNotationReader`
- License review before large `keycodes.c` port (Vim/Neovim license)
- CIDE deletes duplicate `Services/ChordNotation/` after platform pins

## References

- [Neovim keycodes.c](https://github.com/neovim/neovim/blob/master/src/nvim/keycodes.c)
- [Neovim `vim.keycode`](https://neovim.io/doc/user/lua/#vim.keycode())
- [Emacs `key-parse`](https://github.com/emacs-mirror/emacs/blob/master/lisp/keymap.el)
- [vimlrs keycodes port](https://docs.rs/vimlrs/latest/vimlrs/ported/keycodes/index.html) (Rust reference for C# port scope)
