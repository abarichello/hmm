using System;
using Assets.ClientApiObjects;
using HeavyMetalMachines.Swordfish.Player;
using Pocketverse;
using SharedUtils.Loading;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	public class AccountItemPreviewWindow : GameHubBehaviour
	{
		public void ShowPreview(ItemTypeScriptableObject item, Action onItemBoughtCallback)
		{
			this._item = item;
			base.gameObject.SetActive(true);
			UILabel uilabel = this.previewItemName2;
			string text = Language.Get(string.Format("{0}_name", item.Name), TranslationSheets.Items);
			this.previewItemName.text = text;
			uilabel.text = text;
			this.previewItemDescription.text = Language.Get(string.Format("{0}_description", item.Name), TranslationSheets.Items);
			this.previewTexture.mainTexture = (Texture)LoadingManager.ResourceContent.GetAsset(item.Name).Asset;
			this._onItemBoughtCallback = onItemBoughtCallback;
			int num = 0;
			int num2 = 0;
			GameHubBehaviour.Hub.Store.GetItemPrice(item.Id, out num, out num2, false);
			if (item.IsSoftPurchasable)
			{
				this.buyGroupSoftCoinGroup.SetActive(true);
			}
			else
			{
				this.buyGroupSoftCoinGroup.SetActive(false);
			}
			if (item.IsHardPurchasable)
			{
				this.buyGroupHardCoinGroup.SetActive(true);
			}
			else
			{
				this.buyGroupHardCoinGroup.SetActive(false);
			}
			this.softPriceLabel.text = string.Format("{1}", Language.Get("SC", "Store"), num.ToString());
			this.hardPriceLabel.text = string.Format("{1}", Language.Get("HC", "Store"), num2.ToString());
		}

		public void CloseWindow()
		{
			base.gameObject.SetActive(false);
		}

		private void OnBuyItemClick()
		{
			this.SharedPreGameGui.ItemBuyWindow.ShowBuyWindow(this._item, new Action(this.SetItemBought), null, null, 1, false);
		}

		private void SetItemBought()
		{
			this.buyGroup.SetActive(false);
			this._onItemBoughtCallback();
		}

		public SharedPreGameGui SharedPreGameGui;

		public UITexture previewTexture;

		public UILabel previewItemName;

		public UILabel previewItemName2;

		public UILabel previewItemDescription;

		public UILabel previewCharacterName;

		public UILabel softPriceLabel;

		public UILabel hardPriceLabel;

		public UILabel characterSoftPriceLabel;

		public UILabel characterHardPriceLabel;

		public GameObject characterSoftPriceGroup;

		public GameObject characterHardPriceGroup;

		public CarGenerator previewCarGenerator;

		public GameObject buyGroup;

		public GameObject buyGroupHardCoinGroup;

		public GameObject buyGroupSoftCoinGroup;

		private ItemTypeScriptableObject _item;

		public GameObject buyPilot;

		private Action _onItemBoughtCallback;

		public UITexture characterIcon;

		public GameObject rotationCharacter;

		public UILabel previewModeLabel;

		private GameObject car;

		private CharacterBag currentBag;

		private bool defaultView;
	}
}
