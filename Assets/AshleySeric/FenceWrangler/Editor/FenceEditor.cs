using System.Collections;
using UnityEngine;
using UnityEditor;

namespace AshleySeric.FenceWrangler
{
	[CustomEditor(typeof(Fence))]
	public class FenceEditor : Editor
	{
		SerializedProperty vertCountProp;
		SerializedProperty triCountProp;
		SerializedProperty postCountProp;
		SerializedProperty picketCountProp;
		SerializedProperty totalLengthProp;
		Editor dataEditor = null;

		void OnEnable()
		{
			// Setup the SerializedProperties.
			vertCountProp = serializedObject.FindProperty("vertexCount");
			triCountProp = serializedObject.FindProperty("triCount");
			postCountProp = serializedObject.FindProperty("postCount");
			picketCountProp = serializedObject.FindProperty("picketCount");
			totalLengthProp = serializedObject.FindProperty("totalLength");

			// Clear cached dataEditor to make sure it's recalled properly
			dataEditor = null;
		}
		private void OnSceneGUI()
		{
			Fence fence = target as Fence;
			fence.ClampSelectionIndex();
			if (fence.sections.Count == 0 || fence.sections[fence.selectedSectionIndex] == null) return;
			Transform fenceTransform = fence.transform;

			Quaternion handleRotation = Tools.pivotRotation == PivotRotation.Local ? fenceTransform.rotation : Quaternion.identity;

			for (int i = 0; i < fence.sections.Count; i++)
			{
				Handles.color = fence.sections[i].data == null ? Color.red : Color.white;
				EditorGUI.BeginChangeCheck();
				fence.sections[i].cornerPoint = Handles.PositionHandle(fence.sections[i].cornerPoint, handleRotation);
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
			fence.ClampSelectionIndex();
			if (GUILayout.Button("Update Fence"))
				fence.BuildFence();
			EditorGUILayout.Space();
			EditorGUILayout.LabelField("Length: " + totalLengthProp.floatValue.ToString("F3") + "m");
			EditorGUILayout.LabelField("Posts: " + postCountProp.intValue);
			EditorGUILayout.LabelField("Pickets: " + picketCountProp.intValue);
			EditorGUILayout.LabelField("Vertices: " + vertCountProp.intValue);
			EditorGUILayout.LabelField("Triangles: " + triCountProp.intValue);
			EditorGUILayout.Space();
			//hide the sections list so we can manually draw that.

			if (fence.sections.Count > 0)
			{
				if (fence.sections[fence.selectedSectionIndex].data)
				{
					if (dataEditor == null)
						dataEditor = Editor.CreateEditor(fence.sections[fence.selectedSectionIndex].data);
					GUILayout.BeginVertical(EditorStyles.helpBox);
					GUILayout.BeginHorizontal();
					GUILayout.FlexibleSpace();
					EditorGUILayout.LabelField(fence.sections[fence.selectedSectionIndex].data.name + " (Preset)", EditorStyles.boldLabel);
					GUILayout.FlexibleSpace();
					GUILayout.EndHorizontal();
					EditorGUI.BeginChangeCheck();
					dataEditor.OnInspectorGUI();
					//Give us live updates when changing presets in the instpector.
					if (EditorGUI.EndChangeCheck())
					{
						fence.BuildFence();
					}
					GUILayout.EndVertical();
				}
				else
				{
					EditorGUILayout.HelpBox("No fence data associated with this section. Please add one.", MessageType.Error);
				}
			}

			DrawPropertiesExcluding(serializedObject, "sections", "m_Script");

			fence.selectedSectionIndex = CustomEditorList.DisplayAndGetIndex(serializedObject.FindProperty("sections"), fence.selectedSectionIndex, false, true, "Edit");

			serializedObject.ApplyModifiedProperties();
		}
	}
}