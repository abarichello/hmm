using System;
using Pocketverse;

namespace HeavyMetalMachines.Combat
{
	public struct KillCallback : Mural.IMuralMessage
	{
		public KillCallback(CombatObject dead)
		{
			this.Dead = dead;
		}

		public string Message
		{
			get
			{
				return "OnKillCallback";
			}
		}

		public CombatObject Dead;

		public const string Msg = "OnKillCallback";

		public interface IKillCallbackListener
		{
			void OnKillCallback(KillCallback evt);
		}
	}
}
