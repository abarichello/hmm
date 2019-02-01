using System;
using Pocketverse;

namespace HeavyMetalMachines.Combat
{
	public struct DestroyFeedbackEffect : Mural.IMuralMessage
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
			void OnDestroyFeedbackEffect(DestroyFeedbackEffect evt);
		}
	}
}
