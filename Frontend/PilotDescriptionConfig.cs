using System;
using System.Collections.Generic;
using HeavyMetalMachines.Character;
using HeavyMetalMachines.Combat.Gadget;
using HeavyMetalMachines.VFX;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	[Serializable]
	public class PilotDescriptionConfig
	{
		public void OnDisable()
		{
			this.PassiveEventTrigger.onHoverOver.Clear();
			this.Gadget0EventTrigger.onHoverOver.Clear();
			this.Gadget1EventTrigger.onHoverOver.Clear();
			this.Gadget2EventTrigger.onHoverOver.Clear();
			this.PassiveEventTrigger.onHoverOut.Clear();
			this.Gadget0EventTrigger.onHoverOut.Clear();
			this.Gadget1EventTrigger.onHoverOut.Clear();
			this.Gadget2EventTrigger.onHoverOut.Clear();
			this._iconSpriteNames = null;
		}

		public void SetNewDescription(CharacterConfig characterConfig, PickModeGUI pickModeGUI, HMMHub hub)
		{
			PilotDescriptionConfig._hub = hub;
			this._pickModeGUI = pickModeGUI;
			this._convertedEnabledColorAlpha = this.EnabledColorAlpha / 255f;
			this._convertedDisabledColorAlpha = this.DisabledColorAlpha / 255f;
			if (characterConfig.CharInfo)
			{
				this.SetupCharacter(characterConfig.CharInfo.LocalizedPickPhrase);
			}
			else
			{
				string localizedDescription = Language.Get("RANDOM_DESCRIPTION_PICKMODE", TranslationSheets.PickMode);
				this.SetupCharacter(localizedDescription);
			}
			this.Setdifficulty(characterConfig.CharInfo);
			this.ConfigRole(characterConfig.CharInfo);
			this.SetSkills(characterConfig.CharInfo);
		}

		private void SetupCharacter(string localizedDescription)
		{
			this.CharacterDescription.text = localizedDescription;
		}

		private void Setdifficulty(HeavyMetalMachines.Character.CharacterInfo charInfo)
		{
			this.ConfigRoleTooltip(this.SupportSprite.transform.parent.gameObject, Language.Get("SUPPORT_ROLE_DESCRIPTION", TranslationSheets.CharactersBaseInfo));
			this.ConfigRoleTooltip(this.CarrierSprite.transform.parent.gameObject, Language.Get("CARRIER_ROLE_DESCRIPTION", TranslationSheets.CharactersBaseInfo));
			this.ConfigRoleTooltip(this.TacklerSprite.transform.parent.gameObject, Language.Get("TACKLER_ROLE_DESCRIPTION", TranslationSheets.CharactersBaseInfo));
			int num = 0;
			Color color = Color.red;
			if (charInfo)
			{
				HeavyMetalMachines.Character.CharacterInfo.Difficulty difficultyKind = charInfo.GetDifficultyKind();
				num = charInfo.Dificult;
				if (PilotDescriptionConfig._hub && PilotDescriptionConfig._hub.GuiScripts)
				{
					switch (difficultyKind)
					{
					case HeavyMetalMachines.Character.CharacterInfo.Difficulty.DifficultyLevel1:
						color = GUIColorsInfo.Instance.DifficultyLevel1;
						break;
					case HeavyMetalMachines.Character.CharacterInfo.Difficulty.DifficultyLevel2:
						color = GUIColorsInfo.Instance.DifficultyLevel2;
						break;
					case HeavyMetalMachines.Character.CharacterInfo.Difficulty.DifficultyLevel3:
						color = GUIColorsInfo.Instance.DifficultyLevel3;
						break;
					case HeavyMetalMachines.Character.CharacterInfo.Difficulty.DifficultyLevel4:
						color = GUIColorsInfo.Instance.DifficultyLevel4;
						break;
					case HeavyMetalMachines.Character.CharacterInfo.Difficulty.DifficultyLevel5:
						color = GUIColorsInfo.Instance.DifficultyLevel5;
						break;
					}
				}
				this.DifficultyLevel.color = color;
				this.DifficultyLevel.text = charInfo.GetDifficultTranslatedText();
			}
			else
			{
				if (PilotDescriptionConfig._hub.GuiScripts)
				{
					color = GUIColorsInfo.Instance.DifficultyLevel0;
				}
				this.DifficultyLevel.color = color;
				this.DifficultyLevel.text = Language.Get("RANDOM_DIFFICULTY", TranslationSheets.CharactersBaseInfo);
			}
			if (!this.ProgressBarParent)
			{
				return;
			}
			UI2DSprite[] componentsInChildren = this.ProgressBarParent.GetComponentsInChildren<UI2DSprite>(true);
			for (int i = 0; i < num; i++)
			{
				UI2DSprite ui2DSprite = componentsInChildren[i];
				ui2DSprite.color = color;
				ui2DSprite.gameObject.SetActive(true);
			}
			color.a = 0.5f;
			for (int j = num; j < componentsInChildren.Length; j++)
			{
				UI2DSprite ui2DSprite2 = componentsInChildren[j];
				ui2DSprite2.gameObject.SetActive(false);
			}
		}

		private void SetSkills(HeavyMetalMachines.Character.CharacterInfo charInfo)
		{
			this._iconSpriteNames = new Dictionary<int, string>(4);
			if (charInfo)
			{
				this.SetupSkillTooltip(this.PassiveIcon, charInfo.Asset + "_GadgetPassive", this.PassiveEventTrigger, charInfo.PassiveGadget);
				this.SetupSkillTooltip(this.Gadget0Icon, charInfo.Asset + "_Gadget01", this.Gadget0EventTrigger, charInfo.CustomGadget0);
				this.SetupSkillTooltip(this.Gadget1Icon, charInfo.Asset + "_Gadget02", this.Gadget1EventTrigger, charInfo.CustomGadget1);
				this.SetupSkillTooltip(this.Gadget2Icon, charInfo.Asset + "_Gadget03", this.Gadget2EventTrigger, charInfo.CustomGadget2);
			}
			else
			{
				this.SetupSkillTooltip(this.PassiveIcon, "skill_random_passive_icon", this.PassiveEventTrigger, null);
				this.SetupSkillTooltip(this.Gadget0Icon, "skill_random_icon", this.Gadget0EventTrigger, null);
				this.SetupSkillTooltip(this.Gadget1Icon, "skill_random_icon", this.Gadget1EventTrigger, null);
				this.SetupSkillTooltip(this.Gadget2Icon, "skill_random_icon", this.Gadget2EventTrigger, null);
			}
		}

		private void SetupSkillTooltip(HMMUI2DDynamicSprite icon, string spritename, UIEventTrigger eventTrigger, GadgetInfo info)
		{
			icon.SpriteName = spritename;
			eventTrigger.onHoverOut.Clear();
			eventTrigger.onHoverOver.Clear();
			if (info == null)
			{
				return;
			}
			eventTrigger.onHoverOut.Add(new EventDelegate(this._pickModeGUI, "HideTooltip"));
			eventTrigger.onHoverOver.Add(new EventDelegate(this._pickModeGUI, "ShowSkillTooltip"));
			eventTrigger.onHoverOver[0].parameters[0].obj = info;
			this._iconSpriteNames[info.GadgetId] = spritename;
		}

		public void HideTooltip()
		{
			if (PilotDescriptionConfig._hub && PilotDescriptionConfig._hub.GuiScripts)
			{
				PilotDescriptionConfig._hub.GuiScripts.TooltipController.HideWindow();
			}
		}

		public void ShowSkillTooltip(GadgetInfo gadget)
		{
			string upgradeDescription = HudGarageShopGadgetObject.GetUpgradeDescription(gadget, gadget.LocalizedDescription, gadget.LocalizedName);
			TooltipInfo tooltipInfo = new TooltipInfo(TooltipInfo.TooltipType.Normal, TooltipInfo.DescriptionSummaryType.None, this.SkillTooltipAnchor, null, this._iconSpriteNames[gadget.GadgetId], gadget.LocalizedName, string.Empty, upgradeDescription, gadget.LocalizedCooldownDescription, string.Empty, string.Empty, this.TooltipSkillGameObject.transform.position, string.Empty);
			if (PilotDescriptionConfig._hub.GuiScripts)
			{
				PilotDescriptionConfig._hub.GuiScripts.TooltipController.TryToOpenWindow(tooltipInfo);
			}
		}

		private void ConfigRole(HeavyMetalMachines.Character.CharacterInfo charInfo)
		{
			this.DisableRole(this.SupportSprite, this.SupportLabel);
			this.DisableRole(this.CarrierSprite, this.CarrierLabel);
			this.DisableRole(this.TacklerSprite, this.TacklerLabel);
			if (!charInfo)
			{
				return;
			}
			switch (charInfo.Role)
			{
			case HeavyMetalMachines.Character.CharacterInfo.DriverRoleKind.Support:
				this.EnableRole(this.SupportSprite, this.SupportLabel);
				break;
			case HeavyMetalMachines.Character.CharacterInfo.DriverRoleKind.Carrier:
				this.EnableRole(this.CarrierSprite, this.CarrierLabel);
				break;
			case HeavyMetalMachines.Character.CharacterInfo.DriverRoleKind.Tackler:
				this.EnableRole(this.TacklerSprite, this.TacklerLabel);
				break;
			case HeavyMetalMachines.Character.CharacterInfo.DriverRoleKind.CarrierSupport:
				this.EnableRole(this.CarrierSprite, this.CarrierLabel);
				this.EnableRole(this.SupportSprite, this.SupportLabel);
				break;
			case HeavyMetalMachines.Character.CharacterInfo.DriverRoleKind.TacklerSupport:
				this.EnableRole(this.TacklerSprite, this.TacklerLabel);
				this.EnableRole(this.SupportSprite, this.SupportLabel);
				break;
			case HeavyMetalMachines.Character.CharacterInfo.DriverRoleKind.CarrierTackler:
				this.EnableRole(this.CarrierSprite, this.CarrierLabel);
				this.EnableRole(this.TacklerSprite, this.TacklerLabel);
				break;
			case HeavyMetalMachines.Character.CharacterInfo.DriverRoleKind.SupportCarrierTackler:
				this.EnableRole(this.SupportSprite, this.SupportLabel);
				this.EnableRole(this.CarrierSprite, this.CarrierLabel);
				this.EnableRole(this.TacklerSprite, this.TacklerLabel);
				break;
			}
		}

		private void EnableRole(UI2DSprite sprite, UILabel label)
		{
			Color color = sprite.color;
			color.a = this._convertedEnabledColorAlpha;
			sprite.color = color;
			Color color2 = label.color;
			color2.a = this._convertedEnabledColorAlpha;
			label.color = color2;
		}

		private void DisableRole(UI2DSprite sprite, UILabel label)
		{
			Color color = sprite.color;
			color.a = this._convertedDisabledColorAlpha;
			sprite.color = color;
			Color color2 = label.color;
			color2.a = 0f;
			label.color = color2;
		}

		private void ConfigRoleTooltip(GameObject target, string translatedText)
		{
			UIEventTrigger component = target.GetComponent<UIEventTrigger>();
			if (!component)
			{
				return;
			}
			component.onHoverOut.Clear();
			component.onHoverOver.Clear();
			component.onHoverOut.Add(new EventDelegate(this._pickModeGUI, "HideTooltip"));
			component.onHoverOver.Add(new EventDelegate(this._pickModeGUI, "ShowRoleTooltip"));
			component.onHoverOver[0].parameters[0].value = translatedText;
		}

		public void ShowRoleTooltip(string translatedText)
		{
			TooltipInfo tooltipInfo = new TooltipInfo(TooltipInfo.TooltipType.SimpleText, TooltipInfo.DescriptionSummaryType.None, this.RoleTooltipAnchor, null, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, this.TooltipRoleGameObject.transform.position, translatedText);
			if (PilotDescriptionConfig._hub.GuiScripts)
			{
				PilotDescriptionConfig._hub.GuiScripts.TooltipController.TryToOpenWindow(tooltipInfo);
			}
		}

		private static HMMHub _hub;

		private PickModeGUI _pickModeGUI;

		public GameObject TargetGameObject;

		public UILabel CharacterDescription;

		[Header("Role")]
		public UI2DSprite SupportSprite;

		public UILabel SupportLabel;

		public UI2DSprite CarrierSprite;

		public UILabel CarrierLabel;

		public UI2DSprite TacklerSprite;

		public UILabel TacklerLabel;

		public float EnabledColorAlpha;

		public float DisabledColorAlpha;

		private float _convertedEnabledColorAlpha;

		private float _convertedDisabledColorAlpha;

		public GameObject TooltipRoleGameObject;

		public PreferredDirection RoleTooltipAnchor = PreferredDirection.Top;

		[Header("Difficulty")]
		public UILabel DifficultyLevel;

		public GameObject ProgressBarParent;

		[Header("Skills")]
		public HMMUI2DDynamicSprite PassiveIcon;

		public UIEventTrigger PassiveEventTrigger;

		public HMMUI2DDynamicSprite Gadget0Icon;

		public UIEventTrigger Gadget0EventTrigger;

		public HMMUI2DDynamicSprite Gadget1Icon;

		public UIEventTrigger Gadget1EventTrigger;

		public HMMUI2DDynamicSprite Gadget2Icon;

		public UIEventTrigger Gadget2EventTrigger;

		public GameObject TooltipSkillGameObject;

		public PreferredDirection SkillTooltipAnchor = PreferredDirection.Bottom;

		private Dictionary<int, string> _iconSpriteNames;
	}
}
