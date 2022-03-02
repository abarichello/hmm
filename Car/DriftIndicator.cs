using System;
using UnityEngine;

namespace HeavyMetalMachines.Car
{
	internal class DriftIndicator
	{
		public void Initialize(Transform driftIndicatorTransform, Texture driftTexture, Color playerColor)
		{
			this._driftIndicatorTransform = driftIndicatorTransform;
			MeshRenderer component = this._driftIndicatorTransform.GetComponent<MeshRenderer>();
			component.material.SetTexture("_MainTex", driftTexture);
			component.material.SetColor("_TintColor", playerColor * 0.5f);
			MeshFilter component2 = this._driftIndicatorTransform.GetComponent<MeshFilter>();
			this._driftMesh = new Mesh
			{
				name = "DriftMesh"
			};
			this._driftMesh.MarkDynamic();
			component2.mesh = this._driftMesh;
		}

		public void RotateDriftIndicator(bool isReverse)
		{
			if (!this._isDriftIndicatorEnable)
			{
				return;
			}
			this._driftIndicatorTransform.localRotation = ((!isReverse) ? Quaternion.Euler(0f, 0f, 0f) : Quaternion.Euler(0f, 180f, 0f));
		}

		public void SetVisibility(bool isVisible)
		{
			this._isDriftIndicatorVisible = isVisible;
			this._driftIndicatorTransform.gameObject.SetActive(isVisible && this._isDriftIndicatorEnable);
		}

		public void SetDriftIndicatorEnable(bool isEnable)
		{
			this._isDriftIndicatorEnable = isEnable;
			this.SetVisibility(this._isDriftIndicatorVisible);
		}

		public void SetupDriftMesh(float arcAngle, float driftIndicatorRadius, float driftIndicatorThickness, float driftIndicatorXDiff, float driftIndicatorZDiff)
		{
			int num = (int)(Mathf.Abs(arcAngle) / 1f) << 2;
			bool flag = num != this._maxNumVert;
			this._maxNumVert = num;
			if (flag)
			{
				this._vertices = new Vector3[this._maxNumVert];
				this._colors = new Color[this._maxNumVert];
				this._uv = new Vector2[this._maxNumVert];
				this._tris = new int[this._maxNumVert * 3];
			}
			float num2 = (float)num;
			float num3 = 0f;
			float num4 = -arcAngle;
			int i = 0;
			while (i < num)
			{
				float num5 = (float)(i & 1);
				this._auxDirectionVector3.x = -Mathf.Sin(0.017453292f * num4) + driftIndicatorXDiff;
				this._auxDirectionVector3.z = Mathf.Cos(0.017453292f * num4) + driftIndicatorZDiff;
				this._vertices[i] = (driftIndicatorRadius + driftIndicatorThickness * num5) * this._auxDirectionVector3;
				this._colors[i] = Color.white;
				this._uv[i].x = num3 / (num2 - 1f);
				this._uv[i].y = num5;
				num4 += 1f * num5;
				i++;
				num3 += 1f;
			}
			for (int j = 0; j < num - 3; j += 2)
			{
				int num6 = 6 * (j >> 1);
				this._tris[num6] = j;
				this._tris[num6 + 1] = j + 1;
				this._tris[num6 + 2] = j + 2;
				this._tris[num6 + 3] = j + 1;
				this._tris[num6 + 4] = j + 3;
				this._tris[num6 + 5] = j + 2;
			}
			this._driftMesh.Clear();
			this._driftMesh.vertices = this._vertices;
			this._driftMesh.colors = this._colors;
			this._driftMesh.uv = this._uv;
			this._driftMesh.triangles = this._tris;
		}

		public void Destroy()
		{
			this._maxNumVert = 0;
			this._vertices = null;
			this._colors = null;
			this._uv = null;
			this._tris = null;
			this._driftIndicatorTransform = null;
		}

		private Transform _driftIndicatorTransform;

		private Vector3 _auxDirectionVector3 = Vector3.zero;

		private int _maxNumVert;

		private Vector3[] _vertices;

		private Color[] _colors;

		private Vector2[] _uv;

		private int[] _tris;

		private Mesh _driftMesh;

		private bool _isDriftIndicatorEnable;

		private bool _isDriftIndicatorVisible;

		private const float DriftMeshAngleStep = 1f;
	}
}
