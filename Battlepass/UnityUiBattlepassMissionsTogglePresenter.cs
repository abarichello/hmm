using System;
using HeavyMetalMachines.Frontend;
using UnityEngine;

namespace HeavyMetalMachines.Battlepass
{
	public class UnityUiBattlepassMissionsTogglePresenter : MonoBehaviour
	{
		public void Setup(bool activeMissionsToggleSelected, IListMissionsGroupPresenterSelector listMissionsGroupPresenterSelector)
		{
			this._listMissionsGroupPresenterSelector = listMissionsGroupPresenterSelector;
			this._activeMissionsToggle.isOn = activeMissionsToggleSelected;
			this._completedMissionsToggle.isOn = !activeMissionsToggleSelected;
		}

		public void SelectActiveMissionsToggle()
		{
			this._activeMissionsToggle.isOn = true;
		}

		[UnityUiComponentCall]
		public void OnActiveMissionsToggleChangeValue(bool isOn)
		{
			this._listMissionsGroupPresenterSelector.ToggleMission(isOn);
		}

		[SerializeField]
		private HmmUiToggle _activeMissionsToggle;

		[SerializeField]
		private HmmUiToggle _completedMissionsToggle;

		private IListMissionsGroupPresenterSelector _listMissionsGroupPresenterSelector;
	}
}
