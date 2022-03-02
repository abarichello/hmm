using System;
using Assets.ClientApiObjects;
using HeavyMetalMachines.Arena.DataTransferObjects;
using HeavyMetalMachines.BI;
using HeavyMetalMachines.Presenting;
using HeavyMetalMachines.Presenting.Unity;
using Hoplon.Serialization;
using UnityEngine;

namespace HeavyMetalMachines.Training.View
{
	public class UnityTrainingSelectionView : MonoBehaviour, ITrainingSelectionView
	{
		private void Awake()
		{
			this._biButtonNameInstance = new ButtonNameInstance(this._biButtonName);
		}

		public IButton MainButton
		{
			get
			{
				return this._mainButton;
			}
		}

		public ButtonNameInstance BiButtonName
		{
			get
			{
				return this._biButtonNameInstance;
			}
		}

		public string BiButtonNameString
		{
			get
			{
				return this._biButtonName;
			}
		}

		public int ArenaIndex
		{
			get
			{
				ArenaBag arenaBag = (ArenaBag)((JsonSerializeable<!0>)this._gameArenaItemType.Bag);
				return arenaBag.ArenaId;
			}
		}

		[SerializeField]
		private UnityButton _mainButton;

		[SerializeField]
		private ItemTypeScriptableObject _gameArenaItemType;

		[SerializeField]
		private string _biButtonName;

		private ButtonNameInstance _biButtonNameInstance;
	}
}
