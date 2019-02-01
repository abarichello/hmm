using System;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat
{
	public class PerkCallbackOnTick : BasePerk
	{
		public override void PerkInitialized()
		{
			if (GameHubBehaviour.Hub.Net.IsClient())
			{
				base.enabled = false;
			}
			else
			{
				this._timedUpdater = new TimedUpdater(this.TickMillis, false, false);
				this._timedUpdater.ShouldHalt();
				if (!this.IgnoreIntantZero)
				{
					this.FireCallback();
				}
			}
		}

		private void FireCallback()
		{
			this._ticks++;
			Mural.Post(new TickCallback(this.Effect), this.Effect.Gadget);
			if (this._ticks == this.TicksToDestroy)
			{
				this.Effect.TriggerDestroy(-1, base.transform.position, false, null, Vector3.zero, BaseFX.EDestroyReason.Default, false);
			}
		}

		private void FixedUpdate()
		{
			if (!this._timedUpdater.ShouldHalt())
			{
				this.FireCallback();
			}
		}

		public static readonly BitLogger Log = new BitLogger(typeof(PerkCallbackOnTick));

		private TimedUpdater _timedUpdater;

		public int TickMillis;

		public bool IgnoreIntantZero;

		public int TicksToDestroy;

		private int _ticks;
	}
}
