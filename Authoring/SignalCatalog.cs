using System;
using System.Collections.Generic;
using UnityEngine;

namespace MochoIndieStudio.Signals.Authoring
{
    /// <summary>
    /// An optional, game-authored list of signal ids and what they mean, surfaced in the
    /// <c>[SignalId]</c> picker alongside ids discovered from <see cref="SignalIdProviderAttribute"/>
    /// classes. Use it for ids a game fires ad-hoc (nothing declares them as a <c>const</c>), or to
    /// document the ones that do. A project may hold several catalogs; the picker merges them all.
    /// </summary>
    [CreateAssetMenu(fileName = "SignalCatalog", menuName = "MIS Signals/Signal Catalog")]
    public sealed class SignalCatalog : ScriptableObject
    {
        /// <summary>One documented signal id.</summary>
        [Serializable]
        public struct Entry
        {
            [Tooltip("The signal id — the first argument to MisSignals.Report.")]
            public string Id;

            [Tooltip("What this signal means / when it fires. Shown in the picker.")]
            [TextArea(1, 3)]
            public string Description;
        }

        [SerializeField]
        private List<Entry> entries = new List<Entry>();

        /// <summary>The catalog's entries, in author order. Never null; fields on an entry may be empty.</summary>
        public IReadOnlyList<Entry> Entries => entries;
    }
}
