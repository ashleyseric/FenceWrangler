using System.Collections;
using UnityEngine;
using UnityEditor;

namespace AshleySeric.FenceWrangler
{
	[CustomEditor(typeof(Fence))]
	public class FenceEditor : Editor
	{
		SerializedProperty showPreset;

		SerializedProperty vertCountProp;
		SerializedProperty triCountProp;
		SerializedProperty postCountProp;
		SerializedProperty picketCountProp;
		SerializedProperty totalLengthProp;
		GUIStyle prevLabel;

		//private PreviewRenderUtility _previewRenderUtility;
		//private MeshFilter _targetMeshFilter;
		//private MeshRenderer _targetMeshRenderer;

		void Awake()
		{
			prevLabel = new GUIStyle(EditorStyles.centeredGreyMiniLabel);
			prevLabel.alignment = TextAnchor.UpperLeft;
		}
		void OnEnable()
		{
			// Setup the SerializedProperties.
			showPreset = serializedObject.FindProperty("editor_showPreset");

			vertCountProp = serializedObject.FindProperty("vertexCount");
			triCountProp = serializedObject.FindProperty("triCount");
			postCountProp = serializedObject.FindProperty("totalPosts");
			picketCountProp = serializedObject.FindProperty("picketCount");
			totalLengthProp = serializedObject.FindProperty("totalLength");
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

			//hide the sections list so we can manually draw it.
			if (fence.sections.Count > 0)
			{
				if (fence.sections[fence.selectedSectionIndex].data)
				{
					Editor dataEditor = Editor.CreateEditor(fence.sections[fence.selectedSectionIndex].data);
					GUILayout.BeginVertical(EditorStyles.helpBox);
					GUILayout.BeginHorizontal();
					EditorGUILayout.LabelField(fence.sections[fence.selectedSectionIndex].data.name + " (Preset)", EditorStyles.boldLabel);
					if (GUILayout.Button(showPreset.boolValue ? "Hide" : "Show", EditorStyles.miniButton, GUILayout.Width(50f)))
						showPreset.boolValue = !showPreset.boolValue;
					GUILayout.EndHorizontal();
					if (showPreset.boolValue)
					{
						EditorGUILayout.Space();
						EditorGUI.BeginChangeCheck();
						dataEditor.OnInspectorGUI();
						//Give us live updates when changing presets in the instpector.
						if (EditorGUI.EndChangeCheck())
						{
							fence.BuildFence();
						}
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
		//private void ValidateData()
		//{
		//	if (_previewRenderUtility == null)
		//	{
		//		_previewRenderUtility = new PreviewRenderUtility();
		//		_previewRenderUtility.m_Camera.transform.position = new Vector3(0, 0, -6);
		//		_previewRenderUtility.m_Camera.transform.rotation = Quaternion.identity;
		//	}
		//	//	Fence fence = (Fence)target as Fence;
		//	//	_targetMeshFilter = fence.meshFilter;
		//	//	_targetMeshRenderer = fence.meshRenderer;//_targetMeshFilter.GetComponent<MeshRenderer>();
		//}
		public override bool HasPreviewGUI()
		{
			//ValidateData();
			return true;
		}
		public override void OnPreviewGUI(Rect r, GUIStyle background)
		{
			// Only render the preview on a repaint event (not continuously)
			if (Event.current.type == EventType.Repaint)
			{
				// Draw info table
				int labelHeight = 15;
				int cellWidth = 80;
				GUI.Label(new Rect(r.x, r.y, cellWidth, labelHeight), "Length:", prevLabel);
				GUI.Label(new Rect(r.x + cellWidth, r.y, cellWidth, labelHeight), totalLengthProp.floatValue + " m", prevLabel);
				GUI.Label(new Rect(r.x, r.y + (labelHeight), cellWidth, labelHeight), "Posts:", prevLabel);
				GUI.Label(new Rect(r.x + cellWidth, r.y + (labelHeight), cellWidth, labelHeight), postCountProp.intValue.ToString(), prevLabel);
				GUI.Label(new Rect(r.x, r.y + (labelHeight*2), cellWidth, labelHeight), "Pickets:", prevLabel);
				GUI.Label(new Rect(r.x + cellWidth, r.y + (labelHeight*2), cellWidth, labelHeight), picketCountProp.intValue.ToString(), prevLabel);
				GUI.Label(new Rect(r.x, r.y + (labelHeight*3), cellWidth, labelHeight), "Verticies:", prevLabel);
				GUI.Label(new Rect(r.x + cellWidth, r.y + (labelHeight * 3), cellWidth, labelHeight), vertCountProp.intValue.ToString(), prevLabel);
				GUI.Label(new Rect(r.x, r.y + (labelHeight*4), cellWidth, labelHeight), "Triangles:", prevLabel);
				GUI.Label(new Rect(r.x + cellWidth, r.y + (labelHeight * 4), cellWidth, labelHeight), triCountProp.intValue.ToString(), prevLabel);

				// Initialize the preview
				//_previewRenderUtility.BeginPreview(r, background);
				// Add mesh to preview
				//_previewRenderUtility.DrawMesh(_targetMeshFilter.sharedMesh, Matrix4x4.identity, _targetMeshRenderer.sharedMaterial, 0);
				// Render the preview
				//_previewRenderUtility.m_Camera.Render();
				// Store the render result.
				//Texture resultRender = _previewRenderUtility.EndPreview();
				// Draw resulting texture into the preview area.
				//GUI.DrawTexture(r, resultRender, ScaleMode.StretchToFill, false);
			}
		}
	}
}