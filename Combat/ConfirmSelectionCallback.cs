using System;
using Pocketverse;

namespace HeavyMetalMachines.Combat
{
	public struct ConfirmSelectionCallback : Mural.IMuralMessage
	{
		public ConfirmSelectionCallback(byte playerAddress, int pilotId)
		{
			this.PlayerAddress = playerAddress;
			this.PilotId = pilotId;
		}

		public string Message
		{
			get
			{
				return "OnConfirmSelectionCallback";
			}
		}

		public byte PlayerAddress;

		public int PilotId;

		public const string Msg = "OnConfirmSelectionCallback";

		public interface IConfirmSelectionCallbackListener
		{
			void OnConfirmSelectionCallback(ConfirmSelectionCallback evt);
		}
	}
}
