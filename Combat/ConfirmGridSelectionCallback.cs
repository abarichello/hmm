using System;
using Pocketverse;

namespace HeavyMetalMachines.Combat
{
	public struct ConfirmGridSelectionCallback : Mural.IMuralMessage
	{
		public ConfirmGridSelectionCallback(byte playerAddress, int gridIndex)
		{
			this.PlayerAddress = playerAddress;
			this.GridIndex = gridIndex;
		}

		public string Message
		{
			get
			{
				return "OnConfirmGridSelectionCallback";
			}
		}

		public byte PlayerAddress;

		public int GridIndex;

		public const string Msg = "OnConfirmGridSelectionCallback";

		public interface IConfirmGridSelectionCallbackListener
		{
			void OnConfirmGridSelectionCallback(ConfirmGridSelectionCallback evt);
		}
	}
}
