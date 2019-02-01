using System;
using Pocketverse;

namespace HeavyMetalMachines.Combat
{
	public struct ConfirmPickCallback : Mural.IMuralMessage
	{
		public ConfirmPickCallback(byte playerAddress, int teamKind, int characterId, Guid lastSkin)
		{
			this.PlayerAddress = playerAddress;
			this.TeamKind = teamKind;
			this.CharacterId = characterId;
			this.LastSkin = lastSkin;
		}

		public string Message
		{
			get
			{
				return "OnConfirmPickCallback";
			}
		}

		public byte PlayerAddress;

		public int TeamKind;

		public int CharacterId;

		public Guid LastSkin;

		public const string Msg = "OnConfirmPickCallback";

		public interface IConfirmPickCallbackListener
		{
			void OnConfirmPickCallback(ConfirmPickCallback evt);
		}
	}
}
