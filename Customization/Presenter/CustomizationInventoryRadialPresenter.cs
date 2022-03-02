using System;
using Assets.ClientApiObjects;
using Assets.ClientApiObjects.Components;
using HeavyMetalMachines.Customization.Business;
using HeavyMetalMachines.Customization.Infra;
using HeavyMetalMachines.DataTransferObjects.Battlepass;
using HeavyMetalMachines.DataTransferObjects.Util;
using HeavyMetalMachines.Frontend;
using HeavyMetalMachines.Input;
using HeavyMetalMachines.Items.DataTransferObjects;
using HeavyMetalMachines.Presenting;
using HeavyMetalMachines.RadialMenu.View;
using Hoplon.Input;
using UniRx;
using UnityEngine;

namespace HeavyMetalMachines.Customization.Presenter
{
	public class CustomizationInventoryRadialPresenter : ICustomizationInventoryRadialPresenter, IPresenter
	{
		public CustomizationInventoryRadialPresenter(IRadialCustomizationView view, ICustomizationService customizationService, CustomizationInventoryComponent inventoryComponent, IInputActiveDeviceChangeNotifier inputActiveDeviceChangeNotifier, IInputGetActiveDevicePoller inputGetActiveDevicePoller, IInputTranslation inputTranslation)
		{
			this._view = view;
			this._customizationService = customizationService;
			this._inventoryComponent = inventoryComponent;
			this._inputActiveDeviceChangeNotifier = inputActiveDeviceChangeNotifier;
			this._inputGetActiveDevicePoller = inputGetActiveDevicePoller;
			this._inputTranslation = inputTranslation;
		}

		private IGetCustomizationChange GetCustomizationChange
		{
			get
			{
				return this._inventoryComponent;
			}
		}

		private IGetCustomizationHoverChange GetCustomizationHoverChange
		{
			get
			{
				return this._inventoryComponent;
			}
		}

		public IObservable<Unit> Initialize()
		{
			this._view.EmoteNameLabel.Text = string.Empty;
			this.InitializeEquipShortcut();
			this._itemChangeDisposable = ObservableExtensions.Subscribe<ItemChangeRequestState>(Observable.Do<ItemChangeRequestState>(this.GetCustomizationChange.OnItemEquipChanged, new Action<ItemChangeRequestState>(this.OnItemEquipChanged)));
			this._itemHoverChangeDisposable = ObservableExtensions.Subscribe<CustomizationInventoryCellItemData>(this.GetCustomizationHoverChange.ObserveHoverChange, new Action<CustomizationInventoryCellItemData>(this.OnItemHoverChange));
			this._inputActiveDeviceChangeNotifierDisposable = ObservableExtensions.Subscribe<InputDevice>(this._inputActiveDeviceChangeNotifier.GetAndObserveActiveDeviceChange(), delegate(InputDevice inputDevice)
			{
				this.UpdateEquipShortcut();
			});
			return Observable.ReturnUnit();
		}

		private void InitializeEquipShortcut()
		{
			ISprite shortcutImageSprite;
			string text;
			if (this._inputTranslation.TryToGetInputJoystickAssetOrFallbackToTranslation(4, ref shortcutImageSprite, ref text))
			{
				this._view.SetupEquipShortcutImage(shortcutImageSprite);
			}
		}

		private void UpdateEquipShortcut()
		{
			if (this._isItemHovered && this._inputGetActiveDevicePoller.GetActiveDevice() == 3)
			{
				ActivatableExtensions.Activate(this._view.EquipGroupActivatable);
			}
			else
			{
				ActivatableExtensions.Deactivate(this._view.EquipGroupActivatable);
			}
		}

		private void OnItemHoverChange(CustomizationInventoryCellItemData data)
		{
			this._isItemHovered = (data != null);
			string text = (data != null) ? Language.Get(data.ItemName, 6) : string.Empty;
			this._view.EmoteNameLabel.Text = text;
			this._view.EquipLabel.Text = Language.Get((!this._isItemHovered || !this._inventoryComponent.GetIsItemEquiped(data)) ? "BATTLEPASS_INVENTORY_EQUIP" : "BATTLEPASS_INVENTORY_UNEQUIP", 35);
			this.UpdateEquipShortcut();
		}

		public IObservable<Unit> Show()
		{
			return Observable.Defer<Unit>(delegate()
			{
				this.RefreshEmotes();
				return this.PlayAnimation(this._view.WindowAnimation, "radial_show");
			});
		}

		private void OnItemEquipChanged(ItemChangeRequestState e)
		{
			if (e.CategoryId == InventoryMapper.EmoteCategoryGuid)
			{
				this.RefreshEmotes();
			}
		}

		public IObservable<Unit> Hide()
		{
			return Observable.Do<Unit>(Observable.Do<Unit>(this.PlayAnimation(this._view.WindowAnimation, "radial_hide"), delegate(Unit _)
			{
				this._isItemHovered = false;
			}), delegate(Unit _)
			{
				this._hideSubject.OnNext(Unit.Default);
			});
		}

		public IObservable<Unit> Dispose()
		{
			if (this._itemChangeDisposable != null)
			{
				this._itemChangeDisposable.Dispose();
				this._itemChangeDisposable = null;
			}
			if (this._itemHoverChangeDisposable != null)
			{
				this._itemHoverChangeDisposable.Dispose();
				this._itemHoverChangeDisposable = null;
			}
			if (this._inputActiveDeviceChangeNotifierDisposable != null)
			{
				this._inputActiveDeviceChangeNotifierDisposable.Dispose();
				this._inputActiveDeviceChangeNotifierDisposable = null;
			}
			return Observable.ReturnUnit();
		}

		public IObservable<Unit> ObserveHide()
		{
			return this._hideSubject;
		}

		private IObservable<Unit> PlayAnimation(Animation anim, string clipName)
		{
			return Observable.Delay<Unit>(Observable.Do<Unit>(Observable.ReturnUnit(), delegate(Unit _)
			{
				anim.Play(clipName);
			}), TimeSpan.FromSeconds((double)anim.GetClip(clipName).length));
		}

		public void RefreshEmotes()
		{
			CustomizationContent currentPlayerCustomizationContent = this._customizationService.GetCurrentPlayerCustomizationContent();
			this.UpdateEmote(40, currentPlayerCustomizationContent, this._view.SpritesheetAnimators[1], this._view.UnequipItems[1]);
			this.UpdateEmote(41, currentPlayerCustomizationContent, this._view.SpritesheetAnimators[2], this._view.UnequipItems[2]);
			this.UpdateEmote(42, currentPlayerCustomizationContent, this._view.SpritesheetAnimators[3], this._view.UnequipItems[3]);
			this.UpdateEmote(43, currentPlayerCustomizationContent, this._view.SpritesheetAnimators[4], this._view.UnequipItems[4]);
		}

		private void UpdateEmote(PlayerCustomizationSlot slot, CustomizationContent customization, ITextureMappingUpdater textureMappingUpdater, IUnequipItem unequipItem)
		{
			ItemTypeScriptableObject itemTypeScriptableObjectBySlot = this._customizationService.GetItemTypeScriptableObjectBySlot(slot, customization);
			unequipItem.Setup(itemTypeScriptableObjectBySlot);
			if (itemTypeScriptableObjectBySlot == null)
			{
				textureMappingUpdater.ChangeVisibility(false);
				return;
			}
			textureMappingUpdater.ChangeVisibility(true);
			EmoteItemTypeComponent component = itemTypeScriptableObjectBySlot.GetComponent<EmoteItemTypeComponent>();
			textureMappingUpdater.TryToLoadAsset(component.spriteSheetName);
			textureMappingUpdater.StartAnimation();
			textureMappingUpdater.Stop();
		}

		private const string ShowAnimation = "radial_show";

		private const string HideAnimation = "radial_hide";

		private readonly ICustomizationService _customizationService;

		private readonly IRadialCustomizationView _view;

		private readonly ISubject<Unit> _hideSubject = new Subject<Unit>();

		private readonly CustomizationInventoryComponent _inventoryComponent;

		private readonly IInputActiveDeviceChangeNotifier _inputActiveDeviceChangeNotifier;

		private readonly IInputGetActiveDevicePoller _inputGetActiveDevicePoller;

		private readonly IInputTranslation _inputTranslation;

		private IDisposable _itemChangeDisposable;

		private IDisposable _itemHoverChangeDisposable;

		private IDisposable _inputActiveDeviceChangeNotifierDisposable;

		private bool _isItemHovered;
	}
}
