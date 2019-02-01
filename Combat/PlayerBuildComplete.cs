using System;
using Pocketverse;

namespace HeavyMetalMachines.Combat
{
	public struct PlayerBuildComplete : Mural.IMuralMessage
	{
		public PlayerBuildComplete(int id, Identifiable obj)
		{
			this.Id = id;
			this.Object = obj;
		}

		public string Message
		{
			get
			{
				return "OnPlayerBuildComplete";
			}
		}

		public int Id;

		public Identifiable Object;

		public const string Msg = "OnPlayerBuildComplete";

		public interface IPlayerBuildCompleteListener
		{
			void OnPlayerBuildComplete(PlayerBuildComplete evt);
		}
	}
}
