using System.Collections.Generic;
using UnityEngine;

namespace AshleySeric.FenceWrangler
{
	public class Fence : MonoBehaviour
	{
		//[SerializeField]
		public FenceData fenceData;
		public Vector3[] corners = new Vector3[]
		{
			new Vector3(1f, 0f, 0f),
			new Vector3(2f, 0f, 0f)
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
			if (fenceData == null)
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

			for (int i = 0; i < corners.Length-1; i++)
			{
				float dist = Vector3.Distance(corners[i], corners[i+1]);
				if (dist <= Mathf.Epsilon) break;
				float numberOfPosts = dist / fenceData.segmentLength;
				for (int j = 0; j < (int)numberOfPosts + 1; j++)
				{
					//Debug.Log("Adding post " + i + " | " + j);
					AddPost(Vector3.Lerp(corners[i], corners[i+1], (float)j / numberOfPosts));
				}
				totalLength += dist;
			}

			// Add the last post as we end the loop before getting there.
			AddPost(corners[corners.Length-1]);

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
		private void AddPost(Vector3 _pos, int meshIndex = 0)
		{
			float length = fenceData.postDimensions.x;
			float width = fenceData.postDimensions.y;
			float height = fenceData.postDimensions.z;
			
			#region Vertices
			Vector3 p0 = _pos + new Vector3( -length * .5f,	-width * .5f, height * .5f );
			Vector3 p1 = _pos + new Vector3( length * .5f, 	-width * .5f, height * .5f );
			Vector3 p2 = _pos + new Vector3( length * .5f, 	-width * .5f, -height * .5f );
			Vector3 p3 = _pos + new Vector3( -length * .5f,	-width * .5f, -height * .5f );
 
			Vector3 p4 = _pos + new Vector3( -length * .5f,	width * .5f,  height * .5f );
			Vector3 p5 = _pos + new Vector3( length * .5f, 	width * .5f,  height * .5f );
			Vector3 p6 = _pos + new Vector3( length * .5f, 	width * .5f,  -height * .5f );
			Vector3 p7 = _pos + new Vector3( -length * .5f,	width * .5f,  -height * .5f );

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
			Vector3 up 	= Vector3.up;
			Vector3 down 	= Vector3.down;
			Vector3 front 	= Vector3.forward;
			Vector3 back 	= Vector3.back;
			Vector3 left 	= Vector3.left;
			Vector3 right 	= Vector3.right;
 
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

			// Triangles are all referencing only the first 28 verticies.
			// I need to add an offset for each iteration to compensate.
			#region Triangles
			int[] _tris = new int[]
			{
				// Bottom
				3, 1, 0,
				3, 2, 1,			
 
				// Left
				3 + 4 * 1, 1 + 4 * 1, 0 + 4 * 1,
				3 + 4 * 1, 2 + 4 * 1, 1 + 4 * 1,
 
				// Front
				3 + 4 * 2, 1 + 4 * 2, 0 + 4 * 2,
				3 + 4 * 2, 2 + 4 * 2, 1 + 4 * 2,
 
				// Back
				3 + 4 * 3, 1 + 4 * 3, 0 + 4 * 3,
				3 + 4 * 3, 2 + 4 * 3, 1 + 4 * 3,
 
				// Right
				3 + 4 * 4, 1 + 4 * 4, 0 + 4 * 4,
				3 + 4 * 4, 2 + 4 * 4, 1 + 4 * 4,
 
				// Top
				3 + 4 * 5, 1 + 4 * 5, 0 + 4 * 5,
				3 + 4 * 5, 2 + 4 * 5, 1 + 4 * 5,
 
			};
			#endregion
	
			verts.AddRange(_verts);
			tris.AddRange(_tris);
			normals.AddRange(_normals);
			uvs.AddRange(_uvs);
			postCount++;
		}
		void OnValidate()
		{
			BuildFence();
		}
		private void OnDrawGizmos()
		{
			Gizmos.color = Color.green;
			for (int i = 0; i < verts.Count; i++)
			{
				Gizmos.DrawSphere(verts[i], 0.03f);
			}
		}
	}
}
