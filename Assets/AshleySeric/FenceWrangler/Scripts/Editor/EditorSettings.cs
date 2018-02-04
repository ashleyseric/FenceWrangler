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

using UnityEditor;
using UnityEngine;

namespace AshleySeric.FenceWrangler.Editor
{
    /// <summary>
    /// Window for editor settings.
    /// </summary>
    class EditorSettings : UnityEditor.EditorWindow
    {
        #region Variables

        private bool drawDebugLines;

        private bool drawSceneLabels;

        private bool useEasyHandles;

        #endregion

        #region Properties

        /// <summary>
        /// Draw lines between fence sections in the scene view.
        /// Referenced from EditorPrefs
        /// </summary>
        public static bool DrawDebugLines
        {
            get
            {
                return EditorPrefs.GetBool("AshleySeric.Fencer.DrawDebugLines");
            }

            set
            {
                EditorPrefs.SetBool("AshleySeric.Fencer.DrawDebugLines", value);
            }
        }

        /// <summary>
        /// Draw section labels in the scene view
        /// Referenced from EditorPrefs
        /// </summary>
        public static bool DrawSceneLabels
        {
            get
            {
                return EditorPrefs.GetBool("AshleySeric.Fencer.DrawSceneLabels");
            }

            set
            {
                EditorPrefs.SetBool("AshleySeric.Fencer.DrawSceneLabels", value);
            }
        }

        /// <summary>
        /// Use simplified handles that automatically conform to the terrain
        /// </summary>
        public static bool UseEasyHandles
        {
            get
            {
                if (!EditorPrefs.HasKey("AshleySeric.Fencer.UseEasyHandles"))
                {
                    // Default to true
                    EditorPrefs.SetBool("AshleySeric.Fencer.UseEasyHandles", true);
                }

                return EditorPrefs.GetBool("AshleySeric.Fencer.UseEasyHandles");
            }

            set
            {
                EditorPrefs.SetBool("AshleySeric.Fencer.UseEasyHandles", value);
            }
        }

        #endregion

        #region Unity Methods

        void OnFocus()
        {
            drawDebugLines = DrawDebugLines;
            drawSceneLabels = DrawSceneLabels;
            useEasyHandles = UseEasyHandles;
        }

        void OnGUI()
        {
            EditorGUILayout.LabelField("Scene View", EditorStyles.boldLabel);

            using (var check = new EditorGUI.ChangeCheckScope())
            {
                drawDebugLines = EditorGUILayout.Toggle(new GUIContent("Draw Debug Lines", "Draw lines between fence sections in the scene view."), drawDebugLines);

                if (check.changed)
                {
                    DrawDebugLines = drawDebugLines;
                }
            }

            using (var check = new EditorGUI.ChangeCheckScope())
            {
                drawSceneLabels = EditorGUILayout.Toggle(new GUIContent("Draw Scene Labels", "Draw section labels in the scene view."), drawSceneLabels);

                if (check.changed)
                {
                    DrawSceneLabels = drawSceneLabels;
                }
            }

            using (var check = new EditorGUI.ChangeCheckScope())
            {
                useEasyHandles = EditorGUILayout.Toggle(new GUIContent("Use Easy Handles", "Draw section labels in the scene view."), useEasyHandles);

                if (check.changed)
                {
                    UseEasyHandles = useEasyHandles;
                }
            }
        }

        #endregion

        #region Methods

        [MenuItem("Window/Fencer/Editor Settings")]
        public static void Open()
        {
            // Get existing open window or if none, make a new one:
            EditorSettings window = (EditorSettings)GetWindow(typeof(EditorSettings), true, "Fencer Editor Settings", true);
            window.minSize = window.maxSize = new Vector2(180, 80);
            window.Show();
        }

        #endregion
    }
}
