using System;
using Pocketverse;

namespace HeavyMetalMachines.Combat
{
	public struct DestroyFeedbackEffectMessage : Mural.IMuralMessage
	{
		public string Message
		{
			get
			{
				return "OnDestroyFeedbackEffect";
			}
		}

		public const string Msg = "OnDestroyFeedbackEffect";

		public interface IDestroyFeedbackEffectListener
		{
			void OnDestroyFeedbackEffect(DestroyFeedbackEffectMessage evt);
		}
	}
}
