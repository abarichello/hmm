using System;
using HeavyMetalMachines.Event;
using Pocketverse;

namespace HeavyMetalMachines.Combat.Gadget
{
	public class KillGadget : BasicCannon
	{
		private KillGadgetInfo MyInfo
		{
			get
			{
				return base.Info as KillGadgetInfo;
			}
		}

		public override void SetInfo(GadgetInfo info)
		{
			base.SetInfo(info);
			GameHubBehaviour.Hub.Events.Players.ListenToObjectUnspawn += this.ListenToObjectUnspawn;
			GameHubBehaviour.Hub.Events.Bots.ListenToObjectUnspawn += this.ListenToObjectUnspawn;
		}

		private void ListenToObjectUnspawn(PlayerEvent data)
		{
			if (data.CauserId == -1 && data.TargetId == this.Combat.Id.ObjId)
			{
				this.FireExtraGadget();
			}
			else if (data.CauserId == this.Combat.Id.ObjId && data.TargetId != -1)
			{
				CombatObject combat = CombatRef.GetCombat(data.TargetId);
				if (combat != null)
				{
					this.FireGadget(base.CannonInfo.Effect, ModifierData.CopyData(this._damage), combat.transform.position);
				}
			}
		}

		protected override void OnDestroy()
		{
			GameHubBehaviour.Hub.Events.Players.ListenToObjectUnspawn -= this.ListenToObjectUnspawn;
			GameHubBehaviour.Hub.Events.Bots.ListenToObjectUnspawn -= this.ListenToObjectUnspawn;
			base.OnDestroy();
		}
	}
}
