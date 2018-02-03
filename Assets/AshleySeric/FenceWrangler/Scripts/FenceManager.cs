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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AshleySeric.FenceWrangler
{
    /// <summary>
    /// Manages fences created at runtime
    /// </summary>
	public class FenceManager : MonoBehaviour
    {
        #region Variables

        private List<Fence> fences = new List<Fence>();

        private LayerMask mask = new LayerMask();

        #endregion

        #region Properties

        public List<Fence> Fences
        {
            get
            {
                return fences;
            }

            set
            {
                fences = value;
            }
        }

        public LayerMask Mask
        {
            get
            {
                return mask;
            }

            set
            {
                mask = value;
            }
        }

        #endregion

        #region Methods

        public void CreateNewFence(FenceData data)
		{
			new GameObject("Fence Creator").AddComponent<FenceCreator>().Initialize(this, data);
		}

		public void AddFence(Fence fence)
		{
			Fences.Add(fence);
		}

        #endregion
    }
}