using System;
using HeavyMetalMachines.DataTransferObjects.Progression;
using HeavyMetalMachines.Infra.ScriptableObjects;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	public class BattlePassHasMissionFeedBack : GameHubBehaviour
	{
		private void OnEnable()
		{
			this.OnBattlepassProgressSet(this._battlepassProgressScriptableObject.Progress);
			BattlepassProgressScriptableObject.OnBattlepassProgressSet += this.OnBattlepassProgressSet;
		}

		private void OnDisable()
		{
			BattlepassProgressScriptableObject.OnBattlepassProgressSet -= this.OnBattlepassProgressSet;
		}

		private void OnBattlepassProgressSet(BattlepassProgress battlepassProgress)
		{
			this._metalpassFeedbackObject.SetActive(battlepassProgress.HasNewMissions());
			this._metalpassFeedbackObject.SetActive(false);
		}

		[SerializeField]
		private BattlepassProgressScriptableObject _battlepassProgressScriptableObject;

		[SerializeField]
		private GameObject _metalpassFeedbackObject;
	}
}
