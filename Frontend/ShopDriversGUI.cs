using System;
using System.Collections.Generic;
using Assets.ClientApiObjects;
using Assets.ClientApiObjects.Components;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	public class ShopDriversGUI : ShopScreen
	{
		public override void CleanUp()
		{
			base.CleanUp();
			for (int i = 0; i < this.AllItems.Count; i++)
			{
				CharactersItem charactersItem = this.AllItems[i];
				charactersItem.CleanUp();
				UnityEngine.Object.Destroy(charactersItem.gameObject);
			}
			this.AllItems.Clear();
			this.CurrentItemsById.Clear();
		}

		public override void Setup()
		{
			base.Setup();
			if (this.AllItems.Count == 0)
			{
				this.CleanUp();
				this.SetupDrivers();
				this.TotalNumberOfFilters = Enum.GetNames(typeof(ShopDriversGUI.CharacterFilter)).Length;
			}
		}

		private void SetupDrivers()
		{
			List<ItemTypeScriptableObject> list = GameHubBehaviour.Hub.InventoryColletion.CategoriesIdToItemTypes[this._characterItemTypeCategoryScriptableObject.Id];
			for (int i = 0; i < list.Count; i++)
			{
				ItemTypeScriptableObject itemTypeScriptableObject = list[i];
				if (!itemTypeScriptableObject.Deleted)
				{
					CharacterItemTypeComponent component = itemTypeScriptableObject.GetComponent<CharacterItemTypeComponent>();
					string characterIcon64Name = component.CharacterIcon64Name;
					string carTextureName = component.CarTextureName;
					CharactersItem charactersItem = this.AddItem(itemTypeScriptableObject, component, i, characterIcon64Name, carTextureName);
					this.AllItems.Add(charactersItem);
					this.CurrentItemsById[itemTypeScriptableObject.Id.ToString()] = charactersItem;
				}
			}
			base.SetupPageToggleControllers(Mathf.CeilToInt((float)this.AllItems.Count / (float)this._itemsPerPage));
		}

		private int AlphabeticComparison(CharactersItem charactersItem, CharactersItem otherItem)
		{
			return charactersItem.CharacterItemTypeScriptableObject.Name.CompareTo(otherItem.CharacterItemTypeScriptableObject.Name);
		}

		private int InverseAlphabeticComparison(CharactersItem charactersItem, CharactersItem otherItem)
		{
			return otherItem.CharacterItemTypeScriptableObject.Name.CompareTo(charactersItem.CharacterItemTypeScriptableObject.Name);
		}

		private int DecreasingHardPriceComparison(CharactersItem charactersItem, CharactersItem otherItem)
		{
			return otherItem.CharacterItemTypeScriptableObject.ReferenceHardPrice.CompareTo(charactersItem.CharacterItemTypeScriptableObject.ReferenceHardPrice);
		}

		private int DecreasingSoftPriceComparison(CharactersItem charactersItem, CharactersItem otherItem)
		{
			long price = otherItem.CharacterItemTypeScriptableObject.ItemTypePrices[0].Price;
			long price2 = charactersItem.CharacterItemTypeScriptableObject.ItemTypePrices[0].Price;
			return price.CompareTo(price2);
		}

		private CharactersItem AddItem(ItemTypeScriptableObject characterItemType, CharacterItemTypeComponent characterComponent, int index, string icon, string cartexture)
		{
			bool flag = true;
			bool flag2 = true;
			CharactersItem charactersItem = this.characterItemPrefab;
			CharactersItem charactersItem2 = UnityEngine.Object.Instantiate<CharactersItem>(charactersItem, new Vector3(-10000f, -10000f, 0f), Quaternion.identity);
			if (!string.IsNullOrEmpty(icon))
			{
				charactersItem2.icon.SpriteName = icon;
			}
			if (!string.IsNullOrEmpty(cartexture))
			{
				charactersItem2.carTexture.SpriteName = cartexture;
			}
			EventDelegate eventDelegate = new EventDelegate(this, "GetFocusOnDriverDetailsScreen");
			eventDelegate.parameters[0] = new EventDelegate.Parameter(charactersItem2, "gameObject");
			charactersItem2.previewButton.onClick.Add(eventDelegate);
			charactersItem2.transform.parent = charactersItem.transform.parent;
			charactersItem2.transform.localScale = charactersItem.transform.localScale;
			Guid id = characterItemType.Id;
			bool flag3 = GameHubBehaviour.Hub.User.Inventory.HasItemOfType(id);
			bool boolValue = GameHubBehaviour.Hub.Config.GetBoolValue(ConfigAccess.AllCharactersFree, false);
			charactersItem2.boughtGO.SetActive(flag3 || boolValue);
			charactersItem2.unboughtGO.SetActive(!flag3 && !boolValue);
			int num;
			int num2;
			GameHubBehaviour.Hub.Store.GetItemPrice(id, out num, out num2, false);
			if (charactersItem2.softPrice)
			{
				charactersItem2.softPrice.text = string.Format("{0}", num);
			}
			if (charactersItem2.hardPrice)
			{
				charactersItem2.hardPrice.text = string.Format("{0}", num2);
			}
			if (num > GameHubBehaviour.Hub.Store.SoftCurrency)
			{
				flag = false;
			}
			if ((long)num2 > GameHubBehaviour.Hub.Store.HardCurrency)
			{
				flag2 = false;
			}
			charactersItem2.softPrice.color = ((!flag) ? this.Shop.NoFundsColor : this.Shop.FameColor);
			charactersItem2.hardPrice.color = ((!flag2) ? this.Shop.NoFundsColor : this.Shop.CashColor);
			charactersItem2.softIcon.color = ((!flag) ? this.Shop.NoFundsColor : Color.white);
			charactersItem2.hardIcon.color = ((!flag2) ? this.Shop.NoFundsColor : Color.white);
			charactersItem2.gameObject.name = index.ToString("000") + "_" + characterItemType.Name;
			charactersItem2.characterName.text = characterComponent.MainAttributes.LocalizedName;
			charactersItem2.CharacterItemTypeScriptableObject = characterItemType;
			return charactersItem2;
		}

		public override void Show()
		{
			base.Show();
			this._itemSelected = false;
			this.CurrentFilter = ShopDriversGUI.CharacterFilter.All;
			this.ShowPage(base.CurrentPage);
			this.gridScript.Reposition();
			this.PageControllersGridPivot.Reposition();
			this.LocalizeAndconfigureFilterTitleLabel();
		}

		public void ShowPage(int page)
		{
			base.CurrentPage = page;
			int num = page * this._itemsPerPage;
			int num2 = page * this._itemsPerPage + this._itemsPerPage - 1;
			this.FilterItemList();
			base.UpdateTotalNumberofPages(Mathf.CeilToInt((float)this.AllItems.Count / (float)this._itemsPerPage));
			for (int i = 0; i < this.AllItems.Count; i++)
			{
				this.AllItems[i].gameObject.SetActive(false);
			}
			for (int j = 0; j < this.AllItems.Count; j++)
			{
				if (j >= num && j <= num2)
				{
					this.AllItems[j].gameObject.SetActive(true);
					this.AllItems[j].gameObject.name = j.ToString("000") + this.AllItems[j].CharacterItemTypeScriptableObject.Name;
					this.RefreshDriverItem(this.AllItems[j]);
				}
			}
			this.gridScript.repositionNow = true;
			base.UpdatePageButtonControllers();
		}

		private void RefreshDriverItem(CharactersItem driverItem)
		{
			Guid id = driverItem.CharacterItemTypeScriptableObject.Id;
			bool flag = GameHubBehaviour.Hub.User.Inventory.HasItemOfType(id);
			driverItem.boughtGO.SetActive(flag);
			driverItem.unboughtGO.SetActive(!flag);
		}

		private void FilterItemList()
		{
			switch (this.CurrentFilter)
			{
			case ShopDriversGUI.CharacterFilter.All:
				this.AllItems.Sort(new Comparison<CharactersItem>(this.AlphabeticComparison));
				break;
			case ShopDriversGUI.CharacterFilter.Inverted:
				this.AllItems.Sort(new Comparison<CharactersItem>(this.InverseAlphabeticComparison));
				break;
			case ShopDriversGUI.CharacterFilter.HighHardPrice:
				this.AllItems.Sort(new Comparison<CharactersItem>(this.DecreasingHardPriceComparison));
				break;
			case ShopDriversGUI.CharacterFilter.HighSoftprice:
				this.AllItems.Sort(new Comparison<CharactersItem>(this.DecreasingSoftPriceComparison));
				break;
			default:
				Debug.LogError("NO FILTER SELECTED - returning fullList", this);
				break;
			}
		}

		public void GoPreviewsPage()
		{
			if (base.CurrentPage == 0)
			{
				return;
			}
			base.CurrentPage--;
			this.ShowPage(base.CurrentPage);
		}

		public void GoNextPage()
		{
			if (base.CurrentPage == base.TotalNumberofPages - 1)
			{
				return;
			}
			base.CurrentPage++;
			this.ShowPage(base.CurrentPage);
		}

		public void GoNextFilter()
		{
			base.CurrentPage = 0;
			if (this.CurrentFilter + 1 >= (ShopDriversGUI.CharacterFilter)this.TotalNumberOfFilters)
			{
				this.CurrentFilter = ShopDriversGUI.CharacterFilter.All;
			}
			else
			{
				this.CurrentFilter++;
			}
			this.ShowPage(base.CurrentPage);
			this.LocalizeAndconfigureFilterTitleLabel();
		}

		public void GoPreviousFilter()
		{
			base.CurrentPage = 0;
			if (this.CurrentFilter - ShopDriversGUI.CharacterFilter.Inverted < 0)
			{
				this.CurrentFilter = (ShopDriversGUI.CharacterFilter)(this.TotalNumberOfFilters - 1);
			}
			else
			{
				this.CurrentFilter--;
			}
			this.ShowPage(base.CurrentPage);
			this.LocalizeAndconfigureFilterTitleLabel();
		}

		private void LocalizeAndconfigureFilterTitleLabel()
		{
			switch (this.CurrentFilter)
			{
			case ShopDriversGUI.CharacterFilter.All:
				this.FilterTitle.text = Language.Get("shop_driver_page_filter_ALL", "Store");
				break;
			case ShopDriversGUI.CharacterFilter.Inverted:
				this.FilterTitle.text = Language.Get("shop_driver_page_filter_ALL_INVERTED", "Store");
				break;
			case ShopDriversGUI.CharacterFilter.HighHardPrice:
				this.FilterTitle.text = Language.Get("shop_driver_page_filter_ALL_HARD_DESCENDING", "Store");
				break;
			case ShopDriversGUI.CharacterFilter.HighSoftprice:
			{
				UILabel filterTitle = this.FilterTitle;
				string text = Language.Get("shop_driver_page_filter_ALL_SOFT_DESCENDING", "Store");
				this.FilterTitle.text = text;
				filterTitle.text = text;
				break;
			}
			}
		}

		public void GetFocusOnDriverDetailsScreen(UnityEngine.Object itemtype)
		{
			if (this._itemSelected)
			{
				return;
			}
			this._itemSelected = true;
			this.Hide();
			CharactersItem component = ((GameObject)itemtype).GetComponent<CharactersItem>();
			this.Shop.ShowDriverDetails(this.CurrentItemsById[component.CharacterItemTypeScriptableObject.Id.ToString()]);
		}

		[Header("INTERNAL")]
		public CharactersItem characterItemPrefab;

		public UIGrid gridScript;

		public UILabel FilterTitle;

		[Header("ITEMS")]
		protected List<CharactersItem> AllItems = new List<CharactersItem>();

		protected Dictionary<string, CharactersItem> CurrentItemsById = new Dictionary<string, CharactersItem>();

		[SerializeField]
		private int _itemsPerPage = 4;

		public ShopDriversGUI.CharacterFilter CurrentFilter;

		public int TotalNumberOfFilters;

		private bool _itemSelected;

		[SerializeField]
		private ItemCategoryScriptableObject _characterItemTypeCategoryScriptableObject;

		public enum CharacterFilter
		{
			All,
			Inverted,
			HighHardPrice,
			HighSoftprice
		}
	}
}
