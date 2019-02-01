using System;
using System.Collections;
using System.Collections.Generic;
using FMod;
using HeavyMetalMachines.Combat.Gadget;
using HeavyMetalMachines.Utils;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	public class HudInstancesShop : GameHubBehaviour
	{
		public void Init()
		{
			GUIUtils.CreateGridPool(this.ItemsGrid, this.ItemsGrid.maxPerLine);
			this.ItemsGrid.hideInactive = false;
			List<Transform> childList = this.ItemsGrid.GetChildList();
			for (int i = 0; i < childList.Count; i++)
			{
				childList[i].transform.name = "item" + i;
				childList[i].gameObject.SetActive(false);
			}
			this.ItemsGrid.hideInactive = true;
			base.gameObject.SetActive(false);
			this._didAnimation = false;
		}

		protected void OnDestroy()
		{
			this._garageController = null;
		}

		public void Show(bool isTutorial)
		{
			base.gameObject.SetActive(true);
			this.EnableInputNavigation(false);
			this.TitleInAnimation.GetComponent<NGUIWidgetAlpha>().alpha = ((!this._didAnimation) ? 0f : 1f);
			this.TitleOutAnimation.GetComponent<NGUIWidgetAlpha>().alpha = 1f;
			Vector3 localPosition = this.TitleOutAnimation.transform.localPosition;
			localPosition.x = 0f;
			this.TitleOutAnimation.transform.localPosition = localPosition;
			this.ItemsGrid.GetComponent<NGUIWidgetAlpha>().alpha = 1f;
			Vector3 localPosition2 = this.ItemsGrid.transform.localPosition;
			localPosition2.x = 0f;
			this.ItemsGrid.transform.localPosition = localPosition2;
			if (string.IsNullOrEmpty(this._selectedUpgradeInfoName) && !isTutorial)
			{
				this._selectedUpgradeInfoName = HudUtils.GetInstanceUpgradeInfo(GameHubBehaviour.Hub.Players.CurrentPlayerData).Name;
			}
			this.ItemsGrid.hideInactive = false;
			List<Transform> childList = this.ItemsGrid.GetChildList();
			GadgetInfo customGadget = GameHubBehaviour.Hub.Players.CurrentPlayerData.Character.CustomGadget0;
			List<UpgradeInfo> list = new List<UpgradeInfo>(customGadget.Upgrades);
			list.Sort((UpgradeInfo x, UpgradeInfo y) => x.GuiOrderIndex.CompareTo(y.GuiOrderIndex));
			int i = 0;
			while (i < childList.Count && i < list.Count)
			{
				HudInstancesShopItem component = childList[i].GetComponent<HudInstancesShopItem>();
				UpgradeInfo upgradeInfo = list[i];
				component.Setup(upgradeInfo, new HudInstancesShopItem.InstancesShopItemOnClickDelegate(this.ButtonOnClick), this._didAnimation);
				component.gameObject.SetActive(true);
				component.SetButtonState(UIButtonColor.State.Normal, true);
				if (!this._didAnimation)
				{
					bool flag = this._selectedUpgradeInfoName == upgradeInfo.Name;
					component.SetSelection(false, false);
					if (flag)
					{
						this._selectionInputNavigationIndex = i;
						this._joypadInputNavigationIndex = i;
					}
				}
				i++;
			}
			while (i < childList.Count)
			{
				childList[i].gameObject.SetActive(false);
				i++;
			}
			this.ItemsGrid.hideInactive = true;
			this.ItemsGrid.Reposition();
			if (!this._didAnimation)
			{
				this._didAnimation = true;
				this.TitleInAnimation.Play();
				base.StartCoroutine(this.AnimateCardsCoroutine(childList));
			}
		}

		private IEnumerator AnimateCardsCoroutine(List<Transform> gridList)
		{
			yield return new WaitForSeconds(this.AnimationCardsPreStartTimeSec);
			if (!base.gameObject.activeSelf)
			{
				yield break;
			}
			for (int i = 0; i < gridList.Count; i++)
			{
				Transform gridListTransform = gridList[i];
				if (gridListTransform.gameObject.activeSelf)
				{
					HudInstancesShopItem shopItem = gridListTransform.GetComponent<HudInstancesShopItem>();
					shopItem.PlayInAnimation();
					yield return new WaitForSeconds(this.AnimationInCardsIntervalTimeSec);
					if (!base.gameObject.activeSelf)
					{
						yield break;
					}
				}
			}
			yield return new WaitForSeconds(this.AnimationInCardsPreSelectionTimeSec);
			if (!base.gameObject.activeSelf)
			{
				yield break;
			}
			this.EnableInputNavigation(true);
			if (!GameHubBehaviour.Hub.Match.LevelIsTutorial())
			{
				this.SelectShopItem(this._selectedUpgradeInfoName, false, true);
			}
			yield break;
		}

		public void Hide()
		{
			base.gameObject.SetActive(false);
			this.EnableInputNavigation(false);
			if (!HudInstancesController.IsInShopState())
			{
				this._didAnimation = false;
			}
		}

		public void HideAnimating()
		{
			if (!base.gameObject.activeInHierarchy)
			{
				return;
			}
			this.EnableInputNavigation(false);
			this.DisableAllItemsUnselected();
			List<Transform> childList = this.ItemsGrid.GetChildList();
			for (int i = 0; i < childList.Count; i++)
			{
				Transform transform = childList[i];
				if (transform.gameObject.activeSelf)
				{
					HudInstancesShopItem component = transform.GetComponent<HudInstancesShopItem>();
					component.PlayOutAnimation(this._selectedUpgradeInfoName == component.GetUpgradeInfoName());
				}
			}
			this.TitleOutAnimation.Play();
		}

		private void EnableInputNavigation(bool isEnabled)
		{
			this._enableInputNavigation = isEnabled;
		}

		private void ButtonOnClick(HudInstancesShopItem shopItem)
		{
			this.SelectShopItem(shopItem.GetUpgradeInfoName(), true, true);
		}

		private void SelectShopItem(string upgradeInfoName, bool triggerEvent = true, bool animateButton = true)
		{
			if (!this._enableInputNavigation)
			{
				return;
			}
			this._selectedUpgradeInfoName = upgradeInfoName;
			List<Transform> childList = this.ItemsGrid.GetChildList();
			for (int i = 0; i < childList.Count; i++)
			{
				HudInstancesShopItem component = childList[i].GetComponent<HudInstancesShopItem>();
				bool flag = component.GetUpgradeInfoName() == upgradeInfoName;
				component.SetSelection(flag, animateButton);
				if (flag)
				{
					this._selectionInputNavigationIndex = i;
					this._joypadInputNavigationIndex = i;
				}
			}
			this._garageController.SelectInstance(upgradeInfoName);
			FMODAudioManager.PlayOneShotAt(this._selectionAudio, Vector3.zero, 0);
			if (triggerEvent && this.OnInstanceSelected != null)
			{
				this.OnInstanceSelected(upgradeInfoName);
			}
		}

		private void DisableAllItemsUnselected()
		{
			List<Transform> childList = this.ItemsGrid.GetChildList();
			for (int i = 0; i < childList.Count; i++)
			{
				HudInstancesShopItem component = childList[i].GetComponent<HudInstancesShopItem>();
				if (component.GetUpgradeInfoName() != this._selectedUpgradeInfoName)
				{
					component.DisableButton();
				}
			}
		}

		public void SetGarageController(GarageController garageController)
		{
			this._garageController = garageController;
		}

		protected void Update()
		{
			if (!this._enableInputNavigation || UICamera.inputHasFocus)
			{
				return;
			}
			this.InputCheckUpdate("Horizontal", ref this._lockKeypadNavigation, new Action<bool>(this.OnKeypadNavigation));
			this.InputCheckUpdate("Joy Horizontal", ref this._lockJoypadNavigation, new Action<bool>(this.OnJoypadNavigation));
			if (Input.GetKeyUp(KeyCode.JoystickButton0))
			{
				this.TryToSelectJoyIndex();
			}
		}

		private void TryToSelectJoyIndex()
		{
			HudInstancesShopItem component = this.ItemsGrid.GetChildList()[this._joypadInputNavigationIndex].GetComponent<HudInstancesShopItem>();
			string upgradeInfoName = component.GetUpgradeInfoName();
			if (upgradeInfoName != this._selectedUpgradeInfoName)
			{
				this.SelectShopItem(upgradeInfoName, true, true);
			}
		}

		private void InputCheckUpdate(string axis, ref bool lockNavigationFlag, Action<bool> onAction)
		{
			float axis2 = Input.GetAxis(axis);
			if (axis2 > 0.1f)
			{
				if (lockNavigationFlag)
				{
					return;
				}
				lockNavigationFlag = true;
				onAction(true);
			}
			else if (axis2 < -0.1f)
			{
				if (lockNavigationFlag)
				{
					return;
				}
				lockNavigationFlag = true;
				onAction(false);
			}
			else
			{
				lockNavigationFlag = false;
			}
		}

		private void OnKeypadNavigation(bool isRight)
		{
			HudInstancesShopItem hudInstancesShopItem = this.UpdateInputNavigationIndex(isRight, ref this._selectionInputNavigationIndex);
			this.SelectShopItem(hudInstancesShopItem.GetUpgradeInfoName(), true, true);
		}

		private void OnJoypadNavigation(bool isRight)
		{
			HudInstancesShopItem hudInstancesShopItem = this.UpdateInputNavigationIndex(isRight, ref this._joypadInputNavigationIndex);
			if (hudInstancesShopItem.GetUpgradeInfoName() == this._selectedUpgradeInfoName)
			{
				hudInstancesShopItem = this.UpdateInputNavigationIndex(isRight, ref this._joypadInputNavigationIndex);
			}
			List<Transform> childList = this.ItemsGrid.GetChildList();
			for (int i = 0; i < childList.Count; i++)
			{
				HudInstancesShopItem component = childList[i].GetComponent<HudInstancesShopItem>();
				if (!(component.GetUpgradeInfoName() == this._selectedUpgradeInfoName))
				{
					component.SetButtonState((!(component == hudInstancesShopItem)) ? UIButtonColor.State.Normal : UIButtonColor.State.Hover, true);
				}
			}
		}

		private HudInstancesShopItem UpdateInputNavigationIndex(bool isRight, ref int index)
		{
			List<Transform> childList = this.ItemsGrid.GetChildList();
			if (isRight)
			{
				index++;
				if (index >= childList.Count)
				{
					index = 0;
				}
			}
			else
			{
				index--;
				if (index < 0)
				{
					index = childList.Count - 1;
				}
			}
			return childList[index].GetComponent<HudInstancesShopItem>();
		}

		private static readonly BitLogger Log = new BitLogger(typeof(HudInstancesShop));

		public HudInstancesShop.OnInstanceSelectedDelegate OnInstanceSelected;

		public UIGrid ItemsGrid;

		[Header("[Animation Config]")]
		public float AnimationCardsPreStartTimeSec = 0.5f;

		public float AnimationInCardsIntervalTimeSec = 0.15f;

		public float AnimationInCardsPreSelectionTimeSec = 1f;

		[SerializeField]
		private Animation TitleInAnimation;

		[SerializeField]
		private Animation TitleOutAnimation;

		[SerializeField]
		private FMODAsset _selectionAudio;

		private GarageController _garageController;

		private string _selectedUpgradeInfoName;

		private bool _enableInputNavigation;

		private bool _lockJoypadNavigation;

		private bool _lockKeypadNavigation;

		private int _selectionInputNavigationIndex;

		private int _joypadInputNavigationIndex;

		private bool _didAnimation;

		public delegate void OnInstanceSelectedDelegate(string id);
	}
}
