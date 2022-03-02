using System;
using Assets.ClientApiObjects;
using Assets.ClientApiObjects.Components;
using HeavyMetalMachines.BI;
using HeavyMetalMachines.Frontend;
using HeavyMetalMachines.Infra.DependencyInjection.Attributes;
using HeavyMetalMachines.Input;
using HeavyMetalMachines.Input.ControllerInput;
using HeavyMetalMachines.OpenUrl;
using HeavyMetalMachines.Presenting;
using HeavyMetalMachines.Presenting.NGui;
using HeavyMetalMachines.Presenting.Unity;
using HeavyMetalMachines.VFX;
using Hoplon.Input;
using Hoplon.Input.Business;
using Hoplon.Input.UiNavigation;
using Pocketverse;
using UniRx;
using UnityEngine;
using Zenject;

namespace HeavyMetalMachines.DriverHelper
{
	public class DriverHelperController : HudWindow, IDriverHelper
	{
		private IUiNavigationGroupHolder UiNavigationGroupHolder
		{
			get
			{
				return this._uiNavigationGroupHolder;
			}
		}

		private IButton ExitButton
		{
			get
			{
				return this._exitButton;
			}
		}

		private IButton CharacterDetailsButton
		{
			get
			{
				return this._characterDetailsButton;
			}
		}

		private IActivatable CharacterDetailsButtonActivatable
		{
			get
			{
				return this._characterDetailsButtonActivatable;
			}
		}

		public bool Visible
		{
			get
			{
				return this.IsVisible;
			}
		}

		public IObservable<bool> VisibilityChanged()
		{
			return this._visibilityObservation;
		}

		public override void ChangeWindowVisibility(bool visible)
		{
			base.ChangeWindowVisibility(visible);
			this._visibilityObservation.OnNext(visible);
			this.LogBi(visible);
			this.SetUiNavigationFocus(visible);
		}

		private void SetUiNavigationFocus(bool focused)
		{
			if (focused)
			{
				this.UiNavigationGroupHolder.AddGroup();
			}
			else
			{
				this.UiNavigationGroupHolder.RemoveGroup();
			}
		}

		private void LogBi(bool visible)
		{
			if (visible)
			{
				switch (GameHubBehaviour.Hub.State.Current.StateKind)
				{
				case GameState.GameStateKind.MainMenu:
					this._buttonBiLogger.LogButtonClick(ButtonName.ShopDriverHelper);
					return;
				case GameState.GameStateKind.Pick:
					this._buttonBiLogger.LogButtonClick(ButtonName.PickDriverHelper);
					return;
				case GameState.GameStateKind.Game:
					this._buttonBiLogger.LogButtonClick(ButtonName.IngameDriverHelper);
					return;
				}
				this._buttonBiLogger.LogButtonClick(ButtonName.UnknownDriverHelper);
			}
		}

		public override bool CanOpen()
		{
			return false;
		}

		public void Setup(IItemType driver, GameState state)
		{
			this._currentDriver = driver;
			this._currentState = state;
			this.ConfigureForCharacterHierarchy(this._currentDriver);
		}

		private void Awake()
		{
			this.OverlayEventTrigger.onClick.Clear();
			this.OverlayEventTrigger.onClick.Add(new EventDelegate(new EventDelegate.Callback(this.OnCloseButtonEventDelegate)));
			this._visibilityObservation = new Subject<bool>();
			this._exitButtonClickDisposable = ObservableExtensions.Subscribe<Unit>(this.ExitButton.OnClick(), delegate(Unit _)
			{
				base.SetWindowVisibility(false);
			});
			this._characterDetailsButtonClickDisposable = ObservableExtensions.Subscribe<Unit>(this.CharacterDetailsButton.OnClick(), delegate(Unit _)
			{
				this.OpenCharacterDetails();
			});
		}

		public override void OnDestroy()
		{
			this._visibilityObservation.Dispose();
			this._exitButtonClickDisposable.Dispose();
			this._characterDetailsButtonClickDisposable.Dispose();
			base.OnDestroy();
		}

		private void OnCloseButtonEventDelegate()
		{
			base.SetWindowVisibility(false);
		}

		private void OnEnable()
		{
			this._inputBindNotifierDisposable = ObservableExtensions.Subscribe<int>(Observable.Do<int>(this._inputBindNotifier.ObserveBind(), delegate(int actionId)
			{
				this.OnKeyChangedCallback();
			}));
			this._inputBindResetDefaultNotifierDisposable = ObservableExtensions.Subscribe<Unit>(Observable.Do<Unit>(this._inputBindNotifier.ObserveResetDefault(), delegate(Unit _)
			{
				this.OnRestoreDefaultCallback();
			}));
			this._inputActiveDeviceChangeNotifierDisposable = ObservableExtensions.Subscribe<InputDevice>(Observable.Do<InputDevice>(this._inputActiveDeviceChangeNotifier.ObserveActiveDeviceChange(), delegate(InputDevice inputDevice)
			{
				this.OnActiveDeviceChange();
			}));
		}

		private void OnActiveDeviceChange()
		{
			this.UpdateIconsAndText();
		}

		private void OnRestoreDefaultCallback()
		{
			this.UpdateIconsAndText();
		}

		private void OnKeyChangedCallback()
		{
			this.UpdateIconsAndText();
		}

		private void UpdateIconsAndText()
		{
			if (this._currentDriver == null)
			{
				return;
			}
			this.ConfigureForCharacterHierarchy(this._currentDriver);
		}

		private void OnDisable()
		{
			if (this._inputBindNotifierDisposable != null)
			{
				this._inputBindNotifierDisposable.Dispose();
				this._inputBindNotifierDisposable = null;
			}
			if (this._inputBindResetDefaultNotifierDisposable != null)
			{
				this._inputBindResetDefaultNotifierDisposable.Dispose();
				this._inputBindResetDefaultNotifierDisposable = null;
			}
			if (this._inputActiveDeviceChangeNotifierDisposable != null)
			{
				this._inputActiveDeviceChangeNotifierDisposable.Dispose();
				this._inputActiveDeviceChangeNotifierDisposable = null;
			}
		}

		private void ConfigureForCharacterHierarchy(IItemType character)
		{
			CharacterItemTypeComponent component = character.GetComponent<CharacterItemTypeComponent>();
			CharacterItemTypeComponent component2 = character.GetComponent<CharacterItemTypeComponent>();
			string str = character.Name.ToUpper();
			this.DriverInfoGui.PilotNameLabel.text = Language.Get(string.Format("{0}_NAME", component2.AssetPrefix.ToUpper()), this.DriverInfoGui.DescriptionsSheet);
			this.DriverInfoGui.PilotIconSprite.SpriteName = HudUtils.GetPlayerIconName(GameHubBehaviour.Hub, character.Id, HudUtils.PlayerIconSize.Size64);
			switch (component.Role)
			{
			case 0:
			case 6:
				this.DriverInfoGui.RoleNameLabel.text = Language.Get(this.DriverInfoGui.RoleSupportDraft, this.DriverInfoGui.RoleTranslationSheet);
				this.DriverInfoGui.RoleNameLabel.color = this.DriverInfoGui.RoleLabelSupportColor;
				break;
			case 1:
			case 3:
			case 5:
				this.DriverInfoGui.RoleNameLabel.text = Language.Get(this.DriverInfoGui.RoleCarrierDraft, this.DriverInfoGui.RoleTranslationSheet);
				this.DriverInfoGui.RoleNameLabel.color = this.DriverInfoGui.RoleLabelCarrierColor;
				break;
			case 2:
			case 4:
				this.DriverInfoGui.RoleNameLabel.text = Language.Get(this.DriverInfoGui.RoleTacklerDraft, this.DriverInfoGui.RoleTranslationSheet);
				this.DriverInfoGui.RoleNameLabel.color = this.DriverInfoGui.RoleLabelTacklerColor;
				break;
			}
			this.GadgetPassiveIcon.transform.parent.gameObject.SetActive(component.HasPassive);
			this.GadgetsGrid.repositionNow = true;
			if (component.HasPassive)
			{
				this.GadgetPassiveIcon.SpriteName = character.Name + "_GadgetPassive";
				this.GadgetPassiveName.text = Language.Get(str + "_GADGET_PASSIVE_NAME", this.DriverInfoGui.NamesSheet);
				if (Language.Has(str + "_GADGET_PASSIVE_COOLDOWN", this.DriverInfoGui.NamesSheet))
				{
					this.GadgetPassiveCooldowDescription.text = Language.Get(str + "_GADGET_PASSIVE_COOLDOWN", this.DriverInfoGui.NamesSheet);
				}
				else
				{
					this.GadgetPassiveCooldowDescription.text = string.Empty;
				}
				this.GadgetPassiveCooldowDescription.gameObject.SetActive(!string.IsNullOrEmpty(this.GadgetPassiveCooldowDescription.text));
				this.GadgetPassiveShortDescription.text = Language.Get(str + "_GADGET_PASSIVE_BASE", this.DriverInfoGui.DescriptionsSheet);
			}
			this.Gadget0Icon.SpriteName = character.Name + "_Gadget01";
			this.Gadget0Name.text = Language.Get(str + "_GADGET_00_NAME", this.DriverInfoGui.NamesSheet);
			this.Gadget0CooldowDescription.text = Language.Get(str + "_GADGET_00_COOLDOWN", this.DriverInfoGui.NamesSheet);
			this.Gadget0CooldowDescription.gameObject.SetActive(!string.IsNullOrEmpty(this.Gadget0CooldowDescription.text));
			this.Gadget0ShortDescription.text = Language.Get(str + "_GADGET_00_BASE", this.DriverInfoGui.DescriptionsSheet);
			this.SetShortcut(5, this.Gadget0ShortcutIcon, this.Gadget0ShortcutLabel, this.Gadget0_label_group);
			this.Gadget1Icon.SpriteName = character.Name + "_Gadget02";
			this.Gadget1Name.text = Language.Get(str + "_GADGET_01_NAME", this.DriverInfoGui.NamesSheet);
			this.Gadget1CooldowDescription.text = Language.Get(str + "_GADGET_01_COOLDOWN", this.DriverInfoGui.NamesSheet);
			this.Gadget1CooldowDescription.gameObject.SetActive(!string.IsNullOrEmpty(this.Gadget1CooldowDescription.text));
			this.Gadget1ShortDescription.text = Language.Get(str + "_GADGET_01_BASE", this.DriverInfoGui.DescriptionsSheet);
			this.SetShortcut(6, this.Gadget1ShortcutIcon, this.Gadget1ShortcutLabel, this.Gadget1_label_group);
			this.GadgetEspeciaIcon.SpriteName = character.Name + "_Gadget03";
			this.GadgetEspecialName.text = Language.Get(str + "_GADGET_02_NAME", this.DriverInfoGui.NamesSheet);
			this.GadgetEspecialCooldowDescription.text = Language.Get(str + "_GADGET_02_COOLDOWN", this.DriverInfoGui.NamesSheet);
			this.GadgetEspecialCooldowDescription.gameObject.SetActive(!string.IsNullOrEmpty(this.GadgetEspecialCooldowDescription.text));
			this.GadgetEspecialShortDescription.text = Language.Get(str + "_GADGET_02_BASE", this.DriverInfoGui.DescriptionsSheet);
			this.SetShortcut(7, this.GadgetEspecialShortcutIcon, this.GadgetEspecialShortcutLabel, this.GadgetEspecial_label_group);
			this.NitroIcon.SpriteName = character.Name + "_GadgetNitro";
			this.NitroName.text = Language.Get(str + "_GADGET_03_NAME", this.DriverInfoGui.NamesSheet);
			this.NitroShortDescription.text = Language.Get(str + "_GADGET_03_BASE", this.DriverInfoGui.DescriptionsSheet);
			this.NitroCooldowDescription.text = Language.Get(str + "_GADGET_03_COOLDOWN", this.DriverInfoGui.NamesSheet);
			this.SetShortcut(15, this.NitroShortcutIcon, this.NitroShortcutLabel, this.Nitro_label_group);
			this.SetupBorder();
			this.SetupSiteDetailsButton();
		}

		private void SetShortcut(ControllerInputActions controlAction, UI2DSprite sprite, UILabel label, GameObject labelGroup)
		{
			ISprite sprite2;
			string text;
			if (this._inputTranslation.TryToGetInputActionActiveDeviceAssetOrFallbackToTranslation(controlAction, ref sprite2, ref text))
			{
				sprite.sprite2D = (sprite2 as UnitySprite).GetSprite();
				labelGroup.SetActive(false);
				sprite.gameObject.SetActive(true);
			}
			else
			{
				label.text = text;
				labelGroup.SetActive(true);
				sprite.gameObject.SetActive(false);
			}
		}

		private void SetupBorder()
		{
			this.MainmenuBorder.SetActive(!this.IsInGame());
		}

		private void SetupSiteDetailsButton()
		{
			if (this.IsInGame())
			{
				ActivatableExtensions.Deactivate(this.CharacterDetailsButtonActivatable);
			}
			else
			{
				ActivatableExtensions.Activate(this.CharacterDetailsButtonActivatable);
			}
		}

		private bool IsInGame()
		{
			return this._currentState is Game;
		}

		private void OpenCharacterDetails()
		{
			IClientButtonBILogger clientButtonBILogger = this._diContainer.Resolve<IClientButtonBILogger>();
			clientButtonBILogger.LogButtonClick(ButtonName.DriverHelperDetails);
			IOpenUrl openUrl = this._diContainer.Resolve<IOpenUrl>();
			openUrl.OpenCharacterHelp(this._currentDriver.Id);
		}

		[Header("[General]")]
		[SerializeField]
		private GameObject MainmenuBorder;

		[SerializeField]
		private NGuiButton _exitButton;

		[SerializeField]
		private NGuiButton _characterDetailsButton;

		[SerializeField]
		private GameObjectActivatable _characterDetailsButtonActivatable;

		[Header("[Driver]")]
		[SerializeField]
		private DriverHelperCharInfo DriverInfoGui;

		[Header("[Gadgets]")]
		[SerializeField]
		private UIGrid GadgetsGrid;

		[SerializeField]
		private HMMUI2DDynamicSprite GadgetPassiveIcon;

		[SerializeField]
		private UILabel GadgetPassiveName;

		[SerializeField]
		private UILabel GadgetPassiveShortDescription;

		[SerializeField]
		private UILabel GadgetPassiveCooldowDescription;

		[SerializeField]
		private HMMUI2DDynamicSprite Gadget0Icon;

		[SerializeField]
		private UI2DSprite Gadget0ShortcutIcon;

		[SerializeField]
		private UILabel Gadget0ShortcutLabel;

		[SerializeField]
		private GameObject Gadget0_label_group;

		[SerializeField]
		private UILabel Gadget0Name;

		[SerializeField]
		private UILabel Gadget0ShortDescription;

		[SerializeField]
		private UILabel Gadget0CooldowDescription;

		[SerializeField]
		private HMMUI2DDynamicSprite Gadget1Icon;

		[SerializeField]
		private UI2DSprite Gadget1ShortcutIcon;

		[SerializeField]
		private UILabel Gadget1ShortcutLabel;

		[SerializeField]
		private GameObject Gadget1_label_group;

		[SerializeField]
		private UILabel Gadget1Name;

		[SerializeField]
		private UILabel Gadget1ShortDescription;

		[SerializeField]
		private UILabel Gadget1CooldowDescription;

		[SerializeField]
		private HMMUI2DDynamicSprite GadgetEspeciaIcon;

		[SerializeField]
		private UI2DSprite GadgetEspecialShortcutIcon;

		[SerializeField]
		private UILabel GadgetEspecialShortcutLabel;

		[SerializeField]
		private GameObject GadgetEspecial_label_group;

		[SerializeField]
		private UILabel GadgetEspecialName;

		[SerializeField]
		private UILabel GadgetEspecialShortDescription;

		[SerializeField]
		private UILabel GadgetEspecialCooldowDescription;

		[SerializeField]
		private UILabel NitroName;

		[SerializeField]
		private UILabel NitroShortDescription;

		[SerializeField]
		private UILabel NitroCooldowDescription;

		[SerializeField]
		private HMMUI2DDynamicSprite NitroIcon;

		[SerializeField]
		private UI2DSprite NitroShortcutIcon;

		[SerializeField]
		private UILabel NitroShortcutLabel;

		[SerializeField]
		private GameObject Nitro_label_group;

		[Header("[Overlay]")]
		[SerializeField]
		private UIEventTrigger OverlayEventTrigger;

		[Header("[Ui Navigation]")]
		[SerializeField]
		private UiNavigationGroupHolder _uiNavigationGroupHolder;

		[InjectOnClient]
		private DiContainer _diContainer;

		[InjectOnClient]
		private IInputTranslation _inputTranslation;

		[InjectOnClient]
		private IInputBindNotifier _inputBindNotifier;

		[InjectOnClient]
		private IInputActiveDeviceChangeNotifier _inputActiveDeviceChangeNotifier;

		[Inject]
		private IClientButtonBILogger _buttonBiLogger;

		private IItemType _currentDriver;

		private GameState _currentState;

		private Subject<bool> _visibilityObservation;

		private IDisposable _inputBindNotifierDisposable;

		private IDisposable _inputBindResetDefaultNotifierDisposable;

		private IDisposable _inputActiveDeviceChangeNotifierDisposable;

		private IDisposable _exitButtonClickDisposable;

		private IDisposable _characterDetailsButtonClickDisposable;
	}
}
