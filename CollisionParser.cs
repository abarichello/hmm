using System;
using HeavyMetalMachines.Combat;
using HeavyMetalMachines.Playback;
using Pocketverse;

namespace HeavyMetalMachines
{
	public class CollisionParser : KeyFrameParser, ICollisionDispatcher
	{
		public override KeyFrameType Type
		{
			get
			{
				return KeyFrameType.CollisionEvent;
			}
		}

		public override void Process(BitStream stream)
		{
			CollisionEvent collisionEvent = default(CollisionEvent);
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
				BombVisualController instance = BombVisualController.GetInstance();
				instance.OnCollisionEvent(collisionEvent.Point, collisionEvent.Normal, collisionEvent.Intensity, collisionEvent.OtherLayer);
			}
		}

		public override bool RewindProcess(IFrame frame)
		{
			return false;
		}

		public void SendData(CollisionEvent evt)
		{
			BitStream stream = base.GetStream();
			evt.WriteToStream(stream);
			int nextFrameId = this._dispatcher.GetNextFrameId();
			this._dispatcher.SendFrame(this.Type.Convert(), false, nextFrameId, -1, stream.ToArray());
		}
	}
}
