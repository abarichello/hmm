using System;
using System.Diagnostics;
using HeavyMetalMachines.Options;
using Holoville.HOTween;
using Holoville.HOTween.Core;
using Pocketverse;
using SharedUtils.Loading;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	public class GuiLoadingController : GameHubBehaviour
	{
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
			this.LoadingProgressBar.value = 0f;
			this.TutorialLoadingStarted = false;
		}

		private void Update()
		{
			this.isFakingProgress &= !LoadingManager.IsLoading;
			this.LoadingProgressBar.value = Mathf.Clamp(this.LoadingProgressBar.value + Time.deltaTime * UnityEngine.Random.value / 60f, this.minFakeProgress, this.maxFakeProgress);
			if (!this.isFakingProgress)
			{
				this.LoadingProgressBar.value = Mathf.Max(this.LoadingProgressBar.value, SingletonMonoBehaviour<LoadingManager>.Instance.InterpolatedProgress);
			}
		}

		public void ShowDefaultLoading(bool useLoadingTransition = false)
		{
			this._shouldLoading = true;
			if (this._loading)
			{
				return;
			}
			this.TutorialUIHolder.SetActive(false);
			this.DefaultBackground.gameObject.SetActive(true);
			this.TutorialBackground.gameObject.SetActive(false);
			this.DefaultBackground.color = ((!useLoadingTransition) ? Color.white : this.backgroundColorLoading);
			this.ShowLoading(useLoadingTransition);
		}

		public void ShowTutorialLoading(bool useLoadingTransition = false)
		{
			this._shouldLoading = true;
			this.TutorialUIHolder.SetActive(true);
			this.TutorialBackground.gameObject.SetActive(true);
			this.DefaultBackground.gameObject.SetActive(false);
			this.TutorialUIHolder.SetActive(true);
			this.TutorialBackground.gameObject.SetActive(true);
			this.DefaultBackground.gameObject.SetActive(false);
			this.LoadingProgressBar.value = 0f;
			this.isFakingProgress = true;
			this.TutorialBackground.color = ((!useLoadingTransition) ? Color.white : this.backgroundColorLoading);
			this.ShowLoading(useLoadingTransition);
		}

		public void ShowLoading(bool loadingTransition)
		{
			this.titleLabel.text = Language.Get("Loading_loading", TranslationSheets.Loading);
			this.ResetText();
			LoadingManager.WarnIfBundleLoad = false;
			this.LoadingPanel.alpha = 0f;
			this.LoadingPanel.gameObject.SetActive(true);
			HOTween.Kill(this.LoadingPanel);
			if (loadingTransition)
			{
				HOTween.To(this.LoadingPanel, 0.5f, new TweenParms().Prop("alpha", 1).UpdateType(UpdateType.TimeScaleIndependentUpdate).OnComplete(new TweenDelegate.TweenCallback(this.OnShowComplete)));
			}
			else
			{
				this.LoadingPanel.alpha = 1f;
				this.OnShowComplete();
			}
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

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event Action OnHideLoading;

		public void HideLoading()
		{
			this._shouldLoading = false;
			if (this.OnHideLoading != null)
			{
				this.OnHideLoading();
			}
			if (!this._loading)
			{
				return;
			}
			if (SingletonMonoBehaviour<LoadingManager>.Instance.InterpolatedProgress < 1f && this.LoadingProgressBar.gameObject.activeInHierarchy)
			{
				SingletonMonoBehaviour<LoadingManager>.Instance.OnInterpolatedProgressFinished += this.OnInterpolatedLoadingProgressFinished;
				return;
			}
			this.OnInterpolatedLoadingProgressFinished();
		}

		private void OnInterpolatedLoadingProgressFinished()
		{
			HOTween.Kill(this.LoadingPanel);
			HOTween.To(this.LoadingPanel, 1f, new TweenParms().Prop("alpha", 0).UpdateType(UpdateType.TimeScaleIndependentUpdate).OnComplete(new TweenDelegate.TweenCallback(this.OnHideComplete)));
			this._loading = false;
			LoadingManager.WarnIfBundleLoad = true;
		}

		private void OnHideComplete()
		{
			this.LoadingPanel.gameObject.SetActive(false);
			if (this.OnHidingAnimationCompleted != null)
			{
				this.OnHidingAnimationCompleted();
			}
		}

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event GuiLoadingController.OnHidingAnimationComplete OnHidingAnimationCompleted;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event GuiLoadingController.OnShowAnimationComplete OnShowAnimationCompleted;

		public void ResetText()
		{
			this.ChangeText(Language.Get("Loading_loading", TranslationSheets.Loading));
		}

		public void ChangeText(string text)
		{
			this.titleLabel.text = text;
		}

		private void ShowRandomizedTip()
		{
			this.TipLabelGroupGameObject.SetActive(false);
			if (this.TipsSettings.Tips.Length == 0)
			{
				GuiLoadingController.Log.ErrorFormat("No tip defined for loading screen. Please check the asset [{0}]", new object[]
				{
					this.TipsSettings
				});
				return;
			}
			LoadingTip loadingTip = this.TipsSettings.Tips[UnityEngine.Random.Range(0, this.TipsSettings.Tips.Length)];
			if (!string.IsNullOrEmpty(loadingTip.TranslationDraft))
			{
				this.TipLabelGroupGameObject.SetActive(true);
				this.TipLabel.text = Language.Get(loadingTip.TranslationDraft, loadingTip.TranslationSheet);
			}
		}

		public static readonly BitLogger Log = new BitLogger(typeof(GuiLoadingController));

		[Header("Default")]
		public UI2DSprite DefaultBackground;

		public UIPanel LoadingPanel;

		public UILabel titleLabel;

		public Color backgroundColorLoading;

		public LoadingTipsSettings TipsSettings;

		public GameObject TipLabelGroupGameObject;

		public UILabel TipLabel;

		[Header("Tutorial")]
		public UI2DSprite TutorialBackground;

		public GameObject TutorialUIHolder;

		public UIProgressBar LoadingProgressBar;

		public bool TutorialLoadingStarted;

		private bool _loading;

		private bool _shouldLoading;

		private const float expectedLoadingTime = 60f;

		private float minFakeProgress;

		private float maxFakeProgress = 0.95f;

		private bool isFakingProgress = true;

		public delegate void OnHidingAnimationComplete();

		public delegate void OnShowAnimationComplete();
	}
}
