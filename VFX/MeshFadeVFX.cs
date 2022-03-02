using System;
using UnityEngine;

namespace HeavyMetalMachines.VFX
{
	[HelpURL("https://confluence.hoplon.com/display/HMM/Mesh+Fade+VFX")]
	internal class MeshFadeVFX : BaseVFX
	{
		public override int Priority
		{
			get
			{
				return base.Priority - 1000;
			}
		}

		private void Awake()
		{
			this._colorPropertyId = Shader.PropertyToID("_Color");
			if (this.enableSpecularIntensity)
			{
				this._specularIntendityPropertyId = Shader.PropertyToID("Gloss");
			}
			this._materialPropertyBlock = new MaterialPropertyBlock();
			this._rendererData = new MeshFadeVFX.RendererControlData[this.renderers.Length + ((!this.GetRendererDuringRuntime) ? 0 : 1)];
			base.enabled = (this.GetRendererDuringRuntime || (this.renderers != null && this.renderers.Length > 0));
		}

		private void OnDestroy()
		{
			this._rendererData = null;
			this._runtimeRenderer = null;
			this._materialPropertyBlock = null;
		}

		private void LateUpdate()
		{
			switch (this._currentState)
			{
			case MeshFadeVFX.States.Idle:
				return;
			case MeshFadeVFX.States.FadeIn:
				this._alpha += Time.deltaTime / this.fadeInTime;
				if (this._alpha > 1f)
				{
					this._alpha = 1f;
					if (this.hasLifeFixedDuration)
					{
						this.fadeOutDelay = this.lifeFixedDuration;
						this._deactivateTime = Time.time;
						this._currentState = MeshFadeVFX.States.Deactivating;
						return;
					}
					if (this.spawnStage == BaseVFX.SpawnState.OnActivate)
					{
						this._currentState = MeshFadeVFX.States.Idle;
					}
					else
					{
						this._deactivateTime = Time.time;
						this._currentState = MeshFadeVFX.States.Deactivating;
					}
				}
				break;
			case MeshFadeVFX.States.FadeOut:
				this._alpha -= Time.deltaTime / this.fadeOutTime;
				if (this._alpha < 0f)
				{
					this._alpha = 0f;
					this._currentState = MeshFadeVFX.States.TurnOff;
				}
				break;
			case MeshFadeVFX.States.TurnOff:
				this.CanCollectToCache = true;
				this._currentState = MeshFadeVFX.States.Idle;
				return;
			case MeshFadeVFX.States.Deactivating:
				if (Time.time - this._deactivateTime < this.fadeOutDelay)
				{
					return;
				}
				this._currentState = MeshFadeVFX.States.FadeOut;
				break;
			}
			for (int i = 0; i < this.renderers.Length; i++)
			{
				Color color = this._rendererData[i].color;
				color.a *= this._alpha;
				this.renderers[i].GetPropertyBlock(this._materialPropertyBlock);
				this._materialPropertyBlock.SetColor(this._colorPropertyId, color);
				if (this.enableSpecularIntensity)
				{
					this._materialPropertyBlock.SetFloat(this._specularIntendityPropertyId, this.SpecularIntensity * this._alpha);
				}
				this.renderers[i].SetPropertyBlock(this._materialPropertyBlock);
				this.renderers[i].enabled = true;
			}
			if (this._runtimeRenderer != null)
			{
				Color color2 = this._rendererData[this._rendererData.Length - 1].color;
				color2.a *= this._alpha;
				this._runtimeRenderer.GetPropertyBlock(this._materialPropertyBlock);
				this._materialPropertyBlock.SetColor(this._colorPropertyId, color2);
				if (this.enableSpecularIntensity)
				{
					this._materialPropertyBlock.SetFloat(this._specularIntendityPropertyId, this.SpecularIntensity * this._alpha);
				}
				this._runtimeRenderer.SetPropertyBlock(this._materialPropertyBlock);
				this._runtimeRenderer.enabled = true;
			}
			if (this._currentState == MeshFadeVFX.States.TurnOff)
			{
				for (int j = 0; j < this.renderers.Length; j++)
				{
					this.renderers[j].enabled = this._rendererData[j].enabled;
				}
				if (this._runtimeRenderer != null)
				{
					this._runtimeRenderer.enabled = this._rendererData[this._rendererData.Length - 1].enabled;
				}
			}
		}

		protected override void OnActivate()
		{
			if (base.enabled && base.gameObject.activeInHierarchy && (this.GetRendererDuringRuntime || (this.renderers != null && this.renderers.Length > 0)))
			{
				this._alpha = 0f;
				this._currentState = MeshFadeVFX.States.FadeIn;
				this.CanCollectToCache = false;
				this.GetRuntimeRenderer();
				for (int i = 0; i < this.renderers.Length; i++)
				{
					this._rendererData[i].enabled = this.renderers[i].enabled;
					this._rendererData[i].color = this.renderers[i].sharedMaterial.GetColor(this._colorPropertyId);
				}
				if (this._runtimeRenderer != null)
				{
					this._rendererData[this._rendererData.Length - 1].enabled = this._runtimeRenderer.enabled;
					this._rendererData[this._rendererData.Length - 1].color = this._runtimeRenderer.sharedMaterial.GetColor(this._colorPropertyId);
				}
			}
			else
			{
				this.CanCollectToCache = true;
			}
		}

		protected override void WillDeactivate()
		{
			this._deactivateTime = Time.time;
			this._currentState = MeshFadeVFX.States.Deactivating;
		}

		protected override void OnDeactivate()
		{
			this._deactivateTime = Time.time;
			this._currentState = MeshFadeVFX.States.Deactivating;
		}

		private void GetRuntimeRenderer()
		{
			if (this.GetRendererDuringRuntime && this._runtimeRenderer == null)
			{
				this._runtimeRenderer = base.GetComponent<Renderer>();
				if (this._runtimeRenderer != null)
				{
					this._rendererData[this._rendererData.Length - 1].color = ((!(this._runtimeRenderer.sharedMaterial != null)) ? this._runtimeRenderer.material.GetColor(this._colorPropertyId) : this._runtimeRenderer.sharedMaterial.GetColor(this._colorPropertyId));
				}
			}
		}

		protected void OnValidate()
		{
			this.fadeInTime = Mathf.Max(0f, this.fadeInTime);
			this.fadeOutTime = Mathf.Max(0f, this.fadeOutTime);
			this.fadeOutDelay = Mathf.Max(0f, this.fadeOutDelay);
			if (this.renderers != null)
			{
				int num = 0;
				for (int i = 0; i < this.renderers.Length; i++)
				{
					if (this.renderers[i] == null)
					{
						num++;
					}
					else if (this.renderers[i].sharedMaterial != null && !this.renderers[i].sharedMaterial.HasProperty("_Color"))
					{
						Debug.LogError(string.Format("MeshFadeVFX requires {0} to have a _Color property!", this.renderers[i].gameObject.name), this.renderers[i].gameObject);
					}
				}
				if (num > 0)
				{
					Renderer[] array = new Renderer[this.renderers.Length - num];
					int j = 0;
					int num2 = 0;
					while (j < this.renderers.Length)
					{
						if (this.renderers[j] != null)
						{
							array[num2++] = this.renderers[j];
						}
						j++;
					}
					this.renderers = array;
				}
			}
		}

		private MeshFadeVFX.States _currentState;

		public Renderer[] renderers;

		private MeshFadeVFX.RendererControlData[] _rendererData;

		private int _colorPropertyId;

		private MaterialPropertyBlock _materialPropertyBlock;

		private float _alpha;

		private float _deactivateTime;

		[Tooltip("It will fade out after a fixed duration, instead of the default 'On destroy' fade out.")]
		[SerializeField]
		private bool hasLifeFixedDuration;

		[Tooltip("Fixed duration time, it'll only be considered if 'Has Life Fixed Duration' is selected.")]
		[SerializeField]
		private float lifeFixedDuration;

		[Tooltip("Time in seconds to fade in.")]
		public float fadeInTime;

		[Tooltip("Time in seconds to fade out.")]
		public float fadeOutTime;

		[Tooltip("Delay in seconds to wait after fading in and then start fading out.")]
		public float fadeOutDelay;

		[Tooltip("Use GetComponent<Renderer> during OnActivate to get the renderer in this GameObject.")]
		public bool GetRendererDuringRuntime;

		private Renderer _runtimeRenderer;

		[Header("Additional properties animation")]
		private int _specularIntendityPropertyId;

		[SerializeField]
		private bool enableSpecularIntensity;

		[SerializeField]
		private float SpecularIntensity = 0.75f;

		private enum States
		{
			Idle,
			FadeIn,
			FadeOut,
			TurnOff,
			Deactivating
		}

		private struct RendererControlData
		{
			public Color color;

			public bool enabled;
		}
	}
}
