using System;
using Pocketverse;

namespace HeavyMetalMachines.Combat
{
	public struct DamageTakenCallback : Mural.IMuralMessage
	{
		public DamageTakenCallback(CombatObject takerCombatObject, ModifierData damage, CombatObject causerCombatObject, int causerEventId, int listenerEffectId, float amount, bool isPos)
		{
			this.TakerCombatObject = takerCombatObject;
			this.Damage = damage;
			this.CauserCombatObject = causerCombatObject;
			this.CauserEventId = causerEventId;
			this.ListenerEffectId = listenerEffectId;
			this.Amount = amount;
			this.IsPos = isPos;
		}

		public string Message
		{
			get
			{
				return "OnDamageTakenCallback";
			}
		}

		public CombatObject TakerCombatObject;

		public CombatObject CauserCombatObject;

		public ModifierData Damage;

		public int CauserEventId;

		public int ListenerEffectId;

		public float Amount;

		public bool IsPos;

		public const string Msg = "OnDamageTakenCallback";

		public interface IDamageTakenCallbackListener
		{
			void OnDamageTakenCallback(DamageTakenCallback evt);
		}
	}
}
