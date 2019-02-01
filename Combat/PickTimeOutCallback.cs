using System;
using Pocketverse;

namespace HeavyMetalMachines.Combat
{
	public struct PickTimeOutCallback : Mural.IMuralMessage
	{
		public PickTimeOutCallback(float customizationTime)
		{
			this.CustomizationTime = customizationTime;
		}

		public string Message
		{
			get
			{
				return "OnPickTimeOutCallback";
			}
		}

		public float CustomizationTime;

		public const string Msg = "OnPickTimeOutCallback";

		public interface IPickTimeOutCallbackListener
		{
			void OnPickTimeOutCallback(PickTimeOutCallback evt);
		}
	}
}
