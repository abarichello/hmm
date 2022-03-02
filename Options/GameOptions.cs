using System;
using System.Diagnostics;
using System.Linq;
using Assets.Standard_Assets.Scripts.HMM.BI;
using Assets.Standard_Assets.Scripts.HMM.PlotKids;
using HeavyMetalMachines.Crossplay;
using HeavyMetalMachines.Infra.ScriptableObjects;
using HeavyMetalMachines.Localization;
using HeavyMetalMachines.Localization.Business;
using Hoplon.Localization.TranslationTable;
using Pocketverse;
using UniRx;
using UnityEngine;
using Zenject;

namespace HeavyMetalMachines.Options
{
	public class GameOptions : GameHubBehaviour
	{
		private void Start()
		{
			this._playerIndicatorLogBi = new PlayerIndicatorLogBI(GameHubBehaviour.Hub, this._optionsScriptableObject);
			this._objectiveIndicatorLogBi = new ObjectiveIndicatorConfigurationsLogBI(GameHubBehaviour.Hub, this._optionsScriptableObject);
		}

		public bool TutorialEnabledHud
		{
			get
			{
				return this._tutorialEnabledHud;
			}
			set
			{
				if (value == this.TutorialEnabledHud && !this.ForceValues)
				{
					return;
				}
				this._tutorialEnabledHud = value;
				this.HasPendingChanges = true;
			}
		}

		public bool TutorialEnabledShop
		{
			get
			{
				return this._tutorialEnabledShop;
			}
			set
			{
				if (value == this.TutorialEnabledShop && !this.ForceValues)
				{
					return;
				}
				this._tutorialEnabledShop = value;
				this.HasPendingChanges = true;
			}
		}

		public bool TutorialEnabledKillCam
		{
			get
			{
				return this._tutorialEnabledKillCam;
			}
			set
			{
				if (value == this.TutorialEnabledKillCam && !this.ForceValues)
				{
					return;
				}
				this._tutorialEnabledKillCam = value;
				this.HasPendingChanges = true;
			}
		}

		public bool TutorialEnabledMainMenu
		{
			get
			{
				return this._tutorialEnabledMainMenu;
			}
			set
			{
				if (value == this.TutorialEnabledMainMenu && !this.ForceValues)
				{
					return;
				}
				this._tutorialEnabledMainMenu = value;
				this.HasPendingChanges = true;
			}
		}

		public bool TutorialEnabled
		{
			get
			{
				return this._tutorialEnabledHud || this._tutorialEnabledShop || this._tutorialEnabledKillCam || this._tutorialEnabledMainMenu;
			}
			set
			{
				if (value == this.TutorialEnabled && !this.ForceValues)
				{
					return;
				}
				this._tutorialEnabledHud = value;
				this._tutorialEnabledShop = value;
				this._tutorialEnabledKillCam = value;
				this._tutorialEnabledMainMenu = value;
				this.HasPendingChanges = true;
			}
		}

		public string[] MovementModeNames
		{
			get
			{
				return new string[]
				{
					Language.Get("GAME_MOVEMENT_SIMULATOR", TranslationContext.Options),
					Language.Get("GAME_MOVEMENT_CONTROLLER", TranslationContext.Options),
					Language.Get("GAME_MOVEMENT_FOLLOWMOUSE", TranslationContext.Options)
				};
			}
		}

		public int MovementModeIndex
		{
			get
			{
				return this._movementModeIndex;
			}
			set
			{
				if (this._movementModeIndex == value && !this.ForceValues)
				{
					return;
				}
				this._movementModeIndex = value;
				this.HasPendingChanges = true;
				if (this.WasUsingControllerJoystick)
				{
					this.Apply();
				}
			}
		}

		public bool InverseReverseControl
		{
			get
			{
				return this._inverseReverseControl;
			}
			set
			{
				if (this._inverseReverseControl == value && !this.ForceValues)
				{
					return;
				}
				this._inverseReverseControl = value;
				this.HasPendingChanges = true;
			}
		}

		public bool TargetHighlight
		{
			get
			{
				return this._targetHighlight;
			}
			set
			{
				if (value == this._targetHighlight && !this.ForceValues)
				{
					return;
				}
				this._targetHighlight = value;
				this.HasPendingChanges = true;
			}
		}

		public bool MemoryWarning
		{
			get
			{
				return this._memoryWarning;
			}
			set
			{
				if (value == this._memoryWarning && !this.ForceValues)
				{
					return;
				}
				this._memoryWarning = value;
				this.HasPendingChanges = true;
			}
		}

		private int LanguageIndexDefault
		{
			get
			{
				return LanguageLocalizationOptions.SystemLanguageToIndex(Application.systemLanguage);
			}
		}

		public string[] LanguageNames
		{
			get
			{
				return (from locale in this._getSupportedLanguages.GetAllLocale()
				select this._getLanguageLocale.GetLocalized(locale)).ToArray<string>();
			}
		}

		public int LanguageIndex
		{
			get
			{
				return this._languageIndex;
			}
			set
			{
				if (this._languageIndex == value && !this.ForceValues)
				{
					return;
				}
				this._languageIndex = value;
				this.HasPendingChanges = true;
			}
		}

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event Action<bool> ListenToShowGadgetsLifebarChanged;

		public bool ShowGadgetsLifebar
		{
			get
			{
				return this._showGadgetsLifebar;
			}
			set
			{
				if (value == this._showGadgetsLifebar && !this.ForceValues)
				{
					return;
				}
				this._showGadgetsLifebar = value;
				this.HasPendingChanges = true;
				if (this.ListenToShowGadgetsLifebarChanged != null)
				{
					this.ListenToShowGadgetsLifebarChanged(this._showGadgetsLifebar);
				}
			}
		}

		public bool CrossplayEnable
		{
			get
			{
				return this._crossplayEnable;
			}
			set
			{
				if (value == this._crossplayEnable && !this.ForceValues)
				{
					return;
				}
				this._crossplayEnable = value;
				this.HasPendingChanges = true;
			}
		}

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event Action<bool> ListenToShowGadgetsCursorChanged;

		public bool ShowGadgetsCursor
		{
			get
			{
				return this._showGadgetsCursor;
			}
			set
			{
				if (value == this._showGadgetsCursor && !this.ForceValues)
				{
					return;
				}
				this._showGadgetsCursor = value;
				this.HasPendingChanges = true;
				if (this.ListenToShowGadgetsCursorChanged != null)
				{
					this.ListenToShowGadgetsCursorChanged(this._showGadgetsCursor);
				}
			}
		}

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event Action<bool> ListenToShowPingChanged;

		public bool ShowPing
		{
			get
			{
				return this._showPing;
			}
			set
			{
				if (value == this._showPing && !this.ForceValues)
				{
					return;
				}
				this._showPing = value;
				this.HasPendingChanges = true;
				if (this.ListenToShowPingChanged != null)
				{
					this.ListenToShowPingChanged(this._showPing);
				}
			}
		}

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event Action OnCounselorActiveChanged;

		public bool CounselorActive
		{
			get
			{
				return this._counselorActive && !SpectatorController.IsSpectating;
			}
			set
			{
				if (value == this._counselorActive && !this.ForceValues)
				{
					return;
				}
				this._counselorActive = value;
				if (this.OnCounselorActiveChanged != null)
				{
					this.OnCounselorActiveChanged();
				}
				this.HasPendingChanges = true;
			}
		}

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event Action OnCounselorHudHintChanged;

		public bool CounselorHudHint
		{
			get
			{
				return this._counselorHudHint && !SpectatorController.IsSpectating;
			}
			set
			{
				if (value == this._counselorHudHint && !this.ForceValues)
				{
					return;
				}
				this._counselorHudHint = value;
				if (this.OnCounselorHudHintChanged != null)
				{
					this.OnCounselorHudHintChanged();
				}
				this.HasPendingChanges = true;
			}
		}

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event Action ShowLifebarTextChanged;

		public bool ShowLifebarText
		{
			get
			{
				return this._showLifebarText;
			}
			set
			{
				if (value == this._showLifebarText && !this.ForceValues)
				{
					return;
				}
				this._showLifebarText = value;
				if (this.ShowLifebarTextChanged != null)
				{
					this.ShowLifebarTextChanged();
				}
				this.HasPendingChanges = true;
			}
		}

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event Action<bool> ShowPlayerIndicatorChanged;

		public bool ShowPlayerIndicator
		{
			get
			{
				return this._showPlayerIndicator;
			}
			set
			{
				if (value == this._showPlayerIndicator && !this.ForceValues)
				{
					return;
				}
				this._showPlayerIndicator = value;
				if (this.ShowPlayerIndicatorChanged != null)
				{
					this.ShowPlayerIndicatorChanged(this._showPlayerIndicator);
				}
				this.HasPendingChanges = true;
			}
		}

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event Action<float> PlayerIndicatorAlphaChanged;

		public float PlayerIndicatorAlpha
		{
			get
			{
				return this._playerIndicatorAlpha;
			}
			set
			{
				if (Math.Abs(value - this._playerIndicatorAlpha) < 0.001f && !this.ForceValues)
				{
					return;
				}
				this._playerIndicatorAlpha = value;
				this.NotifyPlayerIndicatorAlphaChange();
				this.HasPendingChanges = true;
			}
		}

		private void NotifyPlayerIndicatorAlphaChange()
		{
			float converterPlayerIndicatorAlpha = this.GetConverterPlayerIndicatorAlpha();
			if (this.PlayerIndicatorAlphaChanged != null)
			{
				this.PlayerIndicatorAlphaChanged(converterPlayerIndicatorAlpha);
			}
		}

		public float GetConverterPlayerIndicatorAlpha()
		{
			float num = this._optionsScriptableObject.PlayerIndicatorMaxAlpha - this._optionsScriptableObject.PlayerIndicatorMinAlpha;
			return this._playerIndicatorAlpha * num + this._optionsScriptableObject.PlayerIndicatorMinAlpha;
		}

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event Action<bool> ShowObjectiveIndicatorChanged;

		public bool ShowObjectiveIndicator
		{
			get
			{
				return this._showObjectiveIndicator;
			}
			set
			{
				if (value == this._showObjectiveIndicator && !this.ForceValues)
				{
					return;
				}
				this._showObjectiveIndicator = value;
				if (this.ShowObjectiveIndicatorChanged != null)
				{
					this.ShowObjectiveIndicatorChanged(this._showObjectiveIndicator);
				}
				this.HasPendingChanges = true;
			}
		}

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event Action<float> ObjectiveIndicatorAlphaChanged;

		public float ObjectiveIndicatorAlpha
		{
			get
			{
				return this._objectiveIndicatorAlpha;
			}
			set
			{
				if (Math.Abs(value - this._objectiveIndicatorAlpha) < 0.001f && !this.ForceValues)
				{
					return;
				}
				this._objectiveIndicatorAlpha = value;
				if (this.ObjectiveIndicatorAlphaChanged != null)
				{
					this.ObjectiveIndicatorAlphaChanged(this.GetConverterObjectiveIndicatorAlpha());
				}
				this.HasPendingChanges = true;
			}
		}

		public float GetConverterObjectiveIndicatorAlpha()
		{
			float num = this._optionsScriptableObject.ObjectiveIndicatorMaxAlpha - this._optionsScriptableObject.ObjectiveIndicatorMinAlpha;
			return this._objectiveIndicatorAlpha * num + this._optionsScriptableObject.ObjectiveIndicatorMinAlpha;
		}

		public string[] ObjectiveIndicatorTypeNames
		{
			get
			{
				return new string[]
				{
					Language.Get("GAME_OBJECTIVE_INDICATOR_TYPE_GPS", TranslationContext.Options),
					Language.Get("GAME_OBJECTIVE_INDICATOR_TYPE_GPS_RADAR", TranslationContext.Options),
					Language.Get("GAME_OBJECTIVE_INDICATOR_TYPE_NONE", TranslationContext.Options)
				};
			}
		}

		public int ObjectiveIndicatorTypeIndex
		{
			get
			{
				return this._objectiveIndicatorTypeIndex;
			}
			set
			{
				if (this._objectiveIndicatorTypeIndex == value && !this.ForceValues)
				{
					return;
				}
				this._objectiveIndicatorTypeIndex = value;
				this.HasPendingChanges = true;
			}
		}

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event Action<float> ObjectiveIndicatorSizeChanged;

		public float ObjectiveIndicatorSize
		{
			get
			{
				return this._objectiveIndicatorSize;
			}
			set
			{
				if (Math.Abs(value - this._objectiveIndicatorSize) < 0.001f && !this.ForceValues)
				{
					return;
				}
				this._objectiveIndicatorSize = value;
				if (this.ObjectiveIndicatorSizeChanged != null)
				{
					this.ObjectiveIndicatorSizeChanged(this.GetConverterObjectiveIndicatorSize());
				}
				this.HasPendingChanges = true;
			}
		}

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event Action<float> ObjectiveIndicatorQuantityChanged;

		public float ObjectiveIndicatorQuantity
		{
			get
			{
				return this._objectiveIndicatorQuantity;
			}
			set
			{
				if (Math.Abs(value - this._objectiveIndicatorQuantity) < 0.001f && !this.ForceValues)
				{
					return;
				}
				this._objectiveIndicatorQuantity = value;
				if (this.ObjectiveIndicatorQuantityChanged != null)
				{
					this.ObjectiveIndicatorQuantityChanged(this.GetConverterObjectiveIndicatorQuantity());
				}
				this.HasPendingChanges = true;
			}
		}

		private float ConvertSliderValueToRealValue(float min, float max, float current)
		{
			float num = max - min;
			return current * num + min;
		}

		public float GetConverterObjectiveIndicatorSize()
		{
			return this.ConvertSliderValueToRealValue(this._optionsScriptableObject.ObjectiveIndicatorMinSize, this._optionsScriptableObject.ObjectiveIndicatorMaxSize, this._objectiveIndicatorSize);
		}

		public float GetConverterObjectiveIndicatorQuantity()
		{
			return this.ConvertSliderValueToRealValue((float)this._optionsScriptableObject.ObjectiveIndicatorMinArrowQuantity, (float)this._optionsScriptableObject.ObjectiveIndicatorMaxArrowQuantity, this._objectiveIndicatorQuantity);
		}

		public LanguageCode CounselorLanguage
		{
			get
			{
				LanguageCode currentLanguage = Language.CurrentLanguage;
				if (currentLanguage != LanguageCode.DE && currentLanguage != LanguageCode.ES && currentLanguage != LanguageCode.FR && currentLanguage != LanguageCode.PT && currentLanguage != LanguageCode.RU)
				{
					return LanguageCode.EN;
				}
				return currentLanguage;
			}
		}

		private LanguageCode GetDefaultCounselorLanguage()
		{
			LanguageCode currentLanguage = Language.CurrentLanguage;
			if (currentLanguage != LanguageCode.PT && currentLanguage != LanguageCode.PT_BR)
			{
				return LanguageCode.EN;
			}
			return LanguageCode.PT;
		}

		private int GetDefaultCounselorLanguageIndex()
		{
			return LanguageLocalizationOptions.LanguageCodeToIndex(this.GetDefaultCounselorLanguage());
		}

		private int GetDefaultCounselorActive()
		{
			if (GameHubBehaviour.Hub.Net.IsTest())
			{
				return 0;
			}
			return (GameHubBehaviour.Hub.User.GetTotalPlayerLevel() >= 5) ? 0 : 1;
		}

		private int GetDefaultPlayerIndicatorActive()
		{
			if (GameHubBehaviour.Hub.Net.IsTest())
			{
				return 0;
			}
			return (GameHubBehaviour.Hub.User.GetTotalPlayerLevel() >= 1) ? 0 : 1;
		}

		private void LoadPrefs()
		{
			this.ForceValues = true;
			this.TutorialEnabledHud = (GameHubBehaviour.Hub.PlayerPrefs.GetInt(GameOptions.GameOptionPrefs.OPTIONS_GAME_TUTORIALENABLEDHUD.ToString(), (!this.TutorialEnabledDefault) ? 0 : 1) == 1);
			this.TutorialEnabledShop = (GameHubBehaviour.Hub.PlayerPrefs.GetInt(GameOptions.GameOptionPrefs.OPTIONS_GAME_TUTORIALENABLEDSHOP.ToString(), (!this.TutorialEnabledDefault) ? 0 : 1) == 1);
			this.TutorialEnabledKillCam = (GameHubBehaviour.Hub.PlayerPrefs.GetInt(GameOptions.GameOptionPrefs.OPTIONS_GAME_TUTORIALENABLEDKILLCAM.ToString(), (!this.TutorialEnabledDefault) ? 0 : 1) == 1);
			this.TutorialEnabledMainMenu = (GameHubBehaviour.Hub.PlayerPrefs.GetInt(GameOptions.GameOptionPrefs.OPTIONS_GAME_TUTORIALENABLEDMAINMENU.ToString(), (!this.TutorialEnabledDefault) ? 0 : 1) == 1);
			this.InverseReverseControl = (GameHubBehaviour.Hub.PlayerPrefs.GetInt(GameOptions.GameOptionPrefs.OPTIONS_GAME_INVERSEREVERSECONTROL.ToString(), (!ControlOptions.DefaultInvertReverseControl) ? 0 : 1) == 1);
			this.TargetHighlight = (GameHubBehaviour.Hub.PlayerPrefs.GetInt(GameOptions.GameOptionPrefs.OPTIONS_GAME_TARGETHIGHLIGHT.ToString(), (!this.TargetHighlightDefault) ? 0 : 1) == 1);
			this.MemoryWarning = (GameHubBehaviour.Hub.PlayerPrefs.GetInt(GameOptions.GameOptionPrefs.OPTIONS_GAME_MEMORYWARNING.ToString(), (!this.MemoryWarningDefault) ? 0 : 1) == 1);
			this.ShowGadgetsLifebar = (GameHubBehaviour.Hub.PlayerPrefs.GetInt(GameOptions.GameOptionPrefs.OPTIONS_GAME_SHOWGADGETSLIFEBAR.ToString(), (!this.ShowGadgetsLifebarDefault) ? 0 : 1) == 1);
			this.ShowGadgetsCursor = (GameHubBehaviour.Hub.PlayerPrefs.GetInt(GameOptions.GameOptionPrefs.OPTIONS_GAME_SHOWGADGETSCURSOR.ToString(), (!this.ShowGadgetsCursorDefault) ? 0 : 1) == 1);
			this.ShowPing = (GameHubBehaviour.Hub.PlayerPrefs.GetInt(GameOptions.GameOptionPrefs.OPTIONS_GAME_SHOWPING.ToString(), (!this.ShowPingDefault) ? 0 : 1) == 1);
			this.LanguageIndex = this._config.GetIntSetting(GameOptions.GameOptionPrefs.OPTIONS_GAME_LANGUAGE.ToString(), this.LanguageIndexDefault);
			this.CounselorActive = (GameHubBehaviour.Hub.PlayerPrefs.GetInt(GameOptions.GameOptionPrefs.OPTIONS_GAME_COUNSELORACTIVE.ToString(), this.GetDefaultCounselorActive()) == 1);
			this.CounselorHudHint = (GameHubBehaviour.Hub.PlayerPrefs.GetInt(GameOptions.GameOptionPrefs.OPTIONS_GAME_COUNSELORHUDHINT.ToString(), (!this.CounselorHudHintDefault) ? 0 : 1) == 1);
			this.MovementModeIndex = GameHubBehaviour.Hub.PlayerPrefs.GetInt(GameOptions.GameOptionPrefs.OPTIONS_GAME_MOVEMENTMODE.ToString(), (int)ControlOptions.DefaultMovementMode);
			this.ShowLifebarText = (GameHubBehaviour.Hub.PlayerPrefs.GetInt(GameOptions.GameOptionPrefs.OPTIONS_GAME_SHOWLIFEBARTEXT.ToString(), (!this.ShowLifebarTextDefault) ? 0 : 1) == 1);
			this.ShowPlayerIndicator = (GameHubBehaviour.Hub.PlayerPrefs.GetInt(GameOptions.GameOptionPrefs.OPTIONS_GAME_SHOWPLAYERINDICATOR.ToString(), this.GetDefaultPlayerIndicatorActive()) == 1);
			this.PlayerIndicatorAlpha = GameHubBehaviour.Hub.PlayerPrefs.GetFloat(GameOptions.GameOptionPrefs.OPTIONS_GAME_PLAYERINDICATORALPHA.ToString(), this._optionsScriptableObject.PlayerIndicatorDefaultAlpha);
			this.ShowObjectiveIndicator = this.GetInitialDefaultObjectiveIndicator();
			this.ObjectiveIndicatorAlpha = GameHubBehaviour.Hub.PlayerPrefs.GetFloat(GameOptions.GameOptionPrefs.OPTIONS_GAME_OBJECTIVEINDICATORALPHA.ToString(), this._optionsScriptableObject.ObjectiveIndicatorDefaultAlpha);
			this.ObjectiveIndicatorTypeIndex = GameHubBehaviour.Hub.PlayerPrefs.GetInt(GameOptions.GameOptionPrefs.OPTIONS_GAME_OBJECTIVEINDICATORTYPEINDEX.ToString(), this.ObjectiveIndicatorTypeIndexDefault);
			this.ObjectiveIndicatorSize = GameHubBehaviour.Hub.PlayerPrefs.GetFloat(GameOptions.GameOptionPrefs.OPTIONS_GAME_OBJECTIVEINDICATORSIZE.ToString(), this._optionsScriptableObject.ObjectiveIndicatorDefaultSize);
			this.ObjectiveIndicatorQuantity = GameHubBehaviour.Hub.PlayerPrefs.GetFloat(GameOptions.GameOptionPrefs.OPTIONS_GAME_OBJECTIVEINDICATORQUANTITY.ToString(), this._optionsScriptableObject.ObjectiveIndicatorDefaultArrowQuantity);
			this.UseTeamColor = true;
			this.ForceValues = false;
		}

		private void Refresh()
		{
			this.TutorialEnabledHud = (GameHubBehaviour.Hub.PlayerPrefs.GetInt(GameOptions.GameOptionPrefs.OPTIONS_GAME_TUTORIALENABLEDHUD.ToString(), (!this.TutorialEnabledDefault) ? 0 : 1) == 1);
			this.TutorialEnabledShop = (GameHubBehaviour.Hub.PlayerPrefs.GetInt(GameOptions.GameOptionPrefs.OPTIONS_GAME_TUTORIALENABLEDSHOP.ToString(), (!this.TutorialEnabledDefault) ? 0 : 1) == 1);
			this.TutorialEnabledKillCam = (GameHubBehaviour.Hub.PlayerPrefs.GetInt(GameOptions.GameOptionPrefs.OPTIONS_GAME_TUTORIALENABLEDKILLCAM.ToString(), (!this.TutorialEnabledDefault) ? 0 : 1) == 1);
			this.TutorialEnabledMainMenu = (GameHubBehaviour.Hub.PlayerPrefs.GetInt(GameOptions.GameOptionPrefs.OPTIONS_GAME_TUTORIALENABLEDMAINMENU.ToString(), (!this.TutorialEnabledDefault) ? 0 : 1) == 1);
			this.InverseReverseControl = (GameHubBehaviour.Hub.PlayerPrefs.GetInt(GameOptions.GameOptionPrefs.OPTIONS_GAME_INVERSEREVERSECONTROL.ToString(), (!ControlOptions.DefaultInvertReverseControl) ? 0 : 1) == 1);
			this.TargetHighlight = (GameHubBehaviour.Hub.PlayerPrefs.GetInt(GameOptions.GameOptionPrefs.OPTIONS_GAME_TARGETHIGHLIGHT.ToString(), (!this.TargetHighlightDefault) ? 0 : 1) == 1);
			this.MemoryWarning = (GameHubBehaviour.Hub.PlayerPrefs.GetInt(GameOptions.GameOptionPrefs.OPTIONS_GAME_MEMORYWARNING.ToString(), (!this.MemoryWarningDefault) ? 0 : 1) == 1);
			this.ShowGadgetsLifebar = (GameHubBehaviour.Hub.PlayerPrefs.GetInt(GameOptions.GameOptionPrefs.OPTIONS_GAME_SHOWGADGETSLIFEBAR.ToString(), (!this.ShowGadgetsLifebarDefault) ? 0 : 1) == 1);
			this.ShowGadgetsCursor = (GameHubBehaviour.Hub.PlayerPrefs.GetInt(GameOptions.GameOptionPrefs.OPTIONS_GAME_SHOWGADGETSCURSOR.ToString(), (!this.ShowGadgetsCursorDefault) ? 0 : 1) == 1);
			this.ShowPing = (GameHubBehaviour.Hub.PlayerPrefs.GetInt(GameOptions.GameOptionPrefs.OPTIONS_GAME_SHOWPING.ToString(), (!this.ShowPingDefault) ? 0 : 1) == 1);
			this.LanguageIndex = this._config.GetIntSetting(GameOptions.GameOptionPrefs.OPTIONS_GAME_LANGUAGE.ToString(), this.LanguageIndexDefault);
			this.CounselorActive = (GameHubBehaviour.Hub.PlayerPrefs.GetInt(GameOptions.GameOptionPrefs.OPTIONS_GAME_COUNSELORACTIVE.ToString(), this.GetDefaultCounselorActive()) == 1);
			this.CounselorHudHint = (GameHubBehaviour.Hub.PlayerPrefs.GetInt(GameOptions.GameOptionPrefs.OPTIONS_GAME_COUNSELORHUDHINT.ToString(), (!this.CounselorHudHintDefault) ? 0 : 1) == 1);
			this.MovementModeIndex = GameHubBehaviour.Hub.PlayerPrefs.GetInt(GameOptions.GameOptionPrefs.OPTIONS_GAME_MOVEMENTMODE.ToString(), (int)ControlOptions.DefaultMovementMode);
			this.ShowLifebarText = (GameHubBehaviour.Hub.PlayerPrefs.GetInt(GameOptions.GameOptionPrefs.OPTIONS_GAME_SHOWLIFEBARTEXT.ToString(), (!this.ShowLifebarTextDefault) ? 0 : 1) == 1);
			this.ShowPlayerIndicator = (GameHubBehaviour.Hub.PlayerPrefs.GetInt(GameOptions.GameOptionPrefs.OPTIONS_GAME_SHOWPLAYERINDICATOR.ToString(), this.GetDefaultPlayerIndicatorActive()) == 1);
			this.PlayerIndicatorAlpha = GameHubBehaviour.Hub.PlayerPrefs.GetFloat(GameOptions.GameOptionPrefs.OPTIONS_GAME_PLAYERINDICATORALPHA.ToString(), this._optionsScriptableObject.PlayerIndicatorDefaultAlpha);
			this.ShowObjectiveIndicator = this.GetInitialDefaultObjectiveIndicator();
			this.ObjectiveIndicatorAlpha = GameHubBehaviour.Hub.PlayerPrefs.GetFloat(GameOptions.GameOptionPrefs.OPTIONS_GAME_OBJECTIVEINDICATORALPHA.ToString(), this._optionsScriptableObject.ObjectiveIndicatorDefaultAlpha);
			this.ObjectiveIndicatorTypeIndex = GameHubBehaviour.Hub.PlayerPrefs.GetInt(GameOptions.GameOptionPrefs.OPTIONS_GAME_OBJECTIVEINDICATORTYPEINDEX.ToString(), this.ObjectiveIndicatorTypeIndexDefault);
			this.ObjectiveIndicatorSize = GameHubBehaviour.Hub.PlayerPrefs.GetFloat(GameOptions.GameOptionPrefs.OPTIONS_GAME_OBJECTIVEINDICATORSIZE.ToString(), this._optionsScriptableObject.ObjectiveIndicatorDefaultSize);
			this.ObjectiveIndicatorQuantity = GameHubBehaviour.Hub.PlayerPrefs.GetFloat(GameOptions.GameOptionPrefs.OPTIONS_GAME_OBJECTIVEINDICATORQUANTITY.ToString(), this._optionsScriptableObject.ObjectiveIndicatorDefaultArrowQuantity);
			this.HasPendingChanges = false;
		}

		public void Apply()
		{
			if (!this.HasPendingChanges)
			{
				return;
			}
			GameHubBehaviour.Hub.PlayerPrefs.SetInt(GameOptions.GameOptionPrefs.OPTIONS_GAME_TUTORIALENABLEDHUD.ToString(), (!this.TutorialEnabledHud) ? 0 : 1);
			GameHubBehaviour.Hub.PlayerPrefs.SetInt(GameOptions.GameOptionPrefs.OPTIONS_GAME_TUTORIALENABLEDSHOP.ToString(), (!this.TutorialEnabledShop) ? 0 : 1);
			GameHubBehaviour.Hub.PlayerPrefs.SetInt(GameOptions.GameOptionPrefs.OPTIONS_GAME_TUTORIALENABLEDKILLCAM.ToString(), (!this.TutorialEnabledKillCam) ? 0 : 1);
			GameHubBehaviour.Hub.PlayerPrefs.SetInt(GameOptions.GameOptionPrefs.OPTIONS_GAME_TUTORIALENABLEDMAINMENU.ToString(), (!this.TutorialEnabledMainMenu) ? 0 : 1);
			GameHubBehaviour.Hub.PlayerPrefs.SetInt(GameOptions.GameOptionPrefs.OPTIONS_GAME_MOVEMENTMODE.ToString(), this.MovementModeIndex);
			GameHubBehaviour.Hub.PlayerPrefs.SetInt(GameOptions.GameOptionPrefs.OPTIONS_GAME_INVERSEREVERSECONTROL.ToString(), (!ControlOptions.DefaultInvertReverseControl) ? 0 : 1);
			GameHubBehaviour.Hub.PlayerPrefs.SetInt(GameOptions.GameOptionPrefs.OPTIONS_GAME_TARGETHIGHLIGHT.ToString(), (!this.TargetHighlight) ? 0 : 1);
			GameHubBehaviour.Hub.PlayerPrefs.SetInt(GameOptions.GameOptionPrefs.OPTIONS_GAME_MEMORYWARNING.ToString(), (!this.MemoryWarning) ? 0 : 1);
			GameHubBehaviour.Hub.PlayerPrefs.SetInt(GameOptions.GameOptionPrefs.OPTIONS_GAME_SHOWGADGETSLIFEBAR.ToString(), (!this.ShowGadgetsLifebar) ? 0 : 1);
			GameHubBehaviour.Hub.PlayerPrefs.SetInt(GameOptions.GameOptionPrefs.OPTIONS_GAME_SHOWGADGETSCURSOR.ToString(), (!this.ShowGadgetsCursor) ? 0 : 1);
			GameHubBehaviour.Hub.PlayerPrefs.SetInt(GameOptions.GameOptionPrefs.OPTIONS_GAME_SHOWPING.ToString(), (!this.ShowPing) ? 0 : 1);
			this._config.SetSetting(GameOptions.GameOptionPrefs.OPTIONS_GAME_LANGUAGE.ToString(), this.LanguageIndex.ToString());
			GameHubBehaviour.Hub.PlayerPrefs.SetInt(GameOptions.GameOptionPrefs.OPTIONS_GAME_COUNSELORACTIVE.ToString(), (!this.CounselorActive) ? 0 : 1);
			GameHubBehaviour.Hub.PlayerPrefs.SetInt(GameOptions.GameOptionPrefs.OPTIONS_GAME_COUNSELORHUDHINT.ToString(), (!this.CounselorHudHint) ? 0 : 1);
			GameHubBehaviour.Hub.PlayerPrefs.SetInt(GameOptions.GameOptionPrefs.OPTIONS_GAME_SHOWLIFEBARTEXT.ToString(), (!this.ShowLifebarText) ? 0 : 1);
			GameHubBehaviour.Hub.PlayerPrefs.SetInt(GameOptions.GameOptionPrefs.OPTIONS_GAME_SHOWPLAYERINDICATOR.ToString(), (!this.ShowPlayerIndicator) ? 0 : 1);
			GameHubBehaviour.Hub.PlayerPrefs.SetFloat(GameOptions.GameOptionPrefs.OPTIONS_GAME_PLAYERINDICATORALPHA.ToString(), this.PlayerIndicatorAlpha);
			GameHubBehaviour.Hub.PlayerPrefs.SetInt(GameOptions.GameOptionPrefs.OPTIONS_GAME_SHOWOBJECTIVEINDICATOR.ToString(), (!this.ShowObjectiveIndicator) ? 0 : 1);
			GameHubBehaviour.Hub.PlayerPrefs.SetFloat(GameOptions.GameOptionPrefs.OPTIONS_GAME_OBJECTIVEINDICATORALPHA.ToString(), this.ObjectiveIndicatorAlpha);
			GameHubBehaviour.Hub.PlayerPrefs.SetInt(GameOptions.GameOptionPrefs.OPTIONS_GAME_OBJECTIVEINDICATORTYPEINDEX.ToString(), this.ObjectiveIndicatorTypeIndex);
			GameHubBehaviour.Hub.PlayerPrefs.SetFloat(GameOptions.GameOptionPrefs.OPTIONS_GAME_OBJECTIVEINDICATORSIZE.ToString(), this.ObjectiveIndicatorSize);
			GameHubBehaviour.Hub.PlayerPrefs.SetFloat(GameOptions.GameOptionPrefs.OPTIONS_GAME_OBJECTIVEINDICATORQUANTITY.ToString(), this.ObjectiveIndicatorQuantity);
			this.HasPendingChanges = false;
			GameHubBehaviour.Hub.PlayerPrefs.SaveNow();
			this._config.SaveSettings();
		}

		public void CheckInitialConfig()
		{
			this.CheckInitialConfigForPlayerIndicator();
			this.CheckInitialConfigForObjectiveIndicator();
		}

		private void CheckInitialConfigForPlayerIndicator()
		{
			if (!GameHubBehaviour.Hub.PlayerPrefs.HasKey(GameOptions.GameOptionPrefs.OPTIONS_GAME_SHOWPLAYERINDICATOR.ToString()))
			{
				this._playerIndicatorLogBi.LogBIForInitialPlayer();
				GameHubBehaviour.Hub.PlayerPrefs.SetInt(GameOptions.GameOptionPrefs.OPTIONS_GAME_SHOWPLAYERINDICATOR.ToString(), (!this.ShowPlayerIndicator) ? 0 : 1);
				GameHubBehaviour.Hub.PlayerPrefs.SetFloat(GameOptions.GameOptionPrefs.OPTIONS_GAME_PLAYERINDICATORALPHA.ToString(), this.PlayerIndicatorAlpha);
				GameHubBehaviour.Hub.PlayerPrefs.Save();
			}
		}

		private void CheckInitialConfigForObjectiveIndicator()
		{
			if (!GameHubBehaviour.Hub.PlayerPrefs.HasKey(GameOptions.GameOptionPrefs.OPTIONS_GAME_SHOWOBJECTIVEINDICATOR.ToString()))
			{
				this.ShowObjectiveIndicator = this.GetInitialDefaultObjectiveIndicator();
				this._objectiveIndicatorLogBi.LogBIForInitialPlayer();
				GameHubBehaviour.Hub.PlayerPrefs.SetInt(GameOptions.GameOptionPrefs.OPTIONS_GAME_SHOWOBJECTIVEINDICATOR.ToString(), (!this.ShowObjectiveIndicator) ? 0 : 1);
				GameHubBehaviour.Hub.PlayerPrefs.SetFloat(GameOptions.GameOptionPrefs.OPTIONS_GAME_OBJECTIVEINDICATORALPHA.ToString(), this.ObjectiveIndicatorAlpha);
				GameHubBehaviour.Hub.PlayerPrefs.SetFloat(GameOptions.GameOptionPrefs.OPTIONS_GAME_OBJECTIVEINDICATORSIZE.ToString(), this.ObjectiveIndicatorSize);
				GameHubBehaviour.Hub.PlayerPrefs.SetFloat(GameOptions.GameOptionPrefs.OPTIONS_GAME_OBJECTIVEINDICATORQUANTITY.ToString(), this.ObjectiveIndicatorQuantity);
				GameHubBehaviour.Hub.PlayerPrefs.Save();
			}
		}

		private bool GetInitialDefaultObjectiveIndicator()
		{
			if (!GameHubBehaviour.Hub.PlayerPrefs.HasKey(GameOptions.GameOptionPrefs.OPTIONS_GAME_SHOWOBJECTIVEINDICATOR.ToString()))
			{
				int totalPlayerLevel = GameHubBehaviour.Hub.User.GetTotalPlayerLevel();
				return totalPlayerLevel < 5;
			}
			return GameHubBehaviour.Hub.PlayerPrefs.GetInt(GameOptions.GameOptionPrefs.OPTIONS_GAME_SHOWOBJECTIVEINDICATOR.ToString(), (!this.ShowObjectiveIndicatorDefault) ? 0 : 1) == 1;
		}

		public void GetterInfoForBILog()
		{
			this._playerIndicatorLogBi.SaveInitialConfig();
			this._objectiveIndicatorLogBi.SaveInitialConfig();
		}

		public void WriteBILogs()
		{
			this._playerIndicatorLogBi.LogBIPlayerIndicator();
			this._objectiveIndicatorLogBi.LogBIPlayerIndicator();
		}

		public void ApplyOnlyOnStart()
		{
			this.LanguageIndex = this._config.GetIntSetting(GameOptions.GameOptionPrefs.OPTIONS_GAME_LANGUAGE.ToString(), this.LanguageIndexDefault);
			if (LanguageLocalizationOptions.IsLanguageLocalizationIndexValid(this.LanguageIndex))
			{
				return;
			}
			GameOptions.Log.ErrorFormat("Invalid LanguageIndex: {0}", new object[]
			{
				this.LanguageIndex
			});
			this.LanguageIndex = this.LanguageIndexDefault;
			this.Apply();
		}

		public void ResetDefault()
		{
			this.TutorialEnabled = this.TutorialEnabledDefault;
			this.TargetHighlight = this.TargetHighlightDefault;
			this.MemoryWarning = this.MemoryWarningDefault;
			this.ShowGadgetsLifebar = this.ShowGadgetsLifebarDefault;
			this.ShowGadgetsCursor = this.ShowGadgetsCursorDefault;
			this.ShowPing = this.ShowPingDefault;
			this.LanguageIndex = this.LanguageIndexDefault;
			this.CounselorActive = (this.GetDefaultCounselorActive() == 1);
			this.CounselorHudHint = this.CounselorHudHintDefault;
			this.ShowLifebarText = this.ShowLifebarTextDefault;
			this.ShowPlayerIndicator = (this.GetDefaultPlayerIndicatorActive() == 1);
			this.PlayerIndicatorAlpha = this._optionsScriptableObject.PlayerIndicatorDefaultAlpha;
			this.ShowObjectiveIndicator = this.ShowObjectiveIndicatorDefault;
			this.ObjectiveIndicatorAlpha = this._optionsScriptableObject.ObjectiveIndicatorDefaultAlpha;
			this.ObjectiveIndicatorTypeIndex = this.ObjectiveIndicatorTypeIndexDefault;
			this.ObjectiveIndicatorSize = this._optionsScriptableObject.ObjectiveIndicatorDefaultSize;
			this.ObjectiveIndicatorQuantity = this._optionsScriptableObject.ObjectiveIndicatorDefaultArrowQuantity;
		}

		public void ResetMovementModeDefault()
		{
			this.MovementModeIndex = (int)ControlOptions.DefaultMovementMode;
		}

		public void ResetInverseReverseDefault()
		{
			this.InverseReverseControl = ControlOptions.DefaultInvertReverseControl;
		}

		public void Cancel()
		{
			this.Refresh();
		}

		public void Init()
		{
			this.ApplyOnlyOnStart();
			GameHubBehaviour.Hub.PlayerPrefs.ExecOnceOnPrefsLoaded(new Action(this.OnPrefsLoaded));
			ObservableExtensions.Subscribe<bool>(Observable.Do<bool>(this._observeCrossplayChange.GetAndObserve(), new Action<bool>(this.UpdateCrossplayToggle)));
		}

		private void UpdateCrossplayToggle(bool crossplayEnable)
		{
			this.CrossplayEnable = crossplayEnable;
		}

		private void OnPrefsLoaded()
		{
			this.Refresh();
			this.LoadPrefs();
			this.HasPendingChanges = false;
		}

		private static readonly BitLogger Log = new BitLogger(typeof(GameOptions));

		public bool HasPendingChanges;

		private bool ForceValues;

		private readonly bool TutorialEnabledDefault = true;

		private bool _tutorialEnabledHud;

		private bool _tutorialEnabledShop;

		private bool _tutorialEnabledKillCam;

		private bool _tutorialEnabledMainMenu;

		[Header("[Configs]")]
		[SerializeField]
		private OptionsScriptableObject _optionsScriptableObject;

		[Inject]
		private IConfigLoader _config;

		[Inject]
		private IObserveCrossplayChange _observeCrossplayChange;

		[Inject]
		private ILocalizeKey _localizeKey;

		[Inject]
		private IGetSupportedLanguages _getSupportedLanguages;

		[Inject]
		private IGetLanguageLocale _getLanguageLocale;

		private PlayerIndicatorLogBI _playerIndicatorLogBi;

		private ObjectiveIndicatorConfigurationsLogBI _objectiveIndicatorLogBi;

		public bool UseTeamColor;

		private int _movementModeIndex;

		public bool WasUsingControllerJoystick;

		private bool _inverseReverseControl;

		private readonly bool TargetHighlightDefault = true;

		private bool _targetHighlight;

		private readonly bool MemoryWarningDefault = true;

		private bool _memoryWarning;

		private int _languageIndex;

		private readonly bool ShowGadgetsLifebarDefault;

		private bool _showGadgetsLifebar;

		private readonly bool CrossplayEnableDefault = true;

		private bool _crossplayEnable;

		private readonly bool ShowGadgetsCursorDefault;

		private bool _showGadgetsCursor;

		private readonly bool ShowPingDefault;

		private bool _showPing;

		public bool _counselorActive = true;

		private bool _counselorHudHint = true;

		private readonly bool CounselorHudHintDefault = true;

		private bool _showLifebarText;

		private readonly bool ShowLifebarTextDefault = true;

		private bool _showPlayerIndicator = true;

		private float _playerIndicatorAlpha;

		private bool _showObjectiveIndicator = true;

		private readonly bool ShowObjectiveIndicatorDefault = true;

		private float _objectiveIndicatorAlpha;

		private int _objectiveIndicatorTypeIndex;

		private readonly int ObjectiveIndicatorTypeIndexDefault;

		private float _objectiveIndicatorSize;

		private float _objectiveIndicatorQuantity;

		public enum GameOptionPrefs
		{
			OPTIONS_GAME_TUTORIALENABLEDHUD,
			OPTIONS_GAME_TUTORIALENABLEDSHOP,
			OPTIONS_GAME_TUTORIALENABLEDKILLCAM,
			OPTIONS_GAME_TUTORIALENABLEDMAINMENU,
			OPTIONS_GAME_MOVEMENTMODE,
			OPTIONS_GAME_CASTMODE,
			OPTIONS_GAME_TARGETHIGHLIGHT,
			OPTIONS_GAME_LANGUAGE,
			OPTIONS_GAME_MEMORYWARNING,
			OPTIONS_GAME_INVERSEREVERSECONTROL,
			OPTIONS_GAME_SHOWGADGETSLIFEBAR,
			OPTIONS_GAME_COUNSELORACTIVE,
			OPTIONS_GAME_SHOWGADGETSCURSOR,
			OPTIONS_GAME_SHOWPING,
			OPTIONS_GAME_COUNSELORHUDHINT,
			OPTIONS_GAME_SHOWLIFEBARTEXT,
			OPTIONS_GAME_SHOWPLAYERINDICATOR,
			OPTIONS_GAME_PLAYERINDICATORALPHA,
			OPTIONS_GAME_SHOWOBJECTIVEINDICATOR,
			OPTIONS_GAME_OBJECTIVEINDICATORALPHA,
			OPTIONS_GAME_OBJECTIVEINDICATORTYPEINDEX,
			OPTIONS_GAME_OBJECTIVEINDICATORSIZE,
			OPTIONS_GAME_OBJECTIVEINDICATORQUANTITY
		}
	}
}
