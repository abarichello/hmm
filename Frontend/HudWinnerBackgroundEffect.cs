using System;
using HeavyMetalMachines.PostProcessing;
using HeavyMetalMachines.Utils;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	public class HudWinnerBackgroundEffect
	{
		public HudWinnerBackgroundEffect(HudWinnerBackgroundEffect.HudWinnerBackgroundEffectParameres backgroundEffectParameres)
		{
			this._backgroundEffectParameres = backgroundEffectParameres;
		}

		internal void Start(Func<bool> condition)
		{
			this._animationStartTime = Time.timeSinceLevelLoad;
			this._postProcessing = CarCamera.Singleton.postProcessing.Request("HudWinner", condition, false);
			if (this._postProcessing != null)
			{
				this._postProcessing.Enabled = true;
				this._postProcessing.CRTMonitor.Enabled = true;
				this._postProcessing.Saturation.Enabled = true;
				this._postProcessing.Exposure.Enabled = true;
			}
		}

		internal void Update()
		{
			if (this._postProcessing == null)
			{
				return;
			}
			float timeSinceLevelLoad = Time.timeSinceLevelLoad;
			float a = timeSinceLevelLoad - this._animationStartTime;
			float stateLenght = this._backgroundEffectParameres.StateLenght;
			HeavyMetalMachines.Utils.Debug.Assert(stateLenght != 0f, "No state lenght can be ==0.", HeavyMetalMachines.Utils.Debug.TargetTeam.All);
			float t = Mathf.Min(a, stateLenght) / stateLenght;
			this._postProcessing.Exposure.Parameters.Strength = Mathf.Lerp(this._backgroundEffectParameres.StartExposure, this._backgroundEffectParameres.EndExposure, t);
			this._postProcessing.CRTMonitor.Parameters.LineStrength = Mathf.Lerp(this._backgroundEffectParameres.StartCRTMonitorLinesStrength, this._backgroundEffectParameres.EndCRTMonitorLinesStrength, t);
			this._postProcessing.Saturation.Parameters.Value = Mathf.Lerp(this._backgroundEffectParameres.StartSaturation, this._backgroundEffectParameres.EndSaturation, t);
		}

		internal void Finish()
		{
			if (this._postProcessing != null)
			{
				this._postProcessing.Enabled = false;
				this._postProcessing = null;
			}
		}

		private float _animationStartTime;

		private PostProcessingState _postProcessing;

		private readonly HudWinnerBackgroundEffect.HudWinnerBackgroundEffectParameres _backgroundEffectParameres;

		[Serializable]
		public class HudWinnerBackgroundEffectParameres
		{
			[Range(0f, 10f)]
			[Tooltip("Lenght of the state, in seconds.")]
			public float StateLenght = 0.2f;

			[Range(-20f, 20f)]
			public float StartExposure = 1f;

			[Range(-20f, 20f)]
			public float EndExposure = 1f;

			[Range(0f, 1f)]
			public float StartCRTMonitorLinesStrength;

			[Range(0f, 1f)]
			public float EndCRTMonitorLinesStrength;

			[Range(-20f, 20f)]
			public float StartSaturation = 1f;

			[Range(-20f, 20f)]
			public float EndSaturation = 1f;
		}
	}
}
