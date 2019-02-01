using System;
using Assets.ClientApiObjects;
using Assets.ClientApiObjects.Components;
using HeavyMetalMachines.Car;
using HeavyMetalMachines.Character;
using HeavyMetalMachines.Combat;
using HeavyMetalMachines.Frontend;
using HeavyMetalMachines.Options;
using HeavyMetalMachines.Utils;
using HeavyMetalMachines.VFX;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines
{
	public class DriverHelperController : HudWindow
	{
		public override bool CanOpen()
		{
			if (GameHubBehaviour.Hub.State.Current.StateKind == GameState.GameStateKind.Game && GameHubBehaviour.Hub.Match.LevelIsTutorial())
			{
				return false;
			}
			if (this._currentDriver == null)
			{
				return false;
			}
			if (HudWindowManager.Instance.IsWindowVisible<OptionsWindow>())
			{
				return false;
			}
			if (HudWindowManager.Instance.IsWindowVisible<EscMenuGui>())
			{
				return false;
			}
			HudWindowManager.GuiGameState state = HudWindowManager.Instance.State;
			if (state == HudWindowManager.GuiGameState.MainMenu)
			{
				MainMenuGui stateGuiController = GameHubBehaviour.Hub.State.Current.GetStateGuiController<MainMenuGui>();
				return stateGuiController.Shop.Details.IsVisible();
			}
			if (state == HudWindowManager.GuiGameState.Game)
			{
				return GameHubBehaviour.Hub.BombManager.ScoreBoard.CurrentState != BombScoreBoard.State.EndGame;
			}
			return state == HudWindowManager.GuiGameState.PickScreen;
		}

		public void Setup(ItemTypeScriptableObject driver, GameState state)
		{
			this._currentDriver = driver;
			this._currentState = state;
			this.Configure();
		}

		private void Awake()
		{
			this.OverlayEventTrigger.onClick.Clear();
			this.OverlayEventTrigger.onClick.Add(new EventDelegate(new EventDelegate.Callback(this.OnCloseButtonEventDelegate)));
		}

		private void OnCloseButtonEventDelegate()
		{
			base.SetWindowVisibility(false);
		}

		private void OnEnable()
		{
			ControlOptions controls = GameHubBehaviour.Hub.Options.Controls;
			GameHubBehaviour.Hub.GuiScripts.Esc.OnControlModeChangedCallback += this.OnControlModeChangedCallback;
			controls.OnKeyChangedCallback += this.OnKeyChangedCallback;
			controls.OnResetDefaultCallback += this.OnRestoreDefaultCallback;
			controls.OnResetPrimaryDefaultCallback += this.OnRestoreDefaultCallback;
			controls.OnResetSecondaryDefaultCallback += this.OnRestoreDefaultCallback;
		}

		private void OnControlModeChangedCallback(CarInput.DrivingStyleKind drivingStyleKind)
		{
			this.UpdateIconsAndText();
		}

		private void OnRestoreDefaultCallback()
		{
			this.UpdateIconsAndText();
		}

		private void OnKeyChangedCallback(ControlAction controlAction)
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
			ControlOptions controls = GameHubBehaviour.Hub.Options.Controls;
			GameHubBehaviour.Hub.GuiScripts.Esc.OnControlModeChangedCallback -= this.OnControlModeChangedCallback;
			controls.OnKeyChangedCallback -= this.OnKeyChangedCallback;
			controls.OnResetDefaultCallback -= this.OnRestoreDefaultCallback;
			controls.OnResetPrimaryDefaultCallback -= this.OnRestoreDefaultCallback;
			controls.OnResetSecondaryDefaultCallback -= this.OnRestoreDefaultCallback;
		}

		public void Clear()
		{
			this._currentDriver = null;
		}

		public void ConfigureForCharacterHierarchy(ItemTypeScriptableObject character)
		{
			HeavyMetalMachines.Character.CharacterInfo characterInfo = null;
			GameHubBehaviour.Hub.InventoryColletion.CharactersByTypeId.TryGetValue(character.Id, out characterInfo);
			CharacterItemTypeComponent component = character.GetComponent<CharacterItemTypeComponent>();
			string str = character.Name.ToUpper();
			this.DriverInfoGui.PilotNameLabel.text = Language.Get(component.MainAttributes.DraftName, this.DriverInfoGui.DescriptionsSheet);
			this.DriverInfoGui.PilotIconSprite.SpriteName = HudUtils.GetPlayerIconName(GameHubBehaviour.Hub, character.Id, HudUtils.PlayerIconSize.Size64);
			switch (characterInfo.Role)
			{
			case HeavyMetalMachines.Character.CharacterInfo.DriverRoleKind.Support:
			case HeavyMetalMachines.Character.CharacterInfo.DriverRoleKind.SupportCarrierTackler:
				this.DriverInfoGui.RoleNameLabel.text = Language.Get(this.DriverInfoGui.RoleSupportDraft, this.DriverInfoGui.RoleTranslationSheet);
				this.DriverInfoGui.RoleNameLabel.color = this.DriverInfoGui.RoleLabelSupportColor;
				break;
			case HeavyMetalMachines.Character.CharacterInfo.DriverRoleKind.Carrier:
			case HeavyMetalMachines.Character.CharacterInfo.DriverRoleKind.CarrierSupport:
			case HeavyMetalMachines.Character.CharacterInfo.DriverRoleKind.CarrierTackler:
				this.DriverInfoGui.RoleNameLabel.text = Language.Get(this.DriverInfoGui.RoleCarrierDraft, this.DriverInfoGui.RoleTranslationSheet);
				this.DriverInfoGui.RoleNameLabel.color = this.DriverInfoGui.RoleLabelCarrierColor;
				break;
			case HeavyMetalMachines.Character.CharacterInfo.DriverRoleKind.Tackler:
			case HeavyMetalMachines.Character.CharacterInfo.DriverRoleKind.TacklerSupport:
				this.DriverInfoGui.RoleNameLabel.text = Language.Get(this.DriverInfoGui.RoleTacklerDraft, this.DriverInfoGui.RoleTranslationSheet);
				this.DriverInfoGui.RoleNameLabel.color = this.DriverInfoGui.RoleLabelTacklerColor;
				break;
			}
			this.GadgetPassiveIcon.transform.parent.gameObject.SetActive(characterInfo.HasPassive);
			this.GadgetsGrid.repositionNow = true;
			if (characterInfo.HasPassive)
			{
				this.GadgetPassiveIcon.SpriteName = character.Name + "_GadgetPassive";
				this.GadgetPassiveName.text = Language.Get(str + "_GADGET_PASSIVE_NAME", this.DriverInfoGui.NamesSheet);
				this.GadgetPassiveCooldowDescription.text = characterInfo.PassiveGadget.LocalizedCooldownDescription;
				this.GadgetPassiveCooldowDescription.gameObject.SetActive(!string.IsNullOrEmpty(characterInfo.PassiveGadget.DraftCooldownDescription));
				this.GadgetPassiveShortDescription.text = Language.Get(str + "_GADGET_PASSIVE_BASE", this.DriverInfoGui.DescriptionsSheet);
			}
			this.Gadget0Icon.SpriteName = character.Name + "_Gadget01";
			this.Gadget0Name.text = Language.Get(str + "_GADGET_00_NAME", this.DriverInfoGui.NamesSheet);
			this.Gadget0CooldowDescription.text = characterInfo.CustomGadget0.LocalizedCooldownDescription;
			this.Gadget0CooldowDescription.gameObject.SetActive(!string.IsNullOrEmpty(characterInfo.CustomGadget0.DraftCooldownDescription));
			this.Gadget0ShortDescription.text = Language.Get(str + "_GADGET_00_BASE", this.DriverInfoGui.DescriptionsSheet);
			this.SetShortcut(ControlAction.GadgetBasic, this.Gadget0ShortcutIcon, this.Gadget0ShortcutLabel, this.Gadget0_label_group);
			this.Gadget1Icon.SpriteName = character.Name + "_Gadget02";
			this.Gadget1Name.text = Language.Get(str + "_GADGET_01_NAME", this.DriverInfoGui.NamesSheet);
			this.Gadget1CooldowDescription.text = characterInfo.CustomGadget1.LocalizedCooldownDescription;
			this.Gadget1CooldowDescription.gameObject.SetActive(!string.IsNullOrEmpty(characterInfo.CustomGadget1.DraftCooldownDescription));
			this.Gadget1ShortDescription.text = Language.Get(str + "_GADGET_01_BASE", this.DriverInfoGui.DescriptionsSheet);
			this.SetShortcut(ControlAction.Gadget0, this.Gadget1ShortcutIcon, this.Gadget1ShortcutLabel, this.Gadget1_label_group);
			this.GadgetEspeciaIcon.SpriteName = character.Name + "_Gadget03";
			this.GadgetEspecialName.text = Language.Get(str + "_GADGET_02_NAME", this.DriverInfoGui.NamesSheet);
			this.GadgetEspecialCooldowDescription.text = characterInfo.CustomGadget2.LocalizedCooldownDescription;
			this.GadgetEspecialCooldowDescription.gameObject.SetActive(!string.IsNullOrEmpty(characterInfo.CustomGadget2.DraftCooldownDescription));
			this.GadgetEspecialShortDescription.text = Language.Get(str + "_GADGET_02_BASE", this.DriverInfoGui.DescriptionsSheet);
			this.SetShortcut(ControlAction.Gadget1, this.GadgetEspecialShortcutIcon, this.GadgetEspecialShortcutLabel, this.GadgetEspecial_label_group);
			this.NitroIcon.SpriteName = character.Name + "_GadgetNitro";
			this.NitroName.text = characterInfo.BoostGadget.LocalizedName;
			this.NitroShortDescription.text = Language.Get(str + "_GADGET_03_BASE", this.DriverInfoGui.DescriptionsSheet);
			this.NitroCooldowDescription.text = characterInfo.BoostGadget.LocalizedCooldownDescription;
			this.SetShortcut(ControlAction.GadgetBoost, this.NitroShortcutIcon, this.NitroShortcutLabel, this.Nitro_label_group);
			if (this._currentState is Game)
			{
				this.MainmenuBorder.SetActive(false);
				return;
			}
			this.MainmenuBorder.SetActive(true);
		}

		private void Configure()
		{
			this.ConfigureForCharacterHierarchy(this._currentDriver);
		}

		private void SetShortcut(ControlAction controlAction, UI2DSprite sprite, UILabel label, GameObject label_group)
		{
			bool flag = ControlOptions.IsUsingControllerJoystick(GameHubBehaviour.Hub);
			KeyCode keyCode;
			if (flag)
			{
				sprite.sprite2D = GameHubBehaviour.Hub.GuiScripts.JoystickShortcutIcons.GetJoystickShortcutIcon(ControlOptions.GetShortText(controlAction, ControlOptions.ControlActionInputType.Secondary));
				label_group.SetActive(false);
				sprite.gameObject.SetActive(true);
			}
			else if (ControlOptions.IsMouseInput(controlAction, out keyCode))
			{
				label_group.SetActive(false);
				sprite.gameObject.SetActive(true);
				switch (keyCode)
				{
				case KeyCode.Mouse0:
					sprite.sprite2D = this.Mouse0Sprite;
					break;
				case KeyCode.Mouse1:
					sprite.sprite2D = this.Mouse1Sprite;
					break;
				case KeyCode.Mouse2:
					sprite.sprite2D = this.Mouse2Sprite;
					break;
				default:
					HeavyMetalMachines.Utils.Debug.Assert(false, string.Format("UIGadgetContructor.OnKeyChangedCallback - Invalid mouse input [{0}]", keyCode), HeavyMetalMachines.Utils.Debug.TargetTeam.All);
					break;
				}
			}
			else
			{
				label.text = ControlOptions.GetTextlocalized(controlAction, ControlOptions.ControlActionInputType.Primary);
				label_group.SetActive(true);
				sprite.gameObject.SetActive(false);
			}
		}

		public override void ChangeWindowVisibility(bool visible)
		{
			base.ChangeWindowVisibility(visible);
			if (visible)
			{
				GameHubBehaviour.Hub.CursorManager.Push(true, CursorManager.CursorTypes.MatchstatsCursor);
			}
			else
			{
				GameHubBehaviour.Hub.CursorManager.Pop();
			}
		}

		[Header("General")]
		private ItemTypeScriptableObject _currentDriver;

		public Sprite Mouse0Sprite;

		public Sprite Mouse1Sprite;

		public Sprite Mouse2Sprite;

		public GameObject MainmenuBorder;

		[Header("Driver")]
		[SerializeField]
		private DriverHelperController.DriverHelperCharInfo DriverInfoGui;

		[Header("Gadgets")]
		public UIGrid GadgetsGrid;

		public HMMUI2DDynamicSprite GadgetPassiveIcon;

		public UILabel GadgetPassiveName;

		public UILabel GadgetPassiveShortDescription;

		public UILabel GadgetPassiveCooldowDescription;

		public HMMUI2DDynamicSprite Gadget0Icon;

		public UI2DSprite Gadget0ShortcutIcon;

		public UILabel Gadget0ShortcutLabel;

		public GameObject Gadget0_label_group;

		public UILabel Gadget0Name;

		public UILabel Gadget0ShortDescription;

		public UILabel Gadget0CooldowDescription;

		public HMMUI2DDynamicSprite Gadget1Icon;

		public UI2DSprite Gadget1ShortcutIcon;

		public UILabel Gadget1ShortcutLabel;

		public GameObject Gadget1_label_group;

		public UILabel Gadget1Name;

		public UILabel Gadget1ShortDescription;

		public UILabel Gadget1CooldowDescription;

		public HMMUI2DDynamicSprite GadgetEspeciaIcon;

		public UI2DSprite GadgetEspecialShortcutIcon;

		public UILabel GadgetEspecialShortcutLabel;

		public GameObject GadgetEspecial_label_group;

		public UILabel GadgetEspecialName;

		public UILabel GadgetEspecialShortDescription;

		public UILabel GadgetEspecialCooldowDescription;

		public UILabel NitroName;

		public UILabel NitroShortDescription;

		public UILabel NitroCooldowDescription;

		public HMMUI2DDynamicSprite NitroIcon;

		public UI2DSprite NitroShortcutIcon;

		public UILabel NitroShortcutLabel;

		public GameObject Nitro_label_group;

		[Header("[Overlay]")]
		[SerializeField]
		protected UIEventTrigger OverlayEventTrigger;

		private GameState _currentState;

		[Serializable]
		private struct DriverHelperCharInfo
		{
			public Color RoleLabelSupportColor;

			public Color RoleLabelTacklerColor;

			public Color RoleLabelCarrierColor;

			public string RoleSupportDraft;

			public string RoleTacklerDraft;

			public string RoleCarrierDraft;

			public TranslationSheets RoleTranslationSheet;

			public TranslationSheets NamesSheet;

			public TranslationSheets DescriptionsSheet;

			public UILabel PilotNameLabel;

			public UILabel RoleNameLabel;

			public HMMUI2DDynamicSprite PilotIconSprite;
		}
	}
}
