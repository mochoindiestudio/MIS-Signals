using UnityEngine;

namespace MochoIndieStudio.Signals.Authoring
{
    /// <summary>
    /// Marks a <see cref="string"/> field that holds a signal id — the first argument to
    /// <c>MochoIndieStudio.Signals.MisSignals.Report</c>. The field stays a plain string; this only
    /// tells the editor to draw it with a searchable picker of ids the project knows about (from
    /// <see cref="SignalIdProviderAttribute"/> classes and <see cref="SignalCatalog"/> assets). Free
    /// text is always allowed — an unknown id is not an error.
    /// </summary>
    /// <example><code>
    /// [SignalId] [SerializeField] private string eventId;
    /// </code></example>
    public sealed class SignalIdAttribute : PropertyAttribute
    {
    }
}
