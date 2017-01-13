using System.Collections.Generic;
using UnityEngine;

namespace AshleySeric.FenceWrangler
{
	[System.Serializable]
	public class FenceSection
	{
		/// <summary>
		/// Point in space for the beginning of this section of fence.
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

	public class Fence : MonoBehaviour
	{
		[HideInInspector]
		public int selectedSectionIndex = 0;
		public List<FenceSection> sections = new List<FenceSection>
		{
			new FenceSection(new Vector3(1f, 0, 0), null),
			new FenceSection(new Vector3(2f, 0, 1f), null)
		};
		private MeshFilter _mf;
		private MeshRenderer _mr;
		Mesh mesh = null;
		MeshFilter meshFilter
		{
			get
			{
				if (_mf == null)
				{
					if (gameObject.GetComponent<MeshFilter>() == null)
						_mf = gameObject.AddComponent<MeshFilter>();
					else
						_mf = gameObject.GetComponent<MeshFilter>();
				}
				if (_mr == null)
				{
					if (gameObject.GetComponent<MeshRenderer>() == null)
						_mr = gameObject.AddComponent<MeshRenderer>();
					else
						_mr = gameObject.GetComponent<MeshRenderer>();
				}
				return _mf;
			}
		}
		List<Vector3> verts = new List<Vector3>();
		List<int> tris = new List<int>();
		List<Vector3> normals = new List<Vector3>();
		List<Vector2> uvs = new List<Vector2>();

		//Variables to be shown in inspector but not used.
		[SerializeField]
		[HideInInspector]
		int vertexCount = 0;
		[SerializeField]
		[HideInInspector]
		int triCount = 0;
		[SerializeField]
		[HideInInspector]
		int postCount = 0;
		[SerializeField]
		[HideInInspector]
		float totalLength = 0f;
		public LayerMask conformMask;

		public void Start()
		{
			BuildFence();
		}
		public void BuildFence()
		{
			if (sections.Count < 2)
				return;
			if (mesh == null)
				mesh = new Mesh();

			totalLength = 0f;
			postCount = 0;

			mesh.Clear();
			verts = new List<Vector3>();
			tris = new List<int>();
			normals = new List<Vector3>();
			uvs = new List<Vector2>();
			int vertexTotal = 0;
			Quaternion sectionRot = Quaternion.identity;

			for (int i = 0; i < sections.Count-1; i++)
			{
				FenceSection fromSec = sections[i];
				FenceSection toSec = sections[i + 1];
				if (fromSec.data == null) continue;
				float sectionLength = Vector3.Distance(fromSec.cornerPoint, toSec.cornerPoint);
				if (sectionLength <= Mathf.Epsilon) break;
				int numberOfPosts = Mathf.CeilToInt(sectionLength / fromSec.data.segmentLength);
				float segmentLength = sectionLength / numberOfPosts;

				#region Don't Conform
				if (fromSec.data.conformMode == FenceData.ConformMode.none)
				{
					Vector3 lookDir = toSec.cornerPoint - fromSec.cornerPoint;
					lookDir.y = 0;
					sectionRot = Quaternion.LookRotation(lookDir, Vector3.up);	
					Quaternion postRot = Quaternion.FromToRotation(Vector3.left, Vector3.right) * Quaternion.FromToRotation(sectionRot * Vector3.forward, sectionRot * Vector3.up) * sectionRot;

					for (int j = 0; j < (int)numberOfPosts + 1; j++)
					{
						Vector3 postPos = Vector3.Lerp(
								fromSec.cornerPoint, toSec.cornerPoint,
								(float)j / numberOfPosts) +
								new Vector3(0f, fromSec.data.postDimensions.z * 0.5f, 0f);
						// Add post
						AddCube(
							postPos,
							postRot,
							fromSec.data.postDimensions,
							ref vertexTotal
							);
						postCount++;
					}
					postCount++;
					// If the fence is guaranteed to be strait we add the cross 
					// rails for the whole length in one go to save geometry
					//float frontOffset = fromSec.flipFence ? 1f : -1f;
					//foreach (FenceData.Rail rail in fromSec.data.rails)
					//{
					//	AddCube(
					//		fromSec.cornerPoint +
					//		((((fromSec.data.postDimensions.x * 0.5f) + (rail.dimensions.x * 0.5f)) * frontOffset) * targetLeft) +
					//		(sectionLength * 0.5f * targetForward) + (targetUp * rail.heightOffset),
					//		sectionRot,
					//			new Vector3(rail.dimensions.x, rail.dimensions.y, sectionLength),
					//		ref vertexTotal
					//		);
					//}

					foreach (FenceData.Rail rail in fromSec.data.rails)
					{
						float jointOffset = fromSec.data.postJointMode == FenceData.PostJoint.inset ? 0f : (fromSec.data.postDimensions.x * 0.5f) + (rail.dimensions.x * 0.5f);
						jointOffset *= fromSec.flipFence ? 1f : -1f;

						Vector3 railStart = fromSec.cornerPoint + (rail.heightOffset * Vector3.up);
						Vector3 railEnd = toSec.cornerPoint + (rail.heightOffset * Vector3.up);
						float railLength = Vector3.Distance(railStart, railEnd);
						Quaternion railRot = Quaternion.LookRotation(railEnd - railStart, Vector3.up);
						Vector3 railPos = 
							railStart + 
							(railRot * Vector3.forward * railLength * 0.5f) + //offset allong the length so the center point of the cube is halfway down the fence section.
							jointOffset * (sectionRot * Vector3.left);

						AddCube(
							railPos,
							railRot,
							new Vector3(rail.dimensions.x, rail.dimensions.y, railLength),
							ref vertexTotal
							);
					}
				}
				#endregion
				
				#region Conform
				if (fromSec.data.conformMode == FenceData.ConformMode.ground)
				{
					// we add 2 to the number of posts here since we want to
					// want to be sure we get an end post as well.
					Vector3[] postPositions = new Vector3[numberOfPosts+2];
					Vector3[] postAngles = new Vector3[numberOfPosts+2];
					for (int j = 0; j < postPositions.Length; j++)
					{
						Vector3 castPoint = Vector3.Lerp(
								fromSec.cornerPoint,
								toSec.cornerPoint,
								(float)j / numberOfPosts
								);
						RaycastHit hit;
						if (Physics.Raycast(castPoint + (Vector3.up * 1000), Vector3.down, out hit, 2000, conformMask))
						{
							postPositions[j] = hit.point;
							postAngles[j] = hit.normal;
						}
						else
						{
							postPositions[j] = castPoint;
							postAngles[j] = Vector3.up;
						}
					}
					for (int j = 0; j < postPositions.Length - 2; j++)
					{
						Vector3 cpPos = postPositions[j];
						Vector3 npPos = postPositions[j + 1];

						// Find post rotations conforming to the ground normal.
						Quaternion cpNormRot = Quaternion.LookRotation(((cpPos + (postAngles[j] * 2)) - cpPos), npPos - cpPos);
						Quaternion npNormRot = Quaternion.LookRotation(((npPos + (postAngles[j+1] * 2)) - npPos), postPositions[j+2] - npPos);

						// Find non-conforming post rotations.
						Vector3 cpLV = npPos - cpPos;
						cpLV.y = cpLV.y * -fromSec.data.tilt;
						Quaternion cpLR = Quaternion.identity;
						if (cpLV.sqrMagnitude > 0)
							cpLR = Quaternion.LookRotation(cpLV, Vector3.up);
						Quaternion cpStraitAngle = Quaternion.FromToRotation(Vector3.left, Vector3.right) * Quaternion.FromToRotation(cpLR * Vector3.forward, cpLR * Vector3.up) * cpLR;

						Vector3 npLV = postPositions[j + 2] - npPos;
						npLV.y = npLV.y * -fromSec.data.tilt;
						Quaternion npLR = Quaternion.identity;
						if (npLV.sqrMagnitude > 0)
							npLR = Quaternion.LookRotation(npLV, Vector3.up);
						Quaternion npStraitAngle = Quaternion.FromToRotation(Vector3.left, Vector3.right) * Quaternion.FromToRotation(npLR * Vector3.forward, npLR * Vector3.up) * npLR;

						// Lerp post rotations depending on straitness value.
						Quaternion cpRot = Quaternion.Slerp(cpNormRot, cpStraitAngle, 1f - fromSec.data.lean);
						Quaternion npRot = Quaternion.Slerp(npNormRot, npStraitAngle, 1f - fromSec.data.lean);

						// Direction vectors for posts facing upright.
						Vector3 cpDir = (cpRot * Vector3.forward);
						Vector3 npDir = (npRot * Vector3.forward);

						// ####
						// cpNormRot IS GOOD FOR THE ANGLE 
						// BUT cpStraitAngle IS NOT ALIGNING WITH THE FENCE DIRECTION.

						// Add post
						AddCube(
							cpPos + (cpDir * fromSec.data.postDimensions.z * 0.5f),
							cpRot,
							fromSec.data.postDimensions,
							ref vertexTotal
							);

						postCount++;
						//skip the last 3 iteration as we only want to build the end post and not the rails.
						if (j != postPositions.Length - 2)
						{
							//if we conform to the ground we want the rails to be built for each fence section.
							//if (fromSec.data.conform == FenceData.ConformMode.ground)
							{
								foreach (FenceData.Rail rail in fromSec.data.rails)
								{
									float jointOffset = fromSec.data.postJointMode == FenceData.PostJoint.inset ? 0f : (fromSec.data.postDimensions.x * 0.5f) + (rail.dimensions.x * 0.5f);
									jointOffset *= fromSec.flipFence ? 1f : -1f;

									Vector3 railStart = 
										cpPos + 
										(rail.heightOffset * cpDir) +
										(fromSec.data.postDimensions.y * 0.5f * (cpRot * Vector3.up)); //places the start point in the center of the current post.
									Vector3 railEnd = 
										npPos + 
										(rail.heightOffset * npDir) + 
										(fromSec.data.postDimensions.y * 0.5f * (npRot * Vector3.up)); //places the end point in the center of the next post.

									float railLength = Vector3.Distance(railStart, railEnd);
									Quaternion railRot = Quaternion.LookRotation(railEnd - railStart, cpDir);
									Vector3 railPos = 
										railStart + 
										(railRot * Vector3.forward * railLength * 0.5f) +
										(jointOffset * (cpRot * Vector3.left));

									AddCube(
										railPos,
										railRot,
										new Vector3(rail.dimensions.x, rail.dimensions.y, railLength),
										ref vertexTotal
										);
								}
							}
						}
					}
				}
				#endregion
				totalLength += sectionLength;
			}

			mesh.name = "Dynamic Fence";
			mesh.SetVertices(verts);
			mesh.SetUVs(0, uvs);
			mesh.SetTriangles(tris, 0);
			mesh.SetNormals(normals);

			meshFilter.mesh = mesh;
			mesh.RecalculateNormals();
			mesh.RecalculateBounds();

			vertexCount = mesh.vertexCount;
			triCount = mesh.triangles.Length / 3;
		}
		/// <summary>
		/// Call this before using the selection index for lookups as it may now be out of range.
		/// </summary>
		public void ClampSelectionIndex()
		{
			selectedSectionIndex = Mathf.Clamp(selectedSectionIndex, 0, sections.Count - 1);
		}
		private void AddCube(Vector3 _pos, Quaternion orient, Vector3 dimensions, ref int vertexTotal)
		{
			float height = dimensions.z;
			float width = dimensions.x;
			float depth = dimensions.y;

			Vector3 up =	orient * Vector3.up;
			Vector3 down =	orient * Vector3.down;
			Vector3 front = orient * Vector3.forward;
			Vector3 back =	orient * Vector3.back;
			Vector3 left =	orient * Vector3.left;
			Vector3 right = orient * Vector3.right;

			Vector3 leftPos	 = (left * (width * 0.5f));
			Vector3 rightPos = (right * (width * 0.5f));
			Vector3 frontPos = (front * (height * 0.5f));
			Vector3 backPos  = (back * (height * 0.5f));
			Vector3 upPos	 = (up * depth);

			#region Vertices
			Vector3 p0 = _pos + backPos + leftPos + upPos;
			Vector3 p1 = _pos + frontPos + leftPos + upPos;
			Vector3 p2 = _pos + frontPos + leftPos;
			Vector3 p3 = _pos + backPos + leftPos;

			Vector3 p4 = _pos + backPos + rightPos + upPos;
			Vector3 p5 = _pos + frontPos + rightPos + upPos;
			Vector3 p6 = _pos + frontPos + rightPos;
			Vector3 p7 = _pos + backPos + rightPos;

			// same vertices added multiple times to create sharp edges.
			Vector3[] _verts = new Vector3[]
			{
				// Bottom
				p0, p1, p2, p3,
 
				// Left
				p7, p4, p0, p3,
 
				// Front
				p4, p5, p1, p0,
 
				// Back
				p6, p7, p3, p2,
 
				// Right
				p5, p6, p2, p1,
 
				// Top
				p7, p6, p5, p4
			};
			#endregion

			#region Normales
			
 
			Vector3[] _normals = new Vector3[]
			{
				// Bottom
				down, down, down, down,
 
				// Left
				left, left, left, left,
 
				// Front
				front, front, front, front,
 
				// Back
				back, back, back, back,
 
				// Right
				right, right, right, right,
 
				// Top
				up, up, up, up
			};
			#endregion	
 
			#region UVs
			Vector2 _00 = new Vector2( 0f, 0f );
			float w = width;
			float d = depth;
			float h = height;


			//The uv's dont acurately reflect the correct faces
			//Key:
			//left = bottom
			//front = front
			//bottom = right
			//back = back
			//right = top
			//top = right

			Vector2 _d0 = new Vector2(d, 0);
			Vector2 _0d = new Vector2(0, d);
			Vector2 _h0 = new Vector2(h, 0);
			Vector2 _0h = new Vector2(0, h);
			Vector2 _hd = new Vector2(h, d);
			Vector2 _dh = new Vector2(d, h);
			Vector2 _w0 = new Vector2(w, 0);
			Vector2 _0w = new Vector2(0, w);
			Vector2 _wh = new Vector2(w, h);
			Vector2 _wd = new Vector2(w, d);

			Vector2[] _uvs = new Vector2[]
			{
				// Bottom
				_d0,
				_dh,
				_0h,
				_00,
				
				// Left
				_w0,
				_wd,
				_0d,
				_00,
				
				// Front
				_w0,
				_wh,
				_0h,
				_00,
				
				// Back
				_0h,
				_00,
				_w0,
				_wh,
				
				// Right
				_0d,
				_00,
				_w0,
				_wd,

				// Top
				_d0,
				_dh,
				_0h,
				_00,
			};
			#endregion

			//This creates our offset
			//iter *= 24;
			#region Triangles
			int[] _tris = new int[]
			{
				// Bottom
				vertexTotal + 3, vertexTotal + 1, vertexTotal + 0,
				vertexTotal + 3, vertexTotal + 2, vertexTotal + 1,			
 
				// Left
				vertexTotal + 3 + 4 * 1, vertexTotal + 1 + 4 * 1, vertexTotal + 0 + 4 * 1,
				vertexTotal + 3 + 4 * 1, vertexTotal + 2 + 4 * 1, vertexTotal + 1 + 4 * 1,
 
				// Front
				vertexTotal + 3 + 4 * 2, vertexTotal + 1 + 4 * 2, vertexTotal + 0 + 4 * 2,
				vertexTotal + 3 + 4 * 2, vertexTotal + 2 + 4 * 2, vertexTotal + 1 + 4 * 2,
 
				// Back
				vertexTotal + 3 + 4 * 3, vertexTotal + 1 + 4 * 3, vertexTotal + 0 + 4 * 3,
				vertexTotal + 3 + 4 * 3, vertexTotal + 2 + 4 * 3, vertexTotal + 1 + 4 * 3,
 
				// Right
				vertexTotal + 3 + 4 * 4, vertexTotal + 1 + 4 * 4, vertexTotal + 0 + 4 * 4,
				vertexTotal + 3 + 4 * 4, vertexTotal + 2 + 4 * 4, vertexTotal + 1 + 4 * 4,
 
				// Top
				vertexTotal + 3 + 4 * 5, vertexTotal + 1 + 4 * 5, vertexTotal + 0 + 4 * 5,
				vertexTotal + 3 + 4 * 5, vertexTotal + 2 + 4 * 5, vertexTotal + 1 + 4 * 5,

				//// Bottom
				//iter + 3, iter + 1, iter + 0,
				//iter + 3, iter + 2, iter + 1,	
 
				//// Left
				//iter + 7, iter + 5, iter + 4,
				//iter + 7, iter + 6, iter + 5,
 
				//// Front
				//iter + 14, iter + 10, iter + 8,
				//iter + 14, iter + 12, iter + 10,
 
				//// Back
				//iter + 21, iter + 15, iter + 12,
				//iter + 21, iter + 18, iter + 15,
 
				//// Right
				//iter + 28, iter + 20, iter + 16,
				//iter + 28, iter + 24, iter + 20,
 
				//// Top
				//iter + 35, iter + 25, iter + 20,
				//iter + 35, iter + 30, iter + 25,
 
			};
			#endregion

			vertexTotal += 24;

			verts.AddRange(_verts);
			tris.AddRange(_tris);
			normals.AddRange(_normals);
			uvs.AddRange(_uvs);
		}

		public void AddSection(Vector3 position)
		{
			sections.Add(new FenceSection(position, sections.Count > 0 ? sections[sections.Count - 1].data : null));
			BuildFence();
		}
		void OnValidate()
		{
			BuildFence();
		}
		private void OnDrawGizmos()
		{
			Gizmos.color = Color.green;
			//for (int i = 0; i < verts.Count; i++)
			//{
			//	Gizmos.DrawSphere(verts[i], 0.03f);
			//}
		}
	}
}
