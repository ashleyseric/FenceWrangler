/// Copyright 2018 Ashley Seric [contact@ashleyseric.com]
///
/// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
/// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
/// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, 
/// and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
/// 
/// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
/// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
/// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
/// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR 
/// IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

using UnityEditor;
using UnityEngine;

namespace AshleySeric.FenceWrangler
{
    [CustomPropertyDrawer(typeof(FenceSection))]
    public class FenceSectionDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return 40;
        }

        // Draw the property inside the given rect
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // Using BeginProperty / EndProperty on the parent property means that
            // prefab override logic works on the entire property.
            EditorGUI.BeginProperty(position, label, property);
            // Draw label
            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

            // Don't make child fields be indented
            var indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            // Calculate rects

            //Top Row
            Rect scaleLabelRect = new Rect(position.x, position.y, 40, 16);
            Rect scaleRect = new Rect(position.x + 40, position.y, 40, 16);
            Rect FenceDataRect = new Rect(position.x + 82, position.y, position.width - 82, 16);

            //Bottom Row
            Rect flipRect = new Rect(position.x, position.y + 20, 40, 16);
            Rect cornerPointRect = new Rect(position.x + 42, position.y + 20, position.width - 42, 16);

            //Flip toggle button
            SerializedProperty flipProp = property.FindPropertyRelative("flipFence");
            flipProp.boolValue = GUI.Toggle(flipRect, flipProp.boolValue, new GUIContent("Flip", "Flip the facing direction of the fence"), EditorStyles.miniButton);
            EditorGUI.LabelField(scaleLabelRect, new GUIContent("Height", "Height scale modifier for this section of fence"));
            EditorGUI.PropertyField(scaleRect, property.FindPropertyRelative("heightModifier"), GUIContent.none);
            EditorGUI.PropertyField(cornerPointRect, property.FindPropertyRelative("cornerPoint"), GUIContent.none);
            EditorGUI.PropertyField(FenceDataRect, property.FindPropertyRelative("data"), GUIContent.none);

            // Set indent back to what it was
            EditorGUI.indentLevel = indent;

            EditorGUI.EndProperty();
        }
    }
}