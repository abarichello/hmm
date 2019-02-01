using System;
using Hoplon.Common.Time;
using Hoplon.Timeline;
using UnityEngine;

namespace HeavyMetalMachines.UpdateStream
{
	public class PositionTimeline : Timeline<PositionPose>
	{
		public PositionTimeline(IClock clock, uint capacity) : base(clock, capacity, 0.25)
		{
		}

		public override void Interpolate(ref PositionPose first, ref PositionPose second, float t, out PositionPose output)
		{
			output.Position = Vector3.Lerp(first.Position, second.Position, t);
			output.Velocity = Vector3.Lerp(first.Velocity, second.Velocity, t);
			output.Rotation = Quaternion.Slerp(first.Rotation, second.Rotation, t);
		}

		public override void Extrapolate(ref PositionPose obj, float t, out PositionPose output)
		{
			output.Position = obj.Position + obj.Velocity * t;
			output.Velocity = obj.Velocity;
			output.Rotation = obj.Rotation;
		}
	}
}
