# PluginHost hyperlane — conformance checklist

Use when a planet opts into [GUIDERS-ADR-0008](../adr/GUIDERS-ADR-0008-plugin-host-hyperlane.md).

## Host (Forge-class)

- [ ] `host-runtime.manifest.json` at app base (`pluginhost.host-runtime/v1`)
- [ ] Bootstrap before module load (no bundle-order dependency)
- [ ] Collectible modules use `PluginModuleLoadContext` + shared policy
- [ ] CI: bundle order permutation test (strategic orders)
- [ ] Publish plugins/ staging matches manifest preload set

## Distribution (ANPM-class)

- [ ] `anpm.plugin.verify` (or equivalent) on package root
- [ ] `manifest.toml` + `lib/*.dll` layout
- [ ] Pin manifest includes `AIGuiders.PluginHost.*` when used

## Docs

- [ ] Product ADR links GUIDERS-ADR-0008
- [ ] No `AIGuiders.Platform.PluginHost` packages
