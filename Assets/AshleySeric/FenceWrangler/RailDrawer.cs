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
			
			// Calculate rects
			Rect hOffsetLabelRect = new Rect(position.x, position.y, 42, position.height);
			Rect hOffsetRect = new Rect(position.x + 44, position.y, 40, position.height);
			Rect dimensionsLabelRect = new Rect(position.x + 98, position.y, 28, position.height);
			Rect dimensionsRect = new Rect(position.x + 128, position.y, position.width - 128, position.height);

			EditorGUI.LabelField(hOffsetLabelRect, new GUIContent("Height", "Height scale modifier for this section of fence"));
			EditorGUI.PropertyField(hOffsetRect, property.FindPropertyRelative("groundOffset"), GUIContent.none);
			EditorGUI.LabelField(dimensionsLabelRect, new GUIContent("Size", "Dimensions for this rail."));
			EditorGUI.PropertyField(dimensionsRect, property.FindPropertyRelative("width"), GUIContent.none);
			EditorGUI.EndProperty();
		}
	}
}