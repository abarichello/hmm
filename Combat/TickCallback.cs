using System;
using Pocketverse;

namespace HeavyMetalMachines.Combat
{
	public struct TickCallback : Mural.IMuralMessage
	{
		public TickCallback(BaseFX effect)
		{
			this.Effect = effect;
		}

		public string Message
		{
			get
			{
				return "OnTickCallback";
			}
		}

		public BaseFX Effect;

		public const string Msg = "OnTickCallback";

		public interface ITickCallbackCallbackListener
		{
			void OnTickCallback(TickCallback evt);
		}
	}
}
