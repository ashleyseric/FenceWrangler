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
    [CustomEditor(typeof(FenceData))]
    public class FenceDataEditor : UnityEditor.Editor
    {
        #region Static Variables

        protected static bool showPreview = false;

        protected static bool showPosts = false;

        protected static bool showPickets = false;

        protected static bool showConform = false;

        protected static bool showRails = false;

        protected static bool showMaterials = false;

        #endregion

        #region Private Variables

        private SerializedProperty fencePropType;

        // Conform Mode

        private SerializedProperty conformModeProp;

        private SerializedProperty allowObstructionsProp;

        private SerializedProperty leanProp;

        private SerializedProperty tiltProp;

        private SerializedProperty picketConformProp;

        // Posts

        private SerializedProperty segmentLengthProp;

        private SerializedProperty postDimensionsProp;

        private SerializedProperty postJointModeProp;

        // Pickets

        private SerializedProperty picketDimensionsProp;

        private SerializedProperty picketGapProp;

        private SerializedProperty picketGroundOffsetProp;

        // Rails

        private SerializedProperty railThicknessProp;

        private SerializedProperty railsProp;

        // Materials

        private SerializedProperty materialsProp;

        /// <summary>
        /// How many materials does the corresponding FenceType require.
        /// </summary>
        private readonly int[] fenceTypeMaterialCount =
        {
            2,// Farm
            3 // Picket
        };

        private Material _lineMaterial;

        private Texture2D previewTex;

        private ReorderableList materialList;

        private ReorderableList railList;

        #endregion

        #region Properties

        private Material LineMaterial
        {
            get
            {
                if (!_lineMaterial)
                {
                    // Unity has a built-in shader that is useful for drawing
                    // simple colored things.
                    Shader shader = Shader.Find("Hidden/Internal-Colored");
                    _lineMaterial = new Material(shader);
                    _lineMaterial.hideFlags = HideFlags.HideAndDontSave;
                    // Turn on alpha blending
                    _lineMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                    _lineMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                    // Turn backface culling off
                    _lineMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
                    // Turn off depth writes
                    _lineMaterial.SetInt("_ZWrite", 0);
                }
                return _lineMaterial;
            }
        }

        #endregion

        #region Unity Methods

        private void OnEnable()
        {
            fencePropType = serializedObject.FindProperty("type");

            // Conform Mode
            conformModeProp = serializedObject.FindProperty("conformMode");
            allowObstructionsProp = serializedObject.FindProperty("allowObstructions");
            leanProp = serializedObject.FindProperty("lean");
            tiltProp = serializedObject.FindProperty("tilt");
            picketConformProp = serializedObject.FindProperty("picketConform");

            // Posts
            segmentLengthProp = serializedObject.FindProperty("segmentLength");
            postDimensionsProp = serializedObject.FindProperty("postDimensions");
            postJointModeProp = serializedObject.FindProperty("postJointMode");

            // Pickets
            picketDimensionsProp = serializedObject.FindProperty("picketDimensions");
            picketGapProp = serializedObject.FindProperty("picketGap");
            picketGroundOffsetProp = serializedObject.FindProperty("picketGroundOffset");

            // Rails
            railThicknessProp = serializedObject.FindProperty("railThickness");
            railsProp = serializedObject.FindProperty("rails");

            // Materials
            materialsProp = serializedObject.FindProperty("materials");

            // Initialize the preview
            UpdatePreviewTexture(256, 256);

            SetupMaterialReorderableList();

            SetupRailReorderableList();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            FenceData fenceData = target as FenceData;

            // Cache the styles we'll use multiple times times
            GUIStyle foldoutBG = StyleManager.FoldoutBg;

            GUIStyle foldoutBGCollapsed = StyleManager.FoldoutBgCollapsed;

            // Main background panel
            EditorGUILayout.BeginVertical(StyleManager.FoldoutSetBg);

            // Heading
            GUILayout.Label(fenceData.name, StyleManager.FoldoutSetHeading);

            // Preview thumbnail
            Rect prevR = GUILayoutUtility.GetLastRect();
            prevR.x += 6;
            prevR.y += 4;
            prevR.height = 17;
            prevR.width = 17;
            GUI.DrawTexture(prevR, previewTex);

            // Type enum
            EditorGUILayout.PropertyField(fencePropType);

            // Conform
            bool conforming = conformModeProp.intValue != 0;
            Rect toggleConformRect = new Rect();

            using (new GUILayout.VerticalScope(showConform ? foldoutBG : foldoutBGCollapsed))
            {
                if (Foldout(ref showConform, "Conform"))
                {
                    toggleConformRect = GUILayoutUtility.GetLastRect();

                    EditorGUILayout.PropertyField(conformModeProp);

                    if (conformModeProp.enumValueIndex == 1)
                    {
                        EditorGUILayout.PropertyField(allowObstructionsProp);
                        EditorGUILayout.PropertyField(leanProp);
                        EditorGUILayout.PropertyField(tiltProp);
                        if (fencePropType.enumValueIndex == 1) //picket
                            EditorGUILayout.PropertyField(picketConformProp);
                    }
                    GUI.enabled = true;
                    EditorGUILayout.Space();
                }
                else
                    toggleConformRect = GUILayoutUtility.GetLastRect();
            }

            using (var check = new EditorGUI.ChangeCheckScope())
            {
                toggleConformRect.x += 2;
                toggleConformRect.y += 1;
                bool con = EditorGUI.Toggle(toggleConformRect, conforming, StyleManager.toggle);
                if (check.changed)
                {
                    Debug.Log("Changed");
                    conformModeProp.intValue = con ? 1 : 0;
                }
            }

            // Posts
            using (new GUILayout.VerticalScope(showPosts ? foldoutBG : foldoutBGCollapsed))
            {
                if (Foldout(ref showPosts, "Posts"))
                {
                    EditorGUILayout.PropertyField(segmentLengthProp);
                    EditorGUILayout.PropertyField(postDimensionsProp);
                    EditorGUILayout.PropertyField(postJointModeProp);
                    EditorGUILayout.Space();
                }
            }

            // Rails
            using (new GUILayout.VerticalScope(showRails ? foldoutBG : foldoutBGCollapsed))
            {
                if (Foldout(ref showRails, "Rails"))
                {
                    EditorGUILayout.PropertyField(railThicknessProp);
                    EditorGUI.indentLevel += 1;
                    railList.DoLayoutList();
                    EditorGUI.indentLevel -= 1;
                    EditorGUILayout.Space();
                }
            }

            // Pickets
            if (fencePropType.enumValueIndex == 1)
            {
                using (new GUILayout.VerticalScope(showPickets ? foldoutBG : foldoutBGCollapsed))
                {
                    if (Foldout(ref showPickets, "Pickets"))
                    {
                        EditorGUILayout.PropertyField(picketDimensionsProp);
                        EditorGUILayout.PropertyField(picketGapProp);
                        EditorGUILayout.PropertyField(picketGroundOffsetProp);
                        EditorGUILayout.Space();
                    }
                }
            }

            // Materials
            using (new GUILayout.VerticalScope(showMaterials ? foldoutBG : foldoutBGCollapsed))
            {
                if (Foldout(ref showMaterials, "Materials"))
                {
                    //EditorGUI.indentLevel += 1;
                    materialList.DoLayoutList();
                    //EditorGUI.indentLevel -= 1;
                }
            }

            // Preview
            using (new GUILayout.VerticalScope(showPreview ? foldoutBG : foldoutBGCollapsed))
            {
                if (Foldout(ref showPreview, "Preview"))
                {
                    GUILayout.Label(""); //Create Dummy label to get the rect from.
                    Rect previewTexRect = GUILayoutUtility.GetLastRect();
                    previewTexRect.x = (EditorGUIUtility.currentViewWidth * 0.5f) - 128;
                    previewTexRect.width = previewTexRect.height = 256;
                    GUILayout.Space(previewTexRect.height); // Reserve space for the texture to draw into
                    GUI.DrawTexture(previewTexRect, previewTex);
                }
            }

            // If user has edited the gui then update the texture
            if (GUI.changed)
            {
                UpdatePreviewTexture(256, 256);
            }

            serializedObject.ApplyModifiedProperties();
        }

        /// <summary>
        /// Redraw the preview texture into previewTex
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        private void UpdatePreviewTexture(int width, int height)
        {
            // get a temporary RenderTexture //
            RenderTexture renderTexture = RenderTexture.GetTemporary(width, height);

            // set the RenderTexture as global target (that means GL too)
            RenderTexture.active = renderTexture;

            // clear GL //
            GL.Clear(false, true, Color.clear);

            // render GL to the active render texture //
            GL.PushMatrix();
            LineMaterial.SetPass(0);
            GL.LoadOrtho();

            // Flip the pixel matrix so the zero is the bottom,
            // this saves a lot of maths later on.
            GL.LoadPixelMatrix(0, width, 0, height);
            GL.Begin(GL.QUADS);

            Vector3 _postDim = postDimensionsProp.vector3Value;
            float _totalRealWorldWidth = (segmentLengthProp.floatValue + _postDim.y);
            float _segLength = segmentLengthProp.floatValue;
            Vector3 _picketDim = picketDimensionsProp.vector3Value;
            float _picketGap = picketGapProp.floatValue;
            float _groundOffset = picketGroundOffsetProp.floatValue;

            // Find scale for both axis.
            float _scaleX = width / _totalRealWorldWidth;
            float _scaleY = height /
                (
                _postDim.z > (_groundOffset + _picketDim.z) ?
                _postDim.z : (_picketDim.z + _groundOffset)
                );

            // Use whichever scale is smaller to ensure we fit in the frame.
            if (_scaleX > _scaleY)
                _scaleX = _scaleY;
            else
                _scaleY = _scaleX;

            // Apply Scales to everything.
            _postDim = new Vector3(0, _postDim.y * _scaleX, _postDim.z * _scaleY);
            _picketDim = new Vector3(0, _picketDim.y * _scaleX, _picketDim.z * _scaleY);
            _picketGap *= _scaleX;
            _groundOffset *= _scaleY;
            _segLength *= _scaleX;

            float _picketCount = (_segLength - _postDim.y) / (_picketDim.y + _picketGap);

            #region Posts

            GL.Color(new Color32(126, 207, 149, 200));
            GL.Vertex3(0, 0, 0);
            GL.Vertex3(0, _postDim.z, 0);
            GL.Vertex3(_postDim.y, _postDim.z, 0);
            GL.Vertex3(_postDim.y, 0, 0);

            GL.Vertex3(_segLength, 0, 0);
            GL.Vertex3(_segLength, _postDim.z, 0);
            GL.Vertex3(_segLength + _postDim.y, _postDim.z, 0);
            GL.Vertex3(_segLength + _postDim.y, 0, 0);

            #endregion

            #region Pickets
            if (fencePropType.enumValueIndex == 1)
            {
                GL.Color(new Color32(126, 207, 203, 200));
                float _picketOffsetX = _picketDim.y + _picketGap;
                //float _doublePicketLength = _picketDim.y * 2;

                for (int i = 0; i < _picketCount; i++)
                {
                    float offset = i * _picketOffsetX;
                    GL.Vertex3(_postDim.y + offset, _groundOffset, 0);
                    GL.Vertex3(_postDim.y + offset, _groundOffset + _picketDim.z, 0);
                    GL.Vertex3(_postDim.y + offset + _picketDim.y, _groundOffset + _picketDim.z, 0);
                    GL.Vertex3(_postDim.y + offset + _picketDim.y, _groundOffset, 0);
                }
            }
            #endregion

            #region Rails

            GL.Color(new Color32(79, 130, 128, 200));

            for (int i = 0; i < railsProp.arraySize; i++)
            {
                float _start = _postDim.y;
                float _end = _segLength;

                if (postJointModeProp.enumValueIndex == 1)
                {
                    _start = 0;
                    _end = _segLength + _postDim.y;
                }

                SerializedProperty r = railsProp.GetArrayElementAtIndex(i);

                float _rOffset = r.FindPropertyRelative("groundOffset").floatValue * _scaleY;
                float _rWidth = r.FindPropertyRelative("width").floatValue * _scaleY;

                GL.Vertex3(_start, _rOffset, 0);
                GL.Vertex3(_start, _rOffset + _rWidth, 0);
                GL.Vertex3(_end, _rOffset + _rWidth, 0);
                GL.Vertex3(_end, _rOffset, 0);
            }

            #endregion

            GL.End();
            GL.PopMatrix();

            // read the active RenderTexture into a new Texture2D //
            Texture2D newTexture = new Texture2D(width, height);
            newTexture.ReadPixels(new Rect(0, 0, width, height), 0, 0);

            // apply pixels and compress //
            newTexture.Apply(false);
            newTexture.hideFlags = HideFlags.DontSaveInEditor;
            //newTexture.Compress(true);

            // clean up after the party //
            RenderTexture.active = null;
            RenderTexture.ReleaseTemporary(renderTexture);

            // return the goods //
            previewTex = newTexture;
        }

        #endregion

        #region Methods

        private bool Foldout(ref bool foldOut, string title, int indent = 0)
        {
            foldOut = GUILayout.Toggle(foldOut, title, StyleManager.FoldoutHeading, GUILayout.ExpandWidth(true));
            return foldOut;
        }

        private void SetupMaterialReorderableList()
        {
            materialList = new ReorderableList(serializedObject, materialsProp, true, false, true, true);

            materialList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                var element = materialList.serializedProperty.GetArrayElementAtIndex(index);
                rect.y += 2;
                // Material Property
                EditorGUI.PropertyField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), element, GUIContent.none);
            };

            materialList.onCanRemoveCallback = delegate
            {
                // Check if we will have enough materials for this fence type.
                return materialList.count > fenceTypeMaterialCount[(int)fencePropType.enumValueIndex];
            };
        }

        private void SetupRailReorderableList()
        {
            railList = new ReorderableList(serializedObject, railsProp, true, false, true, true);

            railList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                float labWidth = EditorGUIUtility.labelWidth;
                EditorGUIUtility.labelWidth = 80;
                var element = railList.serializedProperty.GetArrayElementAtIndex(index);
                EditorGUI.PropertyField(
                    new Rect(rect.x, rect.y, rect.width * 0.5f, EditorGUIUtility.singleLineHeight),
                    element.FindPropertyRelative("groundOffset"),
                    new GUIContent("Height", "Height scale modifier for this section of fence"));
                EditorGUI.PropertyField(
                    new Rect(rect.x + (rect.width * 0.5f), rect.y, rect.width * 0.5f, EditorGUIUtility.singleLineHeight),
                    element.FindPropertyRelative("width"),
                    new GUIContent("Size", "Dimensions for this rail."));
                EditorGUIUtility.labelWidth = labWidth;
            };
        }

        #endregion
    }
}
