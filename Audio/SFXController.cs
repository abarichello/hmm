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
			SFXController.Log.DebugFormat("[AUDIO] {0} StopAll. Count={1}", new object[]
			{
				base.name,
				this._audios.Count
			});
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

		public void Play(AudioEventAsset asset, Transform target, float volume, byte[] parameterNameBytes, int parameterValue, bool forceResetTimeline)
		{
			FMODAudioManager.FMODAudio fmodaudio;
			if (this._audios.TryGetValue(asset.Id, out fmodaudio))
			{
				if (fmodaudio.IsStopped() || forceResetTimeline)
				{
					fmodaudio.ResetTimeline();
				}
			}
			else
			{
				fmodaudio = FMODAudioManager.PlayAtVolume(asset, target, volume, true);
				if (fmodaudio == null)
				{
					SFXController.Log.WarnFormat("Play called null audio. Ignoring. Id={0} Name={2}", new object[]
					{
						asset.Id,
						base.name
					});
					return;
				}
				this._audios.Add(asset.Id, fmodaudio);
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
			if (parameterNameBytes.Length > 0)
			{
				fmodaudio.SetParameter(parameterNameBytes, (float)parameterValue);
			}
		}

		private static readonly BitLogger Log = new BitLogger(typeof(SFXController));

		private readonly Dictionary<Guid, FMODAudioManager.FMODAudio> _audios = new Dictionary<Guid, FMODAudioManager.FMODAudio>();
	}
}
