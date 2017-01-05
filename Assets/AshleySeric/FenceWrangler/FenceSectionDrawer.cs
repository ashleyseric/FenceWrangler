using UnityEditor;
using UnityEngine;

namespace AshleySeric.FenceWrangler
{
	[CustomPropertyDrawer(typeof(FenceSection))]
	public class FenceSectionDrawer : PropertyDrawer
	{
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
			Rect heightModLabelRect = new Rect(position.x, position.y, 40, position.height);
			Rect heightModifierRect = new Rect(position.x + 40, position.y, 40, position.height);
			Rect flipRect = new Rect(position.x + 80, position.y, 40, position.height);
			Rect cornerPointRect = new Rect(position.x + 120, position.y, position.width - 120, position.height);
			SerializedProperty flipProp = property.FindPropertyRelative("flipFence");

			flipProp.boolValue = GUI.Toggle(flipRect, flipProp.boolValue, new GUIContent("Flip", "Flip the facing direction of the fence"), EditorStyles.miniButton);
			EditorGUI.LabelField(heightModLabelRect, new GUIContent("Scale", "Height scale modifier for this section of fence"));
			EditorGUI.PropertyField(heightModifierRect, property.FindPropertyRelative("heightModifier"), GUIContent.none);
			EditorGUI.PropertyField(cornerPointRect, property.FindPropertyRelative("cornerPoint"), GUIContent.none);

			// Set indent back to what it was
			EditorGUI.indentLevel = indent;

			EditorGUI.EndProperty();
		}
	}
}