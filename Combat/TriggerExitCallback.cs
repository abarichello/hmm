using System;
using HeavyMetalMachines.Combat.Gadget;
using HeavyMetalMachines.Event;
using Pocketverse;

namespace HeavyMetalMachines.Combat
{
	public struct TriggerExitCallback : Mural.IMuralMessage
	{
		public TriggerExitCallback(CombatObject combat, GadgetBehaviour gadget, EffectEvent effectData)
		{
			this.Combat = combat;
			this.Gadget = gadget;
			this.EffectData = effectData;
		}

		public string Message
		{
			get
			{
				return "OnTriggerExitCallback";
			}
		}

		public CombatObject Combat;

		public GadgetBehaviour Gadget;

		public EffectEvent EffectData;

		public const string Msg = "OnTriggerExitCallback";

		public interface ITriggerExitCallbackListener
		{
			void OnTriggerExitCallback(TriggerExitCallback evt);
		}
	}
}
