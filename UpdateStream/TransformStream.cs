using System;
using Hoplon.Timeline;
using Pocketverse;

namespace HeavyMetalMachines.UpdateStream
{
	internal class TransformStream : TimelineStream<TransformPose>
	{
		protected override Timeline<TransformPose> InstantiateTimeline()
		{
			return new TransformTimeline(this.configuration.SmoothClockInstance, 1024u);
		}

		protected override void SetCurrentPose(ref TransformPose pose)
		{
			base.transform.SetPositionAndRotation(pose.Position, pose.Rotation);
			base.transform.localScale = pose.Scale;
		}

		protected override void ResetCurrentPose(ref TransformPose pose)
		{
			pose.Position = base.transform.position;
			pose.Rotation = base.transform.rotation;
			pose.Scale = base.transform.localScale;
		}

		public override void Read(BitStream stream, double offset)
		{
			double num = stream.ReadDouble();
			TransformPose transformPose = new TransformPose
			{
				Position = stream.ReadVector3(),
				Rotation = stream.ReadQuaternion(),
				Scale = stream.ReadVector3()
			};
			this.Timeline.AddPose(num + offset, ref transformPose);
		}

		public override void Write(BitStream stream)
		{
			stream.WriteDouble((double)GameHubBehaviour.Hub.GameTime.GetPlaybackTime() / 1000.0);
			stream.WriteVector3(base.transform.position);
			stream.WriteQuaternion(base.transform.rotation);
			stream.WriteVector3(base.transform.localScale);
		}
	}
}
