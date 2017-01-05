using UnityEngine;
using UnityEditor;

public static class CustomEditorList {

	public static GUIContent
		moveDownBtn = new GUIContent("\x2228", "Move Down"),
		deleteBtn = new GUIContent("-", "Delete"),
		dupBtn = new GUIContent("+", "Duplicate"),
		newBtn = new GUIContent("New Item", "Add a new item");

	public static void Display(SerializedProperty listProp)
	{
		if (!listProp.isArray)
		{
			EditorGUILayout.HelpBox(listProp.name + " must be an array or a list!", MessageType.Error);
			return;
		}
		EditorGUILayout.PropertyField(listProp);
		EditorGUI.indentLevel += 1;
		if (listProp.isExpanded)
		{
			//EditorGUILayout.LabelField(new GUIContent(listProp.FindPropertyRelative("Array.size").intValue.ToString()));
			for (int i = 0; i < listProp.arraySize; i++)
			{
				EditorGUILayout.BeginHorizontal();
				//Show our own label
				EditorGUILayout.LabelField("200", GUILayout.Width(40));
				//Hide label
				EditorGUILayout.PropertyField(listProp.GetArrayElementAtIndex(i), GUIContent.none);
				if(GUILayout.Button(moveDownBtn, EditorStyles.miniButtonLeft, GUILayout.Width(20)))
				{
					listProp.MoveArrayElement(i, i + 1);
				}
				if(GUILayout.Button(dupBtn, EditorStyles.miniButtonMid, GUILayout.Width(20)))
				{
					listProp.InsertArrayElementAtIndex(i);
				}
				if(GUILayout.Button(deleteBtn, EditorStyles.miniButtonRight, GUILayout.Width(20)))
				{
					// Unity clears references instead of deleting the item sometimes, this is just a check
					// to make sure we're actually deleting it.
					int oldSize = listProp.arraySize;
					listProp.DeleteArrayElementAtIndex(i);
					if (listProp.arraySize == oldSize)
					{
						listProp.DeleteArrayElementAtIndex(i);
					}
				}
				EditorGUILayout.EndHorizontal();
			}
			if (listProp.arraySize == 0 && GUILayout.Button(newBtn, EditorStyles.miniButton))
			{
				listProp.arraySize += 1;
			}
		}
		EditorGUI.indentLevel -= 1;
	}
}
