using System;
using System.Collections.Generic;
using FMod;
using UnityEngine;

namespace HeavyMetalMachines.Presenting.Fmod
{
	public class FmodAudio : MonoBehaviour, IAudio
	{
		public void PlayOnce()
		{
			FMODAudioManager.PlayOneShotAt(this._asset, base.transform.position, 0);
		}

		public void PlayLooped()
		{
			FMODAudioManager.FMODAudio item = FMODAudioManager.PlayAt(this._asset, base.transform);
			this._playingAudios.Add(item);
		}

		public void StopAll()
		{
			foreach (FMODAudioManager.FMODAudio fmodaudio in this._playingAudios)
			{
				fmodaudio.Stop();
			}
			this._playingAudios.Clear();
		}

		[SerializeField]
		private AudioEventAsset _asset;

		private List<FMODAudioManager.FMODAudio> _playingAudios = new List<FMODAudioManager.FMODAudio>();
	}
}
