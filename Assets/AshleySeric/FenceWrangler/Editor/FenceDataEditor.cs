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
			EditorGUILayout.PropertyField(lean);
			EditorGUILayout.PropertyField(tilt);
			if (fenceType.enumValueIndex == 1) //picket
			EditorGUILayout.PropertyField(picketConform);
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
			CustomEditorList.Display(serializedObject.FindProperty("rails"));
			EditorGUILayout.Space();
			GUILayout.EndVertical();
			EditorGUI.indentLevel = indent;

			serializedObject.ApplyModifiedProperties();
		}
	}
}
