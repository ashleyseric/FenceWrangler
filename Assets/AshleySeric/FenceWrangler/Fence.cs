using System.Collections.Generic;
using UnityEngine;
using System.Linq;
#if UNITY_EDITOR
using System.Diagnostics;
#endif

namespace AshleySeric.FenceWrangler
{
	public class Fence : MonoBehaviour
	{
		#if UNITY_EDITOR
		[SerializeField][HideInInspector] private int selectedSectionIndex = 0;
		[SerializeField][HideInInspector] private float buildTime = -1f;
        #endif

        public List<FenceSection> sections = new List<FenceSection>();
		private Transform _trans;
		private MeshFilter _mf;
		private MeshRenderer _mr;
		new public Transform transform
		{
			get
			{
				if (!_trans)
				{
					_trans = GetComponent<Transform>();
				}
				return _trans;
			}
		}
		public MeshFilter meshFilter
		{
			get
			{
				CheckRenderers();
				return _mf;
			}
		}
		public MeshRenderer meshRenderer
		{
			get
			{
				CheckRenderers();
				return _mr;
			}
		}
		private Mesh mesh = null;
		private List<Vector3> verts = new List<Vector3>();
		/// <summary>
		/// List of submesh tri lists.
		/// </summary>
		private List<List<int>> submeshTriangles = new List<List<int>>();
		private List<Vector3> normals = new List<Vector3>();
		private List<Vector2> uvs = new List<Vector2>();

		//Variables to be shown in inspector but not used in any calcs
		[SerializeField]
		[HideInInspector]
		int vertexCount = 0;
		[SerializeField]
		[HideInInspector]
		int triCount = 0;
		[SerializeField]
		[HideInInspector]
		int totalPosts = 0;
		[SerializeField]
		[HideInInspector]
		int picketCount = 0;
		[SerializeField]
		[HideInInspector]
		float totalLength = 0f;
		public LayerMask conformMask;

        public void Start()
		{
			BuildFence();
		}
		void CheckRenderers()
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
		}
		/// <summary>
		/// Construct the fence mesh and assign it to the MeshRenderer / MeshFilter components.
		/// This function is expensive so try not to do it very often.
		/// </summary>
		public void BuildFence()
		{
			if (sections.Count < 2)
				return;

            #if UNITY_EDITOR
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
			#endif
			if (mesh == null)
				mesh = new Mesh();

			totalLength = 0f;
			totalPosts = 0;
			picketCount = 0;

			List<Material> allMaterials = new List<Material>();
			mesh.Clear();
			//submeshTriangles.Clear();
			
            // Generate a unique list of materials. This way fence sections can share materails without double ups.
			for (int i = 0; i < sections.Count; i++)
			{
				for (int j = 0; j < sections[i].data.materials.Count; j++)
				{
					Material m = sections[i].data.materials[j];
					if (!allMaterials.Contains(m))
					{
						allMaterials.Add(sections[i].data.materials[j]);
					}
				}
			}
			submeshTriangles.Clear();
			for (int i = 0; i < allMaterials.Count; i++)
			{
				submeshTriangles.Add(new List<int>());
			}
			//Debug.Log("submeshTriangles length: " + submeshTriangles.Count);
			verts = new List<Vector3>();
			normals = new List<Vector3>();
			uvs = new List<Vector2>();
			int vertexTotal = 0;
			Quaternion sectionRot = Quaternion.identity;
			Vector3 pivotPoint = transform.position;
			//List<float> postCounts = new List<float>(); // number of posts for each section
			//List<Vector3> postPositions = new List<Vector3>(); // positions of each post
			//List<Vector3> postAngles = new List<Vector3>(); // angle of each post

			//// Find all the post positions and angles first.
			//for (int i = 0; i < sections.Count - 1; i++)
			//{
			//	FenceSection fromSec = sections[i];
			//	FenceSection toSec = sections[i + 1];
			//	if (fromSec.data == null) continue;
			//	float sectionLength = Vector3.Distance(fromPos, toPos);
			//	if (sectionLength <= Mathf.Epsilon) break;

			//	postCounts.Add(sectionLength / fromSec.data.segmentLength);

			//	for (int j = 0; j < postCounts[i]; j++)
			//	{
			//		Vector3 castPoint = Vector3.Lerp(
			//				fromPos,
			//				toPos,
			//				(float)j / postCounts[i]
			//				);
			//		RaycastHit hit;
			//		if (Physics.Raycast(castPoint + (Vector3.up * 1000), Vector3.down, out hit, 2000, conformMask))
			//		{
			//			postPositions.Add(hit.point);
			//			postAngles.Add(hit.normal);
			//		}
			//		else
			//		{
			//			postPositions.Add(castPoint);
			//			postAngles.Add(Vector3.up);
			//		}
			//	}
			//}

			//for (int i = 0; i < postPositions.Count - 1; i++)
			//{
			//	AddCube(
			//		postPositions[i],
			//		Quaternion.LookRotation(postAngles[i]),
			//		new Vector3(.1f, .1f, 3f),
			//		ref vertexTotal,
			//		3
			//		);
			//}

			// Then we can build the fence pieces
			for (int i = 0; i < sections.Count-1; i++)
			{
				FenceSection fromSec = sections[i];
				FenceSection toSec = sections[i + 1];

				// Convert to local space
				Vector3 startPos = fromSec.cornerPoint - pivotPoint;
				Vector3 endPos = toSec.cornerPoint - pivotPoint;

				if (fromSec.data == null) continue;
				float sectionLength = Vector3.Distance(startPos, endPos);
				if (sectionLength <= Mathf.Epsilon) break;
				float numberOfPosts = sectionLength / fromSec.data.segmentLength;
				int numberOfPostInt = Mathf.CeilToInt(numberOfPosts);

				float segmentLength = sectionLength / numberOfPosts;

				float flipFactor = fromSec.flipFence ? 1f : -1f;
				//post lft = x
				//post fwd = y
				//post up  = z

				int postMeshId = allMaterials.IndexOf(fromSec.data.materials[0]);
				int railMeshId = allMaterials.IndexOf(fromSec.data.materials[1]);
				int picketMeshId = allMaterials.IndexOf(fromSec.data.materials[2]);
				//Debug.Log("postMeshId: " + postMeshId);
				//Debug.Log("railMeshId: " + railMeshId);
				//Debug.Log("picketMeshId: " + picketMeshId);

				float halfPostWidth = fromSec.data.postDimensions.x * 0.5f;
				float halfPostLength = fromSec.data.postDimensions.y * 0.5f;
				float halfPostHeight = fromSec.data.postDimensions.z * 0.5f;
				float halfPicketHeight = fromSec.data.picketDimensions.z * 0.5f;
				float halfPicketLength = fromSec.data.picketDimensions.y * 0.5f;
				float halfPicketWidth = fromSec.data.picketDimensions.x * 0.5f;
				float halfRailThickness = fromSec.data.railThickness * 0.5f;

				float railCenterOffset = fromSec.data.postJointMode == FenceData.PostJoint.inset ? 0f : halfPostWidth + halfRailThickness;
				railCenterOffset *= flipFactor;

				switch (fromSec.data.conformMode)
				{
					case FenceData.ConformMode.none:
						#region Don't Conform
						// Convert back to world space
						startPos += pivotPoint;
						endPos += pivotPoint;
						Vector3 secDir = (endPos - startPos).normalized;
						Vector3 lookDir = secDir;
						lookDir.y = 0;
						sectionRot = Quaternion.LookRotation(lookDir, Vector3.up);	
						Quaternion postRot = Quaternion.FromToRotation(Vector3.left, Vector3.right) * Quaternion.FromToRotation(sectionRot * Vector3.forward, sectionRot * Vector3.up) * sectionRot;

						for (int j = 0; j < numberOfPosts + 1; j++)
						{
							bool lastPost = j == numberOfPosts;
							// Convert result to local space
							Vector3 postPos = Vector3.Lerp(
									startPos, endPos,
									(float)j / numberOfPosts);
							// Add post
							AddCube(
								postPos + 
								new Vector3(0f, halfPostHeight, 0f) + // Slide the post up to half way point
								(secDir * -halfPostLength), //translate it back so the center of the post in the correct location.
								postRot,
								fromSec.data.postDimensions,
								ref vertexTotal,
								postMeshId
								);
							totalPosts++;

							// Add pickets for this segment (if applicable)
							if (fromSec.data.type == FenceData.FenceType.picket && !lastPost)
							{
								Vector3 nextPostPos = Vector3.Lerp(
									startPos, endPos,
									(float)(j + 1) / numberOfPosts);


								Vector3 picketOffset =
									postRot * Vector3.right *   // Remove the thickness of half a post at each end
									 (railCenterOffset - ((halfRailThickness + halfPicketWidth)) * flipFactor);
									//(halfPostWidth - halfPicketLength));		// add offset to but the pickets agains the rails.
								Vector3 picketStart = postPos + (secDir * halfPostLength) + picketOffset;
								Vector3 picketEnd = nextPostPos - (secDir * halfPostLength) + picketOffset;

								int picketsForThisSegment = Mathf.CeilToInt(
									Vector3.Distance(picketStart, picketEnd) / 
									(fromSec.data.picketDimensions.y + fromSec.data.picketGap)
									);
								Vector3 picketHeightVec = sectionRot * Vector3.up * (halfPicketHeight + fromSec.data.picketGroundOffset);
								//Debug.Log("Pickets: " + picketsForThisSegment);
								//Debug.Log("Length: " + Vector3.Distance(picketStart, picketEnd));
								for (int p = 0; p < picketsForThisSegment; p++)
								{ 
									Vector3 picketPos = Vector3.Lerp(
										picketStart, picketEnd,
										(float)p / picketsForThisSegment) +
										picketHeightVec; // Slide picket up to half way.
									// Add picket
									AddCube(
										picketPos,
										postRot,
										fromSec.data.picketDimensions,
										ref vertexTotal,
										picketMeshId
										);
									picketCount++;
								}
							}
						}
						totalPosts++;
						// Add rails
						foreach (FenceData.Rail rail in fromSec.data.rails)
						{
							Vector3 railStart = startPos + (rail.groundOffset * Vector3.up);
							Vector3 railEnd = endPos + (rail.groundOffset * Vector3.up);
							float railLength = Vector3.Distance(railStart, railEnd);
							Quaternion railRot = Quaternion.LookRotation(railEnd - railStart, Vector3.up);
							Vector3 railPos = 
								railStart + 
								(railRot * Vector3.forward * railLength * 0.5f) + //offset allong the length so the center point of the cube is halfway down the fence section.
								railCenterOffset * (sectionRot * Vector3.left);

							AddCube(
								railPos,
								railRot,
								new Vector3(fromSec.data.railThickness, rail.width, railLength),
								ref vertexTotal,
								railMeshId
								);
						}
						#endregion
						break;

					case FenceData.ConformMode.ground:
						#region Conform
						// we add 2 to the number of posts here since we want to
						// want to be sure we get an end post as well.
						Vector3[] _postPositions = new Vector3[numberOfPostInt + 2];
						Vector3[] _postAngles = new Vector3[numberOfPostInt + 2];
						for (int j = 0; j < _postPositions.Length; j++)
						{
							// Get the position in world space
							Vector3 castPoint = Vector3.Lerp(
									fromSec.cornerPoint + pivotPoint,
									toSec.cornerPoint + pivotPoint,
									(float)j / numberOfPosts
									);
							RaycastHit hit;
							if (Physics.Raycast(castPoint + (Vector3.up * 1000), Vector3.down, out hit, 2000, conformMask))
							{
								// Convert result back to local space
								_postPositions[j] = hit.point;
								_postAngles[j] = hit.normal;
							}
							else
							{
								_postPositions[j] = castPoint;
								_postAngles[j] = Vector3.up;
							}
							// Convert back to local space.
							_postPositions[j] -= pivotPoint;
						}
						for (int j = 0; j < _postPositions.Length - 2; j++)
						{
							bool lastPost = j == _postPositions.Length - 2;
							Vector3 cpPos = _postPositions[j];
							Vector3 npPos = _postPositions[j + 1];

							// Find post rotations conforming to the ground normal.
							Quaternion cpNormRot = Quaternion.LookRotation(((cpPos + (_postAngles[j] * 2)) - cpPos), npPos - cpPos);
							Quaternion npNormRot = Quaternion.LookRotation(((npPos + (_postAngles[j + 1] * 2)) - npPos), _postPositions[j + 2] - npPos);

							cpPos += cpNormRot * Vector3.down * halfPostLength;
							npPos += npNormRot * Vector3.down * halfPostLength;

							// Find non-conforming post rotations.
							Vector3 cpLV = npPos - cpPos;
							cpLV.y = cpLV.y * -fromSec.data.tilt;
							Quaternion cpLR = Quaternion.identity;
							if (cpLV.sqrMagnitude > 0)
								cpLR = Quaternion.LookRotation(cpLV, Vector3.up);
							Quaternion cpStraitAngle = Quaternion.FromToRotation(Vector3.left, Vector3.right) * Quaternion.FromToRotation(cpLR * Vector3.forward, cpLR * Vector3.up) * cpLR;

							Vector3 npLV = _postPositions[j + 2] - npPos;
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
								ref vertexTotal,
								postMeshId
								);

							totalPosts++;

							bool railObstructed = false;
							// Build Rails
							// skip the last 3 iteration as we only want to build the end post and not the rails.
							if (j != _postPositions.Length - 2)
							{
								//if we conform to the ground we want the rails to be built for each fence section.
								//if (fromSec.data.conform == FenceData.ConformMode.ground)
								{
									foreach (FenceData.Rail rail in fromSec.data.rails)
									{
										Vector3 railStart =
											cpPos +
											(rail.groundOffset * cpDir) +
											(halfPostLength * (cpRot * Vector3.up)); //places the start point in the center of the current post.
										Vector3 railEnd =
											npPos +
											(rail.groundOffset * npDir) +
											(halfPostLength * (npRot * Vector3.up)); //places the end point in the center of the next post.

										float railLength = Vector3.Distance(railStart, railEnd);
										Quaternion railRot = Quaternion.LookRotation(railEnd - railStart, cpDir);
										// Check for obstructions before building the rail
										if (fromSec.data.allowObstructions || !Physics.Raycast(railStart + pivotPoint, railRot * Vector3.forward, railLength))
										{
											Vector3 railPos =
												railStart +
												(railRot * Vector3.forward * railLength * 0.5f) +
												(railCenterOffset * (cpRot * Vector3.left));

											AddCube(
												railPos,
												railRot,
												new Vector3(fromSec.data.railThickness, rail.width, railLength),
												ref vertexTotal,
												railMeshId
												);
										}
										else
										{
											railObstructed = true;
										}
									}
								}
							}

							// Add pickets
							if (!railObstructed && fromSec.data.type == FenceData.FenceType.picket && !lastPost)
							{
								Vector3 picketStart = cpPos + (cpRot * (Vector3.up * fromSec.data.postDimensions.y)); //
								Vector3 picketEnd = npPos;

								int picketsForThisSegment = Mathf.CeilToInt(
									Vector3.Distance(picketStart, picketEnd) /
									(fromSec.data.picketDimensions.y + fromSec.data.picketGap)
									);

								for (int p = 0; p < picketsForThisSegment; p++)
								{
									float t = (float)p / picketsForThisSegment;
									// Remove the thickness of half a post at each end and
									// add offset to but the pickets agains the rails.
									Quaternion picketRot = Quaternion.Slerp(cpRot, npRot, t);
									Vector3 picketUp = picketRot * Vector3.forward;
									//Vector3 alignVector = (npPos - Vector3.Lerp(picketStart, picketEnd, t)).normalized;
									//picketRot = Quaternion.LookRotation(alignVector, picketRot * Vector3.forward);

									//picketRot = Quaternion.LookRotation()
									Vector3 picketLeft = picketRot * Vector3.left;

									Vector3 vertOffset = picketUp * (halfPicketHeight + fromSec.data.picketGroundOffset);
									Vector3 horizOffset = picketLeft * (railCenterOffset - ((halfRailThickness + halfPicketWidth) * flipFactor));
									Vector3 combinedOffset = vertOffset + horizOffset;

									Vector3 picketStraitPos = Vector3.Lerp(picketStart + combinedOffset, picketEnd + combinedOffset, t); // Slide picket up to half way.
									Vector3 picketConformPos = Vector3.zero;
									RaycastHit hit;
									// #BUG
									// This raycast always returns false.
									if (fromSec.data.picketConform != Mathf.Epsilon && Physics.Raycast(picketStraitPos + (1000f * picketUp), -picketUp, out hit, 2000f))
									{
										// We don't want to apply the full offset since we've already translated it to connect with the rails.
										picketConformPos = hit.point + vertOffset - pivotPoint;
										//Debug.Log("RaycastHit");
									}
									AddCube(
										Vector3.Lerp(picketStraitPos, picketConformPos, fromSec.data.picketConform),
										picketRot,
										fromSec.data.picketDimensions,
										ref vertexTotal,
										picketMeshId
										);
									picketCount++;
								}
							}
						}
						#endregion
						break;
				}
				totalLength += sectionLength;
			}

			// Set properties for the mesh.
			mesh.name = "Dynamic Fence";
			mesh.SetVertices(verts);
			mesh.SetUVs(0, uvs);
			mesh.subMeshCount = submeshTriangles.Count;
			// Apply the tris for each submesh.
			for (int i = 0; i < submeshTriangles.Count; i++)
			{
				mesh.SetTriangles( submeshTriangles[i], i);
			}
			mesh.SetNormals(normals);
			mesh.RecalculateNormals();
			mesh.RecalculateBounds();
			meshFilter.mesh = mesh;
			meshRenderer.sharedMaterials = allMaterials.ToArray();
			vertexCount = mesh.vertexCount;
			triCount = mesh.triangles.Length / 3;

            #if UNITY_EDITOR
            buildTime = (float)stopWatch.ElapsedMilliseconds;
			#endif
		}
		/// <summary>
		/// Call this before using the selection index for lookups as it may now be out of range.
		/// </summary>
		public void ClampSelectionIndex()
		{
			selectedSectionIndex = Mathf.Clamp(selectedSectionIndex, 0, sections.Count - 1);
		}
		/// <summary>
		/// Add a cube to the mesh.
		/// </summary>
		/// <param name="_pos">center position</param>
		/// <param name="orient">rotation</param>
		/// <param name="dimensions">length, width, height</param>
		/// <param name="vertexTotal">total vertices, used to create offset's in tris etc.</param>
		/// <param name="meshID">Add this mesh as a submesh (Optional)</param>
		private void AddCube(Vector3 _pos, Quaternion orient, Vector3 dimensions, ref int vertexTotal, int meshID = 0)
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
 
			};
			#endregion

			// Add the offset for the next mesh to start from.
			vertexTotal += 24;

			verts.AddRange(_verts);
			// Make sure we have enough submesh tri lists.
			//while (meshID >= submeshTriangles.Count -1)
			//	submeshTriangles.Add(new List<int>());
			submeshTriangles[meshID].AddRange(_tris);

			normals.AddRange(_normals);
			uvs.AddRange(_uvs);
		}
		public void AddSection(Vector3 _pos, FenceData _data = null)
		{
			FenceData d = null;
			if (_data) d = _data;
			else d = sections.Count > 0 ? sections[sections.Count - 1].data : null;
			// Localize the position when adding it
			sections.Add(new FenceSection(_pos - transform.position, d));
		}
		public void RemoveSection(int index)
		{
			sections.RemoveAt(index);
		}
		public void RemoveLastSection()
		{
			sections.RemoveAt(sections.Count - 1);
		}
		void OnValidate()
		{
			BuildFence();
		}
	}
}
