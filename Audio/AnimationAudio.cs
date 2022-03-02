using System;
using FMod;
using UnityEngine;

namespace HeavyMetalMachines.Audio
{
	public class AnimationAudio : MonoBehaviour
	{
		public void PlayOneShot(AudioEventAsset asset)
		{
			FMODAudioManager.PlayOneShotAt(asset, base.transform.position, 0);
		}

		public void Play(AudioEventAsset asset)
		{
			FMODAudioManager.PlayAt(asset, base.transform);
		}
	}
}
