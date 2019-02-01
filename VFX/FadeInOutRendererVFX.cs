using System;
using UnityEngine;

namespace HeavyMetalMachines.VFX
{
	public class FadeInOutRendererVFX : BaseVFX
	{
		private void OnEnable()
		{
			this.targetRenderer.gameObject.SetActive(false);
		}

		protected override void OnActivate()
		{
			if (this.targetRenderer && base.gameObject.activeInHierarchy)
			{
				this.targetRenderer.gameObject.SetActive(true);
				this.materials = this.targetRenderer.materials;
				this.currentState = FadeInOutRendererVFX.EState.FadeIn;
				this.currentTime = 0f;
				this.CanCollectToCache = false;
				this.SetMaterialsColor(new Color(0f, 0f, 0f, 0f));
				return;
			}
			this.CanCollectToCache = true;
		}

		protected override void WillDeactivate()
		{
			if (this.targetRenderer == null)
			{
				return;
			}
			if (this.currentState == FadeInOutRendererVFX.EState.Dead)
			{
				return;
			}
			this.currentState = FadeInOutRendererVFX.EState.FadeOut;
			this.currentTime = 0f;
		}

		protected override void OnDeactivate()
		{
			this.WillDeactivate();
		}

		private void SetMaterialsColor(Color color)
		{
			if (this.materials == null)
			{
				return;
			}
			if (color.Equals(this._lastMaterialColor))
			{
				return;
			}
			this._lastMaterialColor = color;
			if (this.keepColor)
			{
				for (int i = 0; i < this.materials.Length; i++)
				{
					Material material = this.materials[i];
					if (!(material == null))
					{
						Color color2 = material.color;
						if (!Mathf.Approximately(color2.a, color.a))
						{
							color2.a = color.a;
							material.color = color2;
						}
					}
				}
				return;
			}
			for (int j = 0; j < this.materials.Length; j++)
			{
				Material material2 = this.materials[j];
				if (!(material2 == null))
				{
					if (!material2.color.Equals(color))
					{
						material2.color = color;
					}
				}
			}
		}

		private void Update()
		{
			if (this.targetRenderer == null)
			{
				return;
			}
			if (this.currentState == FadeInOutRendererVFX.EState.Dead)
			{
				return;
			}
			Color white = Color.white;
			FadeInOutRendererVFX.EState estate = this.currentState;
			if (estate != FadeInOutRendererVFX.EState.Idle)
			{
				if (estate != FadeInOutRendererVFX.EState.FadeIn)
				{
					if (estate == FadeInOutRendererVFX.EState.FadeOut)
					{
						this.currentTime += Time.deltaTime;
						if (this.currentTime > this.fadeOutTime)
						{
							this.currentState = FadeInOutRendererVFX.EState.Dead;
							this.CanCollectToCache = true;
							this.targetRenderer.gameObject.SetActive(false);
						}
						else
						{
							white.a = 1f - this.currentTime / this.fadeOutTime;
						}
					}
				}
				else
				{
					this.currentTime += Time.deltaTime;
					if (this.currentTime > this.fadeInTime)
					{
						this.currentState = FadeInOutRendererVFX.EState.Idle;
					}
					else
					{
						white.a = this.currentTime / this.fadeInTime;
					}
				}
			}
			this.SetMaterialsColor(white);
		}

		public Renderer targetRenderer;

		public float fadeInTime = 0.5f;

		public float fadeOutTime = 0.5f;

		public bool keepColor;

		private Material[] materials;

		private float currentTime;

		private FadeInOutRendererVFX.EState currentState = FadeInOutRendererVFX.EState.Dead;

		private Color _lastMaterialColor;

		private enum EState
		{
			Idle,
			FadeIn,
			FadeOut,
			Dead
		}
	}
}
