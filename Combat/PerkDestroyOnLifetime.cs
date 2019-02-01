using System;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat
{
	public class PerkDestroyOnLifetime : BasePerk, IPerkWithDestruction
	{
		public override void PerkInitialized()
		{
			if (this._useGadgetLifetime)
			{
				this._deathTime = this.Effect.Data.DeathTime;
			}
			else
			{
				this._deathTime = (long)this.Effect.Data.EventTime + (long)(this._lifetime * 1000f);
			}
			this._dead = false;
			if (GameHubBehaviour.Hub.Net.IsClient() || (this.IgnoreNonPositiveLifetime && this.Effect.Data.LifeTime <= 0f))
			{
				base.enabled = false;
			}
		}

		public override void PerkUpdate()
		{
			long num = (long)GameHubBehaviour.Hub.GameTime.GetPlaybackTime();
			if (num < this._deathTime || (this.IgnoreNonPositiveLifetime && this.Effect.Data.LifeTime <= 0f))
			{
				return;
			}
			if (!this._dead && GameHubBehaviour.Hub.Net.IsServer())
			{
				this.ServerDestroy();
				this._dead = true;
			}
		}

		private void ServerDestroy()
		{
			this.Effect.TriggerDestroy((!this.PassTargetOnDestroy || !(this.Effect.Target != null)) ? -1 : this.Effect.Target.ObjId, this.Effect.AttachedTransform.position, false, null, Vector3.zero, BaseFX.EDestroyReason.Lifetime, false);
		}

		public static readonly BitLogger Log = new BitLogger(typeof(PerkDestroyOnLifetime));

		private long _deathTime;

		private bool _dead;

		[SerializeField]
		private float _lifetime;

		[SerializeField]
		private bool _useGadgetLifetime = true;

		public bool PassTargetOnDestroy;

		public bool IgnoreNonPositiveLifetime;
	}
}
