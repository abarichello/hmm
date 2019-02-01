using System;
using HeavyMetalMachines.Infra.ScriptableObjects;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	public class BattlePassHasClaimFeedBack : GameHubBehaviour
	{
		private void OnEnable()
		{
			BattlepassProgressScriptableObject.OnBattlepassProgressSet = (Action)Delegate.Combine(BattlepassProgressScriptableObject.OnBattlepassProgressSet, new Action(this.OnBattlepassProgressSet));
		}

		private void OnDisable()
		{
			BattlepassProgressScriptableObject.OnBattlepassProgressSet = (Action)Delegate.Remove(BattlepassProgressScriptableObject.OnBattlepassProgressSet, new Action(this.OnBattlepassProgressSet));
		}

		private void OnBattlepassProgressSet()
		{
			BattlepassProgress progress = this._battlepassProgressScriptableObject.Progress;
			this._metalpassFeedbackObject.SetActive(progress.HasRewardToClaim(this._sharedConfigs.Battlepass));
		}

		[SerializeField]
		private BattlepassProgressScriptableObject _battlepassProgressScriptableObject;

		[SerializeField]
		private SharedConfigs _sharedConfigs;

		[SerializeField]
		private GameObject _metalpassFeedbackObject;
	}
}
