using System.Collections.Generic;

namespace MochoIndieStudio.Signals
{
    /// <summary>
    /// The one global channel a game uses to announce that something happened —
    /// <c>MisSignals.Report("item_added", "herb", 1)</c>. Systems that care implement
    /// <see cref="ISignalListener"/> and register with <see cref="Subscribe"/>; the MIS Quest
    /// System's quest log and the MIS Inventory System both plug in here, which is how a quest
    /// objective can count inventory pickups without either package referencing the other.
    ///
    /// <para>
    /// This is a stateless forwarder. It holds no game state — only the list of live listeners, each
    /// of which adds and removes itself over its own lifetime. It carries no mutable domain data, so
    /// it does not count as "static mutable state" under the studio guidelines (the same exception
    /// the Quest System's <c>QuestSignals</c> relied on, now shared and generalised).
    /// </para>
    /// <para>
    /// Not thread-safe: call the members from one thread (normally Unity's main thread). Reporting a
    /// signal from inside a listener is fine — the listener list is snapshotted per dispatch, so
    /// subscribing or unsubscribing during <see cref="Report"/> affects the next dispatch, not the
    /// current one.
    /// </para>
    /// <para>
    /// If a consuming project disables Enter Play Mode domain reload, the listener list survives
    /// between play sessions. Listeners that unsubscribe on teardown (as the Quest/Inventory runtimes
    /// do) stay correct; a project that cannot guarantee that should call <see cref="Clear"/> from
    /// its own <c>RuntimeInitializeOnLoadMethod</c>. This package stays UnityEngine-free and so does
    /// not register that hook itself.
    /// </para>
    /// </summary>
    public static class MisSignals
    {
        private static readonly List<ISignalListener> Listeners = new List<ISignalListener>();

        /// <summary>Number of currently subscribed listeners. For diagnostics and tests.</summary>
        public static int ListenerCount => Listeners.Count;

        /// <summary>
        /// Announces a signal to every live listener, synchronously. No-op when
        /// <paramref name="eventId"/> is null or empty, when <paramref name="amount"/> is
        /// <c>&lt;= 0</c>, or when nothing is subscribed.
        /// </summary>
        /// <param name="eventId">Signal identifier such as <c>"reached"</c> or <c>"item_added"</c>. Required.</param>
        /// <param name="payload">Optional qualifier a listener may filter on; <c>null</c> means "unqualified".</param>
        /// <param name="amount">Progress the signal represents; defaults to 1, values <c>&lt;= 0</c> are ignored.</param>
        public static void Report(string eventId, string payload = null, int amount = 1)
        {
            if (string.IsNullOrEmpty(eventId) || amount <= 0)
            {
                return;
            }

            int count = Listeners.Count;
            if (count == 0)
            {
                return;
            }

            // Snapshot before dispatch: a listener may subscribe or unsubscribe (itself or another)
            // while handling the signal, and mutating the list mid-iteration would throw or skip.
            var snapshot = new ISignalListener[count];
            Listeners.CopyTo(snapshot);

            for (int i = 0; i < snapshot.Length; i++)
            {
                snapshot[i].OnSignal(eventId, payload, amount);
            }
        }

        /// <summary>Registers a listener. Null and already-subscribed instances are ignored.</summary>
        public static void Subscribe(ISignalListener listener)
        {
            if (listener != null && !Listeners.Contains(listener))
            {
                Listeners.Add(listener);
            }
        }

        /// <summary>Unregisters a listener. Safe to call for one that is not subscribed.</summary>
        public static void Unsubscribe(ISignalListener listener)
        {
            Listeners.Remove(listener);
        }

        /// <summary>
        /// Removes every listener. Intended for test isolation and for projects that disable domain
        /// reload and need a clean slate on play. Normal runtime code unsubscribes individually.
        /// </summary>
        public static void Clear()
        {
            Listeners.Clear();
        }
    }
}
