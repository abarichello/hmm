using System;
using HeavyMetalMachines.Combat.Gadget;
using Pocketverse;

namespace HeavyMetalMachines.Combat
{
	public struct TriggerStayCallback : Mural.IMuralMessage
	{
		public TriggerStayCallback(CombatObject combat, GadgetBehaviour gadget)
		{
			this.Combat = combat;
			this.Gadget = gadget;
		}

		public string Message
		{
			get
			{
				return "OnTriggerStayCallback";
			}
		}

		public CombatObject Combat;

		public GadgetBehaviour Gadget;

		public const string Msg = "OnTriggerStayCallback";

		public interface ITriggerStayCallbackListener
		{
			void OnTriggerStayCallback(TriggerStayCallback evt);
		}
	}
}
