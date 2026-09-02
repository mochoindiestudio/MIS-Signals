# MIS Signals

`com.mochoindiestudio.signals`

A tiny, engine-light global signal channel shared by the Mocho Indie Studio narrative/simulation
packages — **MIS Dialog System**, **MIS Quest System**, **MIS Inventory System**. It exists so those
packages can integrate *through a game* without referencing each other's assemblies: one package
`Report`s a signal, another (subscribed) reacts.

- Runtime only. **No `UnityEngine` dependency** (`noEngineReferences: true`), no editor tooling.
- One channel, one shape: `(string eventId, string payload, int amount)`.
- Stateless — holds only the live listener list; no game data.

## API

```csharp
using MochoIndieStudio.Signals;

// announce
MisSignals.Report("item_added", "herb", 1);
MisSignals.Report("reached", "well");            // amount defaults to 1
MisSignals.Report("enemy_killed", null, 3);      // no payload

// receive
sealed class QuestBridge : ISignalListener
{
    public void OnSignal(string eventId, string payload, int amount) { /* match & react */ }
}

var bridge = new QuestBridge();
MisSignals.Subscribe(bridge);
// ...
MisSignals.Unsubscribe(bridge);   // on teardown

MisSignals.Clear();               // drop all listeners (tests / domain-reload-off projects)
int n = MisSignals.ListenerCount; // diagnostics
```

- `Report` is a no-op when `eventId` is null/empty, `amount <= 0`, or nothing is subscribed.
- Dispatch is synchronous, on the calling thread. The listener list is snapshotted per `Report`, so
  a listener may subscribe/unsubscribe while handling a signal (it affects the next dispatch).
- Listeners must not throw from `OnSignal`.
- Not thread-safe — call from one thread (Unity's main thread in a game).

## Install

Add to a project's `Packages/manifest.json`:

```jsonc
// during local development (sibling checkout)
"com.mochoindiestudio.signals": "file:../../MIS Signals"

// released
"com.mochoindiestudio.signals": "https://github.com/mochoindiestudio/MIS-Signals.git#v0.1.0"
```

## Scope

Deliberately minimal. No `EventId` / `Payload` value types, no shared authoring widget, no editor
code — just the channel. A more general version (typed payloads, a shared `SignalTrigger`
serializable + property drawer used by all three packages) comes **after** Dialog / Quest / Inventory
are stable against this one. See `docs/` in the MIS Inventory System repo.

## License

MIT — see `LICENSE.md`.
