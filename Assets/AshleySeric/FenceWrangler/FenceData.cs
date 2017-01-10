using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Fence", menuName = "Fence Data", order = 1)]
public class FenceData : ScriptableObject {

	public enum FenceType { picket = 0, pool = 1, corrigatedIron = 2 }
	public enum ConformMode { none = 0, ground = 1 }
	public enum PicketStyle { arrow = 0, flat = 1 }

	public FenceType type = 0;
	public PicketStyle picketStyle;

	[Range(0, 1)]
	public float lean = 1f;
	[Range(0, 1)]
	public float tilt = 1f;
	[Range(0.1f, 50f)]
	public float segmentLength = 2f;
	public Vector3 postDimensions = new Vector3(0.1f, 1f, 0.1f);
	public ConformMode conform = 0;

	[System.Serializable]
	public struct Rail
	{
		public float heightOffset;
		public Vector2 dimensions;
		public Rail(float heightOffset, float width, float height)
		{
			this.heightOffset = heightOffset;
			this.dimensions = new Vector2(width, height);
		}
	}
	public Rail[] rails = new Rail[3] 
	{
		new Rail(0.1f, 0.1f, 0.1f),
		new Rail(0.2f, 0.1f, 0.1f),
		new Rail(0.3f, 0.1f, 0.1f)
	};
	public Vector3 picketDimensions = new Vector3(0.1f, 2f, 0.1f);


	//void OnValidate()
	//{
		//Debug.Log("Validating");
	//}
}
