using System;
using Pocketverse;

namespace HeavyMetalMachines.Combat
{
	public struct ConfirmGridPickCallback : Mural.IMuralMessage
	{
		public ConfirmGridPickCallback(byte playerAddress, int gridIndex, Guid skinSelected)
		{
			this.PlayerAddress = playerAddress;
			this.GridIndex = gridIndex;
			this.SkinSelected = skinSelected;
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

		public Guid SkinSelected;

		public const string Msg = "OnConfirmGridPickCallback";

		public interface IConfirmGridPickCallbackListener
		{
			void OnConfirmGridPickCallback(ConfirmGridPickCallback evt);
		}
	}
}
