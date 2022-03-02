using System;
using HeavyMetalMachines.Combat;
using HeavyMetalMachines.Match;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.VFX
{
	public class HMMTeamLineVFX : LineVFX
	{
		protected override void Awake()
		{
			base.Awake();
			if (!this._materialEnemy)
			{
				this._materialEnemy = this.material;
			}
			this._materialEnemyInstance = Object.Instantiate<Material>(this._materialEnemy);
		}

		protected override void SetLineMaterial()
		{
			if (this._targetFXInfo.Owner == null)
			{
				HMMTeamLineVFX.Log.ErrorFormat("TargetFX Owner is null! Effect Name:{0}", new object[]
				{
					base.name
				});
				return;
			}
			VFXTeam vfxteam;
			if (this.PrevizMode)
			{
				vfxteam = base.CurrentTeam;
			}
			else
			{
				TeamKind team = CombatRef.GetCombat(this._targetFXInfo.Owner.ObjId).Team;
				vfxteam = ((team == GameHubBehaviour.Hub.Players.CurrentPlayerTeam) ? VFXTeam.Ally : VFXTeam.Enemy);
			}
			this._line.material = ((vfxteam != VFXTeam.Ally) ? this._materialEnemyInstance : this._materialInstance);
			this.materialYScale = this._line.material.mainTextureScale.y;
		}

		private static readonly BitLogger Log = new BitLogger(typeof(HMMTeamLineVFX));

		[SerializeField]
		private Material _materialEnemy;

		private Material _materialEnemyInstance;
	}
}
