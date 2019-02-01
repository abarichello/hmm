using System;
using HeavyMetalMachines.Infra.ScriptableObjects;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	public class BattlePassHasMissionFeedBack : GameHubBehaviour
	{
		private void OnEnable()
		{
			this.OnBattlepassProgressSet();
			BattlepassProgressScriptableObject.OnBattlepassProgressSet = (Action)Delegate.Combine(BattlepassProgressScriptableObject.OnBattlepassProgressSet, new Action(this.OnBattlepassProgressSet));
		}

		private void OnDisable()
		{
			BattlepassProgressScriptableObject.OnBattlepassProgressSet = (Action)Delegate.Remove(BattlepassProgressScriptableObject.OnBattlepassProgressSet, new Action(this.OnBattlepassProgressSet));
		}

		private void OnBattlepassProgressSet()
		{
			BattlepassProgress progress = this._battlepassProgressScriptableObject.Progress;
			this._metalpassFeedbackObject.SetActive(progress.HasNewMissions());
		}

		[SerializeField]
		private BattlepassProgressScriptableObject _battlepassProgressScriptableObject;

		[SerializeField]
		private GameObject _metalpassFeedbackObject;
	}
}
