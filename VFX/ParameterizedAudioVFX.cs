using System;
using HeavyMetalMachines.Audio;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.VFX
{
	public class ParameterizedAudioVFX : AbstractAudioVFX
	{
		public void OnValidate()
		{
			this._parameterNameBytes = FmodUtilities.GetBytes(this.parameterName);
		}

		protected override void PlayAudio(Transform target, float volume)
		{
			if (this._targetFXInfo.Owner == null)
			{
				ParameterizedAudioVFX.Log.WarnFormat("[AUDIO] null owner on {0}", new object[]
				{
					base.name
				});
				return;
			}
			if (this._sfxController == null)
			{
				ParameterizedAudioVFX.Log.WarnFormat("[AUDIO] cant find carSFXController on {0}", new object[]
				{
					base.name
				});
				return;
			}
			this._sfxController.Play(base.castAsset, target, volume, this._parameterNameBytes, this.parameterValue, this.forceResetTimeline);
		}

		private static readonly BitLogger Log = new BitLogger(typeof(ParameterizedAudioVFX));

		[SerializeField]
		private string parameterName = string.Empty;

		[SerializeField]
		private int parameterValue = -1;

		[SerializeField]
		private bool forceResetTimeline;

		[SerializeField]
		private SFXController _sfxController;

		[SerializeField]
		private byte[] _parameterNameBytes;
	}
}
