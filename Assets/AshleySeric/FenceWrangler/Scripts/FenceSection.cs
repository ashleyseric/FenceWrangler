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
    [System.Serializable]
    public class FenceSection
    {
        #region Variables

        [SerializeField]
        [HideInInspector]
        private Vector3 cornerPoint = new Vector3(1f, 0, 0);

        [SerializeField]
        [HideInInspector]
        private FenceData data = null;

        [SerializeField]
        [HideInInspector]
        private bool flipFence = false;

        [SerializeField]
        [HideInInspector]
        private float heightModifier = 1f;

        [SerializeField]
        [HideInInspector]
        private float length = 0f;

        #endregion

        #region Properties

        /// <summary>
        /// Point in world space for the beginning of this section of fence.
        /// </summary>
        public Vector3 CornerPoint
        {
            get
            {
                return cornerPoint;
            }

            set
            {
                cornerPoint = value;
            }
        }

        /// <summary>
        /// Fence Data assosciated with this section of fence.
        /// </summary>
        public FenceData Data
        {
            get
            {
                return data;
            }

            set
            {
                data = value;
            }
        }

        /// <summary>
        /// Flip which side the palings are palced on (if applicable).
        /// </summary>
        public bool FlipFence
        {
            get
            {
                return flipFence;
            }

            set
            {
                flipFence = value;
            }
        }

        /// <summary>
        /// Modifies the height for this section of fence.
        /// </summary>
        public float HeightModifier
        {
            get
            {
                return heightModifier;
            }

            set
            {
                heightModifier = value;
            }
        }

        /// <summary>
        /// Length in meters of this section of fence.
        /// </summary>
        public float Length
        {
            get
            {
                return length;
            }

            set
            {
                length = value;
            }
        }

        #endregion

        #region Methods

        public FenceSection(Vector3 position, FenceData data)
        {
            this.CornerPoint = position;
            this.HeightModifier = 1f;
            this.Data = data;
        }

        #endregion
    }
}