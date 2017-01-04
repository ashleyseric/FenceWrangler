using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Fence", menuName = "Fence Data", order = 1)]
public class FenceData : ScriptableObject {

	[Range(0.1f, 50f)]
	public float segmentLength = 2f;
	public Vector3 postDimensions = new Vector3(0.1f, 1f, 0.1f);

	void OnValidate()
	{
		Debug.Log("Validating");
	}
}
