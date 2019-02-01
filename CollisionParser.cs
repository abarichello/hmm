using System;
using HeavyMetalMachines.Combat;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines
{
	public class CollisionParser : KeyFrameParser
	{
		public override KeyFrameType Type
		{
			get
			{
				return KeyFrameType.CollisionEvent;
			}
		}

		public override void Process(Pocketverse.BitStream stream)
		{
			CollisionParser.CollisionEvent collisionEvent = default(CollisionParser.CollisionEvent);
			collisionEvent.ReadFromStream(stream);
			Identifiable @object = GameHubObject.Hub.ObjectCollection.GetObject(collisionEvent.ObjId);
			if (@object == null)
			{
				return;
			}
			CombatFeedback bitComponent = @object.GetBitComponent<CombatFeedback>();
			if (bitComponent != null)
			{
				bitComponent.OnCollisionEventKeyFrame(collisionEvent.Point, collisionEvent.Normal, collisionEvent.Intensity, collisionEvent.OtherLayer);
			}
			else
			{
				BombVisualController instance = BombVisualController.GetInstance(false);
				instance.OnCollisionEvent(collisionEvent.Point, collisionEvent.Normal, collisionEvent.Intensity, collisionEvent.OtherLayer);
			}
		}

		public override bool RewindProcess(IFrame frame)
		{
			return false;
		}

		public void SendData(CollisionParser.CollisionEvent evt)
		{
			Pocketverse.BitStream stream = base.GetStream();
			evt.WriteToStream(stream);
			int frameId = GameHubObject.Hub.PlaybackManager.NextId();
			GameHubObject.Hub.PlaybackManager.SendKeyFrame(this.Type, false, frameId, -1, stream.ToArray());
		}

		public struct CollisionEvent
		{
			internal void ReadFromStream(Pocketverse.BitStream stream)
			{
				this.ObjId = stream.ReadCompressedInt();
				this.Point = stream.ReadVector3();
				stream.ReadCompressedNormVec3Lossy(out this.Normal.x, out this.Normal.y, out this.Normal.z);
				this.Intensity = stream.ReadCompressedFloat();
				this.OtherLayer = stream.ReadByte();
			}

			internal void WriteToStream(Pocketverse.BitStream stream)
			{
				stream.WriteCompressedInt(this.ObjId);
				stream.WriteVector3(this.Point);
				stream.WriteCompressedNormVec3Lossy(this.Normal.x, this.Normal.y, this.Normal.z);
				stream.WriteCompressedFloat(this.Intensity);
				stream.WriteByte(this.OtherLayer);
			}

			public int ObjId;

			public Vector3 Point;

			public Vector3 Normal;

			public float Intensity;

			public byte OtherLayer;
		}
	}
}
