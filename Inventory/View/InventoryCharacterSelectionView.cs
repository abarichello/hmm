using System;
using HeavyMetalMachines.Presenting;
using HeavyMetalMachines.Presenting.Unity;
using UnityEngine;

namespace HeavyMetalMachines.Inventory.View
{
	public class InventoryCharacterSelectionView : MonoBehaviour, IInventoryCharacterSelectionView
	{
		public IActivatable CharactersSelectionGroupActivatable
		{
			get
			{
				return this._charactersSelectionGroupActivatable;
			}
		}

		public IDropdown<int> CharactersDropdown
		{
			get
			{
				return this._charactersDropdown;
			}
		}

		public IButton LeftButton
		{
			get
			{
				return this._leftButton;
			}
		}

		public IButton RightButton
		{
			get
			{
				return this._rightButton;
			}
		}

		[SerializeField]
		private GameObjectActivatable _charactersSelectionGroupActivatable;

		[SerializeField]
		private IntUnityDropdown _charactersDropdown;

		[SerializeField]
		private UnityButton _leftButton;

		[SerializeField]
		private UnityButton _rightButton;
	}
}
