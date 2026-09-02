# GUIDERS-ADR-0056: `.businesslogic` — latent authoring quarry (policy & derivation)

| | |
|---|---|
| **Status** | **Superseded (naming)** — see [0057](./GUIDERS-ADR-0057-cockpit-logic-authoring-quarry.md). Retained as historical sketch. Federation quarry is **`.cockpit.logic`**, not `.businesslogic`. |
| **Level** | Federation authoring hyperlane — sibling to `.catalog`, `.deck` |
| **Date** | 2026-09-01 |
| **Tags** | #guiders #federation #authoring #rules #policy #latent #guiders-ioplang |
| **Related** | [0048](./GUIDERS-ADR-0048-authoring-quarry-family.md) · [0055](./GUIDERS-ADR-0055-surface-wpf-guild-deck-authoring.md) · [0053](./GUIDERS-ADR-0053-planet-responsibilities.md) · [0057](./GUIDERS-ADR-0057-cockpit-logic-authoring-quarry.md) · IOP / Cockpit.DataBus · **Gherkin** (BDD; adjacent, not compatible v0) |

## Context

The federation declarative stack is growing by accretion, not by design committee:

```text
.catalog      → what commands exist & how they wire
.deck         → where attention zones live & layout stars
.dashspec     → what the dashboard/report shows
.chrome (TBD) → cockpit chrome tokens
```

Operators joked about **guiders-ioplang** — a family of SSOT files that *feels* like a language but is really **intent layers**: each file answers one question. The punchline landed on **`.businesslogic`**: *«бизнес правила можно пихать в .businesslogic»*.

> **2026-09-02:** Renamed federation quarry to **`.cockpit.logic`** ([0057](./GUIDERS-ADR-0057-cockpit-logic-authoring-quarry.md)). Cockpit annunciation ≠ planet customer domain. Planet “business logic” stays optional/s sovereign; federation formalizes cockpit ops + conformance only.

That is worth capturing. Not because v0 Studio needs it, but because the **pain is real**:

- command enable/disable scattered in ViewModels;
- «can promote REPL → view?» lives in developer heads, not SSOT;
- prod vs test behavior differs with no auditable rule surface.

**Promotion trigger (when Latent → Proposed):** the same policy appears **three times** in planet C# (or two planets need the same gate). Until then: C# + tests is fine.

## Decision (sketch only)

### 1. Name & scope

**`.businesslogic`** — declare-time artifact for **policy, visibility, validation gates, and derived booleans**. Not a general-purpose language.

| In scope | Out of scope (planet C#) |
|----------|--------------------------|
| `when` / `then` on DataBus facts | HTTP, SQL, file I/O |
| command `allow` / `deny` / `require confirmation` | loops, retry, backoff |
| zone/command `visibility` matrices | complex algorithms |
| simple `derived` expressions | side effects (`Execute` bodies stay in `.catalog` + C#) |
| workflow gates (IOP-lite) | Turing-complete scripts |

**Expressiveness ceiling:** guarded rules + expression language (comparisons, `and`/`or`/`not`, literals). No user-defined functions in v0 sketch.

### 2. Stack placement

```text
Authoring.BusinessLogic     quarry — parse `.businesslogic` → RuleGraph IR
Authoring.Expression        federation expr grammar → neutral Expr IR (shared quarry)
Platform.Rules (TBD)        headless evaluate Expr IR + trace (testable, no WPF)
Surface.*                   subscribe to outcomes (enable flags, tooltips, deny reasons)
```

**Do not** anchor expression syntax on `.dashspec` bind expr. Per [0048](./GUIDERS-ADR-0048-authoring-quarry-family.md) §5, `.dashspec` is **planet sovereign** (DashSpec repo). `.businesslogic` is **federation authoring** — same guild tier as `.catalog` and `.deck`. A federation quarry must not depend on a planet product grammar.

**Sketch decision:** introduce **`Authoring.Expression`** — small shared expr language (literals, comparisons, `and`/`or`/`not`, fact refs). Consumers: `.businesslogic` `when`/`derived`, later `.deck` conditions (`eicas when …`), optional catalog preconditions. **Not** `Notations.Expression` for v0 unless we later need a **runtime wire-in** alphabet (human types expr in a REPL line); declare-time rules parse at build and evaluate from IR.

If DashSpec bind expr converges visually, that is **DashSpec adopting federation subset** (or mapping in planet adapter) — not federation importing DashSpec.

### 2.1 Expression language (federation quarry)

| Layer | Package (draft) | Role |
|-------|-----------------|------|
| Grammar | `Authoring.Expression` | parse `when repl.has-result and …` → `ExprNode` IR |
| Evaluate | `Platform.Rules` or `Authoring.Expression.Eval` | walk IR against fact snapshot |
| Conformance | `Authoring.Conformance` | expr vectors shared across `.businesslogic` consumers |

v0 surface (sketch): `bool|string|number` literals; `==` `!=` `<` `>`; `and` `or` `not`; identifiers = declared `facts`; no calls, no indexing, no lambdas.

Same split as [0048](./GUIDERS-ADR-0048-authoring-quarry-family.md):

```text
declare (Authoring)  →  IR  →  emit / evaluate
```

Emit targets (future): `*Rules.g.cs` partials, DataBus subscription stubs, conformance vectors — **not** replacement of command executors.

### 3. Block syntax (DashSpec parity)

Reuse [0048](./GUIDERS-ADR-0048-authoring-quarry-family.md) §3 conventions:

- `keyword … end keyword`
- `* table` matrices where row-oriented is clearer
- `#` line comments
- `import <grain/…>` for shared fact packs (future)

### 3.1 Gherkin-adjacent (conscious choice)

The `when` / `then` rule blocks read like **Gherkin** (Cucumber / SpecFlow) — intentionally. Same intent layer: declare behavior/policy, not implementation.

| Gherkin | `.businesslogic` (sketch) |
|---------|---------------------------|
| Feature / Scenario | `rule … end rule` |
| Background / context | `facts`, `defaults` |
| Given / When | `when …` (expressions on DataBus facts) |
| Then | `then allow` / `deny` / `require …` on catalog/deck targets |
| Scenario Outline | `visibility table`, `derived table` |

**Not v0:** Cucumber-compatible syntax, natural-language step defs, or E2E scenario files. This quarry targets **command-plane policy** at authoring time (`.catalog` + `.deck` refs), not browser-level BDD. Conformance vectors may *look* like scenarios; emit goes to RulesEngine / `CanExecute`, not SpecFlow bindings.

Avoid narrative step theatre («When the operator feels lucky») — keep rules auditable and machine-evaluable.

### 4. Sketch grammar — what it could look like

**Minimal planet header:**

```text
businesslogic dashspec-studio
```

**Facts** — named boolean/scalar slots bound to Cockpit.DataBus (declared, not fetched here):

```text
facts
  repl.has-result      bool
  repl.schema-stable   bool
  spec.card-selected   bool
  env.name             string
end facts
```

**Rules** — guarded policy blocks:

```text
rules

  rule promote-when-ready
    when repl.has-result and repl.schema-stable
    then allow command promote-to-view
  end rule

  rule promote-block-no-result
    when not repl.has-result
    then deny command promote-to-view
         reason "Run the query first"
  end rule

  rule prod-confirm
    when env.name == "prod"
    then require command promote-to-view
         confirm "Promote query to view in production?"
  end rule

end rules
```

**Visibility table** — cross-ref `.deck` zones and `.catalog` commands:

```text
visibility table
  | target                  | when                                      |
  | zone data-lab           | preset is report-author or data-probe     |
  | command promote-to-view | repl.has-result and repl.schema-stable    |
  | command host.show       | always                                    |
end visibility
```

**Derived table** — named booleans for UI/bindings (expr, not procedures):

```text
derived table
  | name        | type | expr                                           |
  | can-promote | bool | repl.has-result and repl.schema-stable         |
  | is-prod     | bool | env.name == "prod"                             |
end derived
```

**Validation gates** (Data Lab / forms):

```text
gates

  gate repl-before-promote
    on command promote-to-view
    require repl.schema-stable
    else reason "Schema must be stable (no pending edits)"
  end gate

end gates
```

**Close document:**

```text
end businesslogic
```

Normative fixture: `tests/AIGuiders.Platform.Authoring.Tests/Fixtures/Authoring/dashspec-studio.businesslogic`.

### 5. Worked example (Data Lab promote)

See fixture — ties together:

- `.catalog` command id `promote-to-view` (hypothetical v1)
- `.deck` zones `data-lab`, preset `report-author`
- `.businesslogic` gate before promote is allowed

Runtime shape (target, not implemented):

```text
DataBus publishes facts → RulesEngine evaluates → command plane gets Allow/Deny + reason string
```

### 6. IR spine (draft types)

| IR type | Role |
|---------|------|
| `BusinessLogicDocument` | planet id, facts[], rules[], gates[], tables |
| `FactDeclaration` | name, scalar kind, optional DataBus key map |
| `PolicyRule` | when-expr, then-actions[] |
| `ThenAction` | Allow/Deny/RequireConfirm + target (command/zone) |
| `VisibilityRow` | target ref, when-expr |
| `DerivedRow` | name, type, expr |
| `ValidationGate` | trigger (command/event), require-expr, else-reason |

**Evaluation:** deterministic, side-effect free, traceable (rule id + matched when-clause for EICAS/debug).

### 7. guiders-ioplang (glossary)

Informal name for the **Guiders Declarative Stack** — family of authoring notations under `Authoring.*`, not a Turing-complete language. Official escape hatch remains C# `Execute` and planet services.

| Notation | Question it answers |
|----------|---------------------|
| `.catalog` | What can the operator invoke? |
| `.deck` | Where does attention go? |
| `.dashspec` | What does the report show? |
| `.businesslogic` | Under what conditions is it allowed / visible / valid? |
| `.chrome` (TBD) | How does chrome look? |

## Consequences (if promoted)

- Policy becomes diffable SSOT — same codegen/conformance story as `.catalog`.
- Studio/DBA share rule patterns via `import <grain/…>` fact packs.
- ViewModels shrink to projection; fewer «magic» `CanExecute` branches.

## Non-goals (this ADR)

- Parser, emitter, or `Platform.Rules` package implementation.
- Replacing IOP process engine — only **declarative gates** at UI/command boundary.
- SQL or report calculation DSL (stay in `.dashspec` / transforms / C#).
- Nested functions, recursion, arbitrary collections in expression language.

## Open questions

1. **Fact binding:** explicit `bind repl.has-result = databus.repl.result.ready` vs codegen from naming convention?
2. **`deny reason` i18n:** helps table parallel (like `.catalog`) vs inline string only?
3. **Conformance:** `docs/conformance/authoring/businesslogic/*` vectors — same kit as catalog?
4. **Relation to CommandPlane:** emit into catalog profile vs runtime RulesEngine hook?
5. **`Authoring.Expression` vs eval split:** single package vs `Authoring.Expression` + `Platform.Rules.Eval` — package boundary only when implementation starts.

## Reference

| Artifact | Path |
|----------|------|
| Sketch fixture | `tests/.../Fixtures/Authoring/dashspec-studio.businesslogic` |
| KB capture | `AI-Guiders-kb/.../guiders-declarative-stack/latent-businesslogic-v0.md` |
