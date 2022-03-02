using System;
using UnityEngine;

namespace HeavyMetalMachines.PostProcessing
{
	[CreateAssetMenu(menuName = "Scriptable Object/Post Processing Settings")]
	public class PostProcessingSettings : ScriptableObject
	{
		public PostProcessingSettings()
		{
			this.VignetteParams.Ratio = 1f;
			this.VignetteParams.Radius = 1f;
			this.VignetteParams.Slope = 8f;
			this.VignetteParams.Amount = -1f;
		}

		public void ApplySettingsToPostProcessor(MainPostProcessing postProcessor)
		{
			if (!this.VignetteEnable)
			{
				return;
			}
			PostProcessingState postProcessingState = postProcessor.Request("settings", () => true, true);
			postProcessingState.Enabled = true;
			postProcessingState.Vignette.Enabled = this.VignetteEnable;
			postProcessingState.Vignette.Parameters = this.VignetteParams;
		}

		[SerializeField]
		public bool VignetteEnable;

		[SerializeField]
		public Vignette VignetteParams;
	}
}
