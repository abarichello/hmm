using System;
using System.Collections;
using Assets.Standard_Assets.Scripts.HMM.Customization;
using HeavyMetalMachines.CompetitiveMode.View.Loading;
using HeavyMetalMachines.Match;
using HeavyMetalMachines.Players.Presenting;
using HeavyMetalMachines.Publishing;
using HeavyMetalMachines.Publishing.Presenting;
using HeavyMetalMachines.VFX;
using Pocketverse;
using UniRx;
using UnityEngine;
using Zenject;

namespace HeavyMetalMachines.Frontend
{
	public class LoadingVersusPlayer : GameHubBehaviour
	{
		public void UpdatePlayerInfo(LoadingVersusController loadingVersusController, PlayerData playerData, bool isFlipped, IMatchTeams teams)
		{
			Guid id = playerData.CharacterItemType.Id;
			string characterLocalizedName = playerData.GetCharacterLocalizedName();
			LoadingVersusPlayer.Log.DebugFormat("Will UpdatePlayerInfo (UI) for player:{0} isBot:{1} char:{2}", new object[]
			{
				playerData.Name,
				playerData.IsBot,
				characterLocalizedName
			});
			if (!playerData.IsBot)
			{
				ObservableExtensions.Subscribe<Unit>(this._diContainer.Resolve<ILoadingVersusPlayerRankPresenter>().LoadRank(playerData.PlayerId, this._loadingVersusPlayerRankView));
			}
			this.CarSprite.SpriteName = HudUtils.GetCarSkinSpriteName(GameHubBehaviour.Hub.InventoryColletion, id, playerData.Customizations.GetGuidBySlot(59));
			this.BorderIconSprite.flip = ((!isFlipped) ? UIBasicSprite.Flip.Nothing : UIBasicSprite.Flip.Horizontally);
			bool flag = GameHubBehaviour.Hub.Players.CurrentPlayerData.Team == playerData.Team;
			this.CharacterNameLabel.text = characterLocalizedName;
			this.CharacterLevelLabel.text = string.Empty;
			this.CharacterIconSprite.SpriteName = HudUtils.GetPlayerIconName(GameHubBehaviour.Hub, id, HudUtils.PlayerIconSize.Size64);
			string text = NGUIText.EscapeSymbols(playerData.Name);
			IGetDisplayableNickName getDisplayableNickName = this._diContainer.Resolve<IGetDisplayableNickName>();
			text = ((!playerData.IsBot) ? getDisplayableNickName.GetFormattedNickNameWithPlayerTag(playerData.PlayerId, text, new long?(playerData.PlayerTag)) : playerData.Name);
			this.PlayerNameLabel.text = text;
			bool flag2 = playerData == GameHubBehaviour.Hub.Players.CurrentPlayerData;
			this.PingGroupGameObject.SetActive(flag2);
			if (flag)
			{
				this.CharacterNameLabel.color = loadingVersusController.AllyTeamColor;
				if (flag2)
				{
					base.StartCoroutine(this.UpdatePingCoroutine(loadingVersusController));
					this.PlayerNameLabel.color = loadingVersusController.CurrentPlayerColor;
				}
				else
				{
					this.PlayerNameLabel.color = loadingVersusController.AllyPlayerNameColor;
				}
			}
			else
			{
				this.CharacterNameLabel.color = loadingVersusController.EnemyTeamColor;
				this.PlayerNameLabel.color = loadingVersusController.EnemyPlayerNameColor;
			}
			if (playerData.IsBot)
			{
				long playerId = GameHubBehaviour.Hub.Players.CurrentPlayerData.PlayerId;
				if (GameHubBehaviour.Hub.Players.Players.Count > 0)
				{
					playerId = GameHubBehaviour.Hub.Players.Players[SysRandom.Int(0, GameHubBehaviour.Hub.Players.Players.Count)].PlayerId;
				}
				base.StartCoroutine(this.UpdateLoadingCoroutine(playerId));
			}
			else if (!playerData.Connected)
			{
				this.UpdateLoadingInfo(1f);
			}
			else
			{
				base.StartCoroutine(this.UpdateLoadingCoroutine(playerData.PlayerId));
			}
			this.BorderIconSprite.sprite2D = loadingVersusController.AllyBorderSprite;
			this.LoadingProgressBarSprite.sprite2D = loadingVersusController.AllyProgressBarSprite;
			if (flag2)
			{
				this.BorderIconSprite.sprite2D = loadingVersusController.CurrentBorderSprite;
				this.LoadingProgressBarSprite.sprite2D = loadingVersusController.CurrentProgressBarSprite;
			}
			else if (!flag)
			{
				this.BorderIconSprite.sprite2D = loadingVersusController.EnemyBorderSprite;
				this.LoadingProgressBarSprite.sprite2D = loadingVersusController.EnemyProgressBarSprite;
			}
			this.LoadingProgressBar.gameObject.SetActive(true);
			PortraitDecoratorGui.UpdatePortraitSprite(playerData.Customizations, this.FounderBoxSprite, PortraitDecoratorGui.PortraitSpriteType.LoadingVersusBox);
			this.UpdatePsnInfo(playerData);
			this._playerNameInfoGrid.Reposition();
		}

		private void UpdatePsnInfo(PlayerData playerData)
		{
			if (playerData.IsBot)
			{
				this._psnIdGroupGameObject.SetActive(false);
				return;
			}
			IGetPublisherPresentingData getPublisherPresentingData = this._diContainer.Resolve<IGetPublisherPresentingData>();
			Publisher publisherById = Publishers.GetPublisherById(playerData.PublisherId);
			PublisherPresentingData publisherPresentingData = getPublisherPresentingData.Get(publisherById);
			this._psnIdGroupGameObject.SetActive(publisherPresentingData.ShouldShowPublisherUserName);
			if (publisherPresentingData.ShouldShowPublisherUserName)
			{
				this._psnIdLabel.text = playerData.PublisherUserName;
			}
		}

		private IEnumerator UpdatePingCoroutine(LoadingVersusController loadingVersusController)
		{
			while (base.gameObject.activeInHierarchy)
			{
				this.UpdatePingInfo(loadingVersusController, Mathf.RoundToInt(GameHubBehaviour.Hub.Net.GetPing()));
				yield return UnityUtils.WaitForEndOfFrame;
			}
			yield break;
		}

		private IEnumerator UpdateLoadingCoroutine(long playerId)
		{
			while (base.gameObject.activeInHierarchy)
			{
				PlayerData currentPlayerData = GameHubBehaviour.Hub.Players.CurrentPlayerData;
				if (currentPlayerData == null)
				{
					break;
				}
				this.UpdateLoadingInfo((playerId != currentPlayerData.PlayerId) ? GameHubBehaviour.Hub.Match.LoadingProgress.GetPlayerProgress(playerId) : 0f);
				yield return UnityUtils.WaitForEndOfFrame;
			}
			yield break;
		}

		private IEnumerator UpdateLoadingFakeCoroutine()
		{
			float tempTime = 0f;
			while (base.gameObject.activeInHierarchy)
			{
				tempTime += Time.deltaTime;
				this.UpdateLoadingInfo(tempTime / (100f + tempTime));
				yield return UnityUtils.WaitForEndOfFrame;
			}
			yield break;
		}

		public void UpdateLoadingInfo(float progressValue)
		{
			this.LoadingProgressBar.value = progressValue;
		}

		public void UpdatePingInfo(LoadingVersusController loadingVersusController, int pingValue)
		{
			this.PingLabel.text = pingValue.ToString();
			Color color = GUIColorsInfo.Instance.LowPingColor;
			if (pingValue > GameHubBehaviour.Hub.GuiScripts.GUIValues.MediumPing)
			{
				color = ((pingValue <= GameHubBehaviour.Hub.GuiScripts.GUIValues.HighPing) ? GUIColorsInfo.Instance.MediumPingColor : GUIColorsInfo.Instance.HighPingColor);
			}
			this.PingSprite.color = color;
			this.PingLabel.color = color;
		}

		private static readonly BitLogger Log = new BitLogger(typeof(LoadingVersusPlayer));

		[Inject]
		private DiContainer _diContainer;

		public HMMUI2DDynamicSprite CarSprite;

		public HMMUI2DDynamicSprite FounderBoxSprite;

		public UILabel CharacterNameLabel;

		public UILabel CharacterLevelLabel;

		public HMMUI2DDynamicSprite CharacterIconSprite;

		public UILabel PlayerNameLabel;

		public UIProgressBar LoadingProgressBar;

		public UI2DSprite LoadingProgressBarSprite;

		public GameObject PingGroupGameObject;

		public UI2DSprite PingSprite;

		public UILabel PingLabel;

		public UI2DSprite BorderIconSprite;

		[SerializeField]
		private NguiLoadingVersusPlayerRankView _loadingVersusPlayerRankView;

		[SerializeField]
		private UIGrid _playerNameInfoGrid;

		[SerializeField]
		private GameObject _psnIdGroupGameObject;

		[SerializeField]
		private UILabel _psnIdLabel;
	}
}
