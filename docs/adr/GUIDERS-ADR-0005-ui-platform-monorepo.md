# GUIDERS-ADR-0005: UI Platform monorepo (AIGuiders.UI*)

**Status:** accepted (2026-08-25)  
**Tags:** #guiders #ui #human #agent #nuget #open  
**Related:** GUIDERS-ADR-0001 · GUIDERS-ADR-0003 · GUIDERS-ADR-0004 · FORGE-ADR-0049 · FORGE-ADR-0059

---

## Context

AI Guiders products share human+agent UX principles (keyboard surfaces, grouping, human-primary flows). UI markup lived in product repos (Forge `Plugin.View`). GUIDERS-ADR-0001/0003 keep **guiders-platform** headless.

## Decision

1. **Third sibling monorepo:** `guiders-ui-platform` → **`AIGuiders.UI.*`** on nuget.org.
2. **Layers:** `UI.Core` (contracts) · `UI.Tokens` (CSS) · `UI.Web.HTMX` (first adapter); future `Web.Blazor`, `React`, …
3. **Rule:** semantics in Core; markup in adapters; products wire domain routes only.
4. **Forge** is reference consumer — not cross-product UI SSOT.

## Non-goals

- Monolith `AIGuiders.Platform.Web.UI`
- SPA-by-default
- Avalonia/WPF control libraries in v1

## Consequences

- GUIDERS-ADR-0003 «UI / host → products own» amended: **shared human kit** → ui-platform; host wiring stays in products.
- v1 slice: PageChrome, EmptyStates, Tokens — see `guiders-ui-platform` GUIDERS-UI-0002.
- Dual accessibility (human a11y + Agent AX): GUIDERS-UI-0003 in `guiders-ui-platform`.
- Federation framing (sovereign repos, non-annexation): GUIDERS-ADR-0006.
