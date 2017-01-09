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
			Rect flipRect =				new Rect(position.x, position.y + 20, 40, 16);
			Rect cornerPointRect =		new Rect(position.x + 42, position.y + 20, position.width - 42, 16);

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