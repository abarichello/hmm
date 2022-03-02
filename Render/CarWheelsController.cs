using System;
using HeavyMetalMachines.Car;
using HeavyMetalMachines.Utils;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Render
{
	public class CarWheelsController : GameHubBehaviour
	{
		private void Awake()
		{
			this._mainTexId = Shader.PropertyToID("_MainTex_ST");
			this._normalMapId = Shader.PropertyToID("_NormalMap_ST");
		}

		private void Start()
		{
			if (GameHubBehaviour.Hub != null && GameHubBehaviour.Hub.Net.IsServer() && !GameHubBehaviour.Hub.Net.IsTest())
			{
				base.enabled = false;
				return;
			}
		}

		public void Cleanup()
		{
		}

		public void UpdateWheelsMaterialTeam(Color color, float intensity)
		{
			if (this.steerRenderers == null || this.steerRenderers.Length <= 0)
			{
				return;
			}
			Debug.Assert(this.steerRenderers[0] != null && this.steerRenderers[0].sharedMaterial != null, "Null material or renderer on " + base.gameObject.name, Debug.TargetTeam.All);
			this.steerTextureMatrix = this.steerRenderers[0].sharedMaterial.GetVector(this._mainTexId);
			this.steerNormalTexMatrix = this.steerRenderers[0].sharedMaterial.GetVector(this._normalMapId);
			this.steerPropertyBlock = new MaterialPropertyBlock();
			this.steerPropertyBlock.SetVector(this._mainTexId, this.steerTextureMatrix);
			this.steerPropertyBlock.SetVector(this._normalMapId, this.steerNormalTexMatrix);
			int num = Shader.PropertyToID("_Glow");
			int num2 = Shader.PropertyToID("_GlowColor");
			for (int i = 0; i < this.steerRenderers.Length; i++)
			{
				Material sharedMaterial = this.steerRenderers[i].sharedMaterial;
				if (sharedMaterial.HasProperty(num) && sharedMaterial.HasProperty(num2))
				{
					this.steerRenderers[i].GetPropertyBlock(this.steerPropertyBlock);
					this.steerPropertyBlock.SetFloat(num, intensity);
					this.steerPropertyBlock.SetColor(num2, color);
					this.steerRenderers[i].SetPropertyBlock(this.steerPropertyBlock);
				}
			}
		}

		private void SafeDestroy(Object asset)
		{
			if (asset != null)
			{
				if (Application.isPlaying)
				{
					Object.Destroy(asset);
				}
				else
				{
					Object.DestroyImmediate(asset);
				}
			}
		}

		public CarMovement CarMovement
		{
			get
			{
				return this.carMovement;
			}
			set
			{
				this.carMovement = value;
			}
		}

		public void Refresh()
		{
			MeshFilter component = this.RightBackWheel.GetComponent<MeshFilter>();
			if (component && component.sharedMesh)
			{
				this.wheelsRadius = component.sharedMesh.bounds.size.y / 1f;
			}
		}

		private void LateUpdate()
		{
			if (this.carSuspensionGroup.freezePhysics)
			{
				return;
			}
			if (this.carMovement)
			{
				this.velocity = Mathf.Clamp(this.carMovement.VAxis * this.carMovement.MaxLinearSpeed, -8f, 8f);
				this.turning = Mathf.Lerp(this.turning, this.carMovement.HAxis, Time.deltaTime * 5f);
			}
			this.SyncWheelsPosition();
			float num = this.velocity / this.wheelsRadius * 57.29578f;
			this.currentWheelsRotation += num * Time.deltaTime;
			if (this._updateWheelSpin)
			{
				if (this.LeftFrontWheel)
				{
					this.LeftFrontWheel.transform.localRotation = Quaternion.Euler(this.currentWheelsRotation, this.turning * 25f, 0f);
				}
				if (this.RightFrontWheel)
				{
					this.RightFrontWheel.transform.localRotation = Quaternion.Euler(this.currentWheelsRotation, this.turning * 25f, 0f);
				}
				if (this.RightBackWheel)
				{
					this.RightBackWheel.transform.localRotation = Quaternion.Euler(this.currentWheelsRotation, 0f, 0f);
				}
				if (this.LeftBackWheel)
				{
					this.LeftBackWheel.transform.localRotation = Quaternion.Euler(this.currentWheelsRotation, 0f, 0f);
				}
			}
			if (this.steerPropertyBlock != null)
			{
				if (this.invertSteerRotation)
				{
					this.steerTextureMatrix.w = this.currentWheelsRotation / 500f;
				}
				else
				{
					this.steerTextureMatrix.w = -this.currentWheelsRotation / 500f;
				}
				this.steerNormalTexMatrix = this.steerTextureMatrix;
				this.steerPropertyBlock.SetVector(this._mainTexId, this.steerTextureMatrix);
				this.steerPropertyBlock.SetVector(this._normalMapId, this.steerNormalTexMatrix);
				for (int i = 0; i < this.steerRenderers.Length; i++)
				{
					this.steerRenderers[i].SetPropertyBlock(this.steerPropertyBlock);
				}
			}
		}

		private void SyncWheelsPosition()
		{
			if (!this._updateWheelPosition)
			{
				return;
			}
			this.LeftBackWheel.transform.position = this.carSuspensionGroup.backLeftWheel.transform.position;
			this.LeftFrontWheel.transform.position = this.carSuspensionGroup.frontLeftWheel.transform.position;
			this.RightFrontWheel.transform.position = this.carSuspensionGroup.frontRightWheel.transform.position;
			this.RightBackWheel.transform.position = this.carSuspensionGroup.backRightWheel.transform.position;
		}

		public void Reset()
		{
			if (this.carSuspensionGroup)
			{
				this.carSuspensionGroup.Reset();
			}
		}

		public GameObject LeftFrontWheel;

		public GameObject RightFrontWheel;

		public GameObject LeftBackWheel;

		public GameObject RightBackWheel;

		public GameObject wheelsTransform;

		public CarSuspensionGroup carSuspensionGroup;

		private Vector4 steerTextureMatrix;

		private Vector4 steerNormalTexMatrix;

		private MaterialPropertyBlock steerPropertyBlock;

		public Renderer[] steerRenderers;

		public Material skidMarkMaterial;

		public float wheelsRadius = 1f;

		public float velocity;

		[Range(-1f, 1f)]
		public float turning;

		[HideInInspector]
		public float currentWheelsRotation;

		[SerializeField]
		private bool _updateWheelSpin = true;

		[SerializeField]
		private bool _updateWheelPosition = true;

		[SerializeField]
		private bool invertSteerRotation;

		private int _mainTexId;

		private int _normalMapId;

		private float shakeTimming;

		private CarMovement carMovement;

		private float angle;
	}
}
