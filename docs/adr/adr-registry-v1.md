# ADR registry v1 — implementation audit

**Lifecycle:** [status-lifecycle.md](./status-lifecycle.md)  
**Generated:** 2026-09-02 (federation cluster audited; legacy bulk = header SSOT until row-by-row pass)

Columns:

- **Decision** — first tag from ADR header
- **Implementation** — second tag per Cascade convention (`Implemented` / `In progress` / `—`)

## Federation cluster (audited)

| ADR | Decision | Implementation | Evidence / notes |
|-----|----------|----------------|------------------|
| [0062](./GUIDERS-ADR-0062-ide-solution-session-orchestrator.md) | Accepted | In progress | Phase 1b–2 shipped; ports open |
| [0061](./GUIDERS-ADR-0061-language-resolver-center.md) | Accepted | In progress | CDP-0208 host |
| [0063](./GUIDERS-ADR-0063-anchors-federation-reincarnation.md) | Accepted | — | Phase 0 architecture |
| [0060](./GUIDERS-ADR-0060-platform-execution-phase-d-cockpit.md) | Accepted | Implemented | Cockpit rename + DataBus |
| [0059](./GUIDERS-ADR-0059-gdl-hyperlane.md) | Accepted | In progress | Signage + partial quarries |
| [0025](./GUIDERS-ADR-0025-language-intelligence-boundary.md) | Accepted | In progress | Phase 0 migration |
| [0027](./GUIDERS-ADR-0027-mdlinker-doc-anchor-check.md) | Accepted | — | Correspondence pilot pending |
| [0028](./GUIDERS-ADR-0028-documentation-guild-correspondence-family.md) | Accepted | — | GGL horizon |

**F# mirror:** [guiders-fsharp ADR index](https://github.com/AI-Guiders/guiders-fsharp/blob/main/docs/adr/README.md)  
**CDP:** [CDP-ADR-0208](https://github.com/AI-Guiders/cdp-mcp/blob/main/docs/adr/CDP-ADR-0208-language-resolver-center-cdp-host.md) · In progress

## Full index (0001–0063)

| # | Slug | Decision (header) | Implementation |
|---|------|---------------------|--------------|
| 0001 | platform-boundary | ? | audit pending |
| 0002 | avalonia-quarry-gap | ? | audit pending |
| 0003 | platform-ssot-quarry | ? | audit pending |
| 0004 | core-monorepo | ? | audit pending |
| 0005 | ui-platform-monorepo | ? | audit pending |
| 0006 | confederation-charter | ? | audit pending |
| 0007 | aviation-mental-model | ? | audit pending |
| 0008 | plugin-host-hyperlane | Accepted | audit pending |
| 0009 | command-surface-pattern | Accepted | audit pending |
| 0010 | platform-mechanics | Accepted (phased) | In progress |
| 0011 | slash-step-completion | Accepted | audit pending |
| 0012 | arg-picker-completion | Accepted | audit pending |
| 0013 | command-catalog-sources | Accepted | audit pending |
| 0014 | registry-catalog-visitor | Accepted | audit pending |
| 0015 | invocation-mechanics-slash-melody-binding | Accepted | audit pending |
| 0016 | input-notation-quarry-family | Accepted | audit pending |
| 0017 | binding-catalog-family | Accepted | audit pending |
| 0018 | slash-conformance-vectors | Accepted | audit pending |
| 0019 | conformance-hyperlane-monorepo | Accepted | audit pending |
| 0020 | mcplane-agent-ingress | Draft | — |
| 0021 | notations-quarry-family | Accepted | Implemented (v0 per ADR §12) |
| 0022 | utilities-adoption-report | Draft | partial in-repo |
| 0023 | case-workbench-heritage | ? | audit pending |
| 0024 | visual-command-tree-capture-stack | Accepted | partial (headless) |
| 0025 | language-intelligence-boundary | Accepted | In progress (Phase 0) |
| 0026 | notations-bracket-branch | Accepted | Phase 0 stub |
| 0027 | mdlinker-doc-anchor-check | Accepted | — |
| 0028 | documentation-guild-correspondence-family | Accepted | — |
| 0029 | platform-sources-lift | Accepted | audit pending |
| 0030 | combinations-family | Accepted | audit pending |
| 0031 | policy-as-readable-code-overlay-profiles | Accepted | audit pending |
| 0032 | conformance-obligations-policy-specs | Accepted | audit pending |
| 0033 | navigation-family-semantic-scenes | Accepted | audit pending |
| 0034 | csx-lift-navigation-config-xml-anchors | Accepted | audit pending |
| 0035 | slash-value-constructors | Accepted | audit pending |
| 0036 | invocation-engage-glossary | Accepted | audit pending |
| 0037 | slash-locale-typed-value-input | Accepted | audit pending |
| 0038 | prefix-armed-completion | Accepted | audit pending |
| 0039 | command-catalog-family | Accepted | audit pending |
| 0040 | catalog-guild-arg-suggestions | Accepted | audit pending |
| 0041 | catalog-kernel-profiles | Accepted | audit pending |
| 0042 | intermediate-representation-family | Accepted | audit pending |
| 0043 | invocation-line-phase | Accepted | audit pending |
| 0044 | command-catalog-scope | Accepted | audit pending |
| 0045 | command-authoring-dx | Accepted | audit pending |
| 0046 | catalog-path-completion | Accepted | audit pending |
| 0047 | command-for-doi | Accepted | In progress (2026-09-01 wave) |
| 0048 | authoring-quarry-family | Accepted | In progress (2026-09-01 wave) |
| 0049 | federation-pattern-library | Accepted | audit pending |
| 0050 | paths-guild-logical-physical | Accepted | audit pending |
| 0051 | authoring-project-abstraction | Accepted | partial v0 slice |
| 0052 | unified-import-directive | Accepted | partial v0 parser |
| 0053 | planet-responsibilities | Accepted | audit pending |
| 0054 | phrase-slot-completion | Accepted | audit pending |
| 0055 | surface-wpf-guild-deck-authoring | Proposed | — |
| 0056 | businesslogic-authoring-latent | Superseded | — |
| 0057 | cockpit-logic-authoring-quarry | Proposed | signage only |
| 0058 | presentation-topology-ir | Accepted | partial v0 IR |
| 0059 | gdl-hyperlane | Accepted | In progress |
| 0060 | platform-execution-phase-d-cockpit | Accepted | Implemented |
| 0061 | language-resolver-center | Accepted | In progress |
| 0062 | ide-solution-session-orchestrator | Accepted | In progress |
| 0063 | anchors-federation-reincarnation | Accepted | — (Phase 0) |

## Next audit waves

1. **Slash / command catalog** (0011–0019, 0039–0046) — map to `Platform.Execution.Slash.*` packages
2. **Authoring quarries** (0047–0052, 0057–0058) — cross-check Modeling extraction matrix
3. **Navigation / Correspondence** (0027–0028, 0033–0034) — GGL pilot

When a row is verified, update: ADR header → this table → [README.md](./README.md) federation row if applicable.
