using System;
using System.Diagnostics;
using HeavyMetalMachines.Infra.DependencyInjection.Attributes;
using HeavyMetalMachines.Localization;
using HeavyMetalMachines.Options.Presenting;
using HeavyMetalMachines.Publishing;
using Hoplon.Input.UiNavigation;
using Pocketverse;
using Standard_Assets.Scripts.HMM.Util;
using UniRx;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	public class OptionsWindow : HudWindow
	{
		private IUiNavigationGroupHolder UiNavigationGroupHolder
		{
			get
			{
				return this._uiNavigationGroupHolder;
			}
		}

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event OptionsWindow.OnCloseDelegate OnCloseCallback;

		protected void Awake()
		{
			this.WindowGameObject.SetActive(false);
			if (Platform.Current.IsConsole())
			{
				this.GraphicsToggle.gameObject.SetActive(false);
			}
		}

		public void Show(OptionsWindow.OptionScreenKind screen, OptionsWindow.OnCloseDelegate onCloseCallback)
		{
			base.SetWindowVisibility(true);
			this._confirmWindowGuid = Guid.Empty;
			this._currentScreen = screen;
			this.OnCloseCallback = onCloseCallback;
			GameHubBehaviour.Hub.GuiScripts.ScreenResolution.ListenToResolutionChange += this.ScreenResolutionOnListenToResolutionChange;
			this._togglesGrid.Reposition();
		}

		private void ScreenResolutionOnListenToResolutionChange()
		{
			this.Panel.Invalidate(true);
		}

		public override bool CanBeHiddenByEscKey()
		{
			return !this._optionsControllerTabPresenter.IsBinding() && base.CanBeHiddenByEscKey() && !GameHubBehaviour.Hub.GuiScripts.ConfirmWindow.Visible;
		}

		public override void ChangeWindowVisibility(bool visible)
		{
			base.ChangeWindowVisibility(visible);
			if (visible)
			{
				this.ChangeTab(OptionsWindow.OptionScreenKind.Game);
				this.GraphicsToggle.value = false;
				this.CommandsToggle.value = false;
				this.InterfaceToggle.value = true;
				this.AudioToggle.value = false;
				GameHubBehaviour.Hub.CursorManager.Push(true, CursorManager.CursorTypes.OptionsCursor);
				GameHubBehaviour.Hub.Options.Game.GetterInfoForBILog();
				this.UiNavigationGroupHolder.AddHighPriorityGroup();
			}
			else
			{
				GameHubBehaviour.Hub.Options.Game.WriteBILogs();
				GameHubBehaviour.Hub.PlayerPrefs.SaveNow();
				GameHubBehaviour.Hub.CursorManager.Pop();
				if (GameHubBehaviour.Hub.State.IsLoading)
				{
					this.CloseWindow();
				}
				this.UiNavigationGroupHolder.RemoveHighPriorityGroup();
			}
		}

		public override void AnimationOnWindowExit()
		{
			base.AnimationOnWindowExit();
			this.CloseWindow();
		}

		private void CloseWindow()
		{
			GameHubBehaviour.Hub.GuiScripts.ScreenResolution.ListenToResolutionChange -= this.ScreenResolutionOnListenToResolutionChange;
			if (this.OnCloseCallback != null)
			{
				this.OnCloseCallback(this);
			}
			this.OnCloseCallback = null;
		}

		public void HideOptionsWindow()
		{
			this.ChangeTab(OptionsWindow.OptionScreenKind.None);
			base.SetWindowVisibility(false);
		}

		public void OnGraphicsTabButtonPressed()
		{
			this.ChangeTab(OptionsWindow.OptionScreenKind.Graphic);
		}

		public void OnGameTabButtonPressed()
		{
			this.ChangeTab(OptionsWindow.OptionScreenKind.Game);
		}

		public void OnControlTabButtonPressed()
		{
			this.ChangeTab(OptionsWindow.OptionScreenKind.Control);
		}

		public void OnAudioTabButtonPressed()
		{
			this.ChangeTab(OptionsWindow.OptionScreenKind.Audio);
		}

		public void OnResetDefaultButtonPressed()
		{
			this.TryResetDefault();
		}

		private void TryResetDefault()
		{
			string key = string.Empty;
			switch (this._currentScreen)
			{
			case OptionsWindow.OptionScreenKind.Audio:
				key = "ResetAudioDefaultValues";
				break;
			case OptionsWindow.OptionScreenKind.Control:
				key = this._resetControlDraft.CurrentPlatformDraft;
				break;
			case OptionsWindow.OptionScreenKind.Game:
				key = "ResetGameDefaultValues";
				break;
			case OptionsWindow.OptionScreenKind.Graphic:
				key = "ResetGraphicDefaultValues";
				break;
			default:
				OptionsWindow.Log.WarnFormat("TryResetDefault in an invalid screen: [{0}]", new object[]
				{
					this._currentScreen
				});
				return;
			}
			this._confirmWindowGuid = Guid.NewGuid();
			ConfirmWindowProperties properties = new ConfirmWindowProperties
			{
				Guid = this._confirmWindowGuid,
				QuestionText = Language.Get(key, TranslationContext.GUI),
				ConfirmButtonText = Language.Get("YES", TranslationContext.GUI),
				OnConfirm = delegate()
				{
					this.ResetDefaultConfirm();
				},
				RefuseButtonText = Language.Get("NO", TranslationContext.GUI),
				OnRefuse = delegate()
				{
					this.ResetDefaultRefuse();
				}
			};
			GameHubBehaviour.Hub.GuiScripts.ConfirmWindow.OpenConfirmWindow(properties);
		}

		private void ResetDefaultRefuse()
		{
			this.HideConfirmWindow();
			this.ChangeTab(this._currentScreen);
		}

		private void ResetDefaultConfirm()
		{
			this.HideConfirmWindow();
			this.CurrentResetDefault();
			this.ChangeTab(this._currentScreen);
		}

		private void CurrentResetDefault()
		{
			switch (this._currentScreen)
			{
			case OptionsWindow.OptionScreenKind.Audio:
				this.AudioGui.ResetDefault();
				break;
			case OptionsWindow.OptionScreenKind.Control:
				this._optionsControllerTabPresenter.ResetDefault();
				break;
			case OptionsWindow.OptionScreenKind.Game:
				this.GameGui.ResetDefault();
				break;
			case OptionsWindow.OptionScreenKind.Graphic:
				this.GraphicGui.ResetDefault();
				break;
			}
		}

		private void ChangeTab(OptionsWindow.OptionScreenKind newScreen)
		{
			this.HideTab(this._currentScreen);
			this._currentScreen = newScreen;
			this.GraphicsToggle.value = false;
			this.InterfaceToggle.value = false;
			this.AudioToggle.value = false;
			this.CommandsToggle.value = false;
			this.ShowTab(newScreen);
			this._currentToggle.value = true;
			this.ReloadCurrent();
		}

		private void ShowTab(OptionsWindow.OptionScreenKind optionScreenKind)
		{
			switch (optionScreenKind)
			{
			case OptionsWindow.OptionScreenKind.Audio:
				this._currentToggle = this.AudioToggle;
				this.AudioGui.Show();
				break;
			case OptionsWindow.OptionScreenKind.Control:
				this._currentToggle = this.CommandsToggle;
				ObservableExtensions.Subscribe<Unit>(Observable.ContinueWith<Unit, Unit>(this._optionsControllerTabPresenter.Initialize(), (Unit _) => this._optionsControllerTabPresenter.Show()));
				break;
			case OptionsWindow.OptionScreenKind.Game:
				this._currentToggle = this.InterfaceToggle;
				this.GameGui.Show();
				break;
			case OptionsWindow.OptionScreenKind.Graphic:
				this._currentToggle = this.GraphicsToggle;
				this.GraphicGui.Show();
				break;
			}
		}

		private void HideTab(OptionsWindow.OptionScreenKind optionScreenKind)
		{
			switch (optionScreenKind)
			{
			case OptionsWindow.OptionScreenKind.Audio:
				this.AudioGui.Hide();
				break;
			case OptionsWindow.OptionScreenKind.Control:
				ObservableExtensions.Subscribe<Unit>(Observable.ContinueWith<Unit, Unit>(this._optionsControllerTabPresenter.Hide(), (Unit _) => this._optionsControllerTabPresenter.Dispose()));
				break;
			case OptionsWindow.OptionScreenKind.Game:
				this.GameGui.Hide();
				break;
			case OptionsWindow.OptionScreenKind.Graphic:
				this.GraphicGui.Hide();
				break;
			}
		}

		private void ReloadCurrent()
		{
			OptionsWindow.OptionScreenKind currentScreen = this._currentScreen;
			if (currentScreen != OptionsWindow.OptionScreenKind.Graphic)
			{
				if (currentScreen != OptionsWindow.OptionScreenKind.Game)
				{
					if (currentScreen == OptionsWindow.OptionScreenKind.Audio)
					{
						this.AudioGui.ReloadCurrent();
					}
				}
				else
				{
					this.GameGui.ReloadCurrent();
				}
			}
			else
			{
				this.GraphicGui.ReloadCurrent();
			}
		}

		public override void OnDestroy()
		{
			base.OnDestroy();
			if (this._confirmWindowGuid != Guid.Empty)
			{
				this.HideConfirmWindow();
			}
		}

		private void HideConfirmWindow()
		{
			GameHubBehaviour.Hub.GuiScripts.ConfirmWindow.HideConfirmWindow(this._confirmWindowGuid);
			this._confirmWindowGuid = Guid.Empty;
		}

		private static readonly BitLogger Log = new BitLogger(typeof(OptionsWindow));

		public EscMenuAudioGui AudioGui;

		public EscMenuGameGui GameGui;

		public EscMenuGraphicGui GraphicGui;

		public UIToggle GraphicsToggle;

		public UIToggle InterfaceToggle;

		public UIToggle CommandsToggle;

		public UIToggle AudioToggle;

		public new UIPanel Panel;

		[SerializeField]
		private UIGrid _togglesGrid;

		[SerializeField]
		private UiNavigationGroupHolder _uiNavigationGroupHolder;

		[SerializeField]
		private MultiPlatformLocalizationDraft _resetControlDraft;

		private OptionsWindow.OptionScreenKind _currentScreen;

		private UIToggle _currentToggle;

		private Guid _confirmWindowGuid;

		[InjectOnClient]
		private IOptionsControllerTabPresenter _optionsControllerTabPresenter;

		[InjectOnClient]
		private IGetCurrentPublisher _getCurrentPublisher;

		public delegate void OnCloseDelegate(OptionsWindow optionsWindow);

		public enum OptionScreenKind
		{
			Starting,
			Audio,
			Control,
			Game,
			Graphic,
			None
		}
	}
}
