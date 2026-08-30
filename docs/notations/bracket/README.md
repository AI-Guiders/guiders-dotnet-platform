# Notations.Bracket migration checklist

Canonical plan: [GUIDERS-ADR-0026](../adr/GUIDERS-ADR-0026-notations-bracket-branch.md).

## Primitives

```text
KV       = Key + Sign + Value     (first Sign only)
List     = KV { ListSeparator KV } wrapped in Start/End terminals
```

| Surface | ListSeparator | KvSign |
|---------|---------------|--------|
| `[F:a; S:for:2]` | `;` | `:` |
| `doc=README op=scene` | space | `=` |

Inner `for:2` on axis `S:` = same KV with `Sign=':'` on the value substring.

## Two-pass (optional)

1. Bracket list → `BracketAxis` (envelope KV)
2. Value class → `command.path` | `notation.kv` | `line.range` | `bracket.nested`
