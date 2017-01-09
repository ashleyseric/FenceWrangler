using UnityEngine;
using UnityEditor;

public static class CustomEditorList {

	public static GUIContent
		moveDownBtn = new GUIContent("\x2228", "Move Down"),
		deleteBtn = new GUIContent("-", "Delete"),
		dupBtn = new GUIContent("+", "Duplicate"),
		newBtn = new GUIContent("New Item", "Add a new item");

	public static void Display(SerializedProperty listProp, bool showArraySize = false, bool showIndex = false)
	{
		BaseDisplay(listProp, showArraySize, showIndex);
	}
	public static int DisplayAndGetIndex(SerializedProperty listProp, int currentIndex, bool showArraySize = false, bool showIndex = false, string selectLabel = "Sel")
	{
		return BaseDisplay(listProp, showArraySize, showIndex, true, currentIndex, selectLabel);
	}
	static int BaseDisplay(SerializedProperty listProp, bool showArraySize = false, bool showIndex = false, bool getSelection = false, int selectedIndex = -1, string selectLabel = "Sel")
	{
		if (!listProp.isArray)
		{
			EditorGUILayout.HelpBox(listProp.name + " must be an array or a list!", MessageType.Error);
			return 0;
		}
		EditorGUILayout.PropertyField(listProp);
		EditorGUI.indentLevel += 1;
		int sel = selectedIndex;
		if (listProp.isExpanded)
		{
			if (showArraySize)
				EditorGUILayout.PropertyField(listProp.FindPropertyRelative("Array.size"));

			for (int i = 0; i < listProp.arraySize; i++)
			{
				GUIStyle s = new GUIStyle(EditorStyles.helpBox);
				s.border = new RectOffset(5, 2, 2, 2);
				if (i == selectedIndex)
					s.normal.background = EditorStyles.miniButton.normal.background;
				EditorGUILayout.BeginHorizontal(s);

				//Show our own index label
				if (showIndex)
					EditorGUILayout.LabelField(i.ToString(), GUILayout.Width(40));

				//Hide default label
				EditorGUILayout.PropertyField(listProp.GetArrayElementAtIndex(i), GUIContent.none);

				EditorGUILayout.BeginVertical(GUILayout.Width(60));
				EditorGUILayout.BeginHorizontal(GUILayout.Width(60));
				if (GUILayout.Button(moveDownBtn, EditorStyles.miniButtonLeft, GUILayout.Width(20)))
					listProp.MoveArrayElement(i, i + 1);

				if (GUILayout.Button(dupBtn, EditorStyles.miniButtonMid, GUILayout.Width(20)))
					listProp.InsertArrayElementAtIndex(i);

				if (GUILayout.Button(deleteBtn, EditorStyles.miniButtonRight, GUILayout.Width(20)))
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
				if (getSelection && GUILayout.Button(selectLabel, EditorStyles.miniButton, GUILayout.Width(60)))
				{
					sel = i;
				}
				EditorGUILayout.EndVertical();
				EditorGUILayout.EndHorizontal();
			}
			if (listProp.arraySize == 0 && GUILayout.Button(newBtn, EditorStyles.miniButton))
			{
				listProp.arraySize += 1;
			}
		}
		EditorGUI.indentLevel -= 1;
		return sel;
	}
}
