using System;
using System.Collections.Generic;
using System.Reflection;
using MochoIndieStudio.Signals.Authoring;
using UnityEditor;

namespace MochoIndieStudio.Signals.Editor
{
    /// <summary>
    /// Editor-only cache of the signal ids known to the project, gathered from two sources and
    /// deduped by id:
    /// <list type="number">
    ///   <item><c>public const string</c> fields of every <see cref="SignalIdProviderAttribute"/> class;</item>
    ///   <item>every <see cref="SignalCatalog"/> asset's entries (these win on description).</item>
    /// </list>
    /// The cache is lazy: it rebuilds on first use after an invalidation, and is invalidated on
    /// domain reload and whenever a <c>.asset</c> is imported, moved or deleted.
    /// </summary>
    [InitializeOnLoad]
    internal static class SignalIdRegistry
    {
        /// <summary>One known id plus optional human text, for the picker.</summary>
        internal readonly struct Known
        {
            public Known(string id, string description, string source)
            {
                Id = id;
                Description = description;
                Source = source;
            }

            /// <summary>The signal id string.</summary>
            public string Id { get; }

            /// <summary>Human description, or empty. Comes from a <see cref="SignalCatalog"/> entry.</summary>
            public string Description { get; }

            /// <summary>Where this id was found (a type + field name, or a catalog asset name). For tooltips.</summary>
            public string Source { get; }
        }

        private static List<Known> cache;

        static SignalIdRegistry()
        {
            Invalidate();
        }

        /// <summary>The known ids, sorted by id. Rebuilds the cache on first use after an invalidation.</summary>
        internal static IReadOnlyList<Known> Ids
        {
            get
            {
                if (cache == null)
                {
                    Rebuild();
                }

                return cache;
            }
        }

        /// <summary>Drops the cache; the next <see cref="Ids"/> read rebuilds it.</summary>
        internal static void Invalidate()
        {
            cache = null;
        }

        private static void Rebuild()
        {
            var byId = new Dictionary<string, Known>(StringComparer.Ordinal);
            CollectFromProviders(byId);
            CollectFromCatalogs(byId);

            var list = new List<Known>(byId.Values);
            list.Sort((a, b) => string.CompareOrdinal(a.Id, b.Id));
            cache = list;
        }

        private static void CollectFromProviders(Dictionary<string, Known> byId)
        {
            foreach (Type type in TypeCache.GetTypesWithAttribute<SignalIdProviderAttribute>())
            {
                FieldInfo[] fields = type.GetFields(
                    BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);

                for (int i = 0; i < fields.Length; i++)
                {
                    FieldInfo field = fields[i];

                    // public const string only (IsLiteral = const, IsInitOnly rules out static readonly).
                    if (!field.IsLiteral || field.IsInitOnly || field.FieldType != typeof(string))
                    {
                        continue;
                    }

                    var value = (string)field.GetRawConstantValue();
                    if (string.IsNullOrEmpty(value) || byId.ContainsKey(value))
                    {
                        continue;
                    }

                    byId[value] = new Known(value, string.Empty, type.Name + "." + field.Name);
                }
            }
        }

        private static void CollectFromCatalogs(Dictionary<string, Known> byId)
        {
            string[] guids = AssetDatabase.FindAssets("t:" + nameof(SignalCatalog));
            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                var catalog = AssetDatabase.LoadAssetAtPath<SignalCatalog>(path);
                if (catalog == null)
                {
                    continue;
                }

                IReadOnlyList<SignalCatalog.Entry> entries = catalog.Entries;
                for (int e = 0; e < entries.Count; e++)
                {
                    SignalCatalog.Entry entry = entries[e];
                    if (string.IsNullOrEmpty(entry.Id))
                    {
                        continue;
                    }

                    // A catalog entry overrides a bare const so its description shows.
                    byId[entry.Id] = new Known(entry.Id, entry.Description ?? string.Empty, catalog.name);
                }
            }
        }

        /// <summary>Invalidates the cache when any asset changes — cheap, since the rebuild is lazy.</summary>
        private sealed class CatalogWatcher : AssetPostprocessor
        {
            private static void OnPostprocessAllAssets(
                string[] imported, string[] deleted, string[] moved, string[] movedFromAssetPaths)
            {
                if (TouchesAsset(imported) || TouchesAsset(deleted) || TouchesAsset(moved))
                {
                    Invalidate();
                }
            }

            private static bool TouchesAsset(string[] paths)
            {
                for (int i = 0; i < paths.Length; i++)
                {
                    if (paths[i].EndsWith(".asset", StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }

                return false;
            }
        }
    }
}
