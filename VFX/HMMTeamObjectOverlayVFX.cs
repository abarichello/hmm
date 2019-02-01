using System;
using System.Collections.Generic;
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
			HMMTeamObjectOverlayVFX.InstanceCount--;
			if (HMMTeamObjectOverlayVFX.InstanceCount > 0)
			{
				return;
			}
			HMMTeamObjectOverlayVFX.InstanceCount = 0;
			UnityEngine.Object.Destroy(HMMTeamObjectOverlayVFX.RenderBuffer);
			UnityEngine.Object.Destroy(HMMTeamObjectOverlayVFX.QuadMesh);
			UnityEngine.Object.Destroy(this._writerInstance);
			HMMTeamObjectOverlayVFX.RenderBuffer = null;
			HMMTeamObjectOverlayVFX.QuadMesh = null;
			this._writerInstance = null;
		}

		protected void OnRenderObject()
		{
			if (this._filters.Count == 0 || this._targetObject == null)
			{
				return;
			}
			Camera current = Camera.current;
			if (current.GetInstanceID() != CarCamera.Singleton.CameraInstanceId || (current.cullingMask & 1) == 0)
			{
				return;
			}
			RenderTexture active = RenderTexture.active;
			RenderTexture.active = HMMTeamObjectOverlayVFX.RenderBuffer;
			GL.Clear(true, true, new Color(0f, 0f, 0f, 0f));
			Vector3 position = this._targetObject.position;
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
			this._writerInstance.SetMatrix(this._propertyIds.MV, value);
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
				Matrix4x4 matrix = Matrix4x4.TRS(position, q, new Vector3(this.Size, this.Size, this.Size));
				Graphics.DrawMeshNow(HMMTeamObjectOverlayVFX.QuadMesh, matrix, 0);
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
			this._materialInstance = UnityEngine.Object.Instantiate<Material>((team != GameHubBehaviour.Hub.Players.CurrentPlayerTeam) ? this.EnemyMaterial : this.AllyMaterial);
			base.enabled = true;
		}

		protected override void WillDeactivate()
		{
		}

		protected override void OnDeactivate()
		{
			base.enabled = false;
			this._filters.Clear();
			this._targetObject = null;
			UnityEngine.Object.Destroy(this._materialInstance);
			this._materialInstance = null;
		}

		public Shader WriterShader;

		public Material AllyMaterial;

		public Material EnemyMaterial;

		public float Size = 2f;

		public float CenterYOffset;

		public HMMTeamObjectOverlayVFX.ETarget EffectTarget;

		public LayerMask IgnoreLayer = 0;

		protected static RenderTexture RenderBuffer;

		protected static Mesh QuadMesh;

		private static int InstanceCount;

		private readonly List<MeshFilter> _filters = new List<MeshFilter>();

		private Material _materialInstance;

		private Material _writerInstance;

		private Transform _targetObject;

		private HMMTeamObjectOverlayVFX.PropertyIds _propertyIds;

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
