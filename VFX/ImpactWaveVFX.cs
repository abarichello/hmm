using System;
using HeavyMetalMachines.GameCamera;
using HeavyMetalMachines.Infra.DependencyInjection.Attributes;
using UnityEngine;

namespace HeavyMetalMachines.VFX
{
	internal class ImpactWaveVFX : BaseVFX
	{
		private void Awake()
		{
			this.propertyBlock = new MaterialPropertyBlock();
			if (this.startSize == this.finalSize || this.waveSpeed == 0f)
			{
				this.waitForAnimation = false;
			}
			this._colorPropertyId = Shader.PropertyToID("_Color");
			this._perturbIntensityPropertyId = Shader.PropertyToID("_PerturbIntensity");
		}

		private void OnDestroy()
		{
			this.propertyBlock = null;
		}

		private void CreateMesh()
		{
			Vector3[] vertices = new Vector3[]
			{
				new Vector3(-1f, -1f, 0f),
				new Vector3(1f, -1f, 0f),
				new Vector3(1f, 1f, 0f),
				new Vector3(-1f, 1f, 0f)
			};
			Vector2[] uv = new Vector2[]
			{
				new Vector2(0f, 0f),
				new Vector2(1f, 0f),
				new Vector2(1f, 1f),
				new Vector2(0f, 1f)
			};
			int[] array = new int[]
			{
				0,
				1,
				2,
				0,
				2,
				3
			};
			ImpactWaveVFX.planeMesh = new Mesh();
			ImpactWaveVFX.planeMesh.vertices = vertices;
			ImpactWaveVFX.planeMesh.uv = uv;
			ImpactWaveVFX.planeMesh.SetTriangles(array, 0);
		}

		private void LateUpdate()
		{
			if (!this._active)
			{
				return;
			}
			float num;
			if (this.startSize == this.finalSize)
			{
				num = 1f;
			}
			else if (this.finalSize > this.startSize)
			{
				this.currentSize += Time.deltaTime * this.waveSpeed;
				if (this.currentSize >= this.finalSize)
				{
					this.CanCollectToCache = true;
					this._active = false;
					return;
				}
				float num2 = this.finalSize - this.startSize;
				if (num2 <= 0f)
				{
					num2 = 1f;
				}
				num = (this.currentSize - this.startSize) / num2;
				num = Mathf.Clamp01(1f - num * num);
			}
			else
			{
				this.currentSize -= Time.deltaTime * this.waveSpeed;
				if (this.currentSize <= this.finalSize)
				{
					this.CanCollectToCache = true;
					this._active = false;
					return;
				}
				float num3 = this.startSize - this.finalSize;
				if (num3 <= 0f)
				{
					num3 = 1f;
				}
				num = (this.startSize - this.currentSize) / num3;
				num = Mathf.Clamp01(1f - num * num);
			}
			if (ImpactWaveVFX.planeMesh == null)
			{
				this.CreateMesh();
			}
			if (this.followTransorm)
			{
				this.position = base.transform.position;
			}
			Camera camera = Camera.main;
			ImpactWaveVFX.EImpactWaveAlignment eimpactWaveAlignment = this.alignment;
			if (eimpactWaveAlignment != ImpactWaveVFX.EImpactWaveAlignment.CameraAligned)
			{
				if (eimpactWaveAlignment != ImpactWaveVFX.EImpactWaveAlignment.GroundAligned)
				{
					if (eimpactWaveAlignment == ImpactWaveVFX.EImpactWaveAlignment.UseTransform)
					{
						this.matrix.SetTRS(this.position, base.transform.rotation * Quaternion.Euler(90f, 0f, 0f), new Vector3(this.currentSize, this.currentSize, this.currentSize));
					}
				}
				else
				{
					this.matrix.SetTRS(this.position, Quaternion.Euler(90f, 0f, 0f), new Vector3(this.currentSize, this.currentSize, this.currentSize));
				}
			}
			else
			{
				Transform transform = camera.transform;
				if (this._gameCameraEngine != null)
				{
					transform = this._gameCameraEngine.CameraTransform;
				}
				this.matrix.SetTRS(this.position, Quaternion.LookRotation(transform.position - this.position, transform.up), new Vector3(this.currentSize, this.currentSize, this.currentSize));
			}
			this.propertyBlock.SetColor(this._colorPropertyId, new Color(1f, 1f, 1f, num));
			this.propertyBlock.SetFloat(this._perturbIntensityPropertyId, num * this.intensity);
			if (this._gameCameraEngine != null)
			{
				camera = this._gameCameraEngine.UnityCamera;
			}
			Graphics.DrawMesh(ImpactWaveVFX.planeMesh, this.matrix, this.material, base.gameObject.layer, camera, 0, this.propertyBlock);
		}

		protected override void OnActivate()
		{
			this.currentSize = this.startSize;
			this._active = true;
			this.CanCollectToCache = false;
			this.position = base.transform.position;
		}

		protected override void WillDeactivate()
		{
			this._active = false;
			this.CanCollectToCache = true;
		}

		protected override void OnDeactivate()
		{
			if (!this.waitForAnimation)
			{
				this._active = false;
				this.CanCollectToCache = true;
			}
		}

		[InjectOnClient]
		private IGameCameraEngine _gameCameraEngine;

		private static Mesh planeMesh;

		private Matrix4x4 matrix = default(Matrix4x4);

		public Material material;

		public float waveSpeed = 200f;

		public float startSize;

		public float finalSize;

		public bool waitForAnimation;

		public bool followTransorm = true;

		public float intensity = 1f;

		public ImpactWaveVFX.EImpactWaveAlignment alignment;

		private float currentSize;

		private bool _active;

		private MaterialPropertyBlock propertyBlock;

		private Vector3 position;

		private int _colorPropertyId = -1;

		private int _perturbIntensityPropertyId = -1;

		private Transform _cameraTransform;

		public enum EImpactWaveAlignment
		{
			CameraAligned,
			GroundAligned,
			UseTransform
		}
	}
}
