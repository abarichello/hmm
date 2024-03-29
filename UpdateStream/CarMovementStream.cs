﻿using System;
using HeavyMetalMachines.Car;
using HeavyMetalMachines.Infra.Context;
using Hoplon.Timeline;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.UpdateStream
{
	[RequireComponent(typeof(CarInput))]
	[RequireComponent(typeof(CarMovement))]
	public class CarMovementStream : TimelineStream<CarMovementPose>
	{
		protected override Timeline<CarMovementPose> InstantiateTimeline()
		{
			return new CarMovementTimeline(this.configuration.SmoothClockInstance, 1024U);
		}

		protected override void Awake()
		{
			base.Awake();
			this._carInput = base.GetComponent<CarInput>();
			this._turretMovement = base.GetComponent<ITurretMovement>();
			this._carMovement = base.GetComponent<CarMovement>();
			if (base.enabled)
			{
				Rigidbody component = base.GetComponent<Rigidbody>();
				if (component != null)
				{
					component.isKinematic = true;
					component.constraints = 0;
					component.collisionDetectionMode = 0;
				}
			}
		}

		protected override void Update()
		{
			base.Update();
			BombScoreboardState currentState = this.ScoreBoard.CurrentState;
			bool freezeState = this.Timeline.CurrentState == 3 && (currentState == BombScoreboardState.BombDelivery || currentState == BombScoreboardState.PreReplay);
			GameHubBehaviour.Hub.PlayerExperienceBI.SetFreezeState(freezeState);
		}

		protected override void SetCurrentPose(ref CarMovementPose pose)
		{
			base.transform.SetPositionAndRotation(pose.Position, pose.Rotation);
			this._carMovement.LastVelocity = pose.Velocity;
			this._carMovement.LastAngularVelocity = pose.AngularVelocity;
			this._carInput.TargetH = pose.TargetH;
			this._carInput.TargetV = pose.TargetV;
			this._turretMovement.TurretAngle = pose.TurretAngle;
			this._carMovement.HAxis = pose.HAxis;
			this._carMovement.VAxis = pose.VAxis;
			this._carMovement.SpeedZ = pose.SpeedZ;
			this._carMovement.IsDrifting = pose.IsDrifting;
		}

		protected override void ResetCurrentPose(ref CarMovementPose pose)
		{
			pose.Position = base.transform.position;
			pose.Rotation = base.transform.rotation;
			pose.Velocity = this._carMovement.LastVelocity;
			pose.AngularVelocity = this._carMovement.LastAngularVelocity;
			pose.TargetH = this._carInput.TargetH;
			pose.TargetV = this._carInput.TargetV;
			pose.TurretAngle = this._turretMovement.TurretAngle;
			pose.HAxis = this._carMovement.HAxis;
			pose.VAxis = this._carMovement.VAxis;
			pose.SpeedZ = this._carMovement.SpeedZ;
			pose.IsDrifting = this._carMovement.IsDrifting;
		}

		public override void Read(BitStream stream, double offset)
		{
			double num = stream.ReadDouble();
			CarMovementPose carMovementPose = new CarMovementPose
			{
				Position = stream.ReadVector3(),
				Rotation = stream.ReadQuaternion(),
				Velocity = stream.ReadVector3(),
				AngularVelocity = stream.ReadCompressedFloat(),
				TargetH = stream.ReadCompressedFloat(),
				TargetV = stream.ReadCompressedFloat(),
				TurretAngle = stream.ReadCompressedFloat(),
				HAxis = stream.ReadCompressedFloat(),
				VAxis = stream.ReadCompressedFloat(),
				SpeedZ = stream.ReadCompressedFloat(),
				IsDrifting = stream.ReadBool()
			};
			this.Timeline.AddPose(num + offset, ref carMovementPose);
		}

		public override void Write(BitStream stream)
		{
			stream.WriteDouble((double)GameHubBehaviour.Hub.GameTime.GetPlaybackTime() / 1000.0);
			stream.WriteVector3(base.transform.position);
			stream.WriteQuaternion(base.transform.rotation);
			stream.WriteVector3(this._carMovement.LastVelocity);
			stream.WriteCompressedFloat(this._carMovement.LastAngularVelocity);
			stream.WriteCompressedFloat(this._carInput.TargetH);
			stream.WriteCompressedFloat(this._carInput.TargetV);
			stream.WriteCompressedFloat(this._turretMovement.TurretAngle);
			stream.WriteCompressedFloat(this._carMovement.HAxis);
			stream.WriteCompressedFloat(this._carMovement.VAxis);
			stream.WriteCompressedFloat(this._carMovement.SpeedZ);
			stream.WriteBool(this._carMovement.IsDrifting);
		}

		private CarInput _carInput;

		private ITurretMovement _turretMovement;

		private CarMovement _carMovement;
	}
}
