using System;
using System.Collections.Generic;
using FMod;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Audio
{
	[CreateAssetMenu(menuName = "Scriptable Object/Audio/Sfx Controller")]
	public class SFXController : GameHubScriptableObject
	{
		private void StopAll()
		{
			if (this._audios.Count == 0)
			{
				return;
			}
			foreach (FMODAudioManager.FMODAudio fmodaudio in this._audios.Values)
			{
				if (fmodaudio == null)
				{
					break;
				}
				if (!fmodaudio.IsInvalidated())
				{
					fmodaudio.Stop();
					FMODAudioManager.Clean(fmodaudio);
				}
			}
			this._audios.Clear();
		}

		private void OnDestroy()
		{
			this.StopAll();
		}

		public void Play(FMODAsset asset, Transform target, float volume, byte[] parameterBytes, int parameterValue, bool forceResetTimeline)
		{
			FMODAudioManager.FMODAudio fmodaudio;
			if (this._audios.TryGetValue(asset.idGUID, out fmodaudio))
			{
				if (fmodaudio.IsStopped() || forceResetTimeline)
				{
					fmodaudio.ResetTimeline();
				}
			}
			else
			{
				fmodaudio = FMODAudioManager.PlayAtVolume(asset, target, volume, true);
				this._audios.Add(asset.idGUID, fmodaudio);
			}
			if (fmodaudio.IsInvalidated())
			{
				SFXController.Log.ErrorFormat("[AUDIO] invalid event called. name={0} asset={1}", new object[]
				{
					base.name,
					(!(asset == null)) ? asset.name : "null"
				});
				return;
			}
			if (parameterBytes.Length > 0)
			{
				fmodaudio.SetParameter(parameterBytes, (float)parameterValue);
			}
		}

		private static readonly BitLogger Log = new BitLogger(typeof(SFXController));

		private readonly Dictionary<Guid, FMODAudioManager.FMODAudio> _audios = new Dictionary<Guid, FMODAudioManager.FMODAudio>();
	}
}
