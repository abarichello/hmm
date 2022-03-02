using System;
using HeavyMetalMachines.Event;
using Pocketverse;

namespace HeavyMetalMachines.Combat
{
	public struct DestroyEffectMessage : Mural.IMuralMessage
	{
		public DestroyEffectMessage(EffectEvent effectData, EffectRemoveEvent removeData)
		{
			this.EffectData = effectData;
			this.RemoveData = removeData;
		}

		public string Message
		{
			get
			{
				return "OnDestroyEffect";
			}
		}

		public override string ToString()
		{
			return string.Format("EffectData: {0}, RemoveData: {1}", this.EffectData, this.RemoveData);
		}

		public EffectEvent EffectData;

		public EffectRemoveEvent RemoveData;

		public const string Msg = "OnDestroyEffect";

		public interface IDestroyEffectListener
		{
			void OnDestroyEffect(DestroyEffectMessage evt);
		}
	}
}
