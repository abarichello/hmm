using System;
using System.Collections;
using System.Collections.Generic;
using Assets.ClientApiObjects.Specializations;
using HeavyMetalMachines.DataTransferObjects.Progression;
using HeavyMetalMachines.Frontend;
using HeavyMetalMachines.Infra.DependencyInjection.Attributes;
using HeavyMetalMachines.Localization;
using HeavyMetalMachines.Store.Business;
using HeavyMetalMachines.UnityUI;
using HeavyMetalMachines.Utils;
using Pocketverse;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace HeavyMetalMachines.Battlepass
{
	public class UnityUIBattlepassRewardView : MonoBehaviour, IBattlepassRewardView
	{
		private void Awake()
		{
			this._hideSubject = new Subject<Unit>();
			this._battlepassRewardComponent = this._battlepassRewardComponentAsset;
			IStoreBusinessFactory storeBusinessFactory = this._diContainer.Resolve<IStoreBusinessFactory>();
			this._battlepassRewardComponent.SetStoreBusinessFactory(storeBusinessFactory);
			this._unlockItens = this._battlepassRewardComponent.RegisterRewardView(this);
			this._nextRewardButton.interactable = false;
			this._claimAllRewardsButton.interactable = false;
			this._itemCountText = string.Format("{0} {1}", Language.Get("BATTLEPASS_REWARDSCREEN_ITEM", TranslationContext.Battlepass), "{0} / {1}");
			if (this._useMockData)
			{
				this.GetData();
				return;
			}
			this._indexUnlockItens = 0;
			this._maxUnlockItens = this._unlockItens.Itens.Count;
		}

		private void SetupClaimAllButton()
		{
			int num = this._maxUnlockItens - this._indexUnlockItens;
			bool interactable = num > 1;
			this._claimAllRewardsButton.interactable = interactable;
		}

		private void OnDestroy()
		{
			this._battlepassRewardComponent.OnRewardWindowDispose();
		}

		private void SetupRewards(UnityUIBattlepassRewardView.DataPreview data)
		{
			this.SetupTicktInfo(data);
			ProgressionInfo.RewardKind kind = data.Kind;
			if (kind != 1)
			{
				if (kind != 4)
				{
					this._itemFameAmount.gameObject.SetActive(false);
				}
				else
				{
					this.SetupFameLabel(data.Fame, false);
				}
			}
			else
			{
				this.SetupFameLabel(data.Fame, true);
			}
			this.EnableArtPreviewComponentByKind(data.ArtAssetKind);
			this.SetupArtPreview(data);
			if (data.IsExchange)
			{
				this._exchangeFameAmount.text = data.Fame.ToString();
				this.AnimateFeedbackDuplicateItem();
			}
			this.SetupRewardsIndex();
		}

		private void SetupRewardsIndex()
		{
			string format = Language.Format(this._itemCountText, new object[]
			{
				"{0}<color=#{1}>",
				"{2}</color>"
			});
			int num = Mathf.Min(this._indexUnlockItens + 1, this._maxUnlockItens);
			this._itensLabel.text = Language.Format(format, new object[]
			{
				num,
				HudUtils.RGBToHex(this._normalTextColor),
				this._maxUnlockItens
			});
		}

		private void SetupTicktInfo(UnityUIBattlepassRewardView.DataPreview data)
		{
			this._itemDescription.text = Language.Get(data.TitleText, TranslationContext.Items);
			this._itemLevelText.text = (data.Level + 1).ToString();
			this._ticketIcon.texture = ((!data.IsPremium) ? this._tickets.FreeTickeTexture : this._tickets.PremiumTickeTexture);
			this._ticketText.text = ((!data.IsPremium) ? Language.Get(this._tickets.DraftFreeTicket, TranslationContext.BattlepassMissions) : Language.Get(this._tickets.DraftPremiumTicket, TranslationContext.BattlepassMissions));
			this._ticketText.color = ((!data.IsPremium) ? this._tickets.FreeTicketTextColor : this._tickets.PremiumTicketTextColor);
		}

		private void SetupArtPreview(UnityUIBattlepassRewardView.DataPreview data)
		{
			UnityUiBattlepassArtPreview.ArtPreviewData artPreviewData = new UnityUiBattlepassArtPreview.ArtPreviewData
			{
				TitleText = data.TitleText,
				DescriptionText = data.DescriptionText,
				ShowCurrencyIcon = (data.Fame > 0),
				RewardAssetName = data.ArtAssetName,
				RewardAssetKind = data.ArtAssetKind,
				LoreData = new UnityUiBattlepassArtPreview.ArtPreviewLoreData
				{
					IsLocked = false,
					TitleText = data.LoreTitle,
					SubtitleText = data.LoreSubtitle,
					DescriptionText = data.LoreDescription
				}
			};
			this._artPreview.ShowReward(artPreviewData, false);
		}

		private void SetupFameLabel(int amount, bool isSoft)
		{
			this._itemFameAmount.gameObject.SetActive(true);
			this._itemFameAmount.text = amount.ToString();
			if (isSoft)
			{
				this._itemGradientFameAmount.SetColors(this._currencieLabelsConfig.SoftGradientTopColor, this._currencieLabelsConfig.SoftGradientBottomColor);
				this._itemOutlineFameAmount.effectColor = this._currencieLabelsConfig.SoftOutlineColor;
				return;
			}
			this._itemGradientFameAmount.SetColors(this._currencieLabelsConfig.HardGradientTopColor, this._currencieLabelsConfig.HardGradientBottomColor);
			this._itemOutlineFameAmount.effectColor = this._currencieLabelsConfig.HardOutlineColor;
		}

		public void SetVisibility(bool isVisible)
		{
			if (isVisible)
			{
				this._rewardDetail.SetActive(true);
				this._mainWindowCanvas.enabled = true;
				this._mainWindowCanvasGroup.interactable = true;
				this._mainWindowAnimation.Play("CollectRewardAnimation_in");
				this.SetupRewardsIndex();
				base.StartCoroutine(this.WaitRewardWindowInEnd());
				this._isVisible = true;
				UnityUIBattlepassRewardView.Log.Debug("Open Reward Window");
				return;
			}
			this._rewardDetail.SetActive(false);
			this._mainWindowCanvasGroup.interactable = false;
			this._mainWindowCanvas.enabled = false;
			this._isVisible = false;
			UnityUIBattlepassRewardView.Log.Debug("Close Reward Window");
			this._battlepassRewardComponent.HideRewardWindow();
			this._hideSubject.OnNext(Unit.Default);
			this._hideSubject.OnCompleted();
		}

		public void OnButtonNextReward()
		{
			this.DisplayNextReward();
		}

		public void OnButtonClaimAllRewards()
		{
			this.ClaimAllRewards();
		}

		private IEnumerator WaitRewardWindowInEnd()
		{
			UnityUIBattlepassRewardView.DataPreview unlockItem = this._unlockItens.Itens[this._indexUnlockItens];
			this.SetupTicktInfo(unlockItem);
			this.SetupRewardsIndex();
			float reduceAnimTime = this._reduceTimeForAnimationWindowIn;
			if (unlockItem.IsExchange)
			{
				reduceAnimTime = 0f;
			}
			yield return new WaitForSeconds(this._mainWindowAnimation.GetClip("CollectRewardAnimation_in").length - reduceAnimTime);
			this.SetupRewards(unlockItem);
			yield return new WaitForSeconds(reduceAnimTime);
			this._nextRewardButton.interactable = true;
			this.SetupClaimAllButton();
			UnityUIBattlepassRewardView.Log.Debug("Show rewards to Claim");
			yield break;
		}

		public bool IsVisible()
		{
			return this._isVisible;
		}

		public IObservable<Unit> ObserveHide()
		{
			return this._hideSubject;
		}

		private void DisplayNextReward()
		{
			UnityUIBattlepassRewardView.Log.Debug("Claim reward click");
			int num = Mathf.Min(this._indexUnlockItens, this._maxUnlockItens - 1);
			this.ResetDuplicateGroup(num);
			if (this._mainWindowAnimation.IsPlaying("ClaimRewardFeedbackIndividual"))
			{
				this._nextRewardButton.interactable = false;
				this._claimAllRewardsButton.interactable = false;
				GUIUtils.PlayAnimation(this._mainWindowAnimation, false, 2f, "ClaimRewardFeedbackIndividual");
				return;
			}
			this._battlepassRewardComponent.ClaimReward(this._unlockItens.Itens[num].Level, this._unlockItens.Itens[num].IsPremium);
			this._indexUnlockItens++;
			this._mainWindowAnimation.Play("ClaimRewardFeedbackIndividual");
			this.CheckForNextReward();
		}

		private void ClaimAllRewards()
		{
			UnityUIBattlepassRewardView.Log.Debug("Claim all rewards click");
			if (this._mainWindowAnimation.IsPlaying("ClaimRewardFeedbackIndividual"))
			{
				this._nextRewardButton.interactable = false;
				this._claimAllRewardsButton.interactable = false;
				GUIUtils.PlayAnimation(this._mainWindowAnimation, false, 2f, "ClaimRewardFeedbackIndividual");
				return;
			}
			if (this._indexUnlockItens == this._maxUnlockItens)
			{
				UnityUIBattlepassRewardView.Log.Debug("ClaimAllRewards clicked but there is no more rewards to receive");
				return;
			}
			this._nextRewardButton.interactable = false;
			this._claimAllRewardsButton.interactable = false;
			List<ClaimRewardInfo> remainingRewardsIndexesToClaim = this.GetRemainingRewardsIndexesToClaim();
			this._indexUnlockItens = this._maxUnlockItens;
			this._battlepassRewardComponent.ClaimAllRewards(remainingRewardsIndexesToClaim, new Action(this.OnClaimAllRewardsFinished));
			this._claimAllLoadingAnimation.gameObject.SetActive(true);
			this._genericLoadingBackground.SetActive(true);
		}

		private void OnClaimAllRewardsFinished()
		{
			base.StartCoroutine(this.DisplayClaimAllRewardsFeedback());
		}

		private IEnumerator DisplayClaimAllRewardsFeedback()
		{
			yield return new WaitForSecondsRealtime(1f);
			this._claimAllLoadingAnimation.Play("loading_generic_unityui_out");
			do
			{
				yield return null;
			}
			while (this._claimAllLoadingAnimation.IsPlaying("loading_generic_unityui_out"));
			this._claimAllLoadingAnimation.gameObject.SetActive(false);
			this._genericLoadingBackground.SetActive(false);
			this.ResetAnimationSpeed();
			this._mainWindowAnimation.Play("ClaimRewardFeedbackIndividual");
			this.ClaimRewardsEnd();
			yield break;
		}

		private List<ClaimRewardInfo> GetRemainingRewardsIndexesToClaim()
		{
			List<ClaimRewardInfo> list = new List<ClaimRewardInfo>();
			while (this._indexUnlockItens < this._maxUnlockItens)
			{
				ClaimRewardInfo item = new ClaimRewardInfo
				{
					IsPremium = this._unlockItens.Itens[this._indexUnlockItens].IsPremium,
					ClaimIndex = this._unlockItens.Itens[this._indexUnlockItens].Level
				};
				list.Add(item);
				this._indexUnlockItens++;
			}
			return list;
		}

		private void ResetDuplicateGroup(int oldItemIndex)
		{
			if (this._unlockItens.Itens[oldItemIndex].IsExchange)
			{
				this.DisableItemRewardGroup();
				this._mainWindowAnimation.Rewind("FeedbackDuplicatedIten");
				this._mainWindowAnimation.Sample();
				this._mainWindowAnimation.Stop("FeedbackDuplicatedIten");
				return;
			}
			this._duplicateItemGroup.SetActive(false);
		}

		private void CheckForNextReward()
		{
			if (this._indexUnlockItens >= this._maxUnlockItens)
			{
				this.ClaimRewardsEnd();
				return;
			}
			base.StartCoroutine(this.WaitClaimRewardIndividual());
		}

		private void ClaimRewardsEnd()
		{
			if (!this._unlockItens.Itens[this._maxUnlockItens - 1].IsExchange)
			{
				this._duplicateItemGroup.gameObject.SetActive(false);
			}
			this._nextRewardButton.interactable = false;
			this._claimAllRewardsButton.interactable = false;
			this._mainWindowAnimation.Play("ClaimRewardFeedback");
			base.StartCoroutine(this.WaitClaimAnimToHideWindow());
		}

		private IEnumerator WaitClaimRewardIndividual()
		{
			yield return new WaitForSeconds(this._mainWindowAnimation.GetClip("ClaimRewardFeedbackIndividual").length);
			this._nextRewardButton.interactable = true;
			this.SetupClaimAllButton();
			this.ResetAnimationSpeed();
			this.DisableItemRewardGroup();
			int itemIndexCap = Mathf.Min(this._indexUnlockItens, this._maxUnlockItens - 1);
			GUIUtils.ResetAnimation(this._artPreviewAnimation);
			this.SetupRewards(this._unlockItens.Itens[itemIndexCap]);
			yield break;
		}

		private void ResetAnimationSpeed()
		{
			foreach (object obj in this._mainWindowAnimation)
			{
				AnimationState animationState = (AnimationState)obj;
				animationState.speed = 1f;
			}
		}

		private IEnumerator WaitClaimAnimToHideWindow()
		{
			yield return new WaitForSeconds(this._mainWindowAnimation.GetClip("ClaimRewardFeedback").length);
			this.ResetAnimationSpeed();
			UnityUIBattlepassRewardView.Log.Debug("End Reward Window");
			this._battlepassRewardComponent.HideRewardWindow();
			yield break;
		}

		private void GetData()
		{
			this._unlockItens = this._previewMock;
			this._maxUnlockItens = this._unlockItens.Itens.Count;
			this._indexUnlockItens = 0;
		}

		private void AnimateFeedbackDuplicateItem()
		{
			this._duplicateItemGroup.SetActive(true);
			this._mainWindowAnimation.Play("FeedbackDuplicatedIten");
		}

		private void DisableItemRewardGroup()
		{
			this._smallIlustraGroup.gameObject.SetActive(false);
			this._ilustraGroup.gameObject.SetActive(false);
			this._videoGroup.gameObject.SetActive(false);
			this._3DItemImage.enabled = false;
		}

		private void EnableArtPreviewComponentByKind(ItemPreviewKind kind)
		{
			switch (kind)
			{
			case ItemPreviewKind.Sprite:
			case ItemPreviewKind.Lore:
				this._ilustraGroup.gameObject.SetActive(true);
				break;
			case ItemPreviewKind.Model3D:
				this._3DItemImage.enabled = true;
				break;
			case ItemPreviewKind.Video:
				this._videoGroup.gameObject.SetActive(true);
				break;
			case ItemPreviewKind.SmallSprite:
				this._smallIlustraGroup.gameObject.SetActive(true);
				break;
			}
		}

		private static readonly BitLogger Log = new BitLogger(typeof(UnityUIBattlepassRewardView));

		[Header("[Infra]")]
		[SerializeField]
		private BattlepassRewardComponent _battlepassRewardComponentAsset;

		private IBattlepassRewardComponent _battlepassRewardComponent;

		[Header("[Main UI Components]")]
		[SerializeField]
		private GameObject _rewardDetail;

		[SerializeField]
		private GameObject _rewardGroup;

		[SerializeField]
		private GameObject _duplicateItemGroup;

		[SerializeField]
		private GameObject _smallIlustraGroup;

		[SerializeField]
		private GameObject _genericLoadingBackground;

		[SerializeField]
		private GameObject _ilustraGroup;

		[SerializeField]
		private RawImage _3DItemImage;

		[SerializeField]
		private GameObject _videoGroup;

		[SerializeField]
		private Canvas _mainWindowCanvas;

		[SerializeField]
		private CanvasGroup _mainWindowCanvasGroup;

		[SerializeField]
		private Animation _mainWindowAnimation;

		[SerializeField]
		private Animation _artPreviewAnimation;

		[SerializeField]
		private Animation _claimAllLoadingAnimation;

		[SerializeField]
		private Button _nextRewardButton;

		[SerializeField]
		private Button _claimAllRewardsButton;

		[SerializeField]
		private Text _itensLabel;

		[SerializeField]
		private UnityUiBattlepassArtPreview _artPreview;

		[SerializeField]
		private Text _itemDescription;

		[SerializeField]
		private Text _itemLevelText;

		[SerializeField]
		private RawImage _ticketIcon;

		[SerializeField]
		private Text _ticketText;

		[SerializeField]
		private Text _exchangeFameAmount;

		[SerializeField]
		private Text _itemFameAmount;

		[SerializeField]
		private HmmUiGradient _itemGradientFameAmount;

		[SerializeField]
		private Outline _itemOutlineFameAmount;

		[Header("[Config]")]
		[SerializeField]
		private Color _normalTextColor;

		[SerializeField]
		private float _reduceTimeForAnimationWindowIn = 0.3f;

		[SerializeField]
		private UnityUIBattlepassRewardView.TicketsData _tickets;

		[SerializeField]
		private UnityUIBattlepassRewardView.CurrencieLabelsConfig _currencieLabelsConfig;

		private const string DRAFT_ITEM_TEXT = "{0} / {1}";

		private string _itemCountText;

		private int _indexUnlockItens;

		private int _maxUnlockItens;

		private UnityUIBattlepassRewardView.DataReward _unlockItens;

		[Header("[Hack Test]")]
		[SerializeField]
		private bool _useMockData;

		[SerializeField]
		private UnityUIBattlepassRewardView.DataReward _previewMock;

		[InjectOnClient]
		private DiContainer _diContainer;

		private bool _isVisible;

		private ISubject<Unit> _hideSubject;

		[Serializable]
		public struct DataPreview
		{
			public int Level;

			public ItemPreviewKind ArtAssetKind;

			public string ArtAssetName;

			public int Fame;

			public bool IsExchange;

			public string TitleText;

			public string DescriptionText;

			public bool IsPremium;

			public string LoreTitle;

			public string LoreSubtitle;

			public string LoreDescription;

			public ProgressionInfo.RewardKind Kind;
		}

		[Serializable]
		private struct TicketsData
		{
			public Texture FreeTickeTexture;

			public Texture PremiumTickeTexture;

			public string DraftFreeTicket;

			public string DraftPremiumTicket;

			public Color FreeTicketTextColor;

			public Color PremiumTicketTextColor;
		}

		[Serializable]
		private struct CurrencieLabelsConfig
		{
			public Color SoftGradientTopColor;

			public Color SoftGradientBottomColor;

			public Color SoftOutlineColor;

			public Color HardGradientTopColor;

			public Color HardGradientBottomColor;

			public Color HardOutlineColor;
		}

		[Serializable]
		public struct DataReward
		{
			public List<UnityUIBattlepassRewardView.DataPreview> Itens;
		}
	}
}
