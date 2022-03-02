using System;
using Assets.ClientApiObjects.Components;
using Assets.ClientApiObjects.Specializations;
using HeavyMetalMachines.Frontend;
using HeavyMetalMachines.Localization;
using HeavyMetalMachines.Presenting;
using HeavyMetalMachines.Presenting.Unity;
using HeavyMetalMachines.Video;
using Hoplon.Logging;
using ModelViewer;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace HeavyMetalMachines.UnityUI
{
	public class UnityUIInventoryArtPreview : MonoBehaviour, IItemPreviewer
	{
		private void OnEnable()
		{
			if (!this._rewardModelViewer)
			{
				return;
			}
			ModelViewerUnityUI rewardModelViewer = this._rewardModelViewer;
			rewardModelViewer.OnModelLoadedCallback = (Action)Delegate.Combine(rewardModelViewer.OnModelLoadedCallback, new Action(this.OnModelLoaded));
			this._rewardModelViewerImage.enabled = false;
			this._currentRewardAssetKind = ItemPreviewKind.None;
			if (this._skinInfoView != null)
			{
				this._skinInfoView.Hide();
			}
			this._isVisible = false;
		}

		private void Start()
		{
			this._videoPlayer = new ObservableVideoPreviewPresenter(this._viewProvider, this._logger);
			this._videoPlayer.Initialize();
		}

		private void OnDisable()
		{
			if (this._rewardModelViewer)
			{
				ModelViewerUnityUI rewardModelViewer = this._rewardModelViewer;
				rewardModelViewer.OnModelLoadedCallback = (Action)Delegate.Remove(rewardModelViewer.OnModelLoadedCallback, new Action(this.OnModelLoaded));
			}
		}

		private void OnModelLoaded()
		{
			if (this._currentRewardAssetKind == ItemPreviewKind.Model3D)
			{
				this._rewardModelViewerImage.enabled = true;
			}
			this._rewardModelViewer.GetComponent<CanvasGroup>().alpha = 1f;
		}

		private IObservable<Unit> LoadRewardAsset(UnityUIInventoryArtPreview.ArtPreviewData data)
		{
			return Observable.Defer<Unit>(delegate()
			{
				this._currentRewardAssetKind = data.RewardAssetKind;
				this.SetupView(data);
				switch (data.RewardAssetKind)
				{
				case ItemPreviewKind.Sprite:
				case ItemPreviewKind.Lore:
					return this._rewardRawImage.LoadAsset(data.RewardAssetName);
				case ItemPreviewKind.Model3D:
					return this.LoadModel(this._rewardModelViewer, data);
				case ItemPreviewKind.Video:
					return this.LoadVideo(data);
				case ItemPreviewKind.SpriteSheet:
					throw new InvalidOperationException("UnityUIInventoryArtPreview received SpriteSheet. Use RadialMenu instead");
				case ItemPreviewKind.SmallSprite:
					return this._smallRewardRawImage.LoadAsset(data.RewardAssetName);
				default:
					throw new InvalidOperationException(string.Format("Cannot load preview for asset of type {0}.", data.RewardAssetKind));
				}
			});
		}

		private IObservable<Unit> LoadModel(ModelViewerUnityUI modelViewer, UnityUIInventoryArtPreview.ArtPreviewData data)
		{
			modelViewer.gameObject.SetActive(true);
			if (this._skinInfoView != null && data.SkinPrefabComponent != null)
			{
				this._skinInfoView.Show(data.SkinPrefabComponent);
			}
			return Observable.Merge<Unit>(new IObservable<Unit>[]
			{
				this.TryToSetupRewardBackGroundAsset(data.ArtPreviewBackGroundAssetName),
				modelViewer.Load(data.RewardAssetName)
			});
		}

		private IObservable<Unit> LoadVideo(UnityUIInventoryArtPreview.ArtPreviewData data)
		{
			return Observable.Defer<Unit>(delegate()
			{
				if (this._loadingView != null)
				{
					this._loadingView.Show();
				}
				return Observable.Do<Unit>(this._videoPlayer.LoadVideo(data.RewardAssetName), delegate(Unit _)
				{
					if (this._loadingView != null)
					{
						this._loadingView.Hide();
					}
				});
			});
		}

		private void SetupView(UnityUIInventoryArtPreview.ArtPreviewData data)
		{
			this._rewardModelViewerImage.enabled = false;
			bool active = data.RewardAssetKind == ItemPreviewKind.Sprite || data.RewardAssetKind == ItemPreviewKind.Lore;
			this._rewardRawImage.gameObject.SetActive(active);
			this._smallRewardRawImage.gameObject.SetActive(data.RewardAssetKind == ItemPreviewKind.SmallSprite);
			this._videoPlayer.Hide();
			if (this._loadingView != null)
			{
				this._loadingView.Hide();
			}
			if (this._skinInfoView != null)
			{
				this._skinInfoView.Hide();
			}
			if (this._portraitRawImage != null)
			{
				this._portraitRawImage.gameObject.SetActive(false);
			}
		}

		private IObservable<Unit> TryToSetupRewardBackGroundAsset(string backGroundAssetName)
		{
			return Observable.Defer<Unit>(delegate()
			{
				if (string.IsNullOrEmpty(backGroundAssetName) || this._portraitRawImage == null)
				{
					return Observable.ReturnUnit();
				}
				this._portraitRawImage.gameObject.SetActive(true);
				CanvasGroup component = this._portraitRawImage.GetComponent<CanvasGroup>();
				if (component != null)
				{
					component.alpha = 0f;
				}
				return this._portraitRawImage.LoadAsset(backGroundAssetName);
			});
		}

		public void SetupAsset(UnityUIInventoryArtPreview.ArtPreviewData artPreviewData, bool showDescriptionDetail = true)
		{
			this._currentArtPreviewData = artPreviewData;
			this._isVisible = true;
			this._fameRawImage.gameObject.SetActive(false);
			this._fameValueText.gameObject.SetActive(false);
			UnityUIInventoryArtPreview.ArtPreviewLoreData loreData = artPreviewData.LoreData;
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
		}

		public void HideCanvas()
		{
			if (this._loadingView != null)
			{
				this._loadingView.Hide();
			}
			CanvasGroup component = this._rewardModelViewer.GetComponent<CanvasGroup>();
			component.alpha = 0f;
			component.interactable = false;
			component.blocksRaycasts = false;
			component = base.GetComponent<CanvasGroup>();
			component.alpha = 0f;
			component.interactable = false;
			component.blocksRaycasts = false;
			base.gameObject.SetActive(false);
		}

		public void ActivateCanvas()
		{
			base.gameObject.SetActive(true);
			CanvasGroup component = this._rewardModelViewer.GetComponent<CanvasGroup>();
			component.alpha = 1f;
			component.interactable = true;
			component.blocksRaycasts = true;
			component = base.GetComponent<CanvasGroup>();
			component.alpha = 1f;
			component.interactable = true;
			component.blocksRaycasts = true;
		}

		public IObservable<Unit> Show()
		{
			return Observable.Defer<Unit>(delegate()
			{
				this.ActivateCanvas();
				return Observable.ContinueWith<Unit, Unit>(this.LoadRewardAsset(this._currentArtPreviewData), this._animationIn.Play());
			});
		}

		public IObservable<Unit> Hide()
		{
			return Observable.Do<Unit>(this._animationOut.Play(), delegate(Unit _)
			{
				this.HideCanvas();
			});
		}

		public void SetAsset(IArtPreviewData previewData)
		{
			UnityUIInventoryArtPreview.ArtPreviewData artPreviewData = (UnityUIInventoryArtPreview.ArtPreviewData)previewData;
			bool showDescriptionDetail = artPreviewData.TitleText != null && artPreviewData.DescriptionText != null;
			this.SetupAsset(artPreviewData, showDescriptionDetail);
		}

		[Header("[Components]")]
		[SerializeField]
		private UnityAnimation _animationIn;

		[SerializeField]
		private UnityAnimation _animationOut;

		[SerializeField]
		private HmmUiRawImage _smallRewardRawImage;

		[SerializeField]
		private HmmUiRawImage _rewardRawImage;

		[SerializeField]
		private HmmUiRawImage _portraitRawImage;

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
		private RawImage _rewardModelViewerImage;

		[SerializeField]
		private GameObject _descriptionDetailObject;

		[SerializeField]
		private UnityUIInventoryArtPreview.LoreComponentsGui _loreComponentsGui;

		[SerializeField]
		private UnityUiLoadingView _loadingView;

		[SerializeField]
		private UnityUiArtPreviewSkinInfoView _skinInfoView;

		private ObservableVideoPreviewPresenter _videoPlayer;

		[Inject]
		private IViewProvider _viewProvider;

		[Inject]
		private ILogger<ObservableVideoPreviewPresenter> _logger;

		private ItemPreviewKind _currentRewardAssetKind;

		private UnityUIInventoryArtPreview.ArtPreviewData _currentArtPreviewData;

		private bool _isVisible;

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

			public UnityUIInventoryArtPreview.ArtPreviewLoreData LoreData;

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
