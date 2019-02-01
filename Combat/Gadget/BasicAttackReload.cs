using System;
using HeavyMetalMachines.Event;
using Pocketverse;

namespace HeavyMetalMachines.Combat.Gadget
{
	public class BasicAttackReload : BasicAttack
	{
		public new BasicAttackReloadInfo MyInfo
		{
			get
			{
				return base.Info as BasicAttackReloadInfo;
			}
		}

		private bool IsReloading
		{
			get
			{
				return this.Combat.GadgetStates.GetGadgetState(base.Slot).CoolDown > (long)GameHubBehaviour.Hub.GameTime.GetPlaybackTime();
			}
		}

		private bool ReloadRequired
		{
			get
			{
				return this.ChargeCount == 0;
			}
		}

		public override void SetInfo(GadgetInfo gInfo)
		{
			base.SetInfo(gInfo);
			this.ChargeCount = this.MyInfo.MaxAmmo;
		}

		protected override void FireCannon()
		{
			if (this.IsReloading)
			{
				return;
			}
			if (this.ReloadRequired)
			{
				this.Reload();
			}
			else
			{
				this.Fire();
			}
		}

		public void Reload()
		{
			if (this.IsReloading)
			{
				return;
			}
			EffectEvent effectEvent = base.GetEffectEvent(this.MyInfo.ReloadEffect);
			effectEvent.Origin = this.Combat.Transform.position;
			effectEvent.LifeTime = (float)(this.MyInfo.MaxAmmo - this.ChargeCount) * this.MyInfo.ReloadTime / (float)this.MyInfo.MaxAmmo;
			effectEvent.TargetId = this.Combat.Id.ObjId;
			GameHubBehaviour.Hub.Events.TriggerEvent(effectEvent);
			this.ChargeTime = (long)(GameHubBehaviour.Hub.GameTime.GetPlaybackTime() + (int)(effectEvent.LifeTime * 1000f));
			this.ChargeCount = this.MyInfo.MaxAmmo;
		}

		private void Fire()
		{
			this.ChargeCount--;
			base.FireCannon();
		}
	}
}
