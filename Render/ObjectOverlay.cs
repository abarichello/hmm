using System;
using System.Collections.Generic;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Render
{
	[ExecuteInEditMode]
	public class ObjectOverlay : GameHubBehaviour
	{
		protected void Awake()
		{
			ObjectOverlay.InstanceCount++;
			if (GameHubBehaviour.Hub != null && GameHubBehaviour.Hub.Net.IsServer() && !GameHubBehaviour.Hub.Net.IsTest())
			{
				UnityEngine.Object.Destroy(this);
				return;
			}
			this._propertyIds.MV = Shader.PropertyToID("_MV");
			this._propertyIds.Model = Shader.PropertyToID("_Model");
			this._propertyIds.RT = Shader.PropertyToID("_RT");
			this._propertyIds.Color = Shader.PropertyToID("_Color");
			this._propertyIds.OutlineColor = Shader.PropertyToID("_OutlineColor");
			this.IgnoreLayer |= 268435456;
			if (ObjectOverlay.RenderBuffer == null)
			{
				ObjectOverlay.RenderBuffer = new RenderTexture(512, 512, 24);
				ObjectOverlay.RenderBuffer.autoGenerateMips = false;
				ObjectOverlay.RenderBuffer.useMipMap = false;
			}
			if (ObjectOverlay.QuadMesh == null)
			{
				ObjectOverlay.QuadMesh = new Mesh();
				ObjectOverlay.QuadMesh.vertices = new Vector3[]
				{
					new Vector3(-1f, -1f),
					new Vector3(1f, -1f),
					new Vector3(1f, 1f),
					new Vector3(-1f, 1f)
				};
				ObjectOverlay.QuadMesh.uv = new Vector2[]
				{
					new Vector2(0f, 0f),
					new Vector2(1f, 0f),
					new Vector2(1f, 1f),
					new Vector2(0f, 1f)
				};
				ObjectOverlay.QuadMesh.triangles = new int[]
				{
					0,
					1,
					2,
					0,
					2,
					3
				};
			}
			this.SetColors(Color.grey, Color.white);
		}

		protected void OnDestroy()
		{
			ObjectOverlay.InstanceCount--;
			if (ObjectOverlay.InstanceCount > 0)
			{
				return;
			}
			ObjectOverlay.InstanceCount = 0;
			UnityEngine.Object.Destroy(ObjectOverlay.RenderBuffer);
			UnityEngine.Object.Destroy(ObjectOverlay.QuadMesh);
			ObjectOverlay.RenderBuffer = null;
			ObjectOverlay.QuadMesh = null;
		}

		protected void OnEnable()
		{
			MeshFilter[] componentsInChildren = base.GetComponentsInChildren<MeshFilter>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				if ((this.IgnoreLayer.value & 1 << componentsInChildren[i].gameObject.layer) == 0)
				{
					this._filters.Add(componentsInChildren[i]);
				}
			}
		}

		protected void OnDisable()
		{
			this._filters.Clear();
		}

		protected void OnRenderObject()
		{
			if (this._filters.Count == 0)
			{
				return;
			}
			Camera current = Camera.current;
			if (current.GetInstanceID() != CarCamera.Singleton.CameraInstanceId || (current.cullingMask & 1) == 0)
			{
				return;
			}
			if (this.WriterMaterial == null || this.EffectMaterial == null)
			{
				return;
			}
			RenderTexture active = RenderTexture.active;
			RenderTexture.active = ObjectOverlay.RenderBuffer;
			GL.Clear(true, true, new Color(0f, 0f, 0f, 0f));
			Vector3 position = base.transform.position;
			position.y += this.CenterYOffset;
			float num = Vector3.Distance(Camera.current.transform.position, position);
			float fov = 114.59156f * Mathf.Atan(this.Size / num);
			Vector3 position2 = Camera.current.transform.position;
			Quaternion q = Quaternion.LookRotation(Camera.current.transform.position - position, Vector3.up);
			Matrix4x4 matrix4x = Matrix4x4.TRS(position2, Quaternion.LookRotation(position2 - new Vector3(position.x, position.y, position.z), Vector3.up), Vector3.one);
			float zNear = Mathf.Max(num - this.Size * 2f, 1f);
			float zFar = num + this.Size * 2f;
			Matrix4x4 lhs = Matrix4x4.Perspective(fov, 1f, zNear, zFar);
			Matrix4x4 inverse = matrix4x.inverse;
			for (int i = 0; i < 4; i++)
			{
				lhs[1, i] = -lhs[1, i];
			}
			for (int j = 0; j < 4; j++)
			{
				lhs[2, j] = lhs[2, j] * 0.5f + lhs[3, j] * 0.5f;
			}
			Matrix4x4 value = lhs * inverse;
			this.WriterMaterial.SetMatrix(this._propertyIds.MV, value);
			for (int k = 0; k < this._filters.Count; k++)
			{
				this.WriterMaterial.SetMatrix(this._propertyIds.Model, this._filters[k].transform.localToWorldMatrix);
				if (this.WriterMaterial.SetPass(0))
				{
					Graphics.DrawMeshNow(this._filters[k].sharedMesh, this._filters[k].transform.position, Quaternion.identity);
				}
			}
			RenderTexture.active = active;
			this.EffectMaterial.SetTexture(this._propertyIds.RT, ObjectOverlay.RenderBuffer);
			if (this.EffectMaterial.SetPass(0))
			{
				Matrix4x4 matrix = Matrix4x4.TRS(position, q, this.Size * Vector3.one);
				Graphics.DrawMeshNow(ObjectOverlay.QuadMesh, matrix, 0);
			}
		}

		public void SetColors(Color colorA, Color colorB)
		{
			if (this.EffectMaterial != null)
			{
				this.EffectMaterial.SetColor(this._propertyIds.Color, colorA);
				this.EffectMaterial.SetColor(this._propertyIds.OutlineColor, colorB);
			}
		}

		public Material WriterMaterial;

		public Material EffectMaterial;

		public float Size = 2f;

		public float CenterYOffset;

		public LayerMask IgnoreLayer = 0;

		protected static RenderTexture RenderBuffer;

		protected static Mesh QuadMesh;

		private static int InstanceCount;

		private readonly List<MeshFilter> _filters = new List<MeshFilter>();

		private ObjectOverlay.PropertyIds _propertyIds;

		private struct PropertyIds
		{
			public int MV;

			public int Model;

			public int RT;

			public int Color;

			public int OutlineColor;
		}
	}
}
