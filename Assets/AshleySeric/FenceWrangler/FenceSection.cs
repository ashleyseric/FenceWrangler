using UnityEngine;

namespace AshleySeric.FenceWrangler
{
	[System.Serializable]
	public class FenceSection
	{
		/// <summary>
		/// Point in world space for the beginning of this section of fence.
		/// </summary>
		public Vector3 cornerPoint = new Vector3(1f, 0, 0);
		/// <summary>
		/// Fence Data assosciated with this section of fence.
		/// </summary>
		public FenceData data = null;
		/// <summary>
		/// Flip which side the palings are palced on (if applicable).
		/// </summary>
		public bool flipFence = false;
		/// <summary>
		/// Modifies the height for this section of fence.
		/// </summary>
		public float heightModifier = 1f;
		/// <summary>
		/// Length in meters of this section of fence.
		/// </summary>
		public float length = 0f;
		public FenceSection(Vector3 position, FenceData data)
		{
			this.cornerPoint = position;
			this.heightModifier = 1f;
			this.data = data;
		}
	}
}