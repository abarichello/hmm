using System;
using HeavyMetalMachines.Combat;
using HeavyMetalMachines.Utils;
using UnityEngine;

namespace HeavyMetalMachines.VFX
{
	internal class ReleaseHeatAudioVFX : AudioVFX
	{
		protected override void OnActivate()
		{
			bool flag = this._targetFXInfo.Owner != null && this._targetFXInfo.Gadget != null && this.Explosion != null && this.Release != null;
			Debug.Assert(flag, string.Format("Null in {0}. Owner:{1}, Gadget: {2}, Release: {3}, Explosion: {4}", new object[]
			{
				base.name,
				this._targetFXInfo.Owner,
				this._targetFXInfo.Gadget,
				this.Release,
				this.Explosion
			}), Debug.TargetTeam.All);
			if (!flag)
			{
				return;
			}
			this.released = false;
			base.OnActivate();
			if (this.gadgetStateObject == null)
			{
				CombatObject bitComponent = this._targetFXInfo.Owner.GetBitComponent<CombatObject>();
				if (bitComponent != null)
				{
					this.gadgetStateObject = bitComponent.GadgetStates.GetGadgetState(this._targetFXInfo.Gadget.Slot);
				}
			}
		}

		protected override void Finish()
		{
			base.Finish();
			if (this.released)
			{
				return;
			}
			if (this.gadgetStateObject != null && this.gadgetStateObject.Heat > 0.99f)
			{
				Debug.Assert(this.Explosion.IsOneShot, string.Format("{0} trying to play an explosion audio but it is not OneShot. Audio: {1}", base.name, this.Explosion), Debug.TargetTeam.All);
				float volume;
				Transform targetFor = base.GetTargetFor(this.ReleaseTarget, out volume);
				base.CallFMOD(this.Explosion, targetFor, volume);
			}
			else
			{
				this.ReleaseAudio();
			}
		}

		protected void ReleaseAudio()
		{
			if (this.Release == null || this.released)
			{
				return;
			}
			this.released = true;
			Debug.Assert(this.Release.IsOneShot, string.Format("{0} trying to play a release audio but it is not OneShot. Audio: {1}", base.name, this.Release), Debug.TargetTeam.All);
			float volume;
			Transform targetFor = base.GetTargetFor(this.ReleaseTarget, out volume);
			base.CallFMOD(this.Release, targetFor, volume);
		}

		public AbstractAudioVFX.TargetType ReleaseTarget;

		public AudioEventAsset Release;

		public AudioEventAsset Explosion;

		private GadgetData.GadgetStateObject gadgetStateObject;

		private bool released;
	}
}
