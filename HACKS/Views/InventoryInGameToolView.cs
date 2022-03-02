using System;
using HeavyMetalMachines.Presenting;
using HeavyMetalMachines.Presenting.Unity;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace HeavyMetalMachines.HACKS.Views
{
	public class InventoryInGameToolView : MonoBehaviour, IInventoryInGameToolView
	{
		public ICanvas Canvas
		{
			get
			{
				return this._canvas;
			}
		}

		public IButton EquipButton
		{
			get
			{
				return this._equipButton;
			}
		}

		public ILabel CategoryLabel
		{
			get
			{
				return this._categoryLabel;
			}
		}

		public ILabel ItemLabel
		{
			get
			{
				return this._itemLabel;
			}
		}

		public ToggleInventoryToolView InstantiateToggleArmoryToolView()
		{
			ToggleInventoryToolView toggleInventoryToolView = Object.Instantiate<ToggleInventoryToolView>(this._categoryPrefab, this._categoryParent.transform);
			toggleInventoryToolView.gameObject.SetActive(true);
			return toggleInventoryToolView;
		}

		public ItemInventoryToolView InstantiateItemArmoryToolView()
		{
			ItemInventoryToolView itemInventoryToolView = Object.Instantiate<ItemInventoryToolView>(this._itemPrefab, this._itemParent.transform);
			itemInventoryToolView.gameObject.SetActive(true);
			return itemInventoryToolView;
		}

		public void Rebuild()
		{
			this._scrollRect.Rebuild(0);
			this._scrollRect.Rebuild(1);
			this._scrollRect.Rebuild(2);
		}

		public void ResetItemToggles()
		{
			this._itemToggleGroup.SetAllTogglesOff();
		}

		public void ResetCategoryToggles()
		{
			this._categoryToggleGroup.SetAllTogglesOff();
		}

		private void Awake()
		{
			this._viewProvider.Bind<IInventoryInGameToolView>(this, null);
		}

		private void OnDestroy()
		{
			this._viewProvider.Unbind<IInventoryInGameToolView>(null);
		}

		[Inject]
		private IViewProvider _viewProvider;

		[SerializeField]
		private ItemInventoryToolView _itemPrefab;

		[SerializeField]
		private GameObject _itemParent;

		[SerializeField]
		private ToggleGroup _itemToggleGroup;

		[SerializeField]
		private ToggleInventoryToolView _categoryPrefab;

		[SerializeField]
		private GameObject _categoryParent;

		[SerializeField]
		private ToggleGroup _categoryToggleGroup;

		[SerializeField]
		private ScrollRect _scrollRect;

		[SerializeField]
		private UnityCanvas _canvas;

		[SerializeField]
		private UnityButton _equipButton;

		[SerializeField]
		private UnityLabel _categoryLabel;

		[SerializeField]
		private UnityLabel _itemLabel;
	}
}
