using System.Collections;
using UnityEngine;
using UnityEditor;

namespace AshleySeric.FenceWrangler
{
	[CustomEditor(typeof(Fence))]
	public class FenceEditor : Editor
	{
		SerializedProperty vertCountProp;
		SerializedProperty postCountProp;
		SerializedProperty totalLengthProp;
		Editor dataEditor = null;

		void OnEnable()
		{
			// Setup the SerializedProperties.
			vertCountProp = serializedObject.FindProperty("vertexCount");
			postCountProp = serializedObject.FindProperty("postCount");
			totalLengthProp = serializedObject.FindProperty("totalLength");

			// Clear cached dataEditor to make sure it's recalled properly
			dataEditor = null;
		}

		private void OnSceneGUI()
		{
			Fence fence = target as Fence;
			if (fence.fenceData == null) return;
			Transform fenceTransform = fence.transform;

			Quaternion handleRotation = Tools.pivotRotation == PivotRotation.Local ? fenceTransform.rotation : Quaternion.identity;
			Handles.color = Color.white;

			for (int i = 0; i < fence.corners.Length; i++)
			{
				EditorGUI.BeginChangeCheck();
				fence.corners[i] = Handles.PositionHandle((fence.corners[i]), handleRotation);
				if (EditorGUI.EndChangeCheck())
					fence.BuildFence();
				if (i < fence.corners.Length - 1)
					Handles.DrawLine(fence.corners[i], fence.corners[i+1]);
			}
		}

		public override void OnInspectorGUI()
		{
			serializedObject.Update();
			Fence fence = target as Fence;
			if (fence.fenceData)
			{
				if (dataEditor == null)
					dataEditor = Editor.CreateEditor(fence.fenceData);
				GUILayout.BeginVertical(EditorStyles.helpBox);
				GUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();
				EditorGUILayout.LabelField(fence.fenceData.name + " (Preset)", EditorStyles.boldLabel);
				GUILayout.FlexibleSpace();
				GUILayout.EndHorizontal();
				dataEditor.OnInspectorGUI();
				GUILayout.EndVertical();
			}
			if (GUILayout.Button("Update Fence"))
				fence.BuildFence();
			EditorGUILayout.Space();
			EditorGUILayout.LabelField("Length: " + totalLengthProp.floatValue.ToString("F3") + "m");
			EditorGUILayout.LabelField("Posts: " + postCountProp.intValue);
			EditorGUILayout.LabelField("Vertex Count: " + vertCountProp.intValue);
			EditorGUILayout.Space();
			DrawDefaultInspector();
		}
	}
}