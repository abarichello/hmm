using System;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Event
{
	[Serializable]
	public class SceneEvent : IEventContent, IBitStreamSerializable
	{
		public int EventTime { get; set; }

		public EventScopeKind GetKind()
		{
			return EventScopeKind.Scene;
		}

		public override string ToString()
		{
			return string.Format("SE[Obj={0} Cau={1} St={2} @={3}]", new object[]
			{
				this.SceneObjectId,
				this.CauserId,
				this.State,
				(this.Position == null) ? "null" : this.Position.Value.ToString()
			});
		}

		public bool ShouldBuffer()
		{
			return true;
		}

		public void WriteToBitStream(Pocketverse.BitStream bs)
		{
			bs.WriteCompressedInt(this.SceneObjectId);
			bs.WriteCompressedInt((int)this.State);
			bs.WriteCompressedInt(this.CauserId);
			if (this.Position != null)
			{
				bs.WriteBool(true);
				bs.WriteVector3(this.Position.Value);
			}
			else
			{
				bs.WriteBool(false);
			}
		}

		public void ReadFromBitStream(Pocketverse.BitStream bs)
		{
			this.SceneObjectId = bs.ReadCompressedInt();
			this.State = (SceneEvent.StateKind)bs.ReadCompressedInt();
			this.CauserId = bs.ReadCompressedInt();
			if (bs.ReadBool())
			{
				this.Position = new Vector3?(bs.ReadVector3());
			}
			else
			{
				this.Position = null;
			}
		}

		public int SceneObjectId;

		public int CauserId;

		public SceneEvent.StateKind State;

		public Vector3? Position;

		public enum StateKind
		{
			Spawned,
			Unspawned,
			ScrapUnspawned,
			BuffUnspawned
		}
	}
}
