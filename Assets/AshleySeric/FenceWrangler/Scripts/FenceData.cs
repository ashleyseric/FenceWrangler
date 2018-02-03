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
    /// Preset referenced by Fence class to determin how to build the fence.
    /// This should contain data that is relevent to the fence's construction only
    /// and not the fence's behaviour, behaviour is handled by the Fence class itself.
    /// </summary>
    [CreateAssetMenu(fileName = "New Fence", menuName = "Fence Preset", order = 1)]
    public class FenceData : ScriptableObject
    {
        #region Types

        [System.Serializable]
        public struct Rail
        {
            public bool obstructionPreventsPickets;
            public float groundOffset;
            public float width;
            public Rail(float groundOffset = 0.5f, float width = 0.07f, bool obstructionPreventsPickets = false)
            {
                this.obstructionPreventsPickets = obstructionPreventsPickets;
                this.groundOffset = groundOffset;
                this.width = width;
            }
        }

        public enum FenceType { Farm = 0, Picket = 1 }

        public enum ConformType { None = 0, Ground = 1 }

        public enum PostJoint { Inset = 0, Offset = 1 }

        //public enum PicketStyle { arrow = 0, flat = 1 }

        #endregion

        #region Variables

        [SerializeField]
        private FenceType type = 0;

        // Conform

        [SerializeField]
        private ConformType conformMode = 0;

        [Tooltip("Determines if rails should be built through other objects or not.")]
        [SerializeField]
        private bool allowObstructions = false;

        [Range(0, 1)]
        [SerializeField]
        private float lean = 1f;

        [Range(0, 1)]
        [SerializeField]
        private float tilt = 1f;

        [Range(0, 1)]
        [SerializeField]
        private float picketConform = 1f;

        // Posts

        [Range(0.1f, 10f)]
        [SerializeField]
        private float segmentLength = 2f;

        [SerializeField]
        private PostJoint postJointMode = 0;

        [SerializeField]
        private Vector3 postDimensions = new Vector3(0.1f, 0.1f, 1.1f);

        // Pickets

        [SerializeField]
        private Vector3 picketDimensions = new Vector3(0.07f, 0.01f, 1f);

        [SerializeField]
        private float picketGap = 0.1f;

        [SerializeField]
        private float picketGroundOffset = 0.1f;

        // Rails

        [SerializeField]
        private float railThickness = 0.04f;

        [SerializeField]
        private List<Material> materials = new List<Material>(3);

        [SerializeField]
        private Rail[] rails = new Rail[3]
        {
            new Rail(0.2f, 0.1f),
            new Rail(0.5f, 0.1f),
            new Rail(0.8f, 0.1f)
        };

        #endregion

        #region Properties

        public FenceType Type
        {
            get
            {
                return type;
            }

            set
            {
                type = value;
            }
        }

        public ConformType ConformMode
        {
            get
            {
                return conformMode;
            }

            set
            {
                conformMode = value;
            }
        }

        public bool AllowObstructions
        {
            get
            {
                return allowObstructions;
            }

            set
            {
                allowObstructions = value;
            }
        }

        public float Lean
        {
            get
            {
                return lean;
            }

            set
            {
                lean = value;
            }
        }

        public float Tilt
        {
            get
            {
                return tilt;
            }

            set
            {
                tilt = value;
            }
        }

        public float PicketConform
        {
            get
            {
                return picketConform;
            }

            set
            {
                picketConform = value;
            }
        }

        public float SegmentLength
        {
            get
            {
                return segmentLength;
            }

            set
            {
                segmentLength = value;
            }
        }

        public PostJoint PostJointMode
        {
            get
            {
                return postJointMode;
            }

            set
            {
                postJointMode = value;
            }
        }

        public Vector3 PostDimensions
        {
            get
            {
                return postDimensions;
            }

            set
            {
                postDimensions = value;
            }
        }

        public Vector3 PicketDimensions
        {
            get
            {
                return picketDimensions;
            }

            set
            {
                picketDimensions = value;
            }
        }

        public float PicketGap
        {
            get
            {
                return picketGap;
            }

            set
            {
                picketGap = value;
            }
        }

        public float PicketGroundOffset
        {
            get
            {
                return picketGroundOffset;
            }

            set
            {
                picketGroundOffset = value;
            }
        }

        public float RailThickness
        {
            get
            {
                return railThickness;
            }

            set
            {
                railThickness = value;
            }
        }

        public List<Material> Materials
        {
            get
            {
                return materials;
            }

            set
            {
                materials = value;
            }
        }

        public Rail[] Rails
        {
            get
            {
                return rails;
            }

            set
            {
                rails = value;
            }
        }

        #endregion
    }
}