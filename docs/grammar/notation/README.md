# String grammars (`grammar/*`)

Federation **wire string grammars** referenced from `.catalog` via `grammar.*` keys.  
Document structure (blocks, tables) lives in [../authoring/catalog.ebnf](../authoring/catalog.ebnf).

## Axes

| Axis | `.catalog` keys | Grammar ids | Package |
|------|-----------------|-------------|---------|
| Command path | `channels` → `grammar.command` | `command-slash`, `command-console` | `Notations.Command.*` |
| Argument tail | `channels` → `grammar.argument` | `argument-slash`, `argument-kv`, … | `Notations.Argument.*` |
| Keyboard wire | `defaults` → `grammar.keyboard.*` | `keyboard-key-gesture`, `keyboard-vim` | `Notations.Keyboard.*` |

Registry SSOT in code: `NotationGrammarRegistry` (`Authoring.Command.Catalog`).  
Conformance vectors: `tests/.../Fixtures/Notation/*.spec.json`.

## Files

| Grammar id | EBNF | Conformance |
|------------|------|-------------|
| `command-slash` | [command-slash.ebnf](command-slash.ebnf) | `notation/command-slash` |
| `command-console` | [command-console.ebnf](command-console.ebnf) | `notation/command-console` |
| `argument-kv` | [argument-kv.ebnf](argument-kv.ebnf) | `notation/argument-kv` |

Authoring validate uses the **same readers** as runtime (ADR-0047 rule #5).
