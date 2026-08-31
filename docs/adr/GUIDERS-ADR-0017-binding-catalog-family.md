# GUIDERS-ADR-0017: Binding catalog family

| | |
|---|---|
| **Status** | Accepted |
| **Date** | 2026-08-29 |
| **Tags** | #guiders #commandplane #binding #hotkeys #federation |
| **Related** | GUIDERS-ADR-0015 · GUIDERS-ADR-0013 · GUIDERS-ADR-0016 · GUIDERS-ADR-0036 · CIDE `hotkeys.toml` |

## Context

**Binding** maps a **gesture wire** (`Ctrl+Q`) to a **target** (`commandId`, chord root, surface opener). CIDE ships `Hotkeys/hotkeys.toml` as a flat `binding_key = "gesture"` map with user overlay merge.

Platform must own **headless catalog mechanics** (load, merge, index, gesture normalize) — not UI key listeners ([Constitution — Native ports](../GUIDERS-FEDERATION-CONSTITUTION.md#native-ports-not-platform-bindings)).

## Decision

### 1. Binding is a **sibling guild** (like Sources, InputNotation)

```text
CommandPlane.Binding (Core)     ← BindingDescriptor, IBindingSource, BindingCatalogIndex
    ├── .Sources.Toml           ← hotkeys.toml flat map (CIDE quarry)
    ├── .Sources.Json           ← { "bindings": { … } } or flat object
    ├── .Sources.File           ← extension dispatch
    ├── .Sources.Database       ← delegate loader
    └── .Sources                ← meta-bundle
```

**Consumes:** `InputNotation.KeyGesture` for wire → `NormalizedKeySequence` (single-step chord gestures).

**Does not own:** `KeyDown` tunnel, WPF `KeyBinding`, palette focus — planet native.

### 2. Layered merge

Later `IBindingSource` wins per `BindingKey` (case-insensitive) — ship + user overlay (CIDE `%LocalAppData%\CascadeIDE\hotkeys.toml`).

### 3. Well-known keys

| Key | `BindingTargetKind` |
|-----|---------------------|
| `cascade_chord` | `ChordRoot` (gesture **InvocationEngage** — [ADR-0036](GUIDERS-ADR-0036-invocation-engage-glossary.md)) |
| other flat keys | `Command` (`CommandId` = key) |

### 4. Two axes (same pattern as Sources)

```text
FORMAT              TRANSPORT           IR (Core)
Toml / Json    →    File / Database  →  BindingDescriptor + NormalizedKeySequence
```

## Package map

| Package | Role |
|---------|------|
| `CommandPlane.Binding` | Core catalog + merge + `BindingGestureNormalizer` |
| `CommandPlane.Binding.Sources.Toml` | CIDE `hotkeys.toml` quarry |
| `CommandPlane.Binding.Sources.Json` | JSON flat / `bindings` object |
| `CommandPlane.Binding.Sources.File` | `FromFile` + extension dispatch |
| `CommandPlane.Binding.Sources.Database` | delegate → Core |
| `CommandPlane.Binding.Sources` | meta `BindingSources.*` |

## Non-goals

- Universal platform binding runtime
- Default hotkey tables (planetary config)
- VS Code `keybindings.json` array form (defer; native port may map to flat JSON)

## Consequences

- CIDE `HotkeyTomlLoader` / `HotkeyGestureMap` → pin `Binding.Sources.Toml` + overlay compose
- Forge web shortcuts → JSON port or native TS catalog using same schema
- Integration reviews: Binding = catalog; InputNotation = parse wire; planet = match keys

## References

- [GUIDERS-ADR-0015](GUIDERS-ADR-0015-invocation-mechanics-slash-melody-binding.md)
- CIDE [ADR 0030](https://github.com/AI-Guiders/cascade-ide/blob/develop/docs/adr/0030-command-ids-hotkeys-and-ui-registry-layers.md)
- CIDE `Hotkeys/hotkeys.toml`, `HotkeyTomlLoader.cs`
