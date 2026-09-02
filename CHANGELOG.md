# Changelog

All notable changes to this project are documented here. This project adheres to
[Keep a Changelog](https://keepachangelog.com) and [Semantic Versioning](https://semver.org).

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
