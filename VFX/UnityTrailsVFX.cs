using System;
using HeavyMetalMachines.Match;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.VFX
{
	[DisallowMultipleComponent]
	public class UnityTrailsVFX : BaseVFX
	{
		public int AlivePoints
		{
			get
			{
				int num = 0;
				for (int i = 0; i < this._trails.Length; i++)
				{
					num += this._trails[i].positionCount;
				}
				return num;
			}
		}

		protected void OnEnable()
		{
			this.OnDeactivate();
		}

		protected override void OnActivate()
		{
			if (this._team != VFXTeam.Neutral)
			{
				TeamKind team = GameHubBehaviour.Hub.Players.GetPlayerOrBotsByObjectId(this._targetFXInfo.Owner.ObjId).Team;
				bool flag = team != GameHubBehaviour.Hub.Players.CurrentPlayerTeam;
				if ((flag && this._team != VFXTeam.Enemy) || (!flag && this._team != VFXTeam.Ally))
				{
					return;
				}
			}
			for (int i = 0; i < this._trails.Length; i++)
			{
				this._trails[i].Clear();
				this._trails[i].enabled = true;
			}
		}

		protected override void WillDeactivate()
		{
		}

		protected override void OnDeactivate()
		{
			for (int i = 0; i < this._trails.Length; i++)
			{
				this._trails[i].enabled = false;
			}
		}

		protected void OnValidate()
		{
			this._trails = base.gameObject.GetComponentsInChildren<TrailRenderer>(true);
		}

		[SerializeField]
		private VFXTeam _team;

		[SerializeField]
		[ReadOnly]
		private TrailRenderer[] _trails;

		[NonSerialized]
		private int _tagHash;
	}
}
