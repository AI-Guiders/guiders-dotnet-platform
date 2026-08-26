# GUIDERS-VISION-0002: Writing canon layers (guiders-style + project + personal canon)

| | |
|---|---|
| **Status** | **Vision · accepted** (discussion closed 2026-08-26) |
| **Date** | 2026-08-26 |
| **Authors** | operator + agent (Forge Control Center / Razor regression arc) |
| **Supersedes** | chat-only — **keep this file when context compacts** |
| **Normative ADR** | **[CDP-ADR-0207](https://github.com/AI-Guiders/cdp-mcp/blob/main/docs/adr/CDP-ADR-0207-writing-canon-layers.md)** (not KB) |
| **KB mirror** | [KB ADR 019](https://github.com/AI-Guiders/kb/blob/main/knowledge/adr/019-writing-canon-layers-and-guiders-style-v1.md) (index stub) |
| **Related** | KB [011](https://github.com/AI-Guiders/kb/blob/main/knowledge/adr/011-aiguiders-org-collaborative-kb-repo-v1.md), [012](https://github.com/AI-Guiders/kb/blob/main/knowledge/adr/012-multi-canon-workspace-resolution-v1.md) · FORGE-ADR-0052/0059 (ops → project canon, not ADR chain) |

---

## North star

Agents get a **short layered “how we write / how I work” stack** before first edit — routed by **CDP**, not by reading 50 ADRs.

## Two planes (do not merge)

| Plane | Layers |
|-------|--------|
| **code** | KB style → **guiders-style/{lang}** → **project `.cdp/canon.md`** → (scope leaf) |
| **operator** | **personal prefs** (primary canon — habitat, dialogue, git grants) |

Project wins on **code**. Personal wins on **operator workflow** — not on repo UI patterns.

## guiders-style (org, by language)

```text
guiders-style/
  core/
  csharp/   python/   powershell/   …
```

## Well-known project paths

`.cdp/project.toml` + `.cdp/canon.md` · optional `PROJECT-CANON.md` stub.

## Personal (operator)

`knowledge/personal/operator-writing-prefs.md` — **not** in product repo.

## CDP

`canon_stack` in route — separate from hot L0. Token budgets per layer.

## Dogfood P1

`agent-forge/.cdp/canon.md` · `guiders-style` stub · personal prefs skeleton.

## Non-goals

ADR chains for daily edits · personal prefs in git product repo · per-plugin canon files.
