using System;
using Hoplon.Common.Time;
using Hoplon.Timeline;
using UnityEngine;

namespace HeavyMetalMachines.UpdateStream
{
	public class CarMovementTimeline : Timeline<CarMovementPose>
	{
		public CarMovementTimeline(IClock clock, uint capacity) : base(clock, capacity, 0.25)
		{
		}

		public override void Interpolate(ref CarMovementPose first, ref CarMovementPose second, float t, out CarMovementPose result)
		{
			result.Position = Vector3.Lerp(first.Position, second.Position, t);
			result.Rotation = Quaternion.Slerp(first.Rotation, second.Rotation, t);
			result.Velocity = Vector3.Lerp(first.Velocity, second.Velocity, t);
			result.AngularVelocity = Mathf.Lerp(first.AngularVelocity, second.AngularVelocity, t);
			result.TargetV = Mathf.Lerp(first.TargetV, second.TargetV, t);
			result.TargetH = Mathf.Lerp(first.TargetH, second.TargetH, t);
			result.HAxis = Mathf.Lerp(first.HAxis, second.HAxis, t);
			result.VAxis = Mathf.Lerp(first.VAxis, second.VAxis, t);
			result.SpeedZ = Mathf.Lerp(first.SpeedZ, second.SpeedZ, t);
			result.IsDrifting = ((t >= 0.5f) ? second.IsDrifting : first.IsDrifting);
		}

		public override void Extrapolate(ref CarMovementPose obj, float t, out CarMovementPose result)
		{
			result.Position = obj.Position + obj.Velocity * t;
			result.Rotation = obj.Rotation * Quaternion.AngleAxis(obj.AngularVelocity * t * 57.29578f, Vector3.up);
			result.Velocity = obj.Velocity;
			result.AngularVelocity = obj.AngularVelocity;
			result.TargetV = obj.TargetV;
			result.TargetH = obj.TargetH;
			result.HAxis = obj.HAxis;
			result.VAxis = obj.VAxis;
			result.SpeedZ = obj.SpeedZ;
			result.IsDrifting = obj.IsDrifting;
		}
	}
}
