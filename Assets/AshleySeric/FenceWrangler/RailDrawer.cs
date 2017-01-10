using UnityEngine;
using UnityEditor;

namespace AshleySeric.FenceWrangler
{
	[CustomPropertyDrawer(typeof(FenceData.Rail))]
	public class RailDrawer : PropertyDrawer
	{
		// Draw the property inside the given rect
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			EditorGUI.BeginProperty(position, label, property);
			// Draw label
			position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

			// Remove indentation
			int indent = EditorGUI.indentLevel;
			EditorGUI.indentLevel = 0;
			// Calculate rects
			Rect hOffsetLabelRect = new Rect(position.x, position.y, 42, position.height);
			Rect hOffsetRect = new Rect(position.x + 44, position.y, 40, position.height);
			Rect dimensionsLabelRect = new Rect(position.x + 98, position.y, 28, position.height);
			Rect dimensionsRect = new Rect(position.x + 128, position.y, position.width - 128, position.height);
			
			//SerializedProperty heightProp = property.FindPropertyRelative("heightOffset");
			//SerializedProperty dimensionProp = property.FindPropertyRelative("dimensions");
			//EditorGUILayout.LabelField(new GUIContent("Height", "Height scale modifier for this section of fence"), GUILayout.Width(40));
			//heightProp.floatValue = EditorGUILayout.FloatField(heightProp.floatValue);
			//EditorGUILayout.LabelField(new GUIContent("Size:", "Dimensions for this rail."), GUILayout.Width(40));
			//dimensionProp.vector2Value = EditorGUILayout.Vector2Field(GUIContent.none, dimensionProp.vector2Value, GUILayout.MinWidth(50));

			EditorGUI.LabelField(hOffsetLabelRect, new GUIContent("Height", "Height scale modifier for this section of fence"));
			EditorGUI.PropertyField(hOffsetRect, property.FindPropertyRelative("heightOffset"), GUIContent.none);
			EditorGUI.LabelField(dimensionsLabelRect, new GUIContent("Size", "Dimensions for this rail."));
			EditorGUI.PropertyField(dimensionsRect, property.FindPropertyRelative("dimensions"), GUIContent.none);
			// Set indent back to what it was
			
			//reset indent level.
			EditorGUI.indentLevel = indent;
			EditorGUI.EndProperty();
		}
	}
}