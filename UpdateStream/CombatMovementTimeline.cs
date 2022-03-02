using System;
using Hoplon.Time;
using Hoplon.Timeline;
using UnityEngine;

namespace HeavyMetalMachines.UpdateStream
{
	public class CombatMovementTimeline : Timeline<CombatMovementPose>
	{
		public CombatMovementTimeline(IClock clock, uint capacity) : base(clock, capacity, 0.25)
		{
		}

		public override void Interpolate(ref CombatMovementPose first, ref CombatMovementPose second, float t, out CombatMovementPose output)
		{
			output.Position = Vector3.Lerp(first.Position, second.Position, t);
			output.Rotation = Quaternion.Slerp(first.Rotation, second.Rotation, t);
			output.Velocity = Vector3.Lerp(first.Velocity, second.Velocity, t);
			output.AngularVelocity = Mathf.Lerp(first.AngularVelocity, second.AngularVelocity, t);
		}

		public override void Extrapolate(ref CombatMovementPose obj, float t, out CombatMovementPose output)
		{
			output.Position = obj.Position + obj.Velocity * t;
			output.Rotation = obj.Rotation * Quaternion.AngleAxis(obj.AngularVelocity * t * 57.29578f, Vector3.up);
			output.Velocity = obj.Velocity;
			output.AngularVelocity = obj.AngularVelocity;
		}
	}
}
