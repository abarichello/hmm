using System;
using FMod;
using UnityEngine;

namespace HeavyMetalMachines.Audio
{
	public class AnimationAudio : MonoBehaviour
	{
		public void PlayOneShot(FMODAsset asset)
		{
			FMODAudioManager.PlayOneShotAt(asset, base.transform.position, 0);
		}
	}
}
