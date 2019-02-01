using System;
using HeavyMetalMachines.Match;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.VFX
{
	public class HMMTeamDeactivateVFX : BaseVFX
	{
		public override int Priority
		{
			get
			{
				return 1000;
			}
		}

		protected override void OnActivate()
		{
			if (this._targetFXInfo.Owner == null)
			{
				HMMTeamDeactivateVFX.Log.Warn("A TargetFXInfo.Owner is null on " + base.name);
				return;
			}
			TeamKind team = GameHubBehaviour.Hub.Players.GetPlayerOrBotsByObjectId(this._targetFXInfo.Owner.ObjId).Team;
			VFXTeam vfxteam = (team == GameHubBehaviour.Hub.Players.CurrentPlayerTeam) ? VFXTeam.Ally : VFXTeam.Enemy;
			this.TargetGameObject.SetActive(vfxteam == this.TargetTeam);
		}

		protected override void WillDeactivate()
		{
		}

		protected override void OnDeactivate()
		{
		}

		private static BitLogger Log = new BitLogger(typeof(HMMTeamDeactivateVFX));

		public GameObject TargetGameObject;

		public VFXTeam TargetTeam;
	}
}
