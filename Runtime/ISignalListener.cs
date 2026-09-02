namespace MochoIndieStudio.Signals
{
    /// <summary>
    /// Implemented by any system that wants to receive game signals reported through
    /// <see cref="MisSignals"/> — for example a quest log, an achievement tracker or an analytics
    /// sink. The consuming game (or a subsystem) subscribes an instance via
    /// <see cref="MisSignals.Subscribe"/> and removes it again with
    /// <see cref="MisSignals.Unsubscribe"/> when it is disposed.
    /// </summary>
    public interface ISignalListener
    {
        /// <summary>
        /// Invoked once for every signal reported via <see cref="MisSignals.Report"/>, synchronously
        /// on the thread that called <c>Report</c>. Implementations match <paramref name="eventId"/>
        /// and <paramref name="payload"/> against what they care about and ignore everything else.
        /// An implementation must not throw — an exception here propagates to the caller of
        /// <c>Report</c> and stops later listeners from being notified.
        /// </summary>
        /// <param name="eventId">
        /// Signal identifier, e.g. <c>"item_added"</c> or <c>"enemy_killed"</c>. Never null or empty.
        /// </param>
        /// <param name="payload">
        /// Optional free-form qualifier, e.g. an item id or enemy type. May be null, meaning the
        /// signal was reported without one.
        /// </param>
        /// <param name="amount">How much the signal represents; always at least 1.</param>
        void OnSignal(string eventId, string payload, int amount);
    }
}
