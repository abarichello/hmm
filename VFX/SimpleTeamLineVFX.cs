using System;
using HeavyMetalMachines.Match;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.VFX
{
	public class SimpleTeamLineVFX : SimpleLineVFX
	{
		protected override void OnActivate()
		{
			base.OnActivate();
			VFXTeam vfxTeam = this.VfxTeam;
			if (vfxTeam != VFXTeam.Ally)
			{
				if (vfxTeam == VFXTeam.Enemy)
				{
					this._lineRenderer.material = this._enemyTeamMaterial;
				}
			}
			else
			{
				this._lineRenderer.material = this._allyTeamMaterial;
			}
		}

		private VFXTeam VfxTeam
		{
			get
			{
				if (this.PrevizMode)
				{
					return this.CurrentTeam;
				}
				TeamKind team = GameHubBehaviour.Hub.Players.GetPlayerOrBotsByObjectId(this._targetFXInfo.Owner.ObjId).Team;
				if (team == GameHubBehaviour.Hub.Players.CurrentPlayerTeam)
				{
					return VFXTeam.Ally;
				}
				return VFXTeam.Enemy;
			}
		}

		[SerializeField]
		private Material _allyTeamMaterial;

		[SerializeField]
		private Material _enemyTeamMaterial;
	}
}
