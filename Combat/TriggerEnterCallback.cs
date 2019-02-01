using System;
using HeavyMetalMachines.Combat.Gadget;
using HeavyMetalMachines.Event;
using Pocketverse;

namespace HeavyMetalMachines.Combat
{
	public struct TriggerEnterCallback : Mural.IMuralMessage
	{
		public TriggerEnterCallback(CombatObject combat, GadgetBehaviour gadget, EffectEvent effectData)
		{
			this = new TriggerEnterCallback(combat, gadget, effectData, null);
		}

		public TriggerEnterCallback(CombatObject combat, GadgetBehaviour gadget, EffectEvent effectData, BaseFX fx)
		{
			this.FX = fx;
			this.Combat = combat;
			this.Gadget = gadget;
			this.EffectData = effectData;
		}

		public string Message
		{
			get
			{
				return "OnTriggerEnterCallback";
			}
		}

		public CombatObject Combat;

		public GadgetBehaviour Gadget;

		public EffectEvent EffectData;

		public BaseFX FX;

		public const string Msg = "OnTriggerEnterCallback";

		public interface ITriggerEnterCallbackListener
		{
			void OnTriggerEnterCallback(TriggerEnterCallback evt);
		}
	}
}
