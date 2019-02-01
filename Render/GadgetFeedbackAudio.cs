using System;
using FMod;

namespace HeavyMetalMachines.Render
{
	public class GadgetFeedbackAudio : BaseGadgetFeedback
	{
		protected override void OnActivate()
		{
			base.OnActivate();
			FMODAudioManager.PlayOneShotAt(this.activateAudio, base.transform.position, 0);
		}

		protected override void OnDeactivate()
		{
			base.OnDeactivate();
			FMODAudioManager.PlayOneShotAt(this.deactivateAudio, base.transform.position, 0);
		}

		public FMODAsset activateAudio;

		public FMODAsset deactivateAudio;
	}
}
