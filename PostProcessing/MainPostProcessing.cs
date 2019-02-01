using System;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.PostProcessing
{
	[RequireComponent(typeof(Camera))]
	public class MainPostProcessing : GameHubBehaviour
	{
		private void Awake()
		{
			this._propertyIds = new MainPostProcessing.PropertyIds();
			this._stateStack = new CarCameraStack<PostProcessingState>(16);
			this.Request("off", () => true, false);
		}

		private void OnDestroy()
		{
			this._stateStack.Clear();
			this.ClearTemporaryRenderTextures();
		}

		public PostProcessingState Request(string identifier, Func<bool> condition, bool cleanState = false)
		{
			PostProcessingState postProcessingState = (!cleanState && this._stateStack.Count != 0) ? this._stateStack.Peek().Duplicate() : new PostProcessingState();
			return (!this._stateStack.Push(identifier, condition, postProcessingState)) ? null : postProcessingState;
		}

		public void Configure()
		{
			if (this._shaderMaterial == null)
			{
				this._shaderMaterial = new Material(this.MainPostProcessingShader);
			}
			if (this._downsampleMaterial == null)
			{
				this._downsampleMaterial = new Material(this.DownsampleShader);
			}
			PostProcessingState postProcessingState = this._stateStack.Peek();
			if (postProcessingState.Exposure.Enabled)
			{
				this._shaderMaterial.EnableKeyword("EXPOSURE_EFFECT");
			}
			else
			{
				this._shaderMaterial.DisableKeyword("EXPOSURE_EFFECT");
			}
			if (postProcessingState.Saturation.Enabled)
			{
				this._shaderMaterial.EnableKeyword("SATURATION_EFFECT");
			}
			else
			{
				this._shaderMaterial.DisableKeyword("SATURATION_EFFECT");
			}
			if (postProcessingState.CRTMonitor.Enabled)
			{
				this._shaderMaterial.EnableKeyword("CRTMONITORLINES_EFFECT");
			}
			else
			{
				this._shaderMaterial.DisableKeyword("CRTMONITORLINES_EFFECT");
			}
			if (postProcessingState.ScaledOverlayImage.Enabled)
			{
				this._shaderMaterial.EnableKeyword("SCALEDOVERLAYIMAGE_EFFECT");
			}
			else
			{
				this._shaderMaterial.DisableKeyword("SCALEDOVERLAYIMAGE_EFFECT");
			}
			if (postProcessingState.ScreenCenterFlare.Enabled)
			{
				this._shaderMaterial.EnableKeyword("SCREENCENTERFLARE_EFFECT");
			}
			else
			{
				this._shaderMaterial.DisableKeyword("SCREENCENTERFLARE_EFFECT");
			}
			bool flag = postProcessingState.ScaledOverlayImage.Enabled || postProcessingState.ScreenBlur.Enabled;
			if (flag && (this._temporaryRenderTextures == null || this._temporaryRenderTextures.Length == 0))
			{
				this._temporaryRenderTextures = new RenderTexture[4];
				for (int i = 0; i < 4; i++)
				{
					this._temporaryRenderTextures[i] = new RenderTexture(Screen.width / 2, Screen.height / 2, 0);
					this._temporaryRenderTextures[i].name = string.Format("PostProcessingRT {0}", i);
					this._temporaryRenderTextures[i].autoGenerateMips = false;
					this._temporaryRenderTextures[i].useMipMap = false;
				}
			}
		}

		private void ClearTemporaryRenderTextures()
		{
			if (this._temporaryRenderTextures != null)
			{
				for (int i = 0; i < this._temporaryRenderTextures.Length; i++)
				{
					if (!(this._temporaryRenderTextures[i] == null))
					{
						this._temporaryRenderTextures[i].Release();
						UnityEngine.Object.Destroy(this._temporaryRenderTextures[i]);
					}
				}
				this._temporaryRenderTextures = null;
			}
		}

		private void DownSample(RenderTexture source, RenderTexture rt1, RenderTexture rt2, Vector2 threshold)
		{
			if (this._downsampleMaterial.passCount < 3)
			{
				return;
			}
			PostProcessingState postProcessingState = this._stateStack.Peek();
			if (threshold.x == 0f)
			{
				for (int i = 0; i < postProcessingState.ScreenBlur.Parameters.Iterations; i++)
				{
					Graphics.Blit((i != 0) ? rt2 : source, rt1, this._downsampleMaterial, 0);
					Graphics.Blit(rt1, rt2, this._downsampleMaterial, 1);
				}
			}
			else
			{
				this._downsampleMaterial.SetVector(this._propertyIds.Threshold, threshold);
				Graphics.Blit(source, rt1, this._downsampleMaterial, 2);
				for (int j = 0; j < postProcessingState.ScreenBlur.Parameters.Iterations; j++)
				{
					Graphics.Blit(rt1, rt2, this._downsampleMaterial, 0);
					Graphics.Blit(rt2, rt1, this._downsampleMaterial, 1);
				}
			}
		}

		public void OnRenderImage(RenderTexture source, RenderTexture target)
		{
			this._stateStack.ShouldUpdate();
			PostProcessingState postProcessingState = this._stateStack.Peek();
			if (!postProcessingState.Enabled)
			{
				Graphics.Blit(source, target);
				return;
			}
			int num = postProcessingState.GenerateMask();
			if (this._effectsMask != num)
			{
				this.Configure();
			}
			this._effectsMask = num;
			if (this._effectsMask == 0)
			{
				Graphics.Blit(source, target);
				return;
			}
			source.filterMode = FilterMode.Bilinear;
			if (postProcessingState.Exposure.Enabled || postProcessingState.ScaledOverlayImage.Enabled)
			{
				this._shaderMaterial.SetFloat(this._propertyIds.Exposure, postProcessingState.Exposure.Parameters.Strength);
				this._shaderMaterial.SetFloat(this._propertyIds.ExposureOffset, postProcessingState.Exposure.Parameters.Shift);
				this._shaderMaterial.SetFloat(this._propertyIds.Gamma, postProcessingState.Exposure.Parameters.Gamma);
			}
			if (postProcessingState.Saturation.Enabled)
			{
				this._shaderMaterial.SetFloat(this._propertyIds.Saturation, postProcessingState.Saturation.Parameters.Value);
			}
			if (postProcessingState.ScaledOverlayImage.Enabled)
			{
				this._shaderMaterial.SetFloat(this._propertyIds.VerticalScale, postProcessingState.ScaledOverlayImage.Parameters.VerticalScale);
				this._shaderMaterial.SetFloat(this._propertyIds.HorizontalScale, postProcessingState.ScaledOverlayImage.Parameters.HorizontalScale);
				this._shaderMaterial.SetFloat(this._propertyIds.OverlaySaturation, postProcessingState.ScaledOverlayImage.Parameters.Saturation);
				this._temporaryRenderTextures[0].filterMode = FilterMode.Bilinear;
				this._temporaryRenderTextures[1].filterMode = FilterMode.Bilinear;
				this.DownSample(source, this._temporaryRenderTextures[0], this._temporaryRenderTextures[1], default(Vector4));
				this._shaderMaterial.SetTexture(this._propertyIds.DownPass, this._temporaryRenderTextures[1]);
				this._shaderMaterial.SetFloat(this._propertyIds.ScreenCenterFlareRadius, postProcessingState.ScreenCenterFlare.Parameters.Radius);
			}
			if (postProcessingState.ScreenBlur.Enabled)
			{
				this._temporaryRenderTextures[0].filterMode = FilterMode.Bilinear;
				this._temporaryRenderTextures[1].filterMode = FilterMode.Bilinear;
				this.DownSample(source, this._temporaryRenderTextures[0], this._temporaryRenderTextures[1], default(Vector4));
				this._shaderMaterial.SetTexture(this._propertyIds.DownPass, this._temporaryRenderTextures[1]);
			}
			if (postProcessingState.CRTMonitor.Enabled)
			{
				this._shaderMaterial.SetFloat(this._propertyIds.ScanlineStrength, postProcessingState.CRTMonitor.Parameters.LineStrength);
			}
			if (postProcessingState.ScreenCenterFlare.Enabled)
			{
				this._shaderMaterial.SetFloat(this._propertyIds.ScreenCenterFlareRadius, postProcessingState.ScreenCenterFlare.Parameters.Radius);
			}
			if (postProcessingState.ScreenBlur.Enabled)
			{
				this._shaderMaterial.EnableKeyword("SCREEN_BLUR");
			}
			else
			{
				this._shaderMaterial.DisableKeyword("SCREEN_BLUR");
			}
			this._shaderMaterial.SetFloat(this._propertyIds.ScreenBlur, postProcessingState.ScreenBlur.Parameters.Strength);
			Graphics.Blit(source, target, this._shaderMaterial);
		}

		public Shader MainPostProcessingShader;

		public Shader DownsampleShader;

		private Material _shaderMaterial;

		private Material _downsampleMaterial;

		private int _effectsMask;

		private RenderTexture[] _temporaryRenderTextures;

		private CarCameraStack<PostProcessingState> _stateStack;

		private MainPostProcessing.PropertyIds _propertyIds;

		private class PropertyIds
		{
			public PropertyIds()
			{
				this.Threshold = Shader.PropertyToID("_Threshold");
				this.Exposure = Shader.PropertyToID("_Exposure");
				this.ExposureOffset = Shader.PropertyToID("_ExposureOffset");
				this.Gamma = Shader.PropertyToID("_Gamma");
				this.Saturation = Shader.PropertyToID("_Saturation");
				this.VerticalScale = Shader.PropertyToID("_VerticalScale");
				this.HorizontalScale = Shader.PropertyToID("_HorizontalScale");
				this.OverlaySaturation = Shader.PropertyToID("_OverlaySaturation");
				this.DownPass = Shader.PropertyToID("_DownPass");
				this.ScreenCenterFlareRadius = Shader.PropertyToID("_ScreenCenterFlareRadius");
				this.ScanlineStrength = Shader.PropertyToID("_ScanlineStrength");
				this.ScreenBlur = Shader.PropertyToID("_ScreenBlur");
			}

			public readonly int Threshold;

			public readonly int Exposure;

			public readonly int ExposureOffset;

			public readonly int Gamma;

			public readonly int Saturation;

			public readonly int VerticalScale;

			public readonly int HorizontalScale;

			public readonly int OverlaySaturation;

			public readonly int DownPass;

			public readonly int ScreenCenterFlareRadius;

			public readonly int ScanlineStrength;

			public readonly int ScreenBlur;
		}
	}
}
