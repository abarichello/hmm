using System;
using Hoplon.Common.Time;
using Hoplon.Timeline;
using UnityEngine;

namespace HeavyMetalMachines.UpdateStream
{
	public class TransformTimeline : Timeline<TransformPose>
	{
		public TransformTimeline(IClock clock, uint capacity) : base(clock, capacity, 0.0)
		{
		}

		public override void Interpolate(ref TransformPose first, ref TransformPose second, float t, out TransformPose output)
		{
			output.Position = Vector3.Lerp(first.Position, second.Position, t);
			output.Rotation = Quaternion.Slerp(first.Rotation, second.Rotation, t);
			output.Scale = Vector3.Lerp(first.Scale, second.Scale, t);
		}

		public override void Extrapolate(ref TransformPose obj, float t, out TransformPose output)
		{
			output = obj;
		}
	}
}
