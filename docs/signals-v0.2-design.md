# MIS Signals v0.2.0 — `[SignalId]` authoring

Status: **design approved 2026-09-03**, building.

## Why

`inventory-system-spec.md §4` deferred a "more general version" of the signal core until Dialog,
Quest and Inventory were all stable against Minimal (v0.1.0). They now are. Of the three deferred
ideas —

1. an `EventId` value type,
2. typed payloads,
3. a shared `SignalTrigger` serializable + drawer —

only the authoring-time typo problem has actually bitten us: the "Water from the Well" demo emits
`"item_collected"` from C# and matches `"item_collected"` in a hand-typed field on a quest `.asset`,
and nothing catches a mismatch. v0.2.0 fixes exactly that and nothing else.

**Not in v0.2.0** (revisit only when a concrete need appears):

- **Shared `SignalSpec` struct** — swapping `DialogEventTrigger` / `SignalCondition` fields for one
  shared serializable is a breaking asset migration for a payoff of "write the drawer once." The
  `[SignalId]` attribute below gives both packages id-validation with *zero* migration; that's the
  important half.
- **Typed payloads / `object context`** — no consumer needs richer payload data. When one does, the
  cheap non-breaking move is an optional `object context = null` param on `Report`; it's a 10-minute
  change, not a design.

## What ships in v0.2.0

The runtime core (`MochoIndieStudio.Signals`, `noEngineReferences: true`) is **untouched**. Two new
assemblies are added:

```
MIS Signals/
  Runtime/     MochoIndieStudio.Signals            (unchanged — MisSignals, ISignalListener)
  Authoring/   MochoIndieStudio.Signals.Authoring  (NEW, references UnityEngine)
  Editor/      MochoIndieStudio.Signals.Editor     (NEW, references Authoring + UnityEditor)
```

### `MochoIndieStudio.Signals.Authoring` (new)

- **`SignalIdAttribute : PropertyAttribute`** — a marker for `string` fields that hold a signal id.
  Purely a drawer hook: **the field stays a plain `string`**, so no serialized asset changes and no
  migration when a consumer adopts it.
  ```csharp
  [SignalId] [SerializeField] private string eventId;
  ```
- **`SignalIdProviderAttribute : Attribute`** (`AttributeTargets.Class`) — marks a static class
  whose `public const string` fields are signal ids the project knows about. Zero-authoring
  discovery: annotate `InventorySignalIds` once and every id it declares appears in the dropdown.
  ```csharp
  [SignalIdProvider]
  public static class InventorySignalIds { public const string ItemAdded = "item_added"; /* … */ }
  ```
- **`SignalCatalog : ScriptableObject`** — `Create ▸ MIS Signals ▸ Signal Catalog`. A game-authored
  list of `{ Id, Description }` entries for ids that aren't backed by a `const` (or to document the
  ones that are). Optional — the drawer works with none.
  ```csharp
  [Serializable] public struct Entry { public string Id; [TextArea] public string Description; }
  ```

This assembly is `autoReferenced: true` and UnityEngine-only (no UnityEditor), so it is safe for a
consumer's **Runtime** asmdef to reference.

### `MochoIndieStudio.Signals.Editor` (new)

- **`SignalIdRegistry`** (internal, `[InitializeOnLoad]`) — builds and caches the set of known ids
  from **both** sources, deduped by id:
  1. reflection via `TypeCache.GetTypesWithAttribute<SignalIdProviderAttribute>()` → every
     `public const string` field value;
  2. every `SignalCatalog` asset (`AssetDatabase.FindAssets("t:SignalCatalog")`).
  Rebuilds on domain reload and on `SignalCatalog` asset import (`AssetPostprocessor`).
- **`SignalIdDrawer : PropertyDrawer`** for `[SignalId]` — renders the string field with a trailing
  “▾” button that opens a searchable popup of known ids (id + description). Picking one sets the
  string. **Free text is always allowed** — a game can still type an ad-hoc id; unknown values show
  a subtle hint icon, not an error.

## Consumer adoption (separate follow-up releases)

Each is a one- or two-line change, no asset migration:

| Package | Change | Version |
|---|---|---|
| MIS Inventory System | `[SignalIdProvider]` on `InventorySignalIds`; `[SignalId]` on any authored id field; Runtime asmdef → `MochoIndieStudio.Signals.Authoring` | 0.2.0 |
| MIS Quest System | `[SignalId]` on `SignalCondition.eventId`; Runtime asmdef → `…Authoring` | 0.5.0 |
| MIS Dialog System | `[SignalId]` on `DialogEventTrigger.eventId`; Runtime asmdef → `…Authoring` | 0.7.0 |

A provider class must hold signal ids **only** — every `public const string` on it is offered.
The Quest demo's `DemoSignals` mixes ids with payload/item-id constants, so it stays un-annotated.

## Compatibility

- Runtime API (`MisSignals`, `ISignalListener`) unchanged — no behaviour change, no bump reason
  there.
- New assemblies are additive. A consumer that doesn't reference `…Authoring` is unaffected.
- SemVer: **minor** (`0.1.0 → 0.2.0`) — additive surface, no breaking change.
