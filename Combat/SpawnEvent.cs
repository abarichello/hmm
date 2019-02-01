using System;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat
{
	public struct SpawnEvent : Mural.IMuralMessage
	{
		public SpawnEvent(int objId, Vector3 pos, SpawnReason reason)
		{
			this.ObjId = objId;
			this.Position = pos;
			this.Reason = reason;
		}

		public string Message
		{
			get
			{
				return "OnObjectSpawned";
			}
		}

		private const string Msg = "OnObjectSpawned";

		public int ObjId;

		public Vector3 Position;

		public SpawnReason Reason;
	}
}
