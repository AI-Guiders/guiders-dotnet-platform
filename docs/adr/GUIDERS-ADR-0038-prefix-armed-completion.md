# GUIDERS-ADR-0038: Prefix-Armed Completion (PAC)

| | |
|---|---|
| **Status** | Accepted |
| **Date** | 2026-08-31 |
| **Tags** | #guiders #commandplane #slash #constructor #pac |
| **Relates to** | GUIDERS-ADR-0012 · GUIDERS-ADR-0035 · GUIDERS-ADR-0037 |

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
        └── ArmConstructor → SlashConstructorSession + pre-filled segments
```

Platform owns:

| Type | Role |
|------|------|
| `ISlashPrefixArmProfile` | Product lexer: partial + route → match |
| `SlashPrefixArmMatch` | Ready wire OR constructor root + segments |
| `SlashPrefixArmedCompletionCoordinator` | Session sync, arm, guidance |
| `SlashInputMode.TypedInput` | User is typing a value prefix (any profile) |

Platform does **not** own domain grammars (dates, paths, durations). Those are profiles.

### 2. Profile contract

```csharp
public interface ISlashPrefixArmProfile
{
    string ProfileId { get; }
    bool TryMatch(string partial, SlashRouteEntry route, out SlashPrefixArmMatch match);
}
```

`TryMatch` is pure — no session mutation. Coordinator applies match to session.

### 3. Integration

`SlashCompletionOptions.PrefixArmProfiles` — explicit list from product host.

`SlashCompletion.GetResult` delegates arg-tail handling to `SlashPrefixArmedCompletionCoordinator` when registry + session + profiles are present.

Constructor session rules (ADR-0037 §5) apply to all PAC profiles.

### 4. Built-in profiles

Platform ships **zero** mandatory domain profiles. Optional adapters live in namespaces (e.g. `Locale/`):

- `SlashLocaleDatePrefixArmProfile` — implements PAC for locale date/range (GUIDERS-ADR-0037).

Products may add: file path, duration, numeric range, enum prefix, etc.

### 5. Conformance

`slash-prefix-armed.spec.json` — profile-agnostic vectors with mock profile fixtures. Date vectors remain in `slash-value-constructor.spec.json`.

## Consequences

- `SlashLocaleTypedConstructorCoordinator` removed → logic in PAC + date profile.
- ADR-0037 reframed as locale **adapter** to PAC, not standalone coordinator.
- DashSpec registers `SlashLocaleDatePrefixArmProfile` in `SlashCompletionOptions`.
