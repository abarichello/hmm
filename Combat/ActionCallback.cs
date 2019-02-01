using System;
using Pocketverse;

namespace HeavyMetalMachines.Combat
{
	public struct ActionCallback : Mural.IMuralMessage
	{
		public ActionCallback(int effectId)
		{
			this.EffectId = effectId;
		}

		public string Message
		{
			get
			{
				return "OnActionCallback";
			}
		}

		public const string Msg = "OnActionCallback";

		public int EffectId;

		public interface IActionCallbackListener
		{
			void OnActionCallback(ActionCallback evt);
		}
	}
}
