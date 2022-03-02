using System;
using Assets.ClientApiObjects.Components;
using Assets.ClientApiObjects.Specializations;
using HeavyMetalMachines.Frontend;
using HeavyMetalMachines.Localization;
using HeavyMetalMachines.Presenting;
using HeavyMetalMachines.Video;
using ModelViewer;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace HeavyMetalMachines.UnityUI
{
	public class UnityUiBattlepassArtPreview : MonoBehaviour, IItemPreviewer
	{
		private void OnEnable()
		{
			if (!this._rewardModelViewer)
			{
				return;
			}
			ModelViewerUnityUI rewardModelViewer = this._rewardModelViewer;
			rewardModelViewer.OnModelLoadedCallback = (Action)Delegate.Combine(rewardModelViewer.OnModelLoadedCallback, new Action(this.OnModelLoaded));
			this._rewardVideoPlayer.OnVideoLoaded += this.OnVideoLoaded;
			this._rewardModelViewerImage.enabled = false;
			this._rewardVideoCanvasGroup.alpha = 0f;
			this._currentRewardAssetKind = ItemPreviewKind.None;
			if (this._skinInfoView != null)
			{
				this._skinInfoView.Hide();
			}
			this._isVisible = false;
		}

		private void OnDisable()
		{
			if (this._rewardModelViewer)
			{
				ModelViewerUnityUI rewardModelViewer = this._rewardModelViewer;
				rewardModelViewer.OnModelLoadedCallback = (Action)Delegate.Remove(rewardModelViewer.OnModelLoadedCallback, new Action(this.OnModelLoaded));
			}
			if (this._rewardVideoPlayer != null)
			{
				this._rewardVideoPlayer.OnVideoLoaded -= this.OnVideoLoaded;
			}
		}

		private void OnModelLoaded()
		{
			if (!this._isVisible)
			{
				return;
			}
			if (this._currentRewardAssetKind == ItemPreviewKind.Model3D)
			{
				this._rewardModelViewerImage.enabled = true;
				this._mainAnimation.Play("release_reward_in");
				this._rewardModelViewer.GetComponent<CanvasGroup>().alpha = 1f;
			}
		}

		private void OnVideoLoaded()
		{
			if (this._currentRewardAssetKind == ItemPreviewKind.Video)
			{
				this.HideLoadingView();
				if (this._isVisible)
				{
					this._rewardVideoCanvasGroup.alpha = 1f;
					this._mainAnimation.Play("release_reward_in");
				}
			}
		}

		private void ShowLoadingView()
		{
			if (this._loadingView != null)
			{
				this._loadingView.Show();
			}
		}

		private void HideLoadingView()
		{
			if (this._loadingView != null)
			{
				this._loadingView.Hide();
			}
		}

		private IObservable<Unit> LoadRewardAsset(UnityUiBattlepassArtPreview.ArtPreviewData data)
		{
			return Observable.ContinueWith<Unit, Unit>(Observable.Delay<Unit>(Observable.Do<Unit>(Observable.ReturnUnit(), delegate(Unit _)
			{
				this.PrepareForAsset(data);
			}), TimeSpan.FromMilliseconds(150.0)), this.LoadAsset(data));
		}

		private void PrepareForAsset(UnityUiBattlepassArtPreview.ArtPreviewData data)
		{
			this._currentRewardAssetKind = data.RewardAssetKind;
			this._rewardModelViewerImage.enabled = false;
			this._rewardRawImage.gameObject.SetActive(false);
			this._smallRewardRawImage.gameObject.SetActive(false);
			this._rewardVideoCanvasGroup.alpha = 0f;
			this._rewardVideoPlayer.Stop();
			if (this._skinInfoView != null)
			{
				this._skinInfoView.Hide();
			}
			if (this._portraitRawImage != null)
			{
				this._portraitRawImage.gameObject.SetActive(false);
			}
			if (this._previewAnimatedSprite != null)
			{
				this._previewAnimatedSprite.gameObject.SetActive(false);
			}
			this.ShowLoadingView();
		}

		private IObservable<Unit> LoadAsset(UnityUiBattlepassArtPreview.ArtPreviewData data)
		{
			switch (data.RewardAssetKind)
			{
			case ItemPreviewKind.Sprite:
			case ItemPreviewKind.Lore:
				return Observable.Do<Unit>(this._rewardRawImage.LoadAsset(data.RewardAssetName), delegate(Unit _)
				{
					this._rewardRawImage.gameObject.SetActive(true);
					this.HideLoadingView();
				});
			case ItemPreviewKind.Model3D:
				return Observable.Do<Unit>(Observable.ReturnUnit(), delegate(Unit _)
				{
					this._rewardModelViewer.gameObject.SetActive(true);
					this._rewardModelViewer.ModelName = data.RewardAssetName;
					if (this._skinInfoView != null && data.SkinPrefabComponent != null)
					{
						this._skinInfoView.Show(data.SkinPrefabComponent);
					}
					this.TryToSetupRewardBackGroundAsset(data.ArtPreviewBackGroundAssetName);
					this.HideLoadingView();
				});
			case ItemPreviewKind.Video:
				return Observable.Do<Unit>(Observable.ReturnUnit(), delegate(Unit _)
				{
					this._rewardVideoPlayer.gameObject.SetActive(true);
					this._rewardVideoPlayer.VideoClipName = data.RewardAssetName;
				});
			case ItemPreviewKind.SpriteSheet:
				return Observable.Do<Unit>(this._previewAnimatedSprite.LoadAsset(data.RewardAssetName), delegate(Unit _)
				{
					this._previewAnimatedSprite.gameObject.SetActive(true);
					this._previewAnimatedSprite.StartAnimation();
					this.HideLoadingView();
				});
			case ItemPreviewKind.SmallSprite:
				return Observable.Do<Unit>(this._smallRewardRawImage.LoadAsset(data.RewardAssetName), delegate(Unit _)
				{
					this._smallRewardRawImage.gameObject.SetActive(true);
					this.HideLoadingView();
				});
			default:
				return Observable.ReturnUnit();
			}
		}

		private void TryToSetupRewardBackGroundAsset(string backGroundAssetName)
		{
			if (string.IsNullOrEmpty(backGroundAssetName) || this._portraitRawImage == null)
			{
				return;
			}
			this._portraitRawImage.gameObject.SetActive(true);
			CanvasGroup component = this._portraitRawImage.GetComponent<CanvasGroup>();
			if (component != null)
			{
				component.alpha = 0f;
			}
			this._portraitRawImage.TryToLoadAsset(backGroundAssetName);
		}

		public void ShowReward(UnityUiBattlepassArtPreview.ArtPreviewData artPreviewData, bool showDescriptionDetail = true)
		{
			if (this._delayShowReward != null)
			{
				this._delayShowReward.Dispose();
			}
			this._isVisible = true;
			this._delayShowReward = ObservableExtensions.Subscribe<Unit>(Observable.Do<Unit>(Observable.Do<Unit>(this.LoadRewardAsset(artPreviewData), delegate(Unit _)
			{
				this.ShowRewardInternal(artPreviewData, showDescriptionDetail);
			}), delegate(Unit _)
			{
				this._delayShowReward = null;
			}));
		}

		private void ShowRewardInternal(UnityUiBattlepassArtPreview.ArtPreviewData artPreviewData, bool showDescriptionDetail)
		{
			if (artPreviewData.ShowCurrencyIcon)
			{
				this._fameRawImage.gameObject.SetActive(true);
				this._fameValueText.gameObject.SetActive(true);
				this._fameValueText.text = Language.GetFormatted("BATTLEPASS_REWARD_DESCRIPTION_AMOUNT", TranslationContext.Battlepass, new object[]
				{
					artPreviewData.CurrencyReward
				});
				this._fameValueText.color = ((!artPreviewData.IsHardCurrency) ? this._softRewardTextColor : this._hardRewardTextColor);
				this._fameRawImage.texture = ((!artPreviewData.IsHardCurrency) ? this._softRewardIconTexture : this._hardRewardIconTexture);
			}
			else
			{
				this._fameRawImage.gameObject.SetActive(false);
				this._fameValueText.gameObject.SetActive(false);
			}
			UnityUiBattlepassArtPreview.ArtPreviewLoreData loreData = artPreviewData.LoreData;
			if (artPreviewData.RewardAssetKind == ItemPreviewKind.Lore)
			{
				this._descriptionDetailObject.SetActive(false);
				this._loreComponentsGui.LockedGameObject.SetActive(loreData.IsLocked);
				this._loreComponentsGui.UnlockedGameObject.SetActive(!loreData.IsLocked);
				if (!loreData.IsLocked)
				{
					this._loreComponentsGui.TitleText.text = Language.Get(loreData.TitleText, TranslationContext.Items);
					this._loreComponentsGui.SubtitleText.text = Language.Get(loreData.SubtitleText, TranslationContext.Items);
					this._loreComponentsGui.DescriptionText.text = Language.Get(loreData.DescriptionText, TranslationContext.Items);
					LayoutRebuilder.ForceRebuildLayoutImmediate(this._loreComponentsGui.DescriptionText.rectTransform);
					this._loreComponentsGui.DescriptionScrollRect.verticalNormalizedPosition = 1f;
				}
			}
			else
			{
				this._loreComponentsGui.LockedGameObject.SetActive(false);
				this._loreComponentsGui.UnlockedGameObject.SetActive(false);
				this._descriptionDetailObject.SetActive(showDescriptionDetail);
				if (showDescriptionDetail)
				{
					this._titleText.text = Language.Get(artPreviewData.TitleText, TranslationContext.Items);
					this._descriptionText.text = Language.Get(artPreviewData.DescriptionText, TranslationContext.Items);
				}
			}
			if (artPreviewData.RewardAssetKind != ItemPreviewKind.Model3D && artPreviewData.RewardAssetKind != ItemPreviewKind.Video)
			{
				this._mainAnimation.Play("release_reward_in");
			}
		}

		public void PreloadModelViewer()
		{
			this._rewardModelViewer.gameObject.SetActive(true);
		}

		public void ReloadModelViewerScene()
		{
			this._rewardModelViewer.GetComponent<CanvasGroup>().alpha = 0f;
			this._rewardModelViewer.ReloadScene();
		}

		public void HideCanvas()
		{
			this.HideLoadingView();
			CanvasGroup component = this._rewardModelViewer.GetComponent<CanvasGroup>();
			component.alpha = 0f;
			component.interactable = false;
			component.blocksRaycasts = false;
			component = base.GetComponent<CanvasGroup>();
			component.alpha = 0f;
			component.interactable = false;
			component.blocksRaycasts = false;
		}

		public void ShowCanvas()
		{
			CanvasGroup component = this._rewardModelViewer.GetComponent<CanvasGroup>();
			component.alpha = 1f;
			component.interactable = true;
			component.blocksRaycasts = true;
			component = base.GetComponent<CanvasGroup>();
			component.alpha = 1f;
			component.interactable = true;
			component.blocksRaycasts = true;
		}

		public void HideReward()
		{
			if (this._isVisible)
			{
				this._isVisible = false;
				this._mainAnimation.Play("release_reward_out");
			}
		}

		public void SetAsset(IArtPreviewData previewData)
		{
			UnityUiBattlepassArtPreview.ArtPreviewData artPreviewData = (UnityUiBattlepassArtPreview.ArtPreviewData)previewData;
			bool showDescriptionDetail = artPreviewData.TitleText != null && artPreviewData.DescriptionText != null;
			this.ShowReward(artPreviewData, showDescriptionDetail);
		}

		[Header("[Components]")]
		[SerializeField]
		private Animation _mainAnimation;

		[SerializeField]
		private HmmUiRawImage _smallRewardRawImage;

		[SerializeField]
		private HmmUiRawImage _rewardRawImage;

		[SerializeField]
		private HmmUiRawImage _portraitRawImage;

		[SerializeField]
		private AnimatedRawImage _previewAnimatedSprite;

		[SerializeField]
		private Text _titleText;

		[SerializeField]
		private Text _descriptionText;

		[SerializeField]
		private RawImage _fameRawImage;

		[SerializeField]
		private Text _fameValueText;

		[SerializeField]
		private ModelViewerUnityUI _rewardModelViewer;

		[SerializeField]
		private VideoPreviewUnityUI _rewardVideoPlayer;

		[SerializeField]
		private RawImage _rewardModelViewerImage;

		[SerializeField]
		private CanvasGroup _rewardVideoCanvasGroup;

		[SerializeField]
		private GameObject _descriptionDetailObject;

		[SerializeField]
		private UnityUiBattlepassArtPreview.LoreComponentsGui _loreComponentsGui;

		[SerializeField]
		private UnityUiLoadingView _loadingView;

		[SerializeField]
		private UnityUiArtPreviewSkinInfoView _skinInfoView;

		[Header("[Configs]")]
		[SerializeField]
		private Texture _softRewardIconTexture;

		[SerializeField]
		private Texture _hardRewardIconTexture;

		[SerializeField]
		private Color _softRewardTextColor;

		[SerializeField]
		private Color _hardRewardTextColor;

		private ItemPreviewKind _currentRewardAssetKind;

		private bool _isVisible;

		private IDisposable _delayShowReward;

		public struct ArtPreviewLoreData
		{
			public bool IsLocked;

			public string TitleText;

			public string SubtitleText;

			public string DescriptionText;
		}

		public struct ArtPreviewData : IArtPreviewData
		{
			public string TitleText;

			public string DescriptionText;

			public ItemPreviewKind RewardAssetKind;

			public string RewardAssetName;

			public bool ShowCurrencyIcon;

			public int CurrencyReward;

			public bool IsHardCurrency;

			public UnityUiBattlepassArtPreview.ArtPreviewLoreData LoreData;

			public string ArtPreviewBackGroundAssetName;

			public SkinPrefabItemTypeComponent SkinPrefabComponent;
		}

		[Serializable]
		private struct LoreComponentsGui
		{
			public GameObject LockedGameObject;

			public GameObject UnlockedGameObject;

			public Text TitleText;

			public Text SubtitleText;

			public Text DescriptionText;

			public ScrollRect DescriptionScrollRect;
		}
	}
}
