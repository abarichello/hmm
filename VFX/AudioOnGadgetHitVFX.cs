using System;
using FMod;

namespace HeavyMetalMachines.VFX
{
	public class AudioOnGadgetHitVFX : OnGadgetHitVFX
	{
		protected override void GadgetHitted()
		{
			base.GadgetHitted();
			FMODAudioManager.PlayOneShotAt(this.audioSFX, base.transform.position, 0);
		}

		public FMODAsset audioSFX;
	}
}
