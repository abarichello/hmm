using System;
using Hoplon.Timeline;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.UpdateStream
{
	[RequireComponent(typeof(Rigidbody))]
	internal class PositionStream : TimelineStream<PositionPose>
	{
		protected override void Awake()
		{
			base.Awake();
			this._rigidbody = base.GetComponent<Rigidbody>();
			if (base.enabled)
			{
				this._rigidbody.isKinematic = true;
				this._rigidbody.constraints = RigidbodyConstraints.None;
				this._rigidbody.collisionDetectionMode = CollisionDetectionMode.Discrete;
			}
		}

		protected override Timeline<PositionPose> InstantiateTimeline()
		{
			return new PositionTimeline(this.configuration.SmoothClockInstance, 1024u);
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

		public override void Read(Pocketverse.BitStream stream, double offset)
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

		public override void Write(Pocketverse.BitStream stream)
		{
			stream.WriteDouble((double)GameHubBehaviour.Hub.GameTime.GetPlaybackTime() / 1000.0);
			stream.WriteVector3(base.transform.position);
			stream.WriteVector3(this._rigidbody.velocity);
			stream.WriteQuaternion(base.transform.rotation);
		}

		private Rigidbody _rigidbody;
	}
}
