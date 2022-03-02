using System;

namespace HeavyMetalMachines.PostProcessing
{
	public class PostProcessingState
	{
		public PostProcessingState()
		{
			this.Exposure.Parameters.Strength = 1f;
			this.Exposure.Parameters.Shift = 0f;
			this.Exposure.Parameters.Gamma = 1f;
			this.ScaledOverlayImage.Parameters.VerticalScale = 1f;
			this.ScaledOverlayImage.Parameters.HorizontalScale = 1f;
			this.ScaledOverlayImage.Parameters.Saturation = 1f;
			this.ScreenCenterFlare.Parameters.Radius = 1f;
			this.Saturation.Parameters.Value = 1f;
			this.CRTMonitor.Parameters.LineStrength = 1f;
			this.ScreenBlur.Parameters.Strength = 1f;
			this.ScreenBlur.Parameters.Iterations = 1;
			this.Vignette.Parameters.Ratio = 1f;
			this.Vignette.Parameters.Radius = 1f;
			this.Vignette.Parameters.Slope = 8f;
			this.Vignette.Parameters.Amount = -1f;
		}

		public PostProcessingState Duplicate()
		{
			return base.MemberwiseClone() as PostProcessingState;
		}

		public int GenerateMask()
		{
			int num = 0;
			int num2 = -1;
			num |= ((!this.Saturation.Enabled) ? 0 : 1) << ++num2;
			num |= ((!this.Exposure.Enabled) ? 0 : 1) << ++num2;
			num |= ((!this.CRTMonitor.Enabled) ? 0 : 1) << ++num2;
			num |= ((!this.ScaledOverlayImage.Enabled) ? 0 : 1) << ++num2;
			num |= ((!this.ScreenCenterFlare.Enabled) ? 0 : 1) << ++num2;
			num |= ((!this.ScreenBlur.Enabled) ? 0 : 1) << ++num2;
			return num | ((!this.Vignette.Enabled) ? 0 : 1) << num2 + 1;
		}

		public bool Enabled;

		public PostProcessingParameters<Exposure> Exposure;

		public PostProcessingParameters<ScaledOverlayImage> ScaledOverlayImage;

		public PostProcessingParameters<ScreenCenterFlare> ScreenCenterFlare;

		public PostProcessingParameters<Saturation> Saturation;

		public PostProcessingParameters<CRTMonitor> CRTMonitor;

		public PostProcessingParameters<ScreenBlur> ScreenBlur;

		public PostProcessingParameters<Vignette> Vignette;
	}
}
