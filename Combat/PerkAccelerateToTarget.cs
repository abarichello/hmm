using System;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat
{
	[RequireComponent(typeof(Rigidbody))]
	public class PerkAccelerateToTarget : PerkMoveToTarget
	{
		public override void PerkInitialized()
		{
			base.PerkInitialized();
			this._accel = this._moveSpeed * this._moveSpeed / (base._trans.position - base.TargetPosition).magnitude / 2f;
			this._moveSpeed = 0f;
		}

		public override void UpdatePosition()
		{
			int playbackTime = GameHubBehaviour.Hub.GameTime.GetPlaybackTime();
			float num = (float)((long)playbackTime - this._lastMillis) * 0.001f;
			this._moveSpeed += this._accel * num;
			base.UpdatePosition();
		}

		protected float _accel;
	}
}
