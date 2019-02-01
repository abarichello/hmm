using System;
using HeavyMetalMachines.Render;

namespace HeavyMetalMachines.VFX
{
	public class HelicopterBeaconVFX : BaseVFX
	{
		protected override void OnActivate()
		{
			if (this._helicopter == null)
			{
				this._helicopter = this._targetFXInfo.Owner.GetComponentInChildren<HelicopterController>();
				if (this._helicopter == null)
				{
					base.enabled = false;
					return;
				}
			}
			this._helicopter.LifeTime = this._targetFXInfo.Gadget.LifeTime;
			this._helicopter.Connect(this._targetFXInfo.EffectTransform, this.FollowTarget);
		}

		protected override void WillDeactivate()
		{
		}

		protected override void OnDeactivate()
		{
			if (this._helicopter)
			{
				this._helicopter.Disconnect(this._targetFXInfo.EffectTransform);
				this._helicopter = null;
			}
		}

		public bool FollowTarget;

		private HelicopterController _helicopter;
	}
}
