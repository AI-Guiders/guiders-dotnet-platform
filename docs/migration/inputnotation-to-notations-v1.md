# InputNotation → Notations.Keyboard migration (v1)

**Status:** obsoletion shipped (GUIDERS-ADR-0021 §7, §12)  
**Sunset:** NuGet package IDs remain published; removal date **TBD** after CIDE/Forge pin `Notations.*`.

## Summary

`AIGuiders.Platform.InputNotation.*` is the legacy keyboard-notation guild name. Federation SSOT now lives under **`AIGuiders.Platform.Notations.Keyboard.*`**. Legacy packages type-forward or meta-reference the Notations equivalents and emit compile-time `[Obsolete]` warnings.

## Package mapping

| Legacy (`InputNotation.*`) | Replacement (`Notations.Keyboard.*`) | Notes |
|----------------------------|--------------------------------------|-------|
| `AIGuiders.Platform.InputNotation` | `AIGuiders.Platform.Notations.Keyboard` | Core IR + `IKeyboardNotationReader` |
| `AIGuiders.Platform.InputNotation.Quarry` | `AIGuiders.Platform.Notations.Keyboard.Quarry` | Shared lexer / normalizer quarry |
| `AIGuiders.Platform.InputNotation.Vim` | `AIGuiders.Platform.Notations.Keyboard.Vim` | Vim-doc wire |
| `AIGuiders.Platform.InputNotation.Neovim` | `AIGuiders.Platform.Notations.Keyboard.Neovim` | Neovim key-notation wire |
| `AIGuiders.Platform.InputNotation.Emacs` | `AIGuiders.Platform.Notations.Keyboard.Emacs` | Emacs `key-parse` wire |
| `AIGuiders.Platform.InputNotation.KeyGesture` | `AIGuiders.Platform.Notations.Keyboard.KeyGesture` | `Ctrl+K` / hotkeys.toml wire |
| `AIGuiders.Platform.InputNotation.All` | `AIGuiders.Platform.Notations.Keyboard.All` | Optional meta-bundle |

## Namespace mapping (core types)

| Legacy type | Replacement |
|-------------|-------------|
| `AIGuiders.Platform.InputNotation.IInputNotationReader` | `AIGuiders.Platform.Notations.Keyboard.IKeyboardNotationReader` |
| `AIGuiders.Platform.InputNotation.NormalizedKeySequence` | `AIGuiders.Platform.Modeling.Notations.Keyboard.NormalizedKeySequence` |
| `AIGuiders.Platform.InputNotation.ChordModifierKeys` | `AIGuiders.Platform.Modeling.Notations.Keyboard.ChordModifierKeys` |
| `AIGuiders.Platform.InputNotation.ChordSemanticNormalizer` | `AIGuiders.Platform.Notations.Keyboard.ChordSemanticNormalizer` |

## Migration steps

1. Update `.csproj` / `PackageReference` from `InputNotation.*` to the matching `Notations.Keyboard.*` row above.
2. Replace `using AIGuiders.Platform.InputNotation` with `AIGuiders.Platform.Notations.Keyboard` and `AIGuiders.Platform.Modeling.Notations.Keyboard` for IR types.
3. Rebuild; address `[Obsolete]` warnings on any remaining legacy type references.
4. Run conformance vectors under `docs/conformance/notation/*` if you port parsers to another stack.

## Related ADRs

- [GUIDERS-ADR-0021](../adr/GUIDERS-ADR-0021-notations-quarry-family.md) — Notations quarry family (v1 Final)
- [GUIDERS-ADR-0016](../adr/GUIDERS-ADR-0016-input-notation-quarry-family.md) — keyboard quarry semantics (package naming superseded by 0021)
