using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AshleySeric.FenceWrangler
{
	/// <summary>
	/// Preset referenced by Fence class to determin how to build the fence.
	/// This should contain data that is relevent to the fence's construction only
	/// and not the fence's behaviour, behaviour is handled by the Fence class itself.
	/// </summary>
	[CreateAssetMenu(fileName = "New Fence", menuName = "Fence Data", order = 1)]
	public class FenceData : ScriptableObject
	{

		public enum FenceType { farm = 0, picket = 1 }
		public enum ConformMode { none = 0, ground = 1 }
		public enum PostJoint { inset = 0, offset = 1 }
		//public enum PicketStyle { arrow = 0, flat = 1 }

		public FenceType type = 0;
		[Header("Conform")]
		public ConformMode conformMode = 0;
		[Range(0, 1)]
		public float lean = 1f;
		[Range(0, 1)]
		public float tilt = 1f;
		[Range(0, 1)]
		public float picketConform = 1f;

		[Header("Posts")]
		[Range(0.1f, 10f)]
		public float segmentLength = 2f;
		public PostJoint postJointMode = 0;
		public Vector3 postDimensions = new Vector3(0.1f, 1f, 0.1f);

		[Header("Pickets")]
		public Vector3 picketDimensions = new Vector3(0.1f, 2f, 0.1f);
		public float picketGap = 0.1f;
		public float picketGroundOffset = 0.1f;

		[Header("Rails")]
		public float railThickness = 0.04f;
		[System.Serializable]
		public struct Rail
		{
			public float groundOffset;
			public float width;
			public Rail(float groundOffset, float width)
			{
				this.groundOffset = groundOffset;
				this.width = width;
			}
		}
		public Rail[] rails = new Rail[3]
		{
		new Rail(0.1f, 0.1f),
		new Rail(0.2f, 0.1f),
		new Rail(0.3f, 0.1f)
		};

		[SerializeField]
		private bool _isDirty = false;
		void OnValidate()
		{
			_isDirty = true;
		}
	}
}