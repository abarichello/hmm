using System;
using Pocketverse;

namespace HeavyMetalMachines.Combat
{
	public struct PlayerSpawnEvent : Mural.IMuralMessage
	{
		public PlayerSpawnEvent(int objId)
		{
			this.ObjId = objId;
		}

		public string Message
		{
			get
			{
				return "OnPlayerSpawned";
			}
		}

		private const string Msg = "OnPlayerSpawned";

		public int ObjId;
	}
}
