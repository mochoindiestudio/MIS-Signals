# MIS Signals

`com.mochoindiestudio.signals`

A tiny, engine-light global signal channel shared by the Mocho Indie Studio narrative/simulation
packages — **MIS Dialog System**, **MIS Quest System**, **MIS Inventory System**. It exists so those
packages can integrate *through a game* without referencing each other's assemblies: one package
`Report`s a signal, another (subscribed) reacts.

- Runtime core has **no `UnityEngine` dependency** (`noEngineReferences: true`).
- One channel, one shape: `(string eventId, string payload, int amount)`.
- Stateless — holds only the live listener list; no game data.
- Optional **Authoring** assembly (v0.2.0): a `[SignalId]` attribute + a searchable id picker for
  editor fields (see below). Runtime consumers that don't reference it are unaffected.

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

## Authoring — `[SignalId]` (optional, v0.2.0)

`MisSignals.Report` takes a bare string, and so does the field a quest/dialog asset stores to match
it — a typo between the two fails silently. The `MochoIndieStudio.Signals.Authoring` assembly turns
that field into a picked value:

```csharp
using MochoIndieStudio.Signals.Authoring;

// 1. Declare your ids in one place and expose them to the picker:
[SignalIdProvider]
public static class InventorySignalIds
{
    public const string ItemAdded   = "item_added";
    public const string ItemRemoved = "item_removed";
}

// 2. Any editor-authored id field gets a searchable dropdown of known ids (still free text):
[SignalId] [SerializeField] private string eventId;
```

The picker lists ids from every `[SignalIdProvider]` class **and** every `SignalCatalog` asset
(`Create ▸ MIS Signals ▸ Signal Catalog` — use it for ids nothing declares as a `const`, or to add
descriptions). The field stays a plain `string`, so adopting `[SignalId]` needs no asset migration.

Reference `MochoIndieStudio.Signals.Authoring` from a consumer's **Runtime** asmdef (it is
UnityEngine-only, no UnityEditor). The picker itself lives in the editor-only
`MochoIndieStudio.Signals.Editor` assembly.

## Install

Add to a project's `Packages/manifest.json`:

```jsonc
// during local development (sibling checkout)
"com.mochoindiestudio.signals": "file:../../MIS Signals"

// released
"com.mochoindiestudio.signals": "https://github.com/mochoindiestudio/MIS-Signals.git#v0.2.0"
```

## Scope

The runtime channel is deliberately minimal — plain `(string eventId, string payload, int amount)`,
no `EventId` / `Payload` value types. Two ideas remain deliberately deferred until a concrete need
appears: a shared `SignalSpec` serializable struct (replacing each package's own id/payload field
pair), and typed payloads. See `docs/signals-v0.2-design.md` for the reasoning.

## License

MIT — see `LICENSE.md`.
