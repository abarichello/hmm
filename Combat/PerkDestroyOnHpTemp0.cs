using System;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat
{
	public class PerkDestroyOnHpTemp0 : BasePerk
	{
		private void FixedUpdate()
		{
			if (this.Effect.Data.SourceCombat.Data.HPTemp <= 0f)
			{
				this.Effect.TriggerDestroy(-1, base._trans.position, false, null, Vector3.zero, BaseFX.EDestroyReason.Default, false);
			}
		}

		public static readonly BitLogger Log = new BitLogger(typeof(PerkDestroyOnHpTemp0));
	}
}
