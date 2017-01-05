using System.Collections.Generic;
using UnityEngine;

namespace AshleySeric.FenceWrangler
{
	[System.Serializable]
	public class FenceSection
	{
		public Vector3 cornerPoint = new Vector3(1f, 0, 0);
		/// <summary>
		/// Flip which side the palings are palced on (if applicable).
		/// </summary>
		public bool flipFence;
		/// <summary>
		/// Modifies the height for this section of fence.
		/// </summary>
		public float heightModifier = 1f;
		/// <summary>
		/// Length in meters of this section of fence.
		/// </summary>
		public float length = 0f;
		public FenceSection(Vector3 position)
		{
			this.cornerPoint = position;
			this.heightModifier = 1f;
		}
	}

	public class Fence : MonoBehaviour
	{
		//[SerializeField]
		public FenceData fenceData;
		public List<FenceSection> sections = new List<FenceSection>
		{
			new FenceSection(new Vector3(1f, 0, 0)),
			new FenceSection(new Vector3(2f, 0, 1f))
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
		int postCount = 0;
		[SerializeField]
		[HideInInspector]
		float totalLength = 0f;
		//private enum EndFenceMode { ExtendLastSection = 0, AddNewSection = 1 }
		//[SerializeField]
		//EndFenceMode endFenceMode = 0;

		public void Start()
		{
			BuildFence();
		}
		public void BuildFence()
		{
			if (fenceData == null || sections.Count < 2)
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

			for (int i = 0, t = 1; i < sections.Count-1; i++)
			{
				float dist = Vector3.Distance(sections[i].cornerPoint, sections[i+1].cornerPoint);
				if (dist <= Mathf.Epsilon) break;
				float numberOfPosts = dist / fenceData.segmentLength;
				for (int j = 0; j < (int)numberOfPosts + 1; j++, t++)
				{
					//Debug.Log("Adding post " + i + " | " + j);
					AddPost(Vector3.Lerp(sections[i].cornerPoint, sections[i+1].cornerPoint, (float)j / numberOfPosts), Quaternion.LookRotation(sections[i+1].cornerPoint - sections[i].cornerPoint, Vector3.up), t);
				}
				totalLength += dist;
			}

			// Add the last post as we end the loop before getting there.
			AddPost(sections[sections.Count-1].cornerPoint, Quaternion.identity);

			mesh.name = "Dynamic Fence";
			mesh.SetVertices(verts);
			mesh.SetUVs(0, uvs);
			mesh.SetTriangles(tris, 0);
			mesh.SetNormals(normals);

			meshFilter.mesh = mesh;
			mesh.RecalculateNormals();
			mesh.RecalculateBounds();

			vertexCount = mesh.vertexCount;
		}
		private void AddPost(Vector3 _pos, Quaternion orient, int iter = 0)
		{
			float length = fenceData.postDimensions.z;
			float width = fenceData.postDimensions.x;
			float height = fenceData.postDimensions.y;

			Vector3 up =	orient * Vector3.up;
			Vector3 down =	orient * Vector3.down;
			Vector3 front = orient * Vector3.forward;
			Vector3 back =	orient * Vector3.back;
			Vector3 left =	orient * Vector3.left;
			Vector3 right = orient * Vector3.right;

			Vector3 leftPos	 = (left * (width * 0.5f));
			Vector3 rightPos = (right * (width * 0.5f));
			Vector3 frontPos = (front * (length * 0.5f));
			Vector3 backPos  = (back * (length * 0.5f));
			Vector3 upPos	 = (up * height);

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
			Vector2 _10 = new Vector2( 1f, 0f );
			Vector2 _01 = new Vector2( 0f, 1f );
			Vector2 _11 = new Vector2( 1f, 1f );
 
			Vector2[] _uvs = new Vector2[]
			{
				// Bottom
				_11, _01, _00, _10,
 
				// Left
				_11, _01, _00, _10,
 
				// Front
				_11, _01, _00, _10,
 
				// Back
				_11, _01, _00, _10,
 
				// Right
				_11, _01, _00, _10,
 
				// Top
				_11, _01, _00, _10,
			};
			#endregion

			//This creates our offset
			iter *= 24;

			#region Triangles
			int[] _tris = new int[]
			{
				// Bottom
				iter + 3, iter + 1, iter + 0,
				iter + 3, iter + 2, iter + 1,			
 
				// Left
				iter + 3 + 4 * 1, iter + 1 + 4 * 1, iter + 0 + 4 * 1,
				iter + 3 + 4 * 1, iter + 2 + 4 * 1, iter + 1 + 4 * 1,
 
				// Front
				iter + 3 + 4 * 2, iter + 1 + 4 * 2, iter + 0 + 4 * 2,
				iter + 3 + 4 * 2, iter + 2 + 4 * 2, iter + 1 + 4 * 2,
 
				// Back
				iter + 3 + 4 * 3, iter + 1 + 4 * 3, iter + 0 + 4 * 3,
				iter + 3 + 4 * 3, iter + 2 + 4 * 3, iter + 1 + 4 * 3,
 
				// Right
				iter + 3 + 4 * 4, iter + 1 + 4 * 4, iter + 0 + 4 * 4,
				iter + 3 + 4 * 4, iter + 2 + 4 * 4, iter + 1 + 4 * 4,
 
				// Top
				iter + 3 + 4 * 5, iter + 1 + 4 * 5, iter + 0 + 4 * 5,
				iter + 3 + 4 * 5, iter + 2 + 4 * 5, iter + 1 + 4 * 5,

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
	
			verts.AddRange(_verts);
			tris.AddRange(_tris);
			normals.AddRange(_normals);
			uvs.AddRange(_uvs);
			postCount++;
		}
		public void AddSection(Vector3 position)
		{
			sections.Add(new FenceSection(position));
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
