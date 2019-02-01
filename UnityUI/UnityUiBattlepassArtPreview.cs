using System;
using Assets.ClientApiObjects.Components;
using Assets.ClientApiObjects.Specializations;
using Assets.Standard_Assets.Scripts.HMM.Video;
using HeavyMetalMachines.Frontend;
using ModelViewer;
using UnityEngine;
using UnityEngine.UI;

namespace HeavyMetalMachines.UnityUI
{
	public class UnityUiBattlepassArtPreview : MonoBehaviour
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
				if (this._loadingView != null)
				{
					this._loadingView.Hide();
				}
				if (this._isVisible)
				{
					this._rewardVideoCanvasGroup.alpha = 1f;
					this._mainAnimation.Play("release_reward_in");
				}
			}
		}

		private void LoadRewardAsset(UnityUiBattlepassArtPreview.ArtPreviewData data)
		{
			this._currentRewardAssetKind = data.RewardAssetKind;
			this._rewardModelViewerImage.enabled = false;
			bool active = data.RewardAssetKind == ItemPreviewKind.Sprite || data.RewardAssetKind == ItemPreviewKind.Lore;
			this._rewardRawImage.gameObject.SetActive(active);
			this._rewardVideoCanvasGroup.alpha = 0f;
			this._rewardVideoPlayer.Stop();
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
			switch (data.RewardAssetKind)
			{
			case ItemPreviewKind.Sprite:
			case ItemPreviewKind.Lore:
				this._rewardRawImage.TryToLoadAsset(data.RewardAssetName);
				break;
			case ItemPreviewKind.Model3D:
				this._rewardModelViewer.gameObject.SetActive(true);
				this._rewardModelViewer.ModelName = data.RewardAssetName;
				if (this._skinInfoView != null)
				{
					this._skinInfoView.Show(data.SkinCustomizations);
				}
				if (!string.IsNullOrEmpty(data.ArtPreviewBackGroundAssetName) && this._portraitRawImage != null)
				{
					this._portraitRawImage.gameObject.SetActive(true);
					this._portraitRawImage.TryToLoadAsset(data.ArtPreviewBackGroundAssetName);
				}
				break;
			case ItemPreviewKind.Video:
				this._rewardVideoPlayer.gameObject.SetActive(true);
				this._rewardVideoPlayer.VideoClipName = data.RewardAssetName;
				if (this._loadingView != null)
				{
					this._loadingView.Show();
				}
				break;
			}
		}

		public void ShowReward(UnityUiBattlepassArtPreview.ArtPreviewData artPreviewData, bool showDescriptionDetail = true)
		{
			this._isVisible = true;
			this.LoadRewardAsset(artPreviewData);
			if (artPreviewData.ShowCurrencyIcon)
			{
				this._fameRawImage.gameObject.SetActive(true);
				this._fameValueText.gameObject.SetActive(true);
				this._fameValueText.text = string.Format(Language.Get("BATTLEPASS_REWARD_DESCRIPTION_AMOUNT", TranslationSheets.Battlepass), artPreviewData.CurrencyReward);
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
					this._loreComponentsGui.TitleText.text = Language.Get(loreData.TitleText, TranslationSheets.Items);
					this._loreComponentsGui.SubtitleText.text = Language.Get(loreData.SubtitleText, TranslationSheets.Items);
					this._loreComponentsGui.DescriptionText.text = Language.Get(loreData.DescriptionText, TranslationSheets.Items);
				}
			}
			else
			{
				this._loreComponentsGui.LockedGameObject.SetActive(false);
				this._loreComponentsGui.UnlockedGameObject.SetActive(false);
				this._descriptionDetailObject.SetActive(showDescriptionDetail);
				if (showDescriptionDetail)
				{
					this._titleText.text = Language.Get(artPreviewData.TitleText, TranslationSheets.Items);
					this._descriptionText.text = Language.Get(artPreviewData.DescriptionText, TranslationSheets.Items);
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

		public void HideReward()
		{
			if (this._isVisible)
			{
				this._isVisible = false;
				this._mainAnimation.Play("release_reward_out");
			}
		}

		[Header("[Components]")]
		[SerializeField]
		private Animation _mainAnimation;

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

		public struct ArtPreviewLoreData
		{
			public bool IsLocked;

			public string TitleText;

			public string SubtitleText;

			public string DescriptionText;
		}

		public struct ArtPreviewData
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

			public SkinPrefabItemTypeComponent.SkinCustomizations SkinCustomizations;
		}

		[Serializable]
		private struct LoreComponentsGui
		{
			public GameObject LockedGameObject;

			public GameObject UnlockedGameObject;

			public Text TitleText;

			public Text SubtitleText;

			public Text DescriptionText;
		}
	}
}
