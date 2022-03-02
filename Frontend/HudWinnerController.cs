using System;
using System.Collections;
using System.Collections.Generic;
using Assets.Standard_Assets.Scripts.HMM.PlotKids;
using ClientAPI.Objects;
using FMod;
using HeavyMetalMachines.Audio.Music;
using HeavyMetalMachines.DataTransferObjects.Battlepass;
using HeavyMetalMachines.DataTransferObjects.Player;
using HeavyMetalMachines.DataTransferObjects.Progression;
using HeavyMetalMachines.Match;
using HeavyMetalMachines.ParentalControl.Restrictions;
using HeavyMetalMachines.Players.Presenting;
using HeavyMetalMachines.PostProcessing;
using HeavyMetalMachines.Publishing.Presenting;
using HeavyMetalMachines.Utils;
using HeavyMetalMachines.VFX;
using Pocketverse;
using UniRx;
using UnityEngine;
using Zenject;

namespace HeavyMetalMachines.Frontend
{
	public class HudWinnerController : HudWindow
	{
		public void Awake()
		{
			TeamKind teamKind = (GameHubBehaviour.Hub.Match.State != MatchData.MatchState.MatchOverRedWins) ? TeamKind.Blue : TeamKind.Red;
			List<PlayerData> playersAndBots = GameHubBehaviour.Hub.Players.PlayersAndBots;
			int num = 0;
			for (int i = 0; i < playersAndBots.Count; i++)
			{
				if (playersAndBots[i].Team == teamKind)
				{
					num++;
				}
			}
			GUIUtils.CreateGridPoolInjected(this.WinnersGrid, this.HudWinnerObjectReference.gameObject, num, this._diContainer);
			this._backgroundEffect = new HudWinnerBackgroundEffect(this.HudWinnerBackgroundEffectParameters, this._postProcessing);
			base.ChangeWindowVisibility(false);
			this.waitForDelayToShowCards = new WaitForSeconds(this.WindowDelayToShowCards);
			this.waitForOpenCardAnimationInSec = new WaitForSeconds(this.WindowOpenCardAnimationInSec);
			this.waitForVisibleTimeInSec = new WaitForSeconds(this.WindowVisibleTimeInSec);
			this.waitForCloseTimeInSec = new WaitForSeconds(this.WindowCloseTimeInSec);
			this._teamGui.MainGroupGameObject.SetActive(false);
			this.count = 0f;
		}

		public override void OnDestroy()
		{
			base.OnDestroy();
			this._backgroundEffect = null;
		}

		public override void ChangeWindowVisibility(bool visible)
		{
			base.ChangeWindowVisibility(visible);
			if (visible)
			{
				this._backgroundEffect.Start(() => this.IsVisible);
				base.StartCoroutine(this.ShowWindowAsync());
			}
			else
			{
				this._backgroundEffect.Finish();
			}
		}

		public void Update()
		{
			if (this.IsVisible)
			{
				this._backgroundEffect.Update();
			}
		}

		private IEnumerator ShowWindowAsync()
		{
			this.Setup();
			FMODAudioManager.PlayAt(this.sfx_ui_matchend_team_characters, base.transform);
			if (!SpectatorController.IsSpectating)
			{
				int characterMusicId = GameHubBehaviour.Hub.Players.CurrentPlayerData.GetCharacterMusicId();
				if (characterMusicId > 0)
				{
					MusicManager.PlayCharacterMusic(characterMusicId);
				}
			}
			yield return base.StartCoroutine(this.RunCardAnimations());
			base.SetWindowVisibility(false);
			yield break;
		}

		private IEnumerator RunCardAnimations()
		{
			if (this.WindowDelayToShowCards > 0f)
			{
				yield return this.waitForDelayToShowCards;
			}
			this.count = GameHubBehaviour.Hub.GameTime.GetPlaybackUnityTime() + 30f;
			List<Transform> winnersGridList = this.WinnersGrid.GetChildList();
			for (int i = 0; i < winnersGridList.Count; i++)
			{
				winnersGridList[i].GetComponent<HudWinnerObject>().PlayAnimationOpen();
				yield return this.waitForOpenCardAnimationInSec;
			}
			yield return this.waitForVisibleTimeInSec;
			if (!SpectatorController.IsSpectating)
			{
				while (GameHubBehaviour.Hub.MatchHistory.CurrentPlayerReward == null && this.count >= GameHubBehaviour.Hub.GameTime.GetPlaybackUnityTime())
				{
					yield return UnityUtils.WaitForEndOfFrame;
				}
				if (this.count < GameHubBehaviour.Hub.GameTime.GetPlaybackUnityTime())
				{
					HudWinnerController.Log.Error("Client did not received PlayerReward");
					RewardsBag rewardsBag = new RewardsBag();
					rewardsBag.MissionProgresses = new MissionProgress[0];
					rewardsBag.OldMissionProgresses = new MissionProgress[0];
					rewardsBag.MissionsCompleted = new MissionCompleted[0];
					GameHubBehaviour.Hub.MatchHistory.OnRewardSet(rewardsBag);
				}
			}
			for (int j = winnersGridList.Count - 1; j >= 0; j--)
			{
				winnersGridList[j].GetComponent<HudWinnerObject>().PlayAnimationClose();
				yield return this.waitForOpenCardAnimationInSec;
			}
			yield return this.waitForCloseTimeInSec;
			base.SetWindowVisibility(false);
			if (this.OnDisableAction != null)
			{
				this.OnDisableAction();
			}
			yield break;
		}

		private void Setup()
		{
			GameHubBehaviour.Hub.CursorManager.ShowAndSetCursor(true, CursorManager.CursorTypes.MatchstatsCursor);
			TeamKind teamKind = (GameHubBehaviour.Hub.Match.State != MatchData.MatchState.MatchOverRedWins) ? TeamKind.Blue : TeamKind.Red;
			bool flag = GameHubBehaviour.Hub.Players.CurrentPlayerData.Team == teamKind;
			this._windowSprites.BackgroundSprite.color = ((!flag) ? this._windowSprites.BackgroundEnemyColor : this._windowSprites.BackgroundAllyColor);
			this._windowSprites.GlowSprite.color = ((!flag) ? this._windowSprites.GlowEnemyColor : this._windowSprites.GlowAllyColor);
			List<PlayerData> playersAndBots = GameHubBehaviour.Hub.Players.PlayersAndBots;
			playersAndBots.Sort(delegate(PlayerData p1, PlayerData p2)
			{
				if (p1.PlayerCarId == GameHubBehaviour.Hub.Players.CurrentPlayerData.PlayerCarId)
				{
					return 1;
				}
				return p1.GridIndex.CompareTo(p2.GridIndex);
			});
			List<Transform> childList = this.WinnersGrid.GetChildList();
			int i = 0;
			int num = 0;
			while (i < playersAndBots.Count)
			{
				PlayerData playerData = playersAndBots[i];
				if (playerData.Team == teamKind)
				{
					HudWinnerObject component = childList[num++].GetComponent<HudWinnerObject>();
					component.gameObject.SetActive(true);
					component.Setup(playerData, this._teams, this._getDisplayableNickName, this._teamNameRestriction);
				}
				i++;
			}
			this.WinnersGrid.Reposition();
			this._teamGui.MainGroupGameObject.SetActive(false);
			Team groupTeam = this._teams.GetGroupTeam(teamKind);
			if (groupTeam == null)
			{
				return;
			}
			string anyTeamTagRestriction = this._teamNameRestriction.GetAnyTeamTagRestriction(groupTeam.CurrentUgmUserPlayerId, groupTeam.Tag);
			this._teamGui.MainGroupGameObject.SetActive(true);
			this._teamGui.TeamImageSprite.SpriteName = groupTeam.ImageUrl;
			this._teamGui.BannerSprite.sprite2D = ((!flag) ? this._teamGui.TeamEnemyBannerSprite : this._teamGui.TeamAllyBannerSprite);
			this._teamGui.TeamTagLabel.text = NGUIText.EscapeSymbols(string.Format("[{0}]", anyTeamTagRestriction));
			this._teamGui.TeamTagLabel.gradientBottom = ((!flag) ? this._windowSprites.LabelTeamEnemyColorBottom : this._windowSprites.LabelTeamAllyColorBottom);
			this._teamGui.TeamTagLabel.gradientTop = ((!flag) ? this._windowSprites.LabelTeamEnemyColorTop : this._windowSprites.LabelTeamAllyColorTop);
			this._teamGui.TeamTagLabel.effectColor = ((!flag) ? this._windowSprites.LabelTeamEnemyColorShadow : this._windowSprites.LabelTeamAllyColorShadow);
			this._teamGui.TeamImageSprite.gradientBottom = ((!flag) ? this._windowSprites.IconTeamEnemyColorBottom : this._windowSprites.IconTeamAllyColorBottom);
			this._teamGui.TeamImageSprite.gradientTop = ((!flag) ? this._windowSprites.IconTeamEnemyColorTop : this._windowSprites.IconTeamAllyColorTop);
			this.FetchAndFillTeamUserGeneratedContentPublisherUserName(groupTeam, this._teamGui.TeamUserGeneratedContentCurrentOwnerPublisherUserNameLabel);
		}

		private void FetchAndFillTeamUserGeneratedContentPublisherUserName(Team team, UILabel label)
		{
			label.text = string.Empty;
			ObservableExtensions.Subscribe<string>(Observable.Do<string>(this._getDisplayablePublisherUserName.GetAsTeamUgcOwner(team.CurrentUgmUserUniversalId), delegate(string displayablePublisherUserName)
			{
				label.Text = displayablePublisherUserName;
			}));
		}

		private static readonly BitLogger Log = new BitLogger(typeof(HudWinnerController));

		[Inject]
		private IMatchTeams _teams;

		[Inject]
		private IGamePostProcessing _postProcessing;

		[Inject]
		private IGetDisplayableNickName _getDisplayableNickName;

		[Inject]
		private ITeamNameRestriction _teamNameRestriction;

		[Inject]
		private DiContainer _diContainer;

		[Inject]
		private IGetDisplayablePublisherUserName _getDisplayablePublisherUserName;

		[Header("[Time Config]")]
		public float WindowDelayToShowCards = 1f;

		public float WindowOpenCardAnimationInSec = 0.5f;

		public float WindowVisibleTimeInSec = 5f;

		public float WindowCloseTimeInSec = 3f;

		[Header("[Audio]")]
		public AudioEventAsset sfx_ui_matchend_team_characters;

		[Header("[Grid Ref]")]
		public UIGrid WinnersGrid;

		public HudWinnerObject HudWinnerObjectReference;

		[SerializeField]
		private HudWinnerController.GuiTeam _teamGui;

		public HudWinnerBackgroundEffect.HudWinnerBackgroundEffectParameres HudWinnerBackgroundEffectParameters;

		public Action OnDisableAction;

		[SerializeField]
		private HudWinnerController.GuiWindowSprites _windowSprites;

		private HudWinnerBackgroundEffect _backgroundEffect;

		private WaitForSeconds waitForDelayToShowCards;

		private WaitForSeconds waitForOpenCardAnimationInSec;

		private WaitForSeconds waitForVisibleTimeInSec;

		private WaitForSeconds waitForCloseTimeInSec;

		private float count;

		[Serializable]
		private struct GuiTeam
		{
			public Sprite TeamAllyBannerSprite;

			public Sprite TeamEnemyBannerSprite;

			public GameObject MainGroupGameObject;

			public UI2DSprite BannerSprite;

			public HMMUI2DDynamicSprite TeamImageSprite;

			public UILabel TeamTagLabel;

			public UILabel TeamUserGeneratedContentCurrentOwnerPublisherUserNameLabel;
		}

		[Serializable]
		private struct GuiWindowSprites
		{
			public Color BackgroundAllyColor;

			public Color BackgroundEnemyColor;

			public Color GlowAllyColor;

			public Color GlowEnemyColor;

			public Color IconTeamAllyColorTop;

			public Color IconTeamAllyColorBottom;

			public Color LabelTeamAllyColorTop;

			public Color LabelTeamAllyColorBottom;

			public Color LabelTeamAllyColorShadow;

			public Color IconTeamEnemyColorTop;

			public Color IconTeamEnemyColorBottom;

			public Color LabelTeamEnemyColorTop;

			public Color LabelTeamEnemyColorBottom;

			public Color LabelTeamEnemyColorShadow;

			public UI2DSprite BackgroundSprite;

			public UI2DSprite GlowSprite;
		}
	}
}
