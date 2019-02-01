using System;
using HeavyMetalMachines.Match;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.VFX
{
	public class HMMTeamSurfaceEffectVFX : SurfaceEffectVFX
	{
		protected override void Awake()
		{
			base.Awake();
			this._allyMaterialInstance = this.MaterialInstance;
			this._enemyMaterialInstance = UnityEngine.Object.Instantiate<Material>(this.EnemyOverlapMaterial);
		}

		protected override void OnActivate()
		{
			TeamKind team = GameHubBehaviour.Hub.Players.GetPlayerOrBotsByObjectId(this._targetFXInfo.Owner.ObjId).Team;
			this.MaterialInstance = ((team != GameHubBehaviour.Hub.Players.CurrentPlayerTeam) ? this._enemyMaterialInstance : this._allyMaterialInstance);
			if (this.EnableFading)
			{
				this.OriginalColor = this.MaterialInstance.GetColor(this.PropertyIds.Color);
			}
			base.OnActivate();
		}

		public Material EnemyOverlapMaterial;

		private Material _allyMaterialInstance;

		private Material _enemyMaterialInstance;
	}
}
