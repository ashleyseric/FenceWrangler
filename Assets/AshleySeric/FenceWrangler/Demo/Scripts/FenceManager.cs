using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AshleySeric.FenceWrangler {
	public class FenceManager : MonoBehaviour {

		public List<Fence> fences = new List<Fence>();
		public LayerMask mask = new LayerMask();

		public void CreateNewFence(FenceData _data)
		{
			new GameObject("Fence Creator").AddComponent<FenceCreator>().Initialize(this, _data);
		}
		public void AddFence(Fence _fence)
		{
			fences.Add(_fence);
		}
	}
}