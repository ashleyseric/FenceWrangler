using UnityEngine;
using UnityEditor;

namespace AshleySeric.FenceWrangler
{
	[CustomEditor(typeof(FenceData))]
	public class FenceDataEditor : Editor
	{
		private SerializedProperty fenceType;
		// Conform Mode
		private SerializedProperty conformMode;
		private SerializedProperty lean;
		private SerializedProperty tilt;
		private SerializedProperty picketConform;
		// Posts
		private SerializedProperty segmentLength;
		private SerializedProperty postDimensions;
		private SerializedProperty postJointMode;
		// Pickets
		private SerializedProperty picketDimensions;
		private SerializedProperty picketGap;
		private SerializedProperty picketGroundOffset;
		// Rails
		private SerializedProperty railThickness;
		private SerializedProperty rails;

		private SerializedProperty isDirty;
		private Material _lineMaterial;
		private Material lineMaterial
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
		private Texture2D previewTex;

		public void Awake()
		{
			fenceType = serializedObject.FindProperty("type");
			// Conform Mode
			conformMode = serializedObject.FindProperty("conformMode");
			lean = serializedObject.FindProperty("lean");
			tilt = serializedObject.FindProperty("tilt");
			picketConform = serializedObject.FindProperty("picketConform");
			// Posts
			segmentLength = serializedObject.FindProperty("segmentLength");
			postDimensions = serializedObject.FindProperty("postDimensions");
			postJointMode = serializedObject.FindProperty("postJointMode");
			// Pickets
			picketDimensions = serializedObject.FindProperty("picketDimensions");
			picketGap = serializedObject.FindProperty("picketGap");
			picketGroundOffset = serializedObject.FindProperty("picketGroundOffset");
			// Rails
			railThickness = serializedObject.FindProperty("railThickness");
			rails = serializedObject.FindProperty("rails");

			isDirty = serializedObject.FindProperty("_isDirty");
		}
		public override void OnInspectorGUI()
		{
			serializedObject.Update();
			FenceData data = target as FenceData;
			//DrawPropertiesExcluding(serializedObject, "rails", "m_Script");

			EditorGUILayout.PropertyField(fenceType);
			EditorGUILayout.Space();

			// Conform
			GUILayout.BeginVertical(EditorStyles.helpBox);
			EditorGUILayout.PropertyField(conformMode);
			if (conformMode.enumValueIndex == 1)
			{
				EditorGUILayout.PropertyField(lean);
				EditorGUILayout.PropertyField(tilt);
				if (fenceType.enumValueIndex == 1) //picket
					EditorGUILayout.PropertyField(picketConform);
			}
			EditorGUILayout.Space();
			GUILayout.EndVertical();
			// Posts
			GUILayout.BeginVertical(EditorStyles.helpBox);
			EditorGUILayout.PropertyField(segmentLength);
			EditorGUILayout.PropertyField(postDimensions);
			EditorGUILayout.PropertyField(postJointMode);
			EditorGUILayout.Space();
			GUILayout.EndVertical();
			// Pickets
			if (fenceType.enumValueIndex == 1)
			{
				GUILayout.BeginVertical(EditorStyles.helpBox);
				EditorGUILayout.PropertyField(picketDimensions);
				EditorGUILayout.PropertyField(picketGap);
				EditorGUILayout.PropertyField(picketGroundOffset);
				EditorGUILayout.Space();
				GUILayout.EndVertical();
			}
			// Rails
			GUILayout.BeginVertical(EditorStyles.helpBox);
			EditorGUILayout.PropertyField(railThickness);
			int indent = EditorGUI.indentLevel;
			EditorGUI.indentLevel = 1;
			CustomEditorList.Display(rails);
			EditorGUILayout.Space();
			GUILayout.EndVertical();
			EditorGUI.indentLevel = indent;

			// Preview
			if (isDirty.boolValue)
			{
				UpdatePreviewTexture(256, 256);
				isDirty.boolValue = false;
			}
			GUILayout.Label(""); //Create Dummy label to get the rect from.
			Rect previewTexRect = GUILayoutUtility.GetLastRect();
			previewTexRect.x = (EditorGUIUtility.currentViewWidth * 0.5f) - 128;
			previewTexRect.width = previewTexRect.height = 256;
			GUILayout.Space(previewTexRect.height); // Reserve space for the texture to draw into
			GUI.DrawTexture(previewTexRect, previewTex);
			serializedObject.ApplyModifiedProperties();
		}
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
			lineMaterial.SetPass(0);
			GL.LoadOrtho();
			// Flip the pixel matrix so the zero is the bottom,
			// this saves a lot of maths later on.
			GL.LoadPixelMatrix(0, width, 0, height);
			GL.Begin(GL.QUADS);

			Vector3 _postDim = postDimensions.vector3Value;
			float _totalRealWorldWidth = (segmentLength.floatValue + _postDim.y);
			float _segLength = segmentLength.floatValue;
			Vector3 _picketDim = picketDimensions.vector3Value;
			float _picketGap = picketGap.floatValue;
			float _groundOffset = picketGroundOffset.floatValue;

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
			GL.Color(new Color32(126, 207, 149, 255));
			GL.Vertex3(0, 0, 0);
			GL.Vertex3(0, _postDim.z, 0);
			GL.Vertex3(_postDim.y, _postDim.z, 0);
			GL.Vertex3(_postDim.y, 0, 0);

			GL.Vertex3(_segLength, 0, 0);
			GL.Vertex3(_segLength, _postDim.z, 0);
			GL.Vertex3(_segLength + _postDim.y, _postDim.z, 0);
			GL.Vertex3(_segLength +_postDim.y, 0, 0);
			#endregion

			#region Pickets
			if (fenceType.enumValueIndex == 1)
			{
				GL.Color(new Color32(126, 207, 203, 255));
				float _hPostLength = _postDim.y * 0.5f;
				float _picketOffsetX = _picketDim.y + _picketGap;
				float _doublePicketLength = _picketDim.y * 2;
				for (int i = 1; i < _picketCount; i++)
				{
					GL.Vertex3(_postDim.y + (i * _picketOffsetX), _groundOffset, 0);
					GL.Vertex3(_postDim.y + (i * _picketOffsetX), _groundOffset + _picketDim.z, 0);
					GL.Vertex3(_picketDim.y + (i * _picketOffsetX), _groundOffset + _picketDim.z, 0);
					GL.Vertex3(_picketDim.y + (i * _picketOffsetX), _groundOffset, 0);
				}
			}
			#endregion

			#region Rails
			GL.Color(new Color32(79, 130, 128, 255));
			for (int i = 0; i < rails.arraySize; i++)
			{
				float _start = _postDim.y;
				float _end = _segLength;

				if (postJointMode.enumValueIndex == 1)
				{
					_start = 0;
					_end = _segLength + _postDim.y;
				}
				SerializedProperty r = rails.GetArrayElementAtIndex(i);
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
	}
}
