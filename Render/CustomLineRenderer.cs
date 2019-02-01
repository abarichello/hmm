using System;
using HeavyMetalMachines.Utils;
using UnityEngine;

namespace HeavyMetalMachines.Render
{
	[ExecuteInEditMode]
	public class CustomLineRenderer : MonoBehaviour
	{
		private void Awake()
		{
			this.Camera = Camera.main;
			this.InitializeMesh();
		}

		private void OnEnable()
		{
			this.InitializeMesh();
		}

		private void OnDisable()
		{
			UnityEngine.Object.Destroy(this.mesh);
			this.mesh = null;
		}

		private void InitializeMesh()
		{
			if (this.LinePoints.Length < 2)
			{
				return;
			}
			if (this.mesh == null)
			{
				this.mesh = new Mesh();
				this.mesh.name = "CustomLineRendererMesh";
				this.mesh.MarkDynamic();
			}
			this.vertices = new Vector3[this.LinePoints.Length * 2];
			this.uvs = new Vector2[this.LinePoints.Length * 2];
			this.indices = new int[(this.LinePoints.Length - 1) * 6];
			for (int i = 0; i < this.uvs.Length; i += 2)
			{
				this.uvs[i].y = 0f;
				this.uvs[i + 1].y = 1f;
			}
			int num = 0;
			for (int j = 0; j < this.indices.Length; j += 6)
			{
				this.indices[j] = num;
				this.indices[j + 1] = num + 1;
				this.indices[j + 2] = num + 2;
				this.indices[j + 3] = num + 1;
				this.indices[j + 4] = num + 3;
				this.indices[j + 5] = num + 2;
				num += 2;
			}
		}

		protected virtual void LateUpdate()
		{
			if (this.LinePoints.Length < 2)
			{
				return;
			}
			this.UpdateMesh();
			Graphics.DrawMesh(this.mesh, Matrix4x4.identity, this.Material, base.gameObject.layer, this.Camera);
		}

		private void UpdateMesh()
		{
			if (this.lastPointCount != this.LinePoints.Length)
			{
				this.lastPointCount = this.LinePoints.Length;
				this.InitializeMesh();
			}
			float num = 0f;
			Vector3 vector = this.LinePoints[1] - this.LinePoints[0];
			Vector3 forward = this.Camera.transform.forward;
			Vector3 b = Vector3.zero;
			for (int i = 0; i < this.LinePoints.Length; i++)
			{
				Vector3 a = this.LinePoints[i];
				int num2 = i * 2;
				if (i > 0)
				{
					vector = a - this.LinePoints[i - 1];
					num += vector.magnitude * this.TileSize;
				}
				ReallyFastMath.FastCross(ref vector, ref forward, ref b);
				b = b.normalized * this.LineWidth;
				this.uvs[num2].x = num;
				this.uvs[num2 + 1].x = num;
				this.vertices[num2] = a + b;
				this.vertices[num2 + 1] = a - b;
			}
			this.mesh.vertices = this.vertices;
			this.mesh.uv = this.uvs;
			this.mesh.SetIndices(this.indices, MeshTopology.Triangles, 0);
			this.mesh.RecalculateBounds();
		}

		private void OnDrawGizmos()
		{
			for (int i = 0; i < this.LinePoints.Length; i++)
			{
				Vector3 center = this.LinePoints[i];
				Gizmos.DrawSphere(center, this.LineWidth * 0.25f);
			}
		}

		public Material Material;

		public float LineWidth = 1f;

		public float TileSize;

		public Vector3[] LinePoints = new Vector3[2];

		protected Mesh mesh;

		private Vector3[] vertices;

		private Vector2[] uvs;

		private int[] indices;

		private int lastPointCount = -1;

		protected Camera Camera;
	}
}
