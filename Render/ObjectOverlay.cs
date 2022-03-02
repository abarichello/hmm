using System;
using System.Collections.Generic;
using HeavyMetalMachines.GameCamera;
using HeavyMetalMachines.Infra.DependencyInjection.Attributes;
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
				Object.Destroy(this);
				return;
			}
			this._propertyIds.MV = Shader.PropertyToID("_MV");
			this._propertyIds.Model = Shader.PropertyToID("_Model");
			this._propertyIds.RT = Shader.PropertyToID("_RT");
			this._propertyIds.Color = Shader.PropertyToID("_Color");
			this._propertyIds.OutlineColor = Shader.PropertyToID("_OutlineColor");
			this.IgnoreLayer |= 268435456;
			this._boundingSpheres = new BoundingSphere[]
			{
				new BoundingSphere(Vector3.zero, this.CullingRadius)
			};
			this._cullingGroup = new CullingGroup();
			this._cullingGroup.targetCamera = ((this._gameCameraEngine == null) ? null : this._gameCameraEngine.UnityCamera);
			this._cullingGroup.SetBoundingSpheres(this._boundingSpheres);
			this._cullingGroup.SetBoundingSphereCount(this._boundingSpheres.Length);
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
			if (this._cullingGroup != null)
			{
				this._cullingGroup.Dispose();
			}
			this._cullingGroup = null;
			this._boundingSpheres = null;
			ObjectOverlay.InstanceCount--;
			if (ObjectOverlay.InstanceCount > 0)
			{
				return;
			}
			ObjectOverlay.InstanceCount = 0;
			Object.Destroy(ObjectOverlay.RenderBuffer);
			Object.Destroy(ObjectOverlay.QuadMesh);
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

		protected void Update()
		{
			Vector3 position = base.transform.position;
			position.y += this.CenterYOffset;
			this._boundingSpheres[0].position = position;
		}

		protected void OnRenderObject()
		{
			if (this._filters.Count == 0)
			{
				return;
			}
			Camera current = Camera.current;
			if (current.GetInstanceID() != this._gameCameraEngine.UnityCamera.GetInstanceID() || (current.cullingMask & 1) == 0)
			{
				return;
			}
			if (this.WriterMaterial == null || this.EffectMaterial == null)
			{
				return;
			}
			if (!this._cullingGroup.IsVisible(0))
			{
				return;
			}
			RenderTexture active = RenderTexture.active;
			RenderTexture.active = ObjectOverlay.RenderBuffer;
			GL.Clear(true, true, new Color(0f, 0f, 0f, 0f));
			Vector3 position = base.transform.position;
			position.y += this.CenterYOffset;
			Vector3 position2 = Camera.current.transform.position;
			float num = Vector3.Distance(position2, position);
			float num2 = 114.59156f * Mathf.Atan(this.Size / num);
			Vector3 vector = position2;
			Quaternion quaternion = Quaternion.LookRotation(position2 - position, Vector3.up);
			Matrix4x4 matrix4x = Matrix4x4.TRS(vector, Quaternion.LookRotation(vector - new Vector3(position.x, position.y, position.z), Vector3.up), Vector3.one);
			float num3 = Mathf.Max(num - this.Size * 2f, 1f);
			float num4 = num + this.Size * 2f;
			Matrix4x4 matrix4x2 = Matrix4x4.Perspective(num2, 1f, num3, num4);
			Matrix4x4 inverse = matrix4x.inverse;
			for (int i = 0; i < 4; i++)
			{
				matrix4x2[1, i] = -matrix4x2[1, i];
			}
			for (int j = 0; j < 4; j++)
			{
				matrix4x2[2, j] = matrix4x2[2, j] * 0.5f + matrix4x2[3, j] * 0.5f;
			}
			Matrix4x4 matrix4x3 = matrix4x2 * inverse;
			this.WriterMaterial.SetMatrix(this._propertyIds.MV, matrix4x3);
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
				Matrix4x4 matrix4x4 = Matrix4x4.TRS(position, quaternion, this.Size * Vector3.one);
				Graphics.DrawMeshNow(ObjectOverlay.QuadMesh, matrix4x4, 0);
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

		[InjectOnClient]
		private IGameCameraEngine _gameCameraEngine;

		public Material WriterMaterial;

		public Material EffectMaterial;

		public float Size = 2f;

		public float CenterYOffset;

		public float CullingRadius = 7f;

		public LayerMask IgnoreLayer = 0;

		protected static RenderTexture RenderBuffer;

		protected static Mesh QuadMesh;

		private static int InstanceCount;

		private readonly List<MeshFilter> _filters = new List<MeshFilter>();

		private ObjectOverlay.PropertyIds _propertyIds;

		private CullingGroup _cullingGroup;

		private BoundingSphere[] _boundingSpheres;

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
