using System;
using System.Collections.Generic;
using System.Diagnostics;
using HeavyMetalMachines.DataTransferObjects.Battlepass;
using HeavyMetalMachines.DataTransferObjects.Progression;
using UnityEngine;

namespace HeavyMetalMachines.Infra.ScriptableObjects
{
	[CreateAssetMenu(menuName = "Scriptable Object/BattlepassProgress")]
	public class BattlepassProgressScriptableObject : ScriptableObject
	{
		public BattlepassProgress Progress
		{
			get
			{
				return this._progress;
			}
			private set
			{
				this._progress = value;
			}
		}

		private void OnEnable()
		{
			if (this._progress == null)
			{
				this._progress = new BattlepassProgress();
				this._progress.MissionProgresses = new List<MissionProgress>();
				this._progress.FreeLevelsClaimed = new bool[0];
				this._progress.PremiumLevelsClaimed = new bool[0];
				this._progress.MissionsCompleted = new List<MissionCompleted>();
			}
		}

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public static event Action<BattlepassProgress> OnBattlepassProgressSet;

		public void UpdateBattlepassProgress(BattlepassProgress progress)
		{
			this.Progress = progress;
			if (BattlepassProgressScriptableObject.OnBattlepassProgressSet != null)
			{
				BattlepassProgressScriptableObject.OnBattlepassProgressSet(progress);
			}
		}

		private BattlepassProgress _progress;
	}
}
