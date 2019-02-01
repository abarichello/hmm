using System;
using System.Diagnostics;
using Assets.Standard_Assets.Scripts.HMM.BI;
using Assets.Standard_Assets.Scripts.HMM.PlotKids;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Options
{
	public class GameOptions : GameHubBehaviour
	{
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event Action<int> ListenToMovementModeChanged;

		private void Start()
		{
			this._playerIndicatorLogBi = new PlayerIndicatorLogBI(GameHubBehaviour.Hub);
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
					Language.Get("GAME_MOVEMENT_SIMULATOR", TranslationSheets.Options),
					Language.Get("GAME_MOVEMENT_CONTROLLER", TranslationSheets.Options),
					Language.Get("GAME_MOVEMENT_FOLLOWMOUSE", TranslationSheets.Options)
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
				int num = 2;
				if (value != num)
				{
					value = num;
					this.WasUsingControllerJoystick = true;
				}
				if (this._movementModeIndex == value && !this.ForceValues)
				{
					return;
				}
				this._movementModeIndex = value;
				this.HasPendingChanges = true;
				if (this.ListenToMovementModeChanged != null)
				{
					this.ListenToMovementModeChanged(value);
				}
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
				return this.SystemLanguageToIndex(Application.systemLanguage);
			}
		}

		public string[] LanguageNames
		{
			get
			{
				return new string[]
				{
					Language.Get("GAME_LANGUAGE_VALUES_EN", TranslationSheets.Options),
					Language.Get("GAME_LANGUAGE_VALUES_BR", TranslationSheets.Options),
					Language.Get("GAME_LANGUAGE_VALUES_RU", TranslationSheets.Options),
					Language.Get("GAME_LANGUAGE_VALUES_DE", TranslationSheets.Options),
					Language.Get("GAME_LANGUAGE_VALUES_FR", TranslationSheets.Options),
					Language.Get("GAME_LANGUAGE_VALUES_ES", TranslationSheets.Options),
					Language.Get("GAME_LANGUAGE_VALUES_TR", TranslationSheets.Options),
					Language.Get("GAME_LANGUAGE_VALUES_PL", TranslationSheets.Options)
				};
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
			float num = 0.804f;
			return this._playerIndicatorAlpha * num + 0.196f;
		}

		public LanguageCode CounselorLanguage
		{
			get
			{
				LanguageCode currentLanguageCode = Language.CurrentLanguageCode;
				if (currentLanguageCode != LanguageCode.DE && currentLanguageCode != LanguageCode.ES && currentLanguageCode != LanguageCode.FR && currentLanguageCode != LanguageCode.PT && currentLanguageCode != LanguageCode.RU)
				{
					return LanguageCode.EN;
				}
				return currentLanguageCode;
			}
		}

		private LanguageCode GetDefaultCounselorLanguage()
		{
			LanguageCode languageCode = Language.CurrentLanguage();
			if (languageCode != LanguageCode.PT && languageCode != LanguageCode.PT_BR)
			{
				return LanguageCode.EN;
			}
			return LanguageCode.PT;
		}

		private int GetDefaultCounselorLanguageIndex()
		{
			return this.LanguageCodeToIndex(this.GetDefaultCounselorLanguage());
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
			this.LanguageIndex = GameHubBehaviour.Hub.Config.GetIntSetting(GameOptions.GameOptionPrefs.OPTIONS_GAME_LANGUAGE.ToString(), this.LanguageIndexDefault);
			this.CounselorActive = (GameHubBehaviour.Hub.PlayerPrefs.GetInt(GameOptions.GameOptionPrefs.OPTIONS_GAME_COUNSELORACTIVE.ToString(), this.GetDefaultCounselorActive()) == 1);
			this.CounselorHudHint = (GameHubBehaviour.Hub.PlayerPrefs.GetInt(GameOptions.GameOptionPrefs.OPTIONS_GAME_COUNSELORHUDHINT.ToString(), (!this.CounselorHudHintDefault) ? 0 : 1) == 1);
			this.MovementModeIndex = GameHubBehaviour.Hub.PlayerPrefs.GetInt(GameOptions.GameOptionPrefs.OPTIONS_GAME_MOVEMENTMODE.ToString(), (int)ControlOptions.DefaultMovementMode);
			this.ShowLifebarText = (GameHubBehaviour.Hub.PlayerPrefs.GetInt(GameOptions.GameOptionPrefs.OPTIONS_GAME_SHOWLIFEBARTEXT.ToString(), (!this.ShowLifebarTextDefault) ? 0 : 1) == 1);
			this.ShowPlayerIndicator = (GameHubBehaviour.Hub.PlayerPrefs.GetInt(GameOptions.GameOptionPrefs.OPTIONS_GAME_SHOWPLAYERINDICATOR.ToString(), this.GetDefaultPlayerIndicatorActive()) == 1);
			this.PlayerIndicatorAlpha = GameHubBehaviour.Hub.PlayerPrefs.GetFloat(GameOptions.GameOptionPrefs.OPTIONS_GAME_PLAYERINDICATORALPHA.ToString(), this.PlayerIndicatorAlphaDefault);
			this.UseTeamColor = true;
			this.ForceValues = false;
			this.CheckInitialConfigForPlayerIndicator();
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
			this.LanguageIndex = GameHubBehaviour.Hub.Config.GetIntSetting(GameOptions.GameOptionPrefs.OPTIONS_GAME_LANGUAGE.ToString(), this.LanguageIndexDefault);
			this.CounselorActive = (GameHubBehaviour.Hub.PlayerPrefs.GetInt(GameOptions.GameOptionPrefs.OPTIONS_GAME_COUNSELORACTIVE.ToString(), this.GetDefaultCounselorActive()) == 1);
			this.CounselorHudHint = (GameHubBehaviour.Hub.PlayerPrefs.GetInt(GameOptions.GameOptionPrefs.OPTIONS_GAME_COUNSELORHUDHINT.ToString(), (!this.CounselorHudHintDefault) ? 0 : 1) == 1);
			this.MovementModeIndex = GameHubBehaviour.Hub.PlayerPrefs.GetInt(GameOptions.GameOptionPrefs.OPTIONS_GAME_MOVEMENTMODE.ToString(), (int)ControlOptions.DefaultMovementMode);
			this.ShowLifebarText = (GameHubBehaviour.Hub.PlayerPrefs.GetInt(GameOptions.GameOptionPrefs.OPTIONS_GAME_SHOWLIFEBARTEXT.ToString(), (!this.ShowLifebarTextDefault) ? 0 : 1) == 1);
			this.ShowPlayerIndicator = (GameHubBehaviour.Hub.PlayerPrefs.GetInt(GameOptions.GameOptionPrefs.OPTIONS_GAME_SHOWPLAYERINDICATOR.ToString(), this.GetDefaultPlayerIndicatorActive()) == 1);
			this.PlayerIndicatorAlpha = GameHubBehaviour.Hub.PlayerPrefs.GetFloat(GameOptions.GameOptionPrefs.OPTIONS_GAME_PLAYERINDICATORALPHA.ToString(), this.PlayerIndicatorAlphaDefault);
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
			GameHubBehaviour.Hub.Config.SetSetting(GameOptions.GameOptionPrefs.OPTIONS_GAME_LANGUAGE.ToString(), this.LanguageIndex.ToString());
			GameHubBehaviour.Hub.PlayerPrefs.SetInt(GameOptions.GameOptionPrefs.OPTIONS_GAME_COUNSELORACTIVE.ToString(), (!this.CounselorActive) ? 0 : 1);
			GameHubBehaviour.Hub.PlayerPrefs.SetInt(GameOptions.GameOptionPrefs.OPTIONS_GAME_COUNSELORHUDHINT.ToString(), (!this.CounselorHudHint) ? 0 : 1);
			GameHubBehaviour.Hub.PlayerPrefs.SetInt(GameOptions.GameOptionPrefs.OPTIONS_GAME_SHOWLIFEBARTEXT.ToString(), (!this.ShowLifebarText) ? 0 : 1);
			GameHubBehaviour.Hub.PlayerPrefs.SetInt(GameOptions.GameOptionPrefs.OPTIONS_GAME_SHOWPLAYERINDICATOR.ToString(), (!this.ShowPlayerIndicator) ? 0 : 1);
			GameHubBehaviour.Hub.PlayerPrefs.SetFloat(GameOptions.GameOptionPrefs.OPTIONS_GAME_PLAYERINDICATORALPHA.ToString(), this.PlayerIndicatorAlpha);
			this.HasPendingChanges = false;
			GameHubBehaviour.Hub.PlayerPrefs.SaveNow();
			GameHubBehaviour.Hub.Config.SaveSettings();
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

		public void GetterInfoForBILog()
		{
			int intWithoutDefault = GameHubBehaviour.Hub.PlayerPrefs.GetIntWithoutDefault(GameOptions.GameOptionPrefs.OPTIONS_GAME_SHOWPLAYERINDICATOR.ToString());
			float floatWithoutDefault = GameHubBehaviour.Hub.PlayerPrefs.GetFloatWithoutDefault(GameOptions.GameOptionPrefs.OPTIONS_GAME_PLAYERINDICATORALPHA.ToString());
			this._playerIndicatorLogBi.SaveInitialConfig(intWithoutDefault, floatWithoutDefault);
		}

		public void WriteBILogs()
		{
			this._playerIndicatorLogBi.LogBIPlayerIndicator();
		}

		public int LanguageCodeToIndex(LanguageCode code)
		{
			for (int i = 0; i < this.LanguageLocalizationValues.Length; i++)
			{
				if (this.LanguageLocalizationValues[i] == code)
				{
					return i;
				}
			}
			GameOptions.Log.ErrorFormat("unsupported LanguageCode {0}", new object[]
			{
				code
			});
			return -1;
		}

		public LanguageCode LanguageIndexToCode(int index)
		{
			if (index >= 0 && index < this.LanguageLocalizationValues.Length)
			{
				return this.LanguageLocalizationValues[index];
			}
			return LanguageCode.N;
		}

		public int SystemLanguageToIndex(SystemLanguage systemLanguage)
		{
			LanguageCode languageCode = Language.LanguageNameToCode(systemLanguage);
			for (int i = 0; i < this.LanguageLocalizationValues.Length; i++)
			{
				if (this.LanguageLocalizationValues[i] == languageCode)
				{
					return i;
				}
			}
			return 0;
		}

		public void ApplyOnlyOnStart()
		{
			this.LanguageIndex = GameHubBehaviour.Hub.Config.GetIntSetting(GameOptions.GameOptionPrefs.OPTIONS_GAME_LANGUAGE.ToString(), this.LanguageIndexDefault);
			if (this.LanguageIndex >= this.LanguageLocalizationValues.Length || this.LanguageIndex < 0)
			{
				GameOptions.Log.ErrorFormat("Invalid LanguageIndex: {0}", new object[]
				{
					this.LanguageIndex
				});
				this.LanguageIndex = this.LanguageIndexDefault;
				this.Apply();
			}
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
			this.PlayerIndicatorAlpha = this.PlayerIndicatorAlphaDefault;
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
			GameHubBehaviour.Hub.PlayerPrefs.ExecOnPrefsLoaded(new Action(this.OnPrefsLoaded));
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

		private const float MIN_PLAYERINDICATOR_ALPHA = 0.196f;

		private const float MAX_PLAYERINDICATOR_ALPHA = 1f;

		public PlayerIndicatorLogBI _playerIndicatorLogBi;

		public bool UseTeamColor;

		private int _movementModeIndex;

		public bool WasUsingControllerJoystick;

		private bool _inverseReverseControl;

		private readonly bool TargetHighlightDefault = true;

		private bool _targetHighlight;

		private readonly bool MemoryWarningDefault = true;

		private bool _memoryWarning;

		private readonly LanguageCode[] LanguageLocalizationValues = new LanguageCode[]
		{
			LanguageCode.EN,
			LanguageCode.PT,
			LanguageCode.RU,
			LanguageCode.DE,
			LanguageCode.FR,
			LanguageCode.ES,
			LanguageCode.TR,
			LanguageCode.PL
		};

		private int _languageIndex;

		private readonly bool ShowGadgetsLifebarDefault;

		private bool _showGadgetsLifebar;

		private readonly bool ShowGadgetsCursorDefault;

		private bool _showGadgetsCursor;

		private readonly bool ShowPingDefault;

		private bool _showPing;

		public bool _counselorActive = true;

		private bool _counselorHudHint = true;

		private readonly bool CounselorHudHintDefault = true;

		private bool _showLifebarText = true;

		private readonly bool ShowLifebarTextDefault;

		private bool _showPlayerIndicator = true;

		private float _playerIndicatorAlpha;

		private readonly float PlayerIndicatorAlphaDefault = 0.555f;

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
			OPTIONS_GAME_PLAYERINDICATORALPHA
		}
	}
}
