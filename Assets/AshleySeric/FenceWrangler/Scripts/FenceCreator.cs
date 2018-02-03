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

using UnityEngine;

namespace AshleySeric.FenceWrangler
{
    /// <summary>
    /// Creates fences at runtime
    /// </summary>
	public class FenceCreator : MonoBehaviour
    {
        #region Member Variables

        private GameObject _gameObject;

        private Fence fence;

        private FenceData data;

        private Vector3 startPos = Vector3.zero;

        private Vector3 endPos = Vector3.zero;

        private FenceManager manager = null;

        private FenceSection endSection = null;

        private Vector3 mousePosition = Vector3.zero;

        private bool initialized = false;

        #endregion

        #region Public Methods

        public void Initialize(FenceManager manager, FenceData data)
        {
            _gameObject = gameObject;
            this.manager = manager;
            fence = _gameObject.AddComponent<Fence>();
            fence.ConformMask = this.manager.Mask;
            fence.AddSection(Vector3.zero, data);
            endSection = fence.sections[0];
        }

        public void SetEndPos(Vector3 pos)
        {
            endSection.CornerPoint = endPos = pos;
        }

        #endregion

        #region Private Methods

        private void AddSection()
        {
            fence.AddSection(endSection.CornerPoint, endSection.Data);
            endSection = fence.sections[fence.sections.Count - 1];
        }

        private void FinishFence()
        {
            manager.AddFence(fence);
            _gameObject.name = "Fence";

            // If there is only one section of fence, we'll cancel building the fence.
            if (fence.sections.Count == 2)
            {
                GameObject.Destroy(this.gameObject);
            }

            fence.RemoveLastSection();
            fence.BuildFence();
            Destroy(this);
        }

        private bool GetMousePosition()
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

        private void Update()
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

        #endregion
    }
}