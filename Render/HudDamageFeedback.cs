using System;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Render
{
	public class HudDamageFeedback : GameHubBehaviour
	{
		public void Start()
		{
			this._mesh = new Mesh();
			this._vertices = new Vector3[8];
			this.ResetVertices();
			this._triangles = new int[this._vertices.Length * 3];
			this._triangles[0] = 0;
			this._triangles[1] = 6;
			this._triangles[2] = 1;
			this._triangles[3] = 1;
			this._triangles[4] = 6;
			this._triangles[5] = 7;
			this._triangles[6] = 7;
			this._triangles[7] = 6;
			this._triangles[8] = 5;
			this._triangles[9] = 5;
			this._triangles[10] = 6;
			this._triangles[11] = 4;
			this._triangles[12] = 4;
			this._triangles[13] = 2;
			this._triangles[14] = 5;
			this._triangles[15] = 5;
			this._triangles[16] = 2;
			this._triangles[17] = 3;
			this._triangles[18] = 3;
			this._triangles[19] = 2;
			this._triangles[20] = 1;
			this._triangles[21] = 1;
			this._triangles[22] = 2;
			this._triangles[23] = 0;
			this._uv = new Vector2[8];
			this.ResetUvs();
			this._mesh.SetTriangles(this._triangles, 0);
			this.MaterialDefaultColor = new Color(0.5f, 0.5f, 0.5f, 1f);
			this._alpha = 0f;
			this.UpdateMaterialColor();
			this._widthOffsetCache = this.WidthOffset;
			this._heightOffsetCache = this.HeightOffset;
			this._screenWidthCache = Screen.width;
			this._screenHeightCache = Screen.height;
		}

		public void SetAlpha(float alpha)
		{
			this._alpha = alpha;
			if (this._alpha > 1f)
			{
				this._alpha = 1f;
			}
			this._alpha *= this.MaxAlpha;
		}

		private void UpdateMaterialColor()
		{
			this._alphaCache = this._alpha;
			this.HudMaterial.SetColor("_TintColor", new Color(this.MaterialDefaultColor.r, this.MaterialDefaultColor.g, this.MaterialDefaultColor.b, this.MaterialDefaultColor.a * this._alpha));
		}

		private void ResetUvs()
		{
			this._uv[0] = new Vector2(0f, 0f);
			this._uv[1] = new Vector2(this.WidthOffset, this.HeightOffset);
			this._uv[2] = new Vector2(1f, 0f);
			this._uv[3] = new Vector2(1f - this.WidthOffset, this.HeightOffset);
			this._uv[4] = new Vector2(1f, 1f);
			this._uv[5] = new Vector2(1f - this.WidthOffset, 1f - this.HeightOffset);
			this._uv[6] = new Vector2(0f, 1f);
			this._uv[7] = new Vector2(this.WidthOffset, 1f - this.HeightOffset);
			this._mesh.uv = this._uv;
			this._widthOffsetCache = this.WidthOffset;
			this._heightOffsetCache = this.HeightOffset;
		}

		private void ResetVertices()
		{
			this.Point0 = new Vector2(0f, 0f);
			this.Point2 = new Vector2((float)Screen.width, 0f);
			this.Point4 = new Vector2((float)Screen.width, (float)Screen.height);
			this.Point6 = new Vector2(0f, (float)Screen.height);
			float num = (float)Screen.width * this.WidthOffset;
			float num2 = (float)Screen.height * this.HeightOffset;
			this.Point1 = new Vector2(num, num2);
			this.Point3 = new Vector2((float)Screen.width - num, num2);
			this.Point5 = new Vector2((float)Screen.width - num, (float)Screen.height - num2);
			this.Point7 = new Vector2(num, (float)Screen.height - num2);
			this._vertices[0] = new Vector3(this.Point0.x, this.Point0.y, 0f);
			this._vertices[2] = new Vector3(this.Point2.x, this.Point2.y, 0f);
			this._vertices[4] = new Vector3(this.Point4.x, this.Point4.y, 0f);
			this._vertices[6] = new Vector3(this.Point6.x, this.Point6.y, 0f);
			this._vertices[1] = new Vector3(this.Point1.x, this.Point1.y, 0f);
			this._vertices[3] = new Vector3(this.Point3.x, this.Point3.y, 0f);
			this._vertices[5] = new Vector3(this.Point5.x, this.Point5.y, 0f);
			this._vertices[7] = new Vector3(this.Point7.x, this.Point7.y, 0f);
			this._mesh.vertices = this._vertices;
			this._screenWidthCache = Screen.width;
			this._screenHeightCache = Screen.height;
		}

		public void Update()
		{
			if (this._widthOffsetCache != this.WidthOffset || this._heightOffsetCache != this.HeightOffset)
			{
				this.ResetUvs();
				this.ResetVertices();
			}
			if (this._screenWidthCache != Screen.width || this._screenHeightCache != Screen.height)
			{
				this.ResetVertices();
			}
		}

		public void OnPostRender()
		{
			GL.LoadPixelMatrix(0f, (float)Screen.width, 0f, (float)Screen.height);
			if (this._alphaCache != this._alpha)
			{
				this.UpdateMaterialColor();
			}
			this.HudMaterial.SetPass(0);
			Graphics.DrawMeshNow(this._mesh, base.transform.position, Quaternion.identity);
		}

		[Header("[Offsets]")]
		public float WidthOffset = 0.37f;

		public float HeightOffset = 0.2f;

		[Header("[Real alpha value from normalized SetAlpha()]")]
		public float MaxAlpha = 10f;

		public Material HudMaterial;

		public Color MaterialDefaultColor;

		public float _alpha;

		private float _alphaCache;

		public Vector3[] _vertices;

		public int[] _triangles;

		public Vector2[] _uv;

		public Vector2 Point0;

		public Vector2 Point2;

		public Vector2 Point4;

		public Vector2 Point6;

		public Vector2 Point1;

		public Vector2 Point3;

		public Vector2 Point5;

		public Vector2 Point7;

		private Mesh _mesh;

		private float _widthOffsetCache;

		private float _heightOffsetCache;

		private int _screenWidthCache;

		private int _screenHeightCache;
	}
}
