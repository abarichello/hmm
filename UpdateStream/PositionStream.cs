using System;
using Hoplon.Timeline;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.UpdateStream
{
	internal class PositionStream : TimelineStream<PositionPose>
	{
		protected override void Awake()
		{
			base.Awake();
			this._rigidbody = base.GetComponent<Rigidbody>();
			if (base.enabled)
			{
				if (!this._rigidbody)
				{
					return;
				}
				this._rigidbody.isKinematic = true;
				this._rigidbody.constraints = 0;
				this._rigidbody.collisionDetectionMode = 0;
			}
			else if (!this._rigidbody)
			{
				PositionStream.Log.ErrorFormat("This script requires a RigidBody to work. Effect name: {0}", new object[]
				{
					base.gameObject.name
				});
			}
		}

		protected override Timeline<PositionPose> InstantiateTimeline()
		{
			return new PositionTimeline(this.configuration.SmoothClockInstance, 1024U);
		}

		protected override void SetCurrentPose(ref PositionPose pose)
		{
			base.transform.position = pose.Position;
			base.transform.rotation = pose.Rotation;
		}

		protected override void ResetCurrentPose(ref PositionPose pose)
		{
			pose.Position = base.transform.position;
			pose.Rotation = base.transform.rotation;
		}

		public override void Read(BitStream stream, double offset)
		{
			double num = stream.ReadDouble();
			PositionPose positionPose = new PositionPose
			{
				Position = stream.ReadVector3(),
				Velocity = stream.ReadVector3(),
				Rotation = stream.ReadQuaternion()
			};
			this.Timeline.AddPose(num + offset, ref positionPose);
		}

		public override void Write(BitStream stream)
		{
			stream.WriteDouble((double)GameHubBehaviour.Hub.GameTime.GetPlaybackTime() / 1000.0);
			stream.WriteVector3(base.transform.position);
			stream.WriteVector3(this._rigidbody.velocity);
			stream.WriteQuaternion(base.transform.rotation);
		}

		private static readonly BitLogger Log = new BitLogger(typeof(PositionStream));

		private Rigidbody _rigidbody;
	}
}
