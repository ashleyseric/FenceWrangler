using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AshleySeric.FenceWrangler
{
	public class FenceCreator : MonoBehaviour
	{
		GameObject obj;
		Fence fence;
		FenceData data;
		Vector3 startPos = Vector3.zero;
		Vector3 endPos = Vector3.zero;
		FenceManager manager = null;
		FenceSection endSection = null;
		Vector3 mousePosition = Vector3.zero;
		bool initialized = false;

		public void Initialize(FenceManager _manager, FenceData _data)
		{
			obj = gameObject;
			manager = _manager;
			fence = obj.AddComponent<Fence>();
			fence.conformMask = manager.mask;
			fence.AddSection(Vector3.zero, _data);
			endSection = fence.sections[0];
		}

		public void SetEndPos(Vector3 _pos)
		{
			endSection.cornerPoint = endPos = _pos;
		}
		void AddSection()
		{
			fence.AddSection(endSection.cornerPoint, endSection.data);
			endSection = fence.sections[fence.sections.Count - 1];
		}
		void FinishFence()
		{
			manager.AddFence(fence);
			obj.name = "Fence";
			// Only one section of fence so we cancel building the fence.
			if (fence.sections.Count == 2)
			{
				GameObject.Destroy(this.gameObject);
			}
			fence.RemoveLastSection();
			fence.BuildFence();
			Destroy(this);
		}
		bool GetMousePosition()
		{
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			RaycastHit hit;
			if (Physics.Raycast(ray, out hit, Mathf.Infinity))
			{
				mousePosition = hit.point;
				return true;
			}
			return false;
		}
		void Update()
		{
			if (GetMousePosition())
			{
				SetEndPos(mousePosition);
				fence.BuildFence();
			}
			if (Input.GetMouseButtonUp(0)) // Left click
			{
				AddSection();
			}
			else if (Input.GetMouseButtonUp(1)) // Right click
			{
				FinishFence();
			}
		}
	}
}