using System;
using HeavyMetalMachines.Utils;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat
{
	public class PerkLinkTensionCombatsOnRange : BasePerk
	{
		public override void PerkInitialized()
		{
			this._srcCombat = base.GetTargetCombat(this.Effect, this.TargetSrc);
			this._dstCombat = base.GetTargetCombat(this.Effect, this.TargetDst);
			Debug.Assert(this._srcCombat != null, string.Format("Could not find TargetSrc for perk:{0} effect:{1} desired PerkTarget:{2}", this, this.Effect.Data.EffectInfo.Effect, this.TargetSrc), Debug.TargetTeam.All);
			Debug.Assert(this._dstCombat != null, string.Format("Could not find TargetDst for perk:{0} effect:{1} desired PerkTarget:{2}", this, this.Effect.Data.EffectInfo.Effect, this.TargetDst), Debug.TargetTeam.All);
			float realRange = (this.TensionRange <= 0f) ? this.Effect.Data.Range : this.TensionRange;
			this._updater = new TimedUpdater(this.TickMillis, false, false);
			this.Tension.Setup(this._srcCombat, this._dstCombat, realRange);
		}

		private void FixedUpdate()
		{
			if (this._updater.ShouldHalt())
			{
				return;
			}
			this.Tension.ExecuteTension();
		}

		[Header("This perk only works with combats!!!")]
		public BasePerk.PerkTarget TargetSrc;

		public BasePerk.PerkTarget TargetDst;

		public int TickMillis = 100;

		public float TensionRange;

		private CombatObject _srcCombat;

		private CombatObject _dstCombat;

		private TimedUpdater _updater;

		public CombatTensionMath Tension = new CombatTensionMath();
	}
}
