using System;
using Pocketverse;

namespace HeavyMetalMachines.Combat
{
	public struct AggroAttackCallback : Mural.IMuralMessage
	{
		public AggroAttackCallback(BaseFX effect, CombatObject aggroTarget)
		{
			this.Effect = effect;
			this.AggroTarget = aggroTarget;
		}

		public string Message
		{
			get
			{
				return "OnAggroAttackCallback";
			}
		}

		public BaseFX Effect;

		public CombatObject AggroTarget;

		public const string Msg = "OnAggroAttackCallback";

		public interface IAggroAttackCallbackListener
		{
			void OnAggroAttackCallback(AggroAttackCallback evt);
		}
	}
}
