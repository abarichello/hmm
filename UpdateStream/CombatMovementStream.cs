using System;
using HeavyMetalMachines.Combat;
using Hoplon.Timeline;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.UpdateStream
{
	[RequireComponent(typeof(CombatMovement))]
	public class CombatMovementStream : TimelineStream<CombatMovementPose>
	{
		protected override Timeline<CombatMovementPose> InstantiateTimeline()
		{
			return new CombatMovementTimeline(this.configuration.SmoothClockInstance, 1024u);
		}

		protected override void Awake()
		{
			base.Awake();
			this._combatMovement = base.GetComponent<CombatMovement>();
			if (base.enabled)
			{
				Rigidbody component = base.GetComponent<Rigidbody>();
				if (component != null)
				{
					component.isKinematic = true;
					component.constraints = RigidbodyConstraints.None;
					component.collisionDetectionMode = CollisionDetectionMode.Discrete;
				}
			}
		}

		protected override void SetCurrentPose(ref CombatMovementPose pose)
		{
			base.transform.SetPositionAndRotation(pose.Position, pose.Rotation);
			this._combatMovement.LastVelocity = pose.Velocity;
			this._combatMovement.LastAngularVelocity = pose.AngularVelocity;
		}

		protected override void ResetCurrentPose(ref CombatMovementPose pose)
		{
			pose.Position = base.transform.position;
			pose.Rotation = base.transform.rotation;
			pose.Velocity = this._combatMovement.LastVelocity;
			pose.AngularVelocity = this._combatMovement.LastAngularVelocity;
		}

		public override void Read(Pocketverse.BitStream stream, double offset)
		{
			double num = stream.ReadDouble();
			CombatMovementPose combatMovementPose = new CombatMovementPose
			{
				Position = stream.ReadVector3(),
				Rotation = stream.ReadQuaternion(),
				Velocity = stream.ReadVector3(),
				AngularVelocity = stream.ReadCompressedFloat()
			};
			this.Timeline.AddPose(num + offset, ref combatMovementPose);
		}

		public override void Write(Pocketverse.BitStream stream)
		{
			stream.WriteDouble((double)GameHubBehaviour.Hub.GameTime.GetPlaybackTime() / 1000.0);
			stream.WriteVector3(base.transform.position);
			stream.WriteQuaternion(base.transform.rotation);
			stream.WriteVector3(this._combatMovement.LastVelocity);
			stream.WriteCompressedFloat(this._combatMovement.LastAngularVelocity);
		}

		private CombatMovement _combatMovement;
	}
}
