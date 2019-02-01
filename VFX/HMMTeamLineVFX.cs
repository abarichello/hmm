using System;
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
			this._materialEnemyInstance = UnityEngine.Object.Instantiate<Material>(this._materialEnemy);
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
			TeamKind team = GameHubBehaviour.Hub.Players.GetPlayerOrBotsByObjectId(this._targetFXInfo.Owner.ObjId).Team;
			VFXTeam vfxteam = (team == GameHubBehaviour.Hub.Players.CurrentPlayerTeam) ? VFXTeam.Ally : VFXTeam.Enemy;
			this._line.material = ((vfxteam != VFXTeam.Ally) ? this._materialEnemyInstance : this._materialInstance);
			this.materialYScale = this._line.material.mainTextureScale.y;
		}

		private static readonly BitLogger Log = new BitLogger(typeof(HMMTeamLineVFX));

		[SerializeField]
		private Material _materialEnemy;

		private Material _materialEnemyInstance;
	}
}
