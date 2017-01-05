using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Fence", menuName = "Fence Data", order = 1)]
public class FenceData : ScriptableObject {

	public enum FenceType { picket = 0, pool = 1, corrigatedIron = 2 }
	public enum PicketStyle { arrow = 0, flat = 1 }

	public FenceType type = 0;
	public PicketStyle picketStyle;

	[Range(0.1f, 50f)]
	public float segmentLength = 2f;
	public Vector3 postDimensions = new Vector3(0.1f, 1f, 0.1f);
	public int crossBeamCount = 2;

	public Vector3 picketDimensions = new Vector3(0.1f, 2f, 0.1f);
	//void OnValidate()
	//{
		//Debug.Log("Validating");
	//}
}
