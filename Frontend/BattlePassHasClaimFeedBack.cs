using System;
using HeavyMetalMachines.DataTransferObjects.Progression;
using HeavyMetalMachines.Infra.ScriptableObjects;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	public class BattlePassHasClaimFeedBack : GameHubBehaviour
	{
		private void OnEnable()
		{
			BattlepassProgressScriptableObject.OnBattlepassProgressSet += this.OnBattlepassProgressSet;
		}

		private void OnDisable()
		{
			BattlepassProgressScriptableObject.OnBattlepassProgressSet -= this.OnBattlepassProgressSet;
		}

		private void OnBattlepassProgressSet(BattlepassProgress battlepassProgress)
		{
			this._metalpassFeedbackObject.SetActive(battlepassProgress.HasRewardToClaim(this._sharedConfigs.Battlepass));
		}

		[SerializeField]
		private BattlepassProgressScriptableObject _battlepassProgressScriptableObject;

		[SerializeField]
		private SharedConfigs _sharedConfigs;

		[SerializeField]
		private GameObject _metalpassFeedbackObject;
	}
}
