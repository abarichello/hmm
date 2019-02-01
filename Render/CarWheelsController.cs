using System;
using HeavyMetalMachines.Car;
using HeavyMetalMachines.Utils;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Render
{
	public class CarWheelsController : GameHubBehaviour
	{
		private void Start()
		{
			if (GameHubBehaviour.Hub != null && GameHubBehaviour.Hub.Net.IsServer() && !GameHubBehaviour.Hub.Net.IsTest())
			{
				base.enabled = false;
				return;
			}
			this.rb = base.transform.root.GetComponentInChildren<Rigidbody>();
			if (this.steerRenderers != null && this.steerRenderers.Length > 0)
			{
				HeavyMetalMachines.Utils.Debug.Assert(this.steerRenderers[0] != null && this.steerRenderers[0].sharedMaterial != null, "Null material or renderer on " + base.gameObject.name, HeavyMetalMachines.Utils.Debug.TargetTeam.All);
				this.steerTextureMatrix = this.steerRenderers[0].sharedMaterial.GetVector("_MainTex_ST");
				this.steerPropertyBlock = new MaterialPropertyBlock();
				this.steerPropertyBlock.SetVector("_MainTex_ST", this.steerTextureMatrix);
			}
		}

		private void OnWheelSuspensionHit()
		{
			this.forcedEmissionDelay = 0.5f;
		}

		public void Cleanup()
		{
		}

		private void SafeDestroy(UnityEngine.Object asset)
		{
			if (asset != null)
			{
				if (Application.isPlaying)
				{
					UnityEngine.Object.Destroy(asset);
				}
				else
				{
					UnityEngine.Object.DestroyImmediate(asset);
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

		private void OnWheelAxisHitGround(float f)
		{
			this.forcedEmissionDelay = 0.5f;
			this.forcedSmokeEmissionDelay = 0.5f;
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
				float num = (float)((this.carMovement.HAxis <= 0f) ? ((this.carMovement.HAxis >= 0f) ? 0 : -1) : 1);
				float num2 = (float)((this.carMovement.VAxis <= 0f) ? ((this.carMovement.VAxis >= 0f) ? 0 : -1) : 1);
				bool flag = num2 != this.oldVAxis;
				if (flag || (this.oldHAxis != num && num != 0f && num2 != 0f))
				{
					this.forcedEmissionDelay = 0.5f;
					if (flag)
					{
						this.forcedSmokeEmissionDelay = 0.5f;
					}
				}
				this.oldHAxis = num;
				this.oldVAxis = num2;
				this.forcedEmissionDelay -= Time.deltaTime;
				this.forcedSmokeEmissionDelay -= Time.deltaTime;
			}
			this.SyncWhellsPosition();
			float num3 = this.velocity / this.wheelsRadius * 57.29578f;
			this.currentWheelsRotation += num3 * Time.deltaTime;
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
				this.steerTextureMatrix.w = -this.currentWheelsRotation / 500f;
				this.steerPropertyBlock.SetVector("_MainTex_ST", this.steerTextureMatrix);
				for (int i = 0; i < this.steerRenderers.Length; i++)
				{
					this.steerRenderers[i].SetPropertyBlock(this.steerPropertyBlock);
				}
			}
		}

		private void SyncWhellsPosition()
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

		private Rigidbody rb;

		private float shakeTimming;

		private CarMovement carMovement;

		private float forcedEmissionDelay;

		private float forcedSmokeEmissionDelay;

		private float oldVAxis;

		private float oldHAxis;

		private float angle;
	}
}
