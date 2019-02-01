using System;
using Pocketverse;

namespace HeavyMetalMachines.Combat
{
	public struct ConfirmSkinCallback : Mural.IMuralMessage
	{
		public ConfirmSkinCallback(byte playerAddress, int teamKind, bool success, string skinGuid)
		{
			this.PlayerAddress = playerAddress;
			this.TeamKind = teamKind;
			this.Success = success;
			this.SkinGuid = skinGuid;
		}

		public string Message
		{
			get
			{
				return "OnConfirmSkinCallback";
			}
		}

		public byte PlayerAddress;

		public int TeamKind;

		public string SkinGuid;

		public bool Success;

		public const string Msg = "OnConfirmSkinCallback";

		public interface IConfirmSkinCallbackListener
		{
			void OnConfirmSkinCallback(ConfirmSkinCallback evt);
		}
	}
}
