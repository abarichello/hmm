using System;
using Pocketverse;

namespace HeavyMetalMachines.Combat
{
	public struct ConfirmGridPickCallback : Mural.IMuralMessage
	{
		public ConfirmGridPickCallback(byte playerAddress, int gridIndex)
		{
			this.PlayerAddress = playerAddress;
			this.GridIndex = gridIndex;
		}

		public string Message
		{
			get
			{
				return "OnConfirmGridPickCallback";
			}
		}

		public byte PlayerAddress;

		public int GridIndex;

		public const string Msg = "OnConfirmGridPickCallback";

		public interface IConfirmGridPickCallbackListener
		{
			void OnConfirmGridPickCallback(ConfirmGridPickCallback evt);
		}
	}
}
