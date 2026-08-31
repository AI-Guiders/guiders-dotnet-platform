# GUIDERS-ADR-0038: Prefix-Armed Completion (PAC)

| | |
|---|---|
| **Status** | Accepted |
| **Date** | 2026-08-31 |
| **Tags** | #guiders #commandplane #slash #constructor #pac #notations #guild |
| **Relates to** | GUIDERS-ADR-0012 · GUIDERS-ADR-0015 · GUIDERS-ADR-0021 · GUIDERS-ADR-0035 · GUIDERS-ADR-0037 |

## Context

Mixed-mode slash input needs: **Tab** completes path segments; **typing** continues the arg value. When the typed prefix is unambiguous, the platform should **arm** a value constructor or reach **Ready** without forcing picker selection.

[GUIDERS-ADR-0037](GUIDERS-ADR-0037-slash-locale-typed-value-input.md) introduced locale date parsing — but PAC is **not a date feature**. Date/locale is one **product profile** atop generic platform mechanics.

## Decision

### 1. PAC — platform-owned, profile-driven

```text
Arg tail partial
    → ISlashPrefixArmProfile.TryMatch (0..N profiles, product-registered)
        ├── NoMatch      → picker / free text / other completion
        ├── Ready        → wire complete (Enter executes)
        └── ArmConstructor → ArgConstructorSession + pre-filled segments
```

Platform owns:

| Type | Role |
|------|------|
| `IPrefixArmProfile` | Product lexer: partial + `PrefixArmSite` → match |
| `PrefixArmMatch` | Ready wire OR constructor root + segments |
| `PrefixArmCoordinator` | Session sync, arm, neutral result |
| `PrefixArmSite` | Surface-neutral arg site (constructors, hints) |
| `ArgInputMode.TypedInput` | User is typing a value prefix (slash projector) |

Platform does **not** own domain grammars (dates, paths, durations). Those are profiles (`CommandPlane.PrefixArmed.Locale` for dates).

### 2. Profile contract

```csharp
public interface IPrefixArmProfile
{
    string ProfileId { get; }
    bool TryMatch(string partial, PrefixArmSite site, out PrefixArmMatch match);
}
```

`TryMatch` is pure — no session mutation. Coordinator applies match to session.

### 3. Integration

`SlashCompletionOptions.PrefixArmProfiles` — explicit list from product host.

`PrefixArmCoordinator` runs on `PrefixArmSite` (surface-neutral). `CommandPlane.Slash` maps `CatalogRouteEntry` → site and `PrefixArmResult` → `SlashCompletionResult`.

Constructor session rules (ADR-0037 §5) apply to all PAC profiles.

### 4. Built-in profiles

Platform ships **zero** mandatory domain profiles. Optional adapters live in namespaces (e.g. `Locale/`):

- `LocaleDatePrefixArmProfile` — in `CommandPlane.PrefixArmed.Locale` (GUIDERS-ADR-0037).

Products may add: file path, duration, numeric range, enum prefix, etc.

### 5. Conformance

`slash-prefix-armed.spec.json` — profile-agnostic vectors with mock profile fixtures. Date vectors remain in `slash-value-constructor.spec.json`.

### 6. Cross-surface guild boundary

PAC is a **CommandPlane mechanic**, not a Slash UI feature. Same rule as [GUIDERS-ADR-0021](GUIDERS-ADR-0021-notations-quarry-family.md): **Notations** parse wire → IR; **mechanics** resolve and complete; **planets** render chrome.

```text
Notations.Command.*  →  path + arg tail IR
        │
        ▼
CommandPlane (catalog, constructors, PAC coordinator)   ← surface-agnostic
        │
        ├── CCL / DashSpec   → SlashInputGuidance, breadcrumb, suggestion table
        ├── Console planet   → readline ghost text, status line, or silent Ready
        ├── MCP / agent      → ReadyWire in envelope; no inline hints required
        └── Forge / other    → port profiles + coordinator; own hint UX
```

| Layer | SSOT | Console planet may differ |
|-------|------|---------------------------|
| `IPrefixArmProfile.TryMatch` | `CommandPlane.PrefixArmed` | same profiles |
| `PrefixArmCoordinator` | `CommandPlane.PrefixArmed` | same arm/ready logic |
| `ArgConstructorSession` | `CommandPlane.Constructors` | same when constructors armed |
| Hints / placeholders / inline UI | **Surface** | harder readline UX — not a mechanic change |

**Console parity:** `Notations.Command.Console` already splits path + kv tail (`ConsoleCommandNotation`). After resolve, PAC runs on the **arg tail partial** the same way as slash CCL — the console host registers the same `PrefixArmProfiles` and calls the coordinator. Whether the user *sees* `TypedInput` hints is entirely the console planet's problem.

**Packages (shipped):** `CommandPlane.Constructors`, `CommandPlane.PrefixArmed`, `CommandPlane.PrefixArmed.Locale`. Console: `CommandPlane` + `PrefixArmed` + profiles — no `CommandPlane.Slash` required.

## Consequences

- `SlashLocaleTypedConstructorCoordinator` removed → logic in PAC + date profile.
- ADR-0037 reframed as locale **adapter** to PAC, not standalone coordinator.
- DashSpec registers `LocaleDatePrefixArmProfile` in `SlashCompletionOptions`.
- Console and other planets adopt PAC via shared profiles + coordinator; hint rendering is local.
