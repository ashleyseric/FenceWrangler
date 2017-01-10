using UnityEngine;
using UnityEditor;

namespace AshleySeric.FenceWrangler
{
	[CustomEditor(typeof(FenceData))]
	public class FenceDataEditor : Editor
	{
		public override void OnInspectorGUI()
		{
			serializedObject.Update();
			FenceData data= target as FenceData;
			DrawPropertiesExcluding(serializedObject, "rails", "m_Script");
			int indent = EditorGUI.indentLevel;
			EditorGUI.indentLevel = 1;
			CustomEditorList.Display(serializedObject.FindProperty("rails"));
			EditorGUI.indentLevel = indent;
			serializedObject.ApplyModifiedProperties();
		}
	}
}
