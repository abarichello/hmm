using System;
using System.Collections.Generic;
using HeavyMetalMachines.GameCamera;
using HeavyMetalMachines.Infra.DependencyInjection.Attributes;
using HeavyMetalMachines.Match;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.VFX
{
	public class HMMTeamObjectOverlayVFX : BaseVFX
	{
		protected void Awake()
		{
			base.enabled = false;
			HMMTeamObjectOverlayVFX.InstanceCount++;
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
			if (HMMTeamObjectOverlayVFX.RenderBuffer == null)
			{
				HMMTeamObjectOverlayVFX.RenderBuffer = new RenderTexture(512, 512, 24);
				HMMTeamObjectOverlayVFX.RenderBuffer.autoGenerateMips = false;
				HMMTeamObjectOverlayVFX.RenderBuffer.useMipMap = false;
			}
			if (HMMTeamObjectOverlayVFX.QuadMesh == null)
			{
				HMMTeamObjectOverlayVFX.QuadMesh = new Mesh();
				HMMTeamObjectOverlayVFX.QuadMesh.vertices = new Vector3[]
				{
					new Vector3(-1f, -1f),
					new Vector3(1f, -1f),
					new Vector3(1f, 1f),
					new Vector3(-1f, 1f)
				};
				HMMTeamObjectOverlayVFX.QuadMesh.uv = new Vector2[]
				{
					new Vector2(0f, 0f),
					new Vector2(1f, 0f),
					new Vector2(1f, 1f),
					new Vector2(0f, 1f)
				};
				HMMTeamObjectOverlayVFX.QuadMesh.triangles = new int[]
				{
					0,
					1,
					2,
					0,
					2,
					3
				};
			}
			this._writerInstance = new Material(this.WriterShader);
		}

		protected void OnDestroy()
		{
			this._cullingGroup.Dispose();
			this._cullingGroup = null;
			this._boundingSpheres = null;
			this._writerInstance = null;
			HMMTeamObjectOverlayVFX.InstanceCount--;
			if (HMMTeamObjectOverlayVFX.InstanceCount > 0)
			{
				return;
			}
			HMMTeamObjectOverlayVFX.InstanceCount = 0;
			Object.Destroy(HMMTeamObjectOverlayVFX.RenderBuffer);
			Object.Destroy(HMMTeamObjectOverlayVFX.QuadMesh);
			Object.Destroy(this._writerInstance);
			HMMTeamObjectOverlayVFX.RenderBuffer = null;
			HMMTeamObjectOverlayVFX.QuadMesh = null;
		}

		protected void Update()
		{
			Vector3 position = this._targetObject.position;
			position.y += this.CenterYOffset;
			this._boundingSpheres[0].position = position;
		}

		protected void OnRenderObject()
		{
			if (this._filters.Count == 0 || this._targetObject == null)
			{
				return;
			}
			Camera current = Camera.current;
			if (current.GetInstanceID() != this._gameCameraEngine.UnityCamera.GetInstanceID() || (current.cullingMask & 1) == 0)
			{
				return;
			}
			if (!this._cullingGroup.IsVisible(0))
			{
				return;
			}
			RenderTexture active = RenderTexture.active;
			RenderTexture.active = HMMTeamObjectOverlayVFX.RenderBuffer;
			GL.Clear(true, true, new Color(0f, 0f, 0f, 0f));
			Vector3 position = this._targetObject.position;
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
			this._writerInstance.SetMatrix(this._propertyIds.MV, matrix4x3);
			for (int k = 0; k < this._filters.Count; k++)
			{
				this._writerInstance.SetMatrix(this._propertyIds.Model, this._filters[k].transform.localToWorldMatrix);
				if (this._writerInstance.SetPass(0))
				{
					Graphics.DrawMeshNow(this._filters[k].sharedMesh, this._filters[k].transform.position, Quaternion.identity);
				}
			}
			RenderTexture.active = active;
			this._materialInstance.SetTexture(this._propertyIds.RT, HMMTeamObjectOverlayVFX.RenderBuffer);
			if (this._materialInstance.SetPass(0))
			{
				Matrix4x4 matrix4x4 = Matrix4x4.TRS(position, quaternion, new Vector3(this.Size, this.Size, this.Size));
				Graphics.DrawMeshNow(HMMTeamObjectOverlayVFX.QuadMesh, matrix4x4, 0);
			}
		}

		public void SetColors(Color colorA, Color colorB)
		{
			this._materialInstance.SetColor(this._propertyIds.Color, colorA);
			this._materialInstance.SetColor(this._propertyIds.OutlineColor, colorB);
		}

		protected override void OnActivate()
		{
			this._targetObject = base.transform;
			switch (this.EffectTarget)
			{
			case HMMTeamObjectOverlayVFX.ETarget.Owner:
				this._targetObject = this._targetFXInfo.Owner.transform;
				break;
			case HMMTeamObjectOverlayVFX.ETarget.Target:
				this._targetObject = this._targetFXInfo.Target.transform;
				break;
			}
			MeshFilter[] componentsInChildren = this._targetObject.GetComponentsInChildren<MeshFilter>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				if ((this.IgnoreLayer.value & 1 << componentsInChildren[i].gameObject.layer) == 0)
				{
					this._filters.Add(componentsInChildren[i]);
				}
			}
			TeamKind team = GameHubBehaviour.Hub.Players.GetPlayerOrBotsByObjectId(this._targetFXInfo.Owner.ObjId).Team;
			this._materialInstance = Object.Instantiate<Material>((team != GameHubBehaviour.Hub.Players.CurrentPlayerTeam) ? this.EnemyMaterial : this.AllyMaterial);
			this._cullingGroup.enabled = true;
			base.enabled = true;
		}

		protected override void WillDeactivate()
		{
		}

		protected override void OnDeactivate()
		{
			base.enabled = false;
			this._cullingGroup.enabled = false;
			this._filters.Clear();
			this._targetObject = null;
			Object.Destroy(this._materialInstance);
			this._materialInstance = null;
		}

		[InjectOnClient]
		private IGameCameraEngine _gameCameraEngine;

		public Shader WriterShader;

		public Material AllyMaterial;

		public Material EnemyMaterial;

		public float Size = 2f;

		public float CenterYOffset;

		public HMMTeamObjectOverlayVFX.ETarget EffectTarget;

		public LayerMask IgnoreLayer = 0;

		public float CullingRadius = 7f;

		protected static RenderTexture RenderBuffer;

		protected static Mesh QuadMesh;

		private static int InstanceCount;

		private readonly List<MeshFilter> _filters = new List<MeshFilter>();

		private Material _materialInstance;

		private Material _writerInstance;

		private Transform _targetObject;

		private HMMTeamObjectOverlayVFX.PropertyIds _propertyIds;

		private CullingGroup _cullingGroup;

		private BoundingSphere[] _boundingSpheres;

		public enum ETarget
		{
			Owner,
			Target,
			VFX
		}

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
