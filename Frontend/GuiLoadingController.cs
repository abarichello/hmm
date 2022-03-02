using System;
using System.Diagnostics;
using HeavyMetalMachines.Localization;
using HeavyMetalMachines.Options;
using HeavyMetalMachines.Presenting;
using HeavyMetalMachines.VFX;
using Holoville.HOTween;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	public class GuiLoadingController : GameHubBehaviour, IGenericLoadingPresenter
	{
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event GuiLoadingController.OnHidingAnimationComplete OnHidingAnimationCompleted;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event GuiLoadingController.OnShowAnimationComplete OnShowAnimationCompleted;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event Action OnHideLoading;

		public bool IsLoading
		{
			get
			{
				return this._loading || this.LoadingPanel.gameObject.activeInHierarchy;
			}
		}

		private void Awake()
		{
			this._loading = this.LoadingPanel.gameObject.activeInHierarchy;
			this.LoadingPanel.alpha = (float)((!this._loading) ? 0 : 1);
		}

		public void ShowDefaultLoading(bool useLoadingTransition = false)
		{
			if (this._loading)
			{
				GuiLoadingController.Log.Info("Not showing loading screen because it is already being shown.");
				return;
			}
			this.ForceShowDefaultLoading(useLoadingTransition);
		}

		private void ForceShowDefaultLoading(bool useLoadingTransition = false)
		{
			this.TutorialUIHolder.SetActive(false);
			this.DefaultBackground.gameObject.SetActive(true);
			this.TutorialBackground.gameObject.SetActive(false);
			this.DefaultBackground.color = Color.white;
			GuiLoadingController.Log.Info("Showing default loading screen.");
			this.ShowLoading();
		}

		public void ShowTutorialLoading(bool useLoadingTransition = false)
		{
			this.TutorialUIHolder.SetActive(true);
			this.TutorialBackground.gameObject.SetActive(true);
			this.DefaultBackground.gameObject.SetActive(false);
			this.TutorialBackground.color = Color.white;
			this.SetupTutorialInputInfo();
			GuiLoadingController.Log.Info("Showing tutorial loading screen.");
			this.ShowLoading();
		}

		private void SetupTutorialInputInfo()
		{
			this._windowsPlatformGroupGameObject.SetActive(true);
		}

		private void ShowLoading()
		{
			this.titleLabel.text = Language.Get("Loading_loading", TranslationContext.Loading);
			this.ResetText();
			this.LoadingPanel.alpha = 0f;
			this.LoadingPanel.gameObject.SetActive(true);
			HOTween.Kill(this.LoadingPanel);
			this.LoadingPanel.alpha = 1f;
			this.OnShowComplete();
			this.ShowRandomizedTip();
			this._loading = true;
		}

		private void OnShowComplete()
		{
			if (this.OnShowAnimationCompleted != null)
			{
				this.OnShowAnimationCompleted();
			}
		}

		public void HideLoading()
		{
			GuiLoadingController.Log.InfoFormatStackTrace("OnHideLoading", new object[0]);
			if (this.OnHideLoading != null)
			{
				GuiLoadingController.Log.Info("Not hiding loading screen because it is already hidden.");
				this.OnHideLoading();
			}
			if (!this._loading)
			{
				return;
			}
			HOTween.Kill(this.LoadingPanel);
			this.OnHideComplete();
			this._loading = false;
		}

		private void OnHideComplete()
		{
			this.LoadingPanel.gameObject.SetActive(false);
			if (this.OnHidingAnimationCompleted != null)
			{
				this.OnHidingAnimationCompleted();
			}
		}

		private void ResetText()
		{
			this.ChangeText(Language.Get("Loading_loading", TranslationContext.Loading));
		}

		public void ChangeText(string text)
		{
			this.titleLabel.text = text;
		}

		private void ShowRandomizedTip()
		{
			this.TipLabelGroupGameObject.SetActive(false);
			GameState.GameStateKind stateKind = GameHubBehaviour.Hub.State.Current.StateKind;
			if (stateKind == GameState.GameStateKind.Splash || stateKind == GameState.GameStateKind.Welcome)
			{
				return;
			}
			this.ThirdPartyLogosGroupGameObject.SetActive(false);
			LoadingTip[] tipList = this.GetTipList();
			if (tipList.Length == 0)
			{
				GuiLoadingController.Log.ErrorFormat("No tip defined for loading screen [IsConsole={0}]. Please check the asset [{1}].", new object[]
				{
					Platform.Current.IsConsole(),
					this.TipsSettings
				});
				return;
			}
			LoadingTip loadingTip = tipList[Random.Range(0, tipList.Length)];
			if (!string.IsNullOrEmpty(loadingTip.TranslationDraft.CurrentPlatformDraft))
			{
				this.TipLabelGroupGameObject.SetActive(true);
				this.TipLabel.text = Language.Get(loadingTip.TranslationDraft.CurrentPlatformDraft, loadingTip.TranslationSheet);
			}
		}

		private LoadingTip[] GetTipList()
		{
			return (!Platform.Current.IsConsole()) ? this.TipsSettings.Tips : this.TipsSettings.ConsoleTips;
		}

		public void Show()
		{
			this.ForceShowDefaultLoading(false);
		}

		public void Hide()
		{
			this.HideLoading();
		}

		private static readonly BitLogger Log = new BitLogger(typeof(GuiLoadingController));

		[Header("[Default]")]
		[SerializeField]
		private HMMUI2DDynamicTexture DefaultBackground;

		[SerializeField]
		private UIPanel LoadingPanel;

		[SerializeField]
		private UILabel titleLabel;

		[SerializeField]
		private Color backgroundColorLoading;

		[SerializeField]
		private LoadingTipsSettings TipsSettings;

		[SerializeField]
		private GameObject TipLabelGroupGameObject;

		[SerializeField]
		private GameObject ThirdPartyLogosGroupGameObject;

		[SerializeField]
		private UILabel TipLabel;

		[Header("[Tutorial]")]
		[SerializeField]
		private UI2DSprite TutorialBackground;

		[SerializeField]
		private GameObject TutorialUIHolder;

		[SerializeField]
		private GameObject _windowsPlatformGroupGameObject;

		[SerializeField]
		private GameObject _orbisPlatformGroupGameObject;

		[SerializeField]
		private GameObject _xboxOnePlatformGroupGameObject;

		private bool _loading;

		public delegate void OnHidingAnimationComplete();

		public delegate void OnShowAnimationComplete();
	}
}
