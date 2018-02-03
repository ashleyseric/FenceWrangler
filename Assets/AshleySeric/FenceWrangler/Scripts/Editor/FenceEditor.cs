/// Copyright 2018 Ashley Seric [contact@ashleyseric.com]
///
/// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
/// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
/// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, 
/// and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
/// 
/// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
/// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
/// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
/// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR 
/// IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

namespace AshleySeric.FenceWrangler.Editor
{
    [CustomEditor(typeof(Fence))]
    public class FenceEditor : UnityEditor.Editor
    {
        #region Variables

        protected static bool showPreset = true;

        protected static bool showSections = true;

        private int prevSelection = -1;

        private UnityEditor.Editor presetEditor;

        Vector3 lastPosition;

        ReorderableList sectionList;

        GUIStyle prevLabel;

        SerializedProperty vertCountProp;

        SerializedProperty triCountProp;

        SerializedProperty postCountProp;

        SerializedProperty picketCountProp;

        SerializedProperty totalLengthProp;

        SerializedProperty sectionsProp;

        SerializedProperty selectionIndexProp;

        SerializedProperty buildTimeProp;

        //private PreviewRenderUtility _previewRenderUtility;
        //private MeshFilter _targetMeshFilter;
        //private MeshRenderer _targetMeshRenderer;

        #endregion

        #region Unity Methods

        void OnEnable()
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
            sectionsProp = serializedObject.FindProperty("sections");
            selectionIndexProp = serializedObject.FindProperty("selectedSectionIndex");
            buildTimeProp = serializedObject.FindProperty("buildTime");

            SetupSectionsReorderableList();
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

            if (fence.sections.Count == 0 || fence.sections[selectionIndexProp.intValue] == null)
            {
                return;
            }

            Transform fenceTransform = fence.transform;

            Quaternion handleRotation = Tools.pivotRotation == PivotRotation.Local ? fenceTransform.rotation : Quaternion.identity;

            for (int i = 0; i < fence.sections.Count; i++)
            {
                Handles.color = fence.sections[i].Data == null ? Color.red : Color.white;
                EditorGUI.BeginChangeCheck();
                // Convert to world space
                Vector3 _handlePos = fence.sections[i].CornerPoint + pivotPosition;
                fence.sections[i].CornerPoint = Handles.PositionHandle(_handlePos, handleRotation) - pivotPosition;

                if (EditorGUI.EndChangeCheck())
                {
                    fence.BuildFence();
                }

                if (EditorSettings.DrawDebugLines && i < fence.sections.Count - 1)
                {
                    Handles.DrawLine(_handlePos, fence.sections[i + 1].CornerPoint + pivotPosition);
                }

                if (EditorSettings.DrawSceneLabels)
                {
                    Handles.Label(_handlePos, i.ToString(), EditorStyles.boldLabel);
                }
            }

            #region Inputs

            Event e = Event.current;
            switch (e.type)
            {
                case EventType.KeyDown:
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

            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();

                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("Editor Settings", GUILayout.ExpandWidth(false), GUILayout.Width(100), GUILayout.Height(27)))
                    {
                        EditorSettings.Open();
                    }

                    if (GUILayout.Button("Update Fence", GUILayout.ExpandWidth(false), GUILayout.Width(100), GUILayout.Height(27)))
                    {
                        fence.BuildFence();
                    }
                }

                GUILayout.FlexibleSpace();
            }

            EditorGUILayout.Space();

            //hide the sections list so we can manually draw it.
            DrawPropertiesExcluding(serializedObject, "sections", "m_Script");

            // Draw custom sections list
            using (new EditorGUILayout.VerticalScope(StyleManager.FoldoutSetBg))
            {
                EditorGUI.indentLevel += 1;
                showSections = GUILayout.Toggle(showSections, "Sections", StyleManager.FoldoutSetHeading);

                if (showSections)
                {
                    sectionList.DoLayoutList();
                }

                EditorGUI.indentLevel -= 1;
            }

            if (selectionIndexProp.intValue != prevSelection)
            {
                presetEditor = CreateEditor(fence.sections[selectionIndexProp.intValue].Data);
                prevSelection = selectionIndexProp.intValue;
            }
            //EditorGUILayout.EndVertical();

            // draw Fence Preset editor within this editor
            if (fence.sections.Count > 0)
            {
                if (fence.sections[selectionIndexProp.intValue].Data && showPreset)
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
                else
                {
                    EditorGUILayout.HelpBox("No fence data associated with this section. Please add one.", MessageType.Error);
                }
            }

            serializedObject.ApplyModifiedProperties();
        }

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
                GUI.Label(new Rect(r.x, r.y + (labelHeight * 2), cellWidth, labelHeight), "Pickets:", prevLabel);
                GUI.Label(new Rect(r.x + cellWidth, r.y + (labelHeight * 2), cellWidth, labelHeight), picketCountProp.intValue.ToString(), prevLabel);
                GUI.Label(new Rect(r.x, r.y + (labelHeight * 3), cellWidth, labelHeight), "Verticies:", prevLabel);
                GUI.Label(new Rect(r.x + cellWidth, r.y + (labelHeight * 3), cellWidth, labelHeight), vertCountProp.intValue.ToString(), prevLabel);
                GUI.Label(new Rect(r.x, r.y + (labelHeight * 4), cellWidth, labelHeight), "Triangles:", prevLabel);
                GUI.Label(new Rect(r.x + cellWidth, r.y + (labelHeight * 4), cellWidth, labelHeight), triCountProp.intValue.ToString(), prevLabel);
                GUI.Label(new Rect(r.x, r.y + (labelHeight * 5), cellWidth, labelHeight), "Build Time: ", prevLabel);
                GUI.Label(new Rect(r.x + cellWidth, r.y + (labelHeight * 5), cellWidth, labelHeight), buildTimeProp.floatValue.ToString() + "ms", prevLabel);
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

        #endregion

        #region Methods

        private void SetupSectionsReorderableList()
        {
            // Generate Sections Reorderable List.
            sectionList = new ReorderableList(serializedObject, sectionsProp, true, false, true, true);
            sectionList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                var element = sectionList.serializedProperty.GetArrayElementAtIndex(index);
                rect.y += 2;
                EditorGUI.PropertyField(
                new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight * 2),
                element, GUIContent.none);
            };
            sectionList.onSelectCallback = delegate (ReorderableList l)
            {
                selectionIndexProp.intValue = l.index;
            };
            sectionList.elementHeightCallback = delegate { return EditorGUIUtility.singleLineHeight * 2 + 10; };
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

        #endregion
    }
}