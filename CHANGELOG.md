# Changelog

All notable changes to this project are documented here. This project adheres to
[Keep a Changelog](https://keepachangelog.com) and [Semantic Versioning](https://semver.org).

## [0.2.0] - 2026-09-03

### Added

- **`MochoIndieStudio.Signals.Authoring`** assembly (new, references UnityEngine):
  - `SignalIdAttribute` — `[SignalId]` on a `string` field draws it with a searchable picker of
    project-known ids. The field stays a plain string, so adopting it needs no asset migration.
  - `SignalIdProviderAttribute` — `[SignalIdProvider]` on a static class exposes its
    `public const string` fields to the picker (zero-authoring id discovery).
  - `SignalCatalog` — optional `ScriptableObject` listing `{ Id, Description }` entries
    (`Create ▸ MIS Signals ▸ Signal Catalog`); a project may hold several, all merged.
- **`MochoIndieStudio.Signals.Editor`** assembly (new): `SignalIdDrawer` (the picker) and
  `SignalIdRegistry` (merges provider consts + catalog entries, deduped, rebuilt on domain reload
  and asset import).
- `docs/signals-v0.2-design.md` — the design, and the reasoning for holding the two deferred ideas
  (a shared `SignalSpec` struct, typed payloads) until a concrete need appears.

### Notes

- The runtime core (`MisSignals`, `ISignalListener`, `noEngineReferences: true`) is unchanged — no
  behaviour change, purely additive. A consumer that does not reference `…Authoring` is unaffected.

## [0.1.0] - 2026-09-02

### Added

- `MisSignals` — a static, stateless global signal channel: `Report(eventId, payload, amount)`,
  `Subscribe` / `Unsubscribe` (by `ISignalListener`), `Clear`, `ListenerCount`. Snapshots its
  listener list per dispatch so a listener can (un)subscribe during `Report`.
- `ISignalListener` — `OnSignal(string eventId, string payload, int amount)`.
- `MochoIndieStudio.Signals` runtime asmdef, `noEngineReferences: true` (no UnityEngine dependency).

### Notes

- Extracted as the shared primitive behind the MIS Dialog System, MIS Quest System and MIS Inventory
  System packages. The Inventory System is the first consumer; Dialog and Quest migrate onto it
  separately.
- Deliberately minimal — plain `(string eventId, string payload, int amount)`, no `EventId` /
  `Payload` value types, no authoring widget, no editor code. A more general version (typed payloads,
  a shared `SignalTrigger` serializable + drawer) is planned once the three packages are stable
  against this one.
