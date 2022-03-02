using System;
using HeavyMetalMachines.Combat;
using HeavyMetalMachines.Playback;
using Pocketverse;

namespace HeavyMetalMachines
{
	public class CollisionDispatcher : ICollisionDispatcher
	{
		public CollisionDispatcher(IServerPlaybackDispatcher dispatcher, IFrameProcessorFactory factory, IIdentifiableCollection objectCollection)
		{
			this._dispatcher = dispatcher;
			this._objectCollection = objectCollection;
			factory.GetProvider(OperationKind.Playback).Bind(FrameKind.CollisionEvent, new ProcessFrame(this.Process));
			factory.GetProvider(OperationKind.ReplayPlayback).Bind(FrameKind.CollisionEvent, new ProcessFrame(this.Process));
		}

		private void Process(IFrame frame, IFrameProcessContext ctx)
		{
			CollisionEvent collisionEvent = default(CollisionEvent);
			collisionEvent.ReadFromStream(frame.GetReadData());
			Identifiable @object = this._objectCollection.GetObject(collisionEvent.ObjId);
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

		protected BitStream GetStream()
		{
			if (this._myStream == null)
			{
				this._myStream = new BitStream(1024);
			}
			this._myStream.ResetBitsWritten();
			return this._myStream;
		}

		public void SendData(CollisionEvent evt)
		{
			BitStream stream = this.GetStream();
			evt.WriteToStream(stream);
			int nextFrameId = this._dispatcher.GetNextFrameId();
			this._dispatcher.SendFrame(FrameKind.CollisionEvent, false, nextFrameId, -1, stream.ToArray());
		}

		private const FrameKind Kind = FrameKind.CollisionEvent;

		private IServerPlaybackDispatcher _dispatcher;

		private IIdentifiableCollection _objectCollection;

		private BitStream _myStream;
	}
}
