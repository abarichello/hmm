using System;
using HeavyMetalMachines.Boosters.Business;
using HeavyMetalMachines.DataTransferObjects.Util;
using HeavyMetalMachines.Infra.DependencyInjection.Attributes;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	public class ItemTypeShopBoosterScreen : ItemTypeShopScreen
	{
		private void OnEnable()
		{
			GameHubBehaviour.Hub.User.Inventory.OnInvetoryReload += this.SetActiveBoosterTimeRemaining;
		}

		private void OnDisable()
		{
			GameHubBehaviour.Hub.User.Inventory.OnInvetoryReload -= this.SetActiveBoosterTimeRemaining;
		}

		public override void Show()
		{
			base.Show();
			this._boosterActiveInfoPivot.SetActive(true);
			this.SetActiveBoosterTimeRemaining();
		}

		public override void Hide()
		{
			base.Hide();
			base.gameObject.SetActive(false);
		}

		private void SetActiveBoosterTimeRemaining()
		{
			if (this._getLocalPlayerXpBooster.IsActive())
			{
				this._boosterActiveInfodescription.gameObject.SetActive(true);
				this._boosterActiveInfodescription.text = this._getLocalPlayerXpBooster.GetBoosterInfo();
				this._boosterInactiveInfodescription.gameObject.SetActive(false);
				return;
			}
			this._boosterActiveInfodescription.gameObject.SetActive(false);
			this._boosterInactiveInfodescription.gameObject.SetActive(true);
		}

		public override void OnStoreItemClick(StoreItem item)
		{
			this._clickedBuyItem = item;
			GameHubBehaviour.Hub.GuiScripts.SharedPreGameWindow.ItemBuyWindow.ShowBuyWindow(item.StoreItemType, new Action(this.OnBoughtFinished), null, new Action(this.OnGoToShopCash), 1, true);
		}

		private void OnBoughtFinished()
		{
			this.ReloadInventory(this._clickedBuyItem.StoreItemType.ItemCategoryId);
			this._clickedBuyItem = null;
		}

		private void OnGoToShopCash()
		{
			GameHubBehaviour.Hub.GuiScripts.SharedPreGameWindow.ItemBuyWindow.CloseWindow();
			this.Hide();
			GameHubBehaviour.Hub.State.Current.GetStateGuiController<MainMenuGui>().OnCashTopButtonClick();
		}

		private void ReloadInventory(Guid inventoryCategoryId)
		{
			InventoryBag.InventoryKind inventoryKind;
			InventoryMapper.CategoriesToInventoryKind.TryGetValue(inventoryCategoryId, out inventoryKind);
			GameHubBehaviour.Hub.User.Inventory.ReloadInventoryByID(GameHubBehaviour.Hub.User.Inventory.GetInventoryByKind(inventoryKind).Id);
		}

		[SerializeField]
		private GameObject _boosterActiveInfoPivot;

		[SerializeField]
		private UILabel _boosterActiveInfodescription;

		[SerializeField]
		private UILabel _boosterInactiveInfodescription;

		[InjectOnClient]
		private IGetLocalPlayerXpBooster _getLocalPlayerXpBooster;

		private StoreItem _clickedBuyItem;
	}
}
