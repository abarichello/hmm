using System;
using HeavyMetalMachines.Event;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat.Gadget
{
	public class Teleport : BasicCannon
	{
		public TeleportInfo MyInfo
		{
			get
			{
				return base.Info as TeleportInfo;
			}
		}

		public override void SetInfo(GadgetInfo gInfo)
		{
			base.SetInfo(gInfo);
			TeleportInfo myInfo = this.MyInfo;
			this._travelDistance = new Upgradeable(myInfo.TravelDistanceUpgrade, myInfo.TravelDistance, myInfo.UpgradesValues);
		}

		protected override void SetLevel(string upgradeName, int level)
		{
			base.SetLevel(upgradeName, level);
			this._travelDistance.SetLevel(upgradeName, level);
		}

		protected override int FireGadget()
		{
			TeleportInfo myInfo = this.MyInfo;
			EffectEvent effectEvent = new EffectEvent();
			effectEvent.CopyInfo(myInfo.Effect);
			effectEvent.SourceGadget = this;
			effectEvent.SourceCombat = this.Combat;
			effectEvent.SourceSlot = base.Slot;
			effectEvent.SourceId = this.Parent.ObjId;
			Transform transform = this.Combat.transform;
			Vector3 position = transform.position;
			Vector3 forward = transform.forward;
			effectEvent.Range = this.GetRange();
			effectEvent.Origin = position;
			Vector3 to = position + forward * this._travelDistance.Get();
			to.y = 0f;
			effectEvent.Target = base.GetValidPosition(position, to);
			effectEvent.Direction = forward;
			effectEvent.Modifiers = this._damage;
			float num = Vector3.Distance(position, effectEvent.Target);
			effectEvent.LifeTime = num / myInfo.MoveSpeed;
			return GameHubBehaviour.Hub.Events.TriggerEvent(effectEvent);
		}

		public static readonly BitLogger Log = new BitLogger(typeof(Teleport));

		private Upgradeable _travelDistance;
	}
}
