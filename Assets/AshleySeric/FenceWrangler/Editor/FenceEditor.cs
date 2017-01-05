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

			for (int i = 0; i < fence.sections.Count; i++)
			{
				EditorGUI.BeginChangeCheck();
				fence.sections[i].cornerPoint = Handles.PositionHandle((fence.sections[i].cornerPoint), handleRotation);
				if (EditorGUI.EndChangeCheck())
					fence.BuildFence();
				if (i < fence.sections.Count - 1)
					Handles.DrawLine(fence.sections[i].cornerPoint, fence.sections[i+1].cornerPoint);
				Handles.Label(fence.sections[i].cornerPoint, i.ToString(), EditorStyles.boldLabel);
			}

			#region Inputs
			Event e = Event.current;
			switch (e.type)
			{
				case EventType.keyDown:
					{
						if (e.keyCode == KeyCode.C)
						{
							Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
							RaycastHit hit;
							if (Physics.Raycast(ray, out hit, Mathf.Infinity))
							{
								Undo.RegisterCompleteObjectUndo(fence, "Added Section");
								fence.AddSection(hit.point);
							}
						}
						break;
					}
			}
			#endregion
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
			//hide the sections list so we can manually draw that.
			DrawPropertiesExcluding(serializedObject, "sections");

			CustomEditorList.Display(serializedObject.FindProperty("sections"));

			serializedObject.ApplyModifiedProperties();
		}
	}
}