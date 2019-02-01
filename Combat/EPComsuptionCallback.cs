using System;
using Pocketverse;

namespace HeavyMetalMachines.Combat
{
	public struct EPComsuptionCallback : Mural.IMuralMessage
	{
		public EPComsuptionCallback(CombatObject combat, int amount)
		{
			this.Combat = combat;
			this.Amount = amount;
		}

		public string Message
		{
			get
			{
				return "OnEPComsuptionCallback";
			}
		}

		public CombatObject Combat;

		public int Amount;

		public const string Msg = "OnEPComsuptionCallback";

		public interface IEPComsuptionCallbackListener
		{
			void OnEPComsuptionCallback(EPComsuptionCallback evt);
		}
	}
}
