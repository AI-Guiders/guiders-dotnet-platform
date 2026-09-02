# GUIDERS-ADR-0057: `.cockpit.logic` — federation cockpit annunciation quarry

| | |
|---|---|
| **Status** | **Proposed** (signage + fixtures; parser not scheduled) |
| **Level** | Federation authoring hyperlane — sibling to `.catalog`, `.deck` |
| **Date** | 2026-09-02 |
| **Tags** | #guiders #federation #authoring #cockpit #annunciation #dark-cockpit #conformance |
| **Related** | [0048](./GUIDERS-ADR-0048-authoring-quarry-family.md) · [0007](./GUIDERS-ADR-0007-aviation-mental-model.md) · [0055](./GUIDERS-ADR-0055-surface-wpf-guild-deck-authoring.md) · [0056](./GUIDERS-ADR-0056-businesslogic-authoring-latent.md) (superseded naming) · [0053](./GUIDERS-ADR-0053-planet-responsibilities.md) |

## Context

Federation declarative stack:

```text
.catalog       → what commands exist & how they wire
.deck          → where attention zones live & layout
.dashspec      → planet report/dashboard meaning (sovereign)
```

Operators asked whether **cockpit behavior** — when lamps light, which mode, EICAS visibility, master caution — could live in text instead of manuals and scattered ViewModel code.

**Critical distinction** (KB: `agent-notes/knowledge/work/projects/aiguiders-open/guiders-federation/cockpit-logic-signage-v1.md`):

| Layer | Owner | Example |
|-------|-------|---------|
| **Cockpit operational logic** | **Federation** | Dark Cockpit, EICAS only on drift, git-gate annunciation |
| **Planet domain / customer rules** | **Planet** (optional sketch) | promote-to-view gates, URSA metric policy, SQL semantics |

[GUIDERS-ADR-0056](./GUIDERS-ADR-0056-businesslogic-authoring-latent.md) used the name `.businesslogic` — **misleading**. That quarry is **renamed** to `.cockpit.logic`. Planet “business logic” is **not** a federation grammar target.

**Promotion trigger (Proposed → Accepted):** parser v0 + one conformance runner green on `dark-cockpit.spec.json`.

## Decision

### 1. Name & scope — `.cockpit.logic`

**`.cockpit.logic`** — declare-time artifact for **cockpit annunciation, visibility, and operational gates** on federation fact contracts.

| In scope (federation) | Out of scope (planet) |
|-----------------------|------------------------|
| `facts` on Cockpit.DataBus ids | HTTP, SQL, ETL |
| `when` / `then fire alert` | customer workflow BPM |
| `alerting` severity class | Turing-complete scripts |
| `projectors table` (EICAS, master-caution, …) | domain-specific promote rules unless planet-local |
| `principles` (Dark Cockpit, …) | full “enterprise business rules” formalization |
| command `allow` / `deny` on **cockpit-level** gates | `Execute` bodies |

**Expressiveness ceiling:** guarded rules + `Authoring.Expression` subset (comparisons, `and`/`or`/`not`, literals). Deterministic, side-effect free, traceable (rule id + matched `when` for EICAS/debug).

### 2. Federation vs planet (normative)

| | Federation (formalize + conformance) | Planet (optional, sovereign) |
|--|--------------------------------------|------------------------------|
| **Question** | How does the **cockpit** behave? | What does the **domain** require? |
| **Artifacts** | `.catalog`, `.deck`, `.cockpit.logic`, principle grains | `.dashspec`, C#, local sketches |
| **Syntax** | Authoring.Core kit ([0048](./GUIDERS-ADR-0048-authoring-quarry-family.md) §3) | Planet choice |
| **CI** | parse · emit · conformance vectors | planet tests |
| **Example** | EICAS hidden when all alerts nominal | card bind expr, warm-view ETL |

Planet **may** ship a `<planet>.cockpit.logic` that `import`s federation principle grains and adds planet facts — but federation **does not** parse planet domain DSLs.

### 3. Stack placement

```text
Authoring.Cockpit.Logic     quarry — parse `.cockpit.logic` → CockpitRuleGraph IR
Authoring.Expression        shared expr grammar (with .deck conditions later)
Platform.Cockpit.Rules      headless evaluate IR + trace (no WPF)
Surface.*                   subscribe to outcomes (EicasStrip, zone visibility)
```

**Split from `.deck`:**

| File | Owns |
|------|------|
| `.deck` | zone ids, presets, topology, `eicas when alerts` (**projection hook**) |
| `.cockpit.logic` | **what** counts as alert, severity, **where** it projects |

### 4. Block syntax (Authoring parity)

Reuse [0048](./GUIDERS-ADR-0048-authoring-quarry-family.md) §3:

- `cockpit.logic <scope> … end cockpit.logic`
- `keyword … end keyword`
- `* table` for projectors / visibility
- `#` comments
- `import <grain/path>` for federation principle packs

### 5. Sections (v0 sketch)

```text
facts           # bool/string/scalar slots — federation fact id contract
derived         # named expr → bool (optional table)
rules           # when … then fire alert | allow | deny …
alerting        # alert id → class, message
projectors table
principles      # federation norms (Dark Cockpit) — may live in importable grains
end cockpit.logic
```

### 6. Federation principle — Dark Cockpit

Normative prose: [0007](./GUIDERS-ADR-0007-aviation-mental-model.md) — *EICAS quiet in norm; alerting is not the default UI.*

Formal sketch (grain + conformance, not parser output yet):

- Fixture: `tests/.../Fixtures/Authoring/federation-dark-cockpit.cockpit.logic.gdl`
- Conformance: `docs/conformance/authoring/cockpit/dark-cockpit.spec.json`

### 7. Worked example — planet cockpit file

`tests/.../Fixtures/Authoring/dashspec-studio.cockpit.logic.gdl` — imports Dark Cockpit grain, adds `need-commit` alert on git+verification facts. **Not parsed in v0** — normative example for codegen/conformance design.

### 8. Emit boundary (future)

```text
.cockpit.logic  →  Authoring.Cockpit.Logic parser  →  CockpitRuleGraph IR
                →  Surface.Wpf.CodeGen (or Platform.Rules emit)  →  *CockpitRules.g.cs
                →  DataBus subscription stubs
```

Planet hand-writes: domain views, SQL, `.dashspec` content — not annunciation wiring.

## Consequences

- `.businesslogic` name **deprecated** for federation ([0056](./GUIDERS-ADR-0056-businesslogic-authoring-latent.md) retained as historical sketch).
- Dark Cockpit and fleet principles become **testable** via conformance, not tribal knowledge.
- `.deck` `eicas when alerts` gains a formal upstream: RulesEngine alert stream.

## Non-goals (v0 signage)

- Parser / RulesEngine implementation
- Planet domain rule formalization
- Replacing `Platform.Cockpit.*` severity enums
- Merging cockpit IR into `.deck`

## Open questions

1. **Package name:** `Authoring.Cockpit.Logic` vs `Authoring.Cockpit`?
2. **Principles:** separate `.principle` files or `principles` block + `import` only?
3. **CLI:** `dotnet cockpit-logic emit` in guiders-assist vs authoring-toolchain host?
4. **LSP:** shared with `.deck` / `.catalog` authoring server?

## Reference fixtures

| Path | Role |
|------|------|
| `tests/.../federation-dark-cockpit.cockpit.logic.gdl` | Federation Dark Cockpit grain |
| `tests/.../dashspec-studio.cockpit.logic.gdl` | Planet example (`need-commit`) |
| `docs/conformance/authoring/cockpit/dark-cockpit.spec.json` | Conformance vectors |
