using System;
using System.Collections.Generic;
using Assets.ClientApiObjects;
using Assets.ClientApiObjects.Components;
using HeavyMetalMachines.Characters;
using HeavyMetalMachines.Infra.DependencyInjection.Attributes;
using HeavyMetalMachines.Input;
using HeavyMetalMachines.Input.ControllerInput;
using HeavyMetalMachines.Localization;
using HeavyMetalMachines.Presenting;
using HeavyMetalMachines.Utils;
using HeavyMetalMachines.VFX;
using Hoplon.Input;
using Hoplon.Input.Business;
using Hoplon.ToggleableFeatures;
using Pocketverse;
using UniRx;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	public class PickModeStatusInfo : GameHubBehaviour
	{
		public void Initialize()
		{
			this._isVisible = false;
			this._mainPanel.GetComponent<NGUIPanelAlpha>().Alpha = 0f;
		}

		protected void Start()
		{
			this._inputBindNotifierDisposable = ObservableExtensions.Subscribe<int>(Observable.Do<int>(this._inputBindNotifier.ObserveBind(), delegate(int actionId)
			{
				this.ControlsOnKeyChangedCallback(actionId);
			}));
			this._inputBindResetDefaultNotifierDisposable = ObservableExtensions.Subscribe<Unit>(Observable.Do<Unit>(this._inputBindNotifier.ObserveResetDefault(), delegate(Unit _)
			{
				this.ControlsOnResetAllOrPrimaryCallback();
			}));
			this._inputActiveDeviceChangeNotifierDisposable = ObservableExtensions.Subscribe<InputDevice>(this._inputActiveDeviceChangeNotifier.ObserveActiveDeviceChange(), new Action<InputDevice>(this.OnInputActiveDeviceChange));
		}

		protected void OnDestroy()
		{
			this._currentCharAssetPrefix = string.Empty;
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

		public void Setup(IItemType charItemType)
		{
			CharacterItemTypeComponent component = charItemType.GetComponent<CharacterItemTypeComponent>();
			this._currentCharAssetPrefix = component.AssetPrefix;
			this.StatsInfoGui.NameLabel.text = component.GetCharacterLocalizedName();
			this.StatsInfoGui.NameLabel.TryUpdateText();
			this.SetSkills(component);
			this.SetupRole(component);
			PickModeStats pickModeStats = component.PickModeStats;
			float[] array = new float[]
			{
				pickModeStats.Durability,
				pickModeStats.Repair,
				pickModeStats.Control,
				pickModeStats.Damage,
				pickModeStats.Mobility
			};
			for (int i = 0; i < array.Length; i++)
			{
				this.StatsInfoGui.StatsSliders[i].value = this.GetStatInfoSliderValue(array[i]);
			}
			bool flag = component.Difficulty <= CharacterDifficulty.DifficultyLevel2;
			if (flag)
			{
				this.StatsInfoGui.RecommendationSprite.gameObject.SetActive(true);
				this.StatsInfoGui.RecommendationSprite.UpdateAnchors();
			}
			else
			{
				this.StatsInfoGui.RecommendationSprite.gameObject.SetActive(false);
			}
			bool active = GameHubBehaviour.Hub.Characters.IsCharacterUnderRotationForPlayer(component.CharacterId, GameHubBehaviour.Hub.User.Bag);
			this.StatsInfoGui.RotationGroup.SetActive(active);
		}

		private void ControlsOnResetAllOrPrimaryCallback()
		{
			if (!string.IsNullOrEmpty(this._currentCharAssetPrefix))
			{
				this.SetAllSkillActionDescriptions(this._currentCharAssetPrefix);
			}
		}

		private void ControlsOnKeyChangedCallback(ControllerInputActions controlAction)
		{
			switch (controlAction)
			{
			case 5:
			case 6:
			case 7:
				break;
			default:
				if (controlAction != 15)
				{
					return;
				}
				break;
			}
			if (!string.IsNullOrEmpty(this._currentCharAssetPrefix))
			{
				this.SetSkillActionDescription(this._currentCharAssetPrefix, controlAction);
			}
		}

		private void SetupRole(CharacterItemTypeComponent charComponent)
		{
			Sprite sprite2D;
			string key;
			switch (charComponent.Role)
			{
			case 0:
				sprite2D = this.ClassSupportSprite;
				key = "SUPPORT_TITLE_PICKMODE";
				break;
			case 1:
				sprite2D = this.ClassCarrierSprite;
				key = "TRANSPORTER_TITLE_PICKMODE";
				break;
			case 2:
				sprite2D = this.ClassTacklerSprite;
				key = "INTERCEPTOR_TITLE_PICKMODE";
				break;
			default:
				Debug.Assert(false, string.Format("Pick Mode Status ERROR. Invalid Role for [{0}]: [{1}]", charComponent.AssetPrefix, charComponent.Role), Debug.TargetTeam.All);
				return;
			}
			this.StatsInfoGui.SkinLabel.text = charComponent.GetRoleLocalized();
			this.StatsInfoGui.ClassSprite.sprite2D = sprite2D;
			this.StatsInfoGui.ClassTooltip.TooltipText = Language.Get(key, TranslationContext.PickMode);
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

		private void SetSkills(CharacterItemTypeComponent charComponent)
		{
			this.StatsInfoGui.GadgetPassiveDescription.transform.parent.gameObject.SetActive(charComponent.HasPassive);
			this.StatsInfoGui.GadgetPassiveDescription.text = Language.Get(charComponent.AssetPrefix.ToUpper() + "_GADGET_PASSIVE_SHORT", TranslationContext.CharactersShortInfo);
			this.SetAllSkillActionDescriptions(charComponent.AssetPrefix);
			this.SetSkillsSprites(charComponent.AssetPrefix, charComponent.HasPassive);
		}

		private void SetSkillsSprites(string assetPrefix, bool hasPassive)
		{
			if (hasPassive)
			{
				this.StatsInfoGui.GadgetPassiveSprite.SpriteName = assetPrefix + "_GadgetPassive";
			}
			this.StatsInfoGui.Gadget0Sprite.SpriteName = assetPrefix + "_Gadget01";
			this.StatsInfoGui.Gadget1Sprite.SpriteName = assetPrefix + "_Gadget02";
			this.StatsInfoGui.Gadget2Sprite.SpriteName = assetPrefix + "_Gadget03";
			this.StatsInfoGui.GadgetNitroSprite.SpriteName = assetPrefix + "_GadgetNitro";
		}

		private void SetAllSkillActionDescriptions(string assetPrefix)
		{
			this.SetSkillActionDescription(assetPrefix, 5);
			this.SetSkillActionDescription(assetPrefix, 6);
			this.SetSkillActionDescription(assetPrefix, 7);
			this.SetSkillActionDescription(assetPrefix, 15);
		}

		private void SetSkillActionDescription(string assetPrefix, ControllerInputActions inputAction)
		{
			string str = "_GADGET_00_SHORT";
			UILabel uilabel = this.StatsInfoGui.Gadget0Description;
			if (inputAction != 6)
			{
				if (inputAction != 7)
				{
					if (inputAction == 15)
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
			ISprite sprite;
			string text;
			this._inputTranslation.TryToGetInputActionKeyboardMouseAssetOrFallbackToTranslation(inputAction, ref sprite, ref text);
			uilabel.text = Language.GetFormatted(assetPrefix.ToUpper() + str, TranslationContext.CharactersShortInfo, new object[]
			{
				text
			});
		}

		public float GetTooltipWidth()
		{
			return this._bgSprite.localSize.x;
		}

		public void SetVisibility(bool isVisible)
		{
			if (isVisible && this._inputGetActiveDevicePoller.GetActiveDevice() == 3)
			{
				return;
			}
			if (isVisible == this._isVisible)
			{
				return;
			}
			this._isVisible = isVisible;
			if (isVisible)
			{
				this._animation.Play("TooltipPickmodeInAnimation");
			}
			else
			{
				this._animation.Play("TooltipPickmodeOutAnimation");
			}
		}

		private void OnInputActiveDeviceChange(InputDevice activeDevice)
		{
			if (this._isVisible)
			{
				this.SetVisibility(false);
			}
		}

		[SerializeField]
		private UIPanel _mainPanel;

		[SerializeField]
		private Animation _animation;

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

		private string _currentCharAssetPrefix;

		[InjectOnClient]
		private IInputTranslation _inputTranslation;

		[InjectOnClient]
		private IInputBindNotifier _inputBindNotifier;

		[InjectOnClient]
		private IIsFeatureToggled _isFeatureToggled;

		[InjectOnClient]
		private IInputGetActiveDevicePoller _inputGetActiveDevicePoller;

		[InjectOnClient]
		private IInputActiveDeviceChangeNotifier _inputActiveDeviceChangeNotifier;

		private IDisposable _inputBindNotifierDisposable;

		private IDisposable _inputBindResetDefaultNotifierDisposable;

		private IDisposable _inputActiveDeviceChangeNotifierDisposable;

		private bool _isVisible;

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

			public List<UISlider> StatsSliders;

			[Range(0f, 1f)]
			public List<float> StatsSliderSlots;

			public GameObject RotationGroup;
		}
	}
}
