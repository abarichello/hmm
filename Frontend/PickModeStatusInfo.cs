using System;
using System.Collections.Generic;
using HeavyMetalMachines.Character;
using HeavyMetalMachines.Combat.Gadget;
using HeavyMetalMachines.Options;
using HeavyMetalMachines.Utils;
using HeavyMetalMachines.VFX;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	public class PickModeStatusInfo : GameHubBehaviour
	{
		protected void Start()
		{
			GameHubBehaviour.Hub.Options.Controls.OnResetDefaultCallback += this.ControlsOnResetAllOrPrimaryCallback;
			GameHubBehaviour.Hub.Options.Controls.OnResetPrimaryDefaultCallback += this.ControlsOnResetAllOrPrimaryCallback;
			GameHubBehaviour.Hub.Options.Controls.OnKeyChangedCallback += this.ControlsOnKeyChangedCallback;
		}

		protected void OnDestroy()
		{
			this._currentCharInfo = null;
			GameHubBehaviour.Hub.Options.Controls.OnResetDefaultCallback -= this.ControlsOnResetAllOrPrimaryCallback;
			GameHubBehaviour.Hub.Options.Controls.OnResetPrimaryDefaultCallback -= this.ControlsOnResetAllOrPrimaryCallback;
			GameHubBehaviour.Hub.Options.Controls.OnKeyChangedCallback -= this.ControlsOnKeyChangedCallback;
		}

		public void Setup(HeavyMetalMachines.Character.CharacterInfo charInfo)
		{
			this._currentCharInfo = charInfo;
			this.StatsInfoGui.NameLabel.text = charInfo.LocalizedName;
			this.StatsInfoGui.NameLabel.TryUpdateText();
			this.SetSkills(charInfo);
			this.SetupRole(charInfo);
			HeavyMetalMachines.Character.CharacterInfo.PickModeStats pickModeStatsInfo = charInfo.PickModeStatsInfo;
			float[] array = new float[]
			{
				pickModeStatsInfo.Durability,
				pickModeStatsInfo.Repair,
				pickModeStatsInfo.Control,
				pickModeStatsInfo.Damage,
				pickModeStatsInfo.Mobility
			};
			for (int i = 0; i < array.Length; i++)
			{
				this.StatsInfoGui.StatsSliders[i].value = this.GetStatInfoSliderValue(array[i]);
			}
			bool flag = charInfo.Dificult <= 2;
			if (flag)
			{
				this.StatsInfoGui.RecommendationSprite.gameObject.SetActive(true);
				this.StatsInfoGui.RecommendationSprite.UpdateAnchors();
			}
			else
			{
				this.StatsInfoGui.RecommendationSprite.gameObject.SetActive(false);
			}
			bool active = GameHubBehaviour.Hub.Characters.IsCharacterUnderRotationForPlayer(charInfo.CharacterId, GameHubBehaviour.Hub.User.Bag);
			this.StatsInfoGui.RotationGroup.SetActive(active);
		}

		private void ControlsOnResetAllOrPrimaryCallback()
		{
			if (this._currentCharInfo != null)
			{
				this.SetAllSkillActionDescriptions(this._currentCharInfo);
			}
		}

		private void ControlsOnKeyChangedCallback(ControlAction controlAction)
		{
			switch (controlAction)
			{
			case ControlAction.GadgetBasic:
			case ControlAction.Gadget0:
			case ControlAction.Gadget1:
				break;
			default:
				if (controlAction != ControlAction.GadgetBoost)
				{
					return;
				}
				break;
			}
			if (this._currentCharInfo != null)
			{
				this.SetSkillActionDescription(this._currentCharInfo, controlAction);
			}
		}

		private void SetupRole(HeavyMetalMachines.Character.CharacterInfo charInfo)
		{
			Sprite sprite2D;
			string key;
			switch (charInfo.Role)
			{
			case HeavyMetalMachines.Character.CharacterInfo.DriverRoleKind.Support:
				sprite2D = this.ClassSupportSprite;
				key = "SUPPORT_TITLE_PICKMODE";
				break;
			case HeavyMetalMachines.Character.CharacterInfo.DriverRoleKind.Carrier:
				sprite2D = this.ClassCarrierSprite;
				key = "TRANSPORTER_TITLE_PICKMODE";
				break;
			case HeavyMetalMachines.Character.CharacterInfo.DriverRoleKind.Tackler:
				sprite2D = this.ClassTacklerSprite;
				key = "INTERCEPTOR_TITLE_PICKMODE";
				break;
			default:
				HeavyMetalMachines.Utils.Debug.Assert(false, string.Format("Pick Mode Status ERROR. Invalid Role for [{0}]: [{1}]", charInfo.Asset, charInfo.Role), HeavyMetalMachines.Utils.Debug.TargetTeam.All);
				return;
			}
			this.StatsInfoGui.SkinLabel.text = charInfo.GetRoleTranslation();
			this.StatsInfoGui.ClassSprite.sprite2D = sprite2D;
			this.StatsInfoGui.ClassTooltip.TooltipText = Language.Get(key, TranslationSheets.PickMode);
		}

		private float GetStatInfoSliderValue(float value)
		{
			if (Mathf.Approximately(value, 0f))
			{
				return 0f;
			}
			for (int i = 0; i < this.StatsInfoGui.StatsSliderSlots.Count; i++)
			{
				float num = this.StatsInfoGui.StatsSliderSlots[i];
				if (value < num)
				{
					return num;
				}
			}
			return 1f;
		}

		public void OnHelpButtonClick()
		{
			GameHubBehaviour.Hub.GuiScripts.DriverHelper.SetWindowVisibility(true);
		}

		private void SetSkills(HeavyMetalMachines.Character.CharacterInfo charInfo)
		{
			this.StatsInfoGui.GadgetPassiveDescription.transform.parent.gameObject.SetActive(charInfo.HasPassive);
			this.StatsInfoGui.GadgetPassiveDescription.text = Language.Get(charInfo.Asset.ToUpper() + "_GADGET_PASSIVE_SHORT", TranslationSheets.CharactersShortInfo);
			this.SetAllSkillActionDescriptions(charInfo);
			this.SetupSkill(this.StatsInfoGui.GadgetPassiveSprite, charInfo.Asset + "_GadgetPassive", this.StatsInfoGui.GadgetPassiveEventTrigger, charInfo.PassiveGadget, this.StatsInfoGui.GadgetPassiveTooltipPositionGameObject);
			this.SetupSkill(this.StatsInfoGui.Gadget0Sprite, charInfo.Asset + "_Gadget01", this.StatsInfoGui.Gadget0EventTrigger, charInfo.CustomGadget0, this.StatsInfoGui.Gadget0TooltipPositionGameObject);
			this.SetupSkill(this.StatsInfoGui.Gadget1Sprite, charInfo.Asset + "_Gadget02", this.StatsInfoGui.Gadget1EventTrigger, charInfo.CustomGadget1, this.StatsInfoGui.Gadget1TooltipPositionGameObject);
			this.SetupSkill(this.StatsInfoGui.Gadget2Sprite, charInfo.Asset + "_Gadget03", this.StatsInfoGui.Gadget2EventTrigger, charInfo.CustomGadget2, this.StatsInfoGui.Gadget2TooltipPositionGameObject);
			this.SetupSkill(this.StatsInfoGui.GadgetNitroSprite, charInfo.Asset + "_GadgetNitro", this.StatsInfoGui.GadgetNitroEventTrigger, charInfo.BoostGadget, this.StatsInfoGui.GadgetNitroTooltipPositionGameObject);
		}

		private void SetAllSkillActionDescriptions(HeavyMetalMachines.Character.CharacterInfo charInfo)
		{
			this.SetSkillActionDescription(charInfo, ControlAction.GadgetBasic);
			this.SetSkillActionDescription(charInfo, ControlAction.Gadget0);
			this.SetSkillActionDescription(charInfo, ControlAction.Gadget1);
			this.SetSkillActionDescription(charInfo, ControlAction.GadgetBoost);
		}

		private void SetSkillActionDescription(HeavyMetalMachines.Character.CharacterInfo charInfo, ControlAction controlAction)
		{
			string str = "_GADGET_00_SHORT";
			UILabel uilabel = this.StatsInfoGui.Gadget0Description;
			if (controlAction != ControlAction.Gadget0)
			{
				if (controlAction != ControlAction.Gadget1)
				{
					if (controlAction == ControlAction.GadgetBoost)
					{
						uilabel = this.StatsInfoGui.GadgetBoostDescription;
						str = "_GADGET_03_SHORT";
					}
				}
				else
				{
					uilabel = this.StatsInfoGui.Gadget2Description;
					str = "_GADGET_02_SHORT";
				}
			}
			else
			{
				uilabel = this.StatsInfoGui.Gadget1Description;
				str = "_GADGET_01_SHORT";
			}
			string textlocalized = ControlOptions.GetTextlocalized(controlAction, ControlOptions.ControlActionInputType.Primary);
			uilabel.text = string.Format(Language.Get(charInfo.Asset.ToUpper() + str, TranslationSheets.CharactersShortInfo), textlocalized);
		}

		private void SetupSkill(HMMUI2DDynamicSprite icon, string spritename, UIEventTrigger eventTrigger, GadgetInfo info, GameObject tooltipPositionGameObject)
		{
			icon.SpriteName = spritename;
			if (eventTrigger)
			{
				eventTrigger.onHoverOut.Clear();
				eventTrigger.onHoverOver.Clear();
				eventTrigger.onHoverOut.Add(new EventDelegate(this, "HideTooltip"));
				eventTrigger.onHoverOver.Add(new EventDelegate(this, "ShowSkillTooltip"));
				eventTrigger.onHoverOver[0].parameters[0].obj = info;
				eventTrigger.onHoverOver[0].parameters[1].obj = tooltipPositionGameObject;
				eventTrigger.onHoverOver[0].parameters[2].value = spritename;
			}
		}

		public void HideTooltip()
		{
			GameHubBehaviour.Hub.GuiScripts.TooltipController.HideWindow();
		}

		public void ShowSkillTooltip(GadgetInfo gadgetInfo, GameObject tooltipPositionGameObject, string spriteName)
		{
			string upgradeDescription = HudGarageShopGadgetObject.GetUpgradeDescription(gadgetInfo, gadgetInfo.LocalizedDescription, gadgetInfo.LocalizedName);
			TooltipInfo tooltipInfo = new TooltipInfo(TooltipInfo.TooltipType.Normal, TooltipInfo.DescriptionSummaryType.None, PreferredDirection.Left, null, spriteName, gadgetInfo.LocalizedName, string.Empty, upgradeDescription, gadgetInfo.LocalizedCooldownDescription, string.Empty, string.Empty, tooltipPositionGameObject.transform.position, string.Empty);
			if (GameHubBehaviour.Hub.GuiScripts)
			{
				GameHubBehaviour.Hub.GuiScripts.TooltipController.TryToOpenWindow(tooltipInfo);
			}
		}

		public float GetTooltipWidth()
		{
			return this._bgSprite.localSize.x;
		}

		public void SetVisibility(bool isVisible)
		{
			this._mainPanel.alpha = ((!isVisible) ? 0f : 1f);
		}

		[SerializeField]
		private UIPanel _mainPanel;

		[Header("[Gui Components]")]
		[SerializeField]
		private PickModeStatusInfo.StatsGui StatsInfoGui;

		[SerializeField]
		private UI2DSprite _bgSprite;

		[Header("[Class Sprites]")]
		[SerializeField]
		private Sprite ClassCarrierSprite;

		[SerializeField]
		private Sprite ClassSupportSprite;

		[SerializeField]
		private Sprite ClassTacklerSprite;

		private HeavyMetalMachines.Character.CharacterInfo _currentCharInfo;

		[Serializable]
		private struct StatsGui
		{
			public UI2DSprite ClassSprite;

			public HMMTooltipTrigger ClassTooltip;

			public UILabel NameLabel;

			public UILabel SkinLabel;

			public UI2DSprite RecommendationSprite;

			public UILabel GadgetPassiveDescription;

			public UILabel Gadget0Description;

			public UILabel Gadget1Description;

			public UILabel Gadget2Description;

			public UILabel GadgetBoostDescription;

			public HMMUI2DDynamicSprite GadgetPassiveSprite;

			public HMMUI2DDynamicSprite Gadget0Sprite;

			public HMMUI2DDynamicSprite Gadget1Sprite;

			public HMMUI2DDynamicSprite Gadget2Sprite;

			public HMMUI2DDynamicSprite GadgetNitroSprite;

			public UIEventTrigger GadgetPassiveEventTrigger;

			public UIEventTrigger Gadget0EventTrigger;

			public UIEventTrigger Gadget1EventTrigger;

			public UIEventTrigger Gadget2EventTrigger;

			public UIEventTrigger GadgetNitroEventTrigger;

			public GameObject GadgetPassiveTooltipPositionGameObject;

			public GameObject Gadget0TooltipPositionGameObject;

			public GameObject Gadget1TooltipPositionGameObject;

			public GameObject Gadget2TooltipPositionGameObject;

			public GameObject GadgetNitroTooltipPositionGameObject;

			public List<UISlider> StatsSliders;

			[Range(0f, 1f)]
			public List<float> StatsSliderSlots;

			public GameObject RotationGroup;
		}
	}
}
