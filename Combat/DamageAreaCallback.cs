using System;
using System.Collections.Generic;
using HeavyMetalMachines.Combat.Gadget;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat
{
	public struct DamageAreaCallback : Mural.IMuralMessage
	{
		public DamageAreaCallback(List<CombatObject> damagedPlayers, Vector3 origin, BaseFX effect, GadgetSlot targetGadgetSlot)
		{
			this.DamagedPlayers = new List<CombatObject>(damagedPlayers);
			this.Origin = origin;
			this.Effect = effect;
			this.TargetGadgetSlot = targetGadgetSlot;
		}

		public string Message
		{
			get
			{
				return "OnDamageAreaCallback";
			}
		}

		public List<CombatObject> DamagedPlayers;

		public Vector3 Origin;

		public BaseFX Effect;

		public GadgetSlot TargetGadgetSlot;

		public const string Msg = "OnDamageAreaCallback";

		public interface IDamageAreaCallbackListener
		{
			void OnDamageAreaCallback(DamageAreaCallback evt);
		}
	}
}
