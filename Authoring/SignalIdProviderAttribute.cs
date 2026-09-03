using System;

namespace MochoIndieStudio.Signals.Authoring
{
    /// <summary>
    /// Marks a static class whose <c>public const string</c> fields are signal ids the project uses.
    /// The <c>[SignalId]</c> picker lists every id it finds this way, so annotating an ids-holder
    /// such as <c>InventorySignalIds</c> once makes all of its ids discoverable with no
    /// <see cref="SignalCatalog"/> asset to maintain.
    /// </summary>
    /// <example><code>
    /// [SignalIdProvider]
    /// public static class InventorySignalIds
    /// {
    ///     public const string ItemAdded = "item_added";
    ///     public const string ItemRemoved = "item_removed";
    /// }
    /// </code></example>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public sealed class SignalIdProviderAttribute : Attribute
    {
    }
}
