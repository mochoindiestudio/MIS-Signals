using System;
using System.Collections.Generic;
using MochoIndieStudio.Signals.Authoring;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace MochoIndieStudio.Signals.Editor
{
    /// <summary>
    /// Draws a <see cref="SignalIdAttribute"/> string field as a normal text field plus a button that
    /// opens a searchable picker of ids known to the project (<see cref="SignalIdRegistry"/>). The
    /// field stays free text — an unknown id is allowed and shown with a subtle hint icon, never an
    /// error.
    /// </summary>
    [CustomPropertyDrawer(typeof(SignalIdAttribute))]
    public sealed class SignalIdDrawer : PropertyDrawer
    {
        private const float ButtonWidth = 20f;
        private const float Gap = 2f;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.String)
            {
                EditorGUI.PropertyField(position, property, label);
                return;
            }

            EditorGUI.BeginProperty(position, label, property);

            var fieldRect = new Rect(position.x, position.y, position.width - ButtonWidth - Gap, position.height);
            var buttonRect = new Rect(fieldRect.xMax + Gap, position.y, ButtonWidth, position.height);

            bool known = string.IsNullOrEmpty(property.stringValue) || IsKnown(property.stringValue);
            GUIContent fieldLabel = label;
            if (!known)
            {
                fieldLabel = new GUIContent(label.text, "This id is not declared by any [SignalIdProvider] class or SignalCatalog. It will still work — just check for a typo.");
            }

            EditorGUI.PropertyField(fieldRect, property, fieldLabel);

            var buttonContent = new GUIContent("▾", "Pick a known signal id");
            if (GUI.Button(buttonRect, buttonContent, EditorStyles.miniButton))
            {
                SerializedObject serializedObject = property.serializedObject;
                string propertyPath = property.propertyPath;

                var dropdown = new SignalIdDropdown(new AdvancedDropdownState(), chosen =>
                {
                    serializedObject.Update();
                    SerializedProperty target = serializedObject.FindProperty(propertyPath);
                    if (target != null)
                    {
                        target.stringValue = chosen;
                        serializedObject.ApplyModifiedProperties();
                    }
                });
                dropdown.Show(buttonRect);
            }

            EditorGUI.EndProperty();
        }

        private static bool IsKnown(string id)
        {
            IReadOnlyList<SignalIdRegistry.Known> ids = SignalIdRegistry.Ids;
            for (int i = 0; i < ids.Count; i++)
            {
                if (string.Equals(ids[i].Id, id, StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }

        private sealed class SignalIdDropdown : AdvancedDropdown
        {
            private readonly Action<string> onPick;
            private readonly List<string> ids = new List<string>();

            public SignalIdDropdown(AdvancedDropdownState state, Action<string> onPick) : base(state)
            {
                this.onPick = onPick;
                minimumSize = new Vector2(260f, 320f);
            }

            protected override AdvancedDropdownItem BuildRoot()
            {
                var root = new AdvancedDropdownItem("Signal Id");
                ids.Clear();

                IReadOnlyList<SignalIdRegistry.Known> known = SignalIdRegistry.Ids;
                if (known.Count == 0)
                {
                    root.AddChild(new AdvancedDropdownItem("No known ids — type one, or add a SignalCatalog")
                    {
                        enabled = false
                    });
                    return root;
                }

                for (int i = 0; i < known.Count; i++)
                {
                    root.AddChild(new AdvancedDropdownItem(known[i].Id) { id = ids.Count });
                    ids.Add(known[i].Id);
                }

                return root;
            }

            protected override void ItemSelected(AdvancedDropdownItem item)
            {
                if (item != null && item.id >= 0 && item.id < ids.Count)
                {
                    onPick?.Invoke(ids[item.id]);
                }
            }
        }
    }
}
