using System.Collections;
using UnityEngine;
using UnityEditor;

namespace AshleySeric.FenceWrangler
{
	[CustomEditor(typeof(Fence))]
	public class FenceEditor : Editor
	{
		protected static bool showPreset = true;
		protected static bool showDebugLines = true;

		private int prevSelection = -1;
		private Editor presetEditor;
		Vector3 lastPosition;
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
			lastPosition = (target as Fence).transform.position;
			prevLabel = new GUIStyle(EditorStyles.centeredGreyMiniLabel);
			prevLabel.alignment = TextAnchor.UpperLeft;
			// Setup the SerializedProperties.
			vertCountProp = serializedObject.FindProperty("vertexCount");
			triCountProp = serializedObject.FindProperty("triCount");
			postCountProp = serializedObject.FindProperty("totalPosts");
			picketCountProp = serializedObject.FindProperty("picketCount");
			totalLengthProp = serializedObject.FindProperty("totalLength");
		}
		private void OnSceneGUI()
		{
			Fence fence = target as Fence;
			Vector3 pivotPosition = fence.transform.position;
			if (lastPosition != pivotPosition)
			{
				//Debug.Log(lastPosition);
				fence.BuildFence();
				lastPosition = pivotPosition;
			}
			fence.ClampSelectionIndex();
			if (fence.sections.Count == 0 || fence.sections[fence.selectedSectionIndex] == null) return;
			Transform fenceTransform = fence.transform;

			Quaternion handleRotation = Tools.pivotRotation == PivotRotation.Local ? fenceTransform.rotation : Quaternion.identity;

			for (int i = 0; i < fence.sections.Count; i++)
			{
				Handles.color = fence.sections[i].data == null ? Color.red : Color.white;
				EditorGUI.BeginChangeCheck();
				// Convert to world space
				Vector3 _handlePos = fence.sections[i].cornerPoint + pivotPosition;
				fence.sections[i].cornerPoint = Handles.PositionHandle(_handlePos, handleRotation) - pivotPosition;
				if (EditorGUI.EndChangeCheck())
					fence.BuildFence();
				if (showDebugLines && i < fence.sections.Count - 1)
					Handles.DrawLine(_handlePos, fence.sections[i+1].cornerPoint + pivotPosition);
				Handles.Label(_handlePos, i.ToString(), EditorStyles.boldLabel);
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
								fence.BuildFence();
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

			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			if (GUILayout.Button("Update Fence", GUILayout.ExpandWidth(false), GUILayout.Width(100), GUILayout.Height(27)))
				fence.BuildFence();
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();
			EditorGUILayout.Space();
			showDebugLines = GUILayout.Toggle(showDebugLines, "Show Debug Lines", EditorStyles.toggle);
			//hide the sections list so we can manually draw it.
			DrawPropertiesExcluding(serializedObject, "sections", "m_Script");

			// Draw custom sections list
			EditorGUILayout.BeginVertical(EditorStyles.helpBox);
			EditorGUI.indentLevel += 1;
			fence.selectedSectionIndex = CustomEditorList.DisplayAndGetIndex(serializedObject.FindProperty("sections"), fence.selectedSectionIndex, false, true, "Edit");
			EditorGUI.indentLevel -= 1;
			if (fence.selectedSectionIndex != prevSelection)
			{
				presetEditor = Editor.CreateEditor(fence.sections[fence.selectedSectionIndex].data);
				prevSelection = fence.selectedSectionIndex;
			}
			EditorGUILayout.EndVertical();

			// draw Fence Preset editor within this editor
			if (fence.sections.Count > 0)
			{
				if (fence.sections[fence.selectedSectionIndex].data)
				{
					
					GUILayout.BeginVertical(EditorStyles.helpBox);
					EditorGUI.indentLevel += 1;
					showPreset = EditorGUILayout.Foldout(showPreset, fence.sections[fence.selectedSectionIndex].data.name + " (Preset)", true);
					EditorGUI.indentLevel -= 1;
					if (showPreset)
					{
						EditorGUILayout.Space();
						EditorGUI.BeginChangeCheck();
						presetEditor.OnInspectorGUI();
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