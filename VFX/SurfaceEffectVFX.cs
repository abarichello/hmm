using System;
using System.Collections.Generic;
using HeavyMetalMachines.Render;
using HeavyMetalMachines.Utils;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.VFX
{
	public class SurfaceEffectVFX : BaseVFX
	{
		public ManagedPropertyBlock ExternalBlock { get; private set; }

		protected virtual void Awake()
		{
			this._rendererHolders = new List<SurfaceEffectVFX.RendererHolder>(5);
			this._getComponentCache = new List<MeshRenderer>(5);
			this.ExternalBlock = new ManagedPropertyBlock();
			this.MaterialInstance = Object.Instantiate<Material>(this.overlapMaterial);
			this.PropertyIds.Color = Shader.PropertyToID("_Color");
			this.PropertyIds.MainObjectTransform = Shader.PropertyToID("_MainObjectTransform");
			this.PropertyIds.MainObjectInverseTransform = Shader.PropertyToID("_MainObjectInverseTransform");
			this.PropertyIds.DirectionShaderPropName = Shader.PropertyToID(this.DirectionShaderPropName);
			this.EnableFading = false;
			if (this.MaterialInstance != null && this.MaterialInstance.HasProperty(this.PropertyIds.Color) && (this.FadeInDuration > 0f || this.FadeOutDuration > 0f))
			{
				this.EnableFading = true;
				this.OriginalColor = this.MaterialInstance.GetColor(this.PropertyIds.Color);
			}
			if (this.EnableDirection && string.IsNullOrEmpty(this.DirectionShaderPropName))
			{
				this.DirectionShaderPropName = SurfaceEffectVFX.DefaultDirectionShaderPropName;
			}
		}

		private void InitializeFX()
		{
			if (this._rendererHolders.Capacity < this._getComponentCache.Count)
			{
				this._rendererHolders.Capacity = this._getComponentCache.Count;
			}
			for (int i = 0; i < this._getComponentCache.Count; i++)
			{
				MeshRenderer meshRenderer = this._getComponentCache[i];
				if (!(meshRenderer == null))
				{
					if ((this.ignoreLayer.value & 1 << meshRenderer.gameObject.layer) == 0)
					{
						MeshFilter component = meshRenderer.GetComponent<MeshFilter>();
						if (component.sharedMesh == null)
						{
							SurfaceEffectVFX.Log.WarnFormat("Broken MeshFilter. GO Name: {0} Effect GO Name: {1}", new object[]
							{
								this._targetTransform.gameObject.name,
								base.gameObject.name
							});
						}
						else
						{
							SurfaceEffectVFX.RendererHolder item = new SurfaceEffectVFX.RendererHolder
							{
								MeshFilter = component,
								Renderer = meshRenderer,
								PropertyBlock = SurfaceEffectVFX.GetPropertyBlockFromCache()
							};
							if (this.EnableFading)
							{
								item.PropertyBlock.SetColor(this.PropertyIds.Color, this.OriginalColor);
							}
							if (this.shouldHideObject)
							{
								item.ShouldReenable = item.Renderer.enabled;
								item.Renderer.enabled = false;
							}
							this._rendererHolders.Add(item);
						}
					}
				}
			}
			this._getComponentCache.Clear();
			this._initialized = true;
			this._isDeactivating = false;
			this.CanCollectToCache = false;
			this._fadeState = SurfaceEffectVFX.FadeState.FadeIn;
			this._fadeStartTime = Time.time;
		}

		private void LateUpdate()
		{
			if (!this._initialized)
			{
				return;
			}
			float time = Time.time;
			if (this._isDeactivating && this._fadeState == SurfaceEffectVFX.FadeState.Complete)
			{
				this.Disable();
				return;
			}
			if (this.ignoreXZRotation)
			{
				Matrix4x4 matrix4x = Matrix4x4.TRS(this._targetTransform.position, Quaternion.Euler(0f, this._targetTransform.eulerAngles.y, 0f), Vector3.one);
				Matrix4x4 inverse = matrix4x.inverse;
				this.MaterialInstance.SetMatrix(this.PropertyIds.MainObjectTransform, matrix4x);
				this.MaterialInstance.SetMatrix(this.PropertyIds.MainObjectInverseTransform, inverse);
			}
			else
			{
				Matrix4x4 localToWorldMatrix = this._targetTransform.localToWorldMatrix;
				Matrix4x4 worldToLocalMatrix = this._targetTransform.worldToLocalMatrix;
				this.MaterialInstance.SetMatrix(this.PropertyIds.MainObjectTransform, localToWorldMatrix);
				this.MaterialInstance.SetMatrix(this.PropertyIds.MainObjectInverseTransform, worldToLocalMatrix);
			}
			if (this.EnableDirection && this.UpdateDirectionEveryFrame)
			{
				this._direction = (this._directionTarget.position - this._directionOrigin.position).normalized;
			}
			Color originalColor = this.OriginalColor;
			if (this.EnableFading)
			{
				SurfaceEffectVFX.FadeState fadeState = this._fadeState;
				if (fadeState != SurfaceEffectVFX.FadeState.FadeIn)
				{
					if (fadeState == SurfaceEffectVFX.FadeState.FadeOut)
					{
						if (time < this._fadeStartTime + this.FadeOutDuration)
						{
							originalColor.a *= Mathf.Clamp01(1f - (time - this._fadeStartTime) / this.FadeOutDuration);
						}
						else
						{
							this._fadeState = SurfaceEffectVFX.FadeState.Complete;
						}
					}
				}
				else if (time < this._fadeStartTime + this.FadeInDuration)
				{
					originalColor.a *= Mathf.Clamp01((time - this._fadeStartTime) / this.FadeInDuration);
				}
				else if (this._isDeactivating)
				{
					this._fadeState = SurfaceEffectVFX.FadeState.FadeOut;
					this._fadeStartTime = time;
				}
				else
				{
					this._fadeState = SurfaceEffectVFX.FadeState.Idle;
				}
			}
			for (int i = 0; i < this._rendererHolders.Count; i++)
			{
				SurfaceEffectVFX.RendererHolder rendererHolder = this._rendererHolders[i];
				Renderer renderer = rendererHolder.Renderer;
				if ((renderer.enabled || this.shouldHideObject) && renderer.gameObject.activeInHierarchy)
				{
					if (this.EnableDirection)
					{
						rendererHolder.PropertyBlock.SetVector(this.PropertyIds.DirectionShaderPropName, this._direction);
					}
					if (this.EnableFading)
					{
						rendererHolder.PropertyBlock.SetVector(this.PropertyIds.Color, originalColor);
					}
					this.ExternalBlock.CopyTo(rendererHolder.PropertyBlock);
					for (int j = 0; j < rendererHolder.MeshFilter.sharedMesh.subMeshCount; j++)
					{
						Graphics.DrawMesh(rendererHolder.MeshFilter.sharedMesh, rendererHolder.MeshFilter.transform.localToWorldMatrix, this.MaterialInstance, base.gameObject.layer, null, j, rendererHolder.PropertyBlock);
					}
				}
			}
		}

		protected override void OnActivate()
		{
			this._targetTransform = null;
			switch (this.surfaceTarget)
			{
			case SurfaceEffectVFX.ESurfaceTarget.EffectOwner:
				if (this._targetFXInfo.Owner)
				{
					this._targetTransform = this._targetFXInfo.Owner.transform;
				}
				break;
			case SurfaceEffectVFX.ESurfaceTarget.EffectTarget:
				if (this._targetFXInfo.Target)
				{
					this._targetTransform = this._targetFXInfo.Target.transform;
				}
				break;
			case SurfaceEffectVFX.ESurfaceTarget.Effect:
				this._targetTransform = this._targetFXInfo.EffectTransform;
				break;
			case SurfaceEffectVFX.ESurfaceTarget.Self:
				this._targetTransform = base.transform;
				break;
			default:
				Debug.Assert(true, "There's no code for the selected target type!", Debug.TargetTeam.All);
				break;
			}
			if (this._targetTransform == null)
			{
				Debug.Assert(true, "Ops, something is wrong with the " + base.gameObject.name + " prefab config! SurfaceEffectVFX target is null!", Debug.TargetTeam.All);
				return;
			}
			if (this.EnableDirection)
			{
				AttachVFX.AttachType directionOrigin = this.DirectionOrigin;
				if (directionOrigin != AttachVFX.AttachType.EffectOwner)
				{
					if (directionOrigin != AttachVFX.AttachType.EffectTarget)
					{
						if (directionOrigin == AttachVFX.AttachType.Effect)
						{
							this._directionOrigin = this._targetFXInfo.EffectTransform;
						}
					}
					else
					{
						this._directionOrigin = this._targetFXInfo.Target.transform;
					}
				}
				else
				{
					this._directionOrigin = this._targetFXInfo.Owner.transform;
				}
				AttachVFX.AttachType directionTarget = this.DirectionTarget;
				if (directionTarget != AttachVFX.AttachType.EffectOwner)
				{
					if (directionTarget != AttachVFX.AttachType.EffectTarget)
					{
						if (directionTarget == AttachVFX.AttachType.Effect)
						{
							this._directionTarget = this._targetFXInfo.EffectTransform;
						}
					}
					else
					{
						this._directionTarget = this._targetFXInfo.Target.transform;
					}
				}
				else
				{
					this._directionTarget = this._targetFXInfo.Owner.transform;
				}
				this._direction = (this._directionTarget.position - this._directionOrigin.position).normalized;
			}
			CarComponentHub component = this._targetTransform.GetComponent<CarComponentHub>();
			Transform transform;
			if (component != null)
			{
				transform = component.carGenerator.transform;
			}
			else
			{
				transform = this._targetTransform;
			}
			transform.GetComponentsInChildren<MeshRenderer>(true, this._getComponentCache);
			this.InitializeFX();
		}

		protected override void WillDeactivate()
		{
		}

		protected override void OnDeactivate()
		{
			if (this.EnableFading)
			{
				this._isDeactivating = true;
				if (this._fadeState == SurfaceEffectVFX.FadeState.Idle)
				{
					this._fadeState = SurfaceEffectVFX.FadeState.FadeOut;
					this._fadeStartTime = Time.time;
				}
			}
			else
			{
				this.Disable();
			}
		}

		private void Disable()
		{
			this._initialized = false;
			this.CanCollectToCache = true;
			for (int i = 0; i < this._rendererHolders.Count; i++)
			{
				SurfaceEffectVFX.RendererHolder rendererHolder = this._rendererHolders[i];
				if (rendererHolder.Renderer != null && rendererHolder.ShouldReenable)
				{
					rendererHolder.Renderer.enabled = true;
				}
				SurfaceEffectVFX.PropertyBlockCache.Push(rendererHolder.PropertyBlock);
				rendererHolder.PropertyBlock = null;
			}
			this._rendererHolders.Clear();
			this.ExternalBlock.Clear();
		}

		private static MaterialPropertyBlock GetPropertyBlockFromCache()
		{
			MaterialPropertyBlock materialPropertyBlock;
			if (SurfaceEffectVFX.PropertyBlockCache.Count > 0)
			{
				materialPropertyBlock = SurfaceEffectVFX.PropertyBlockCache.Pop();
				materialPropertyBlock.Clear();
			}
			else
			{
				materialPropertyBlock = new MaterialPropertyBlock();
			}
			return materialPropertyBlock;
		}

		private static readonly BitLogger Log = new BitLogger(typeof(SurfaceEffectVFX));

		private static readonly Stack<MaterialPropertyBlock> PropertyBlockCache = new Stack<MaterialPropertyBlock>();

		public SurfaceEffectVFX.ESurfaceTarget surfaceTarget = SurfaceEffectVFX.ESurfaceTarget.Self;

		public Material overlapMaterial;

		public bool shouldHideObject;

		public bool isAlwaysOn;

		public bool ignoreXZRotation;

		public LayerMask ignoreLayer;

		public bool EnableDirection;

		public bool UpdateDirectionEveryFrame;

		public AttachVFX.AttachType DirectionOrigin;

		public AttachVFX.AttachType DirectionTarget = AttachVFX.AttachType.EffectTarget;

		public string DirectionShaderPropName = SurfaceEffectVFX.DefaultDirectionShaderPropName;

		public static readonly string DefaultDirectionShaderPropName = "_LocalRefPoint";

		[Tooltip("Fade in duration in seconds")]
		public float FadeInDuration;

		[Tooltip("Fade out duration in seconds")]
		public float FadeOutDuration;

		protected Color OriginalColor;

		protected bool EnableFading;

		private float _fadeStartTime;

		protected SurfaceEffectVFX.MaterialProperties PropertyIds;

		protected Material MaterialInstance;

		private bool _initialized;

		private SurfaceEffectVFX.FadeState _fadeState;

		private Transform _targetTransform;

		private bool _isDeactivating;

		private Transform _directionOrigin;

		private Transform _directionTarget;

		private Vector4 _direction = Vector4.zero;

		private List<SurfaceEffectVFX.RendererHolder> _rendererHolders;

		private List<MeshRenderer> _getComponentCache;

		public enum ESurfaceTarget
		{
			EffectOwner,
			EffectTarget,
			Effect,
			Self
		}

		private enum FadeState
		{
			Idle,
			FadeIn,
			FadeOut,
			Complete
		}

		protected struct MaterialProperties
		{
			public int Color;

			public int MainObjectTransform;

			public int MainObjectInverseTransform;

			public int DirectionShaderPropName;
		}

		[Serializable]
		private struct RendererHolder
		{
			public MaterialPropertyBlock PropertyBlock;

			public MeshFilter MeshFilter;

			public MeshRenderer Renderer;

			public bool ShouldReenable;
		}
	}
}
