using System;
using FMod;
using HeavyMetalMachines.Utils;
using Pocketverse.MuralContext;
using UnityEngine;

namespace HeavyMetalMachines.VFX
{
	public class AudioVFX : AbstractAudioVFX, ICleanupListener
	{
		protected override void PlayAudio(Transform target, float volume)
		{
			AudioEventAsset castAsset = base.castAsset;
			if (castAsset == null)
			{
				return;
			}
			this.castPlayingAudio = this.CallFMOD(castAsset, target, volume);
		}

		protected override void Finish()
		{
			if (this.castPlayingAudio != null && !this.castPlayingAudio.IsInvalidated())
			{
				if (this.CastForceStop)
				{
					this.castPlayingAudio.Stop();
				}
				else if (this.CastKeyOff)
				{
					if (!this.castPlayingAudio.KeyOff())
					{
						this.castPlayingAudio.Stop();
					}
				}
				else
				{
					AudioEventAsset castAsset = base.castAsset;
					Debug.Assert(castAsset.IsOneShot, string.Format("{0} launched a cast audio but it is not OneShot. Audio: {1}", base.name, castAsset), Debug.TargetTeam.All);
				}
			}
		}

		public void OnCleanup(CleanupMessage msg)
		{
			AudioEventAsset castAsset = base.castAsset;
			if (this.castPlayingAudio != null && !this.castPlayingAudio.IsInvalidated() && castAsset != null && !castAsset.IsOneShot)
			{
				this.castPlayingAudio.Stop();
			}
		}

		protected FMODAudioManager.FMODAudio CallFMOD(AudioEventAsset asset, Transform target, float volume)
		{
			if (target != null)
			{
				return FMODAudioManager.PlayAtVolume(asset, target, volume, false);
			}
			if (!this.CastKeyOff)
			{
				FMODAudioManager.PlayAtVolume(asset, base.transform.position, volume);
			}
			return null;
		}

		public bool CastKeyOff;

		public bool CastForceStop;

		private FMODAudioManager.FMODAudio castPlayingAudio;
	}
}
