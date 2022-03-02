using System;
using ClientAPI.Objects;
using HeavyMetalMachines.Arena;
using HeavyMetalMachines.Infra.Context;
using HeavyMetalMachines.Match;
using HeavyMetalMachines.ParentalControl.Restrictions;
using HeavyMetalMachines.Publishing.Presenting;
using HeavyMetalMachines.Utils;
using HeavyMetalMachines.VFX;
using Pocketverse;
using UniRx;
using UnityEngine;
using Zenject;

namespace HeavyMetalMachines.Frontend
{
	public class HudScoreController : HudWindow
	{
		public void Awake()
		{
			base.SetWindowVisibility(false);
			this.ApplyArenaConfig();
		}

		public void Start()
		{
			GameHubBehaviour.Hub.BombManager.ListenToPhaseChange += this.BombManagerOnPhaseChange;
			this._currentMatchState = GameHubBehaviour.Hub.BombManager.CurrentBombGameState;
			this.UpdateMatchScore();
			this.LeftTeamGameObject.SetActive(false);
			this.RightTeamGameObject.SetActive(false);
			this.SetGroupTeamInfo(TeamKind.Blue);
			this.SetGroupTeamInfo(TeamKind.Red);
		}

		private void ApplyArenaConfig()
		{
			TeamKind currentPlayerTeam = GameHubBehaviour.Hub.Players.CurrentPlayerTeam;
			IGameArenaInfo currentArena = GameHubBehaviour.Hub.ArenaConfig.GetCurrentArena();
			this._roundDurationMillis = (int)(currentArena.RoundTimeSeconds * 1000f);
			TimeSpan timeSpan = new TimeSpan(0, 0, 0, 0, this._roundDurationMillis);
			bool flag = currentPlayerTeam == currentArena.TugOfWarFlipTeam;
			this.SetAllyAndEnemySides(flag);
			if (flag)
			{
				this.FlipTeamsColorsAndTextures();
			}
		}

		private void FlipTeamsColorsAndTextures()
		{
			Color color = this.LeftColorSprites[0].color;
			Color color2 = this.RightColorSprites[0].color;
			Sprite sprite2D = this.LeftColorFrames[0].sprite2D;
			Sprite sprite2D2 = this.RightColorFrames[0].sprite2D;
			for (int i = 0; i < this._allyHudComponents.ScoreSprites.Length; i++)
			{
				this._allyHudComponents.ScoreSprites[i].color = color;
				this._allyHudComponents.ScoreSprites[i].alpha = 0f;
				this._allyHudComponents.ScoreFrames[i].sprite2D = sprite2D;
				this._enemyHudComponents.ScoreSprites[i].color = color2;
				this._enemyHudComponents.ScoreSprites[i].alpha = 0f;
				this._enemyHudComponents.ScoreFrames[i].sprite2D = sprite2D2;
			}
			color = this.LeftScoreLabel.gradientTop;
			color2 = this.RightScoreLabel.gradientTop;
			this._allyHudComponents.ScoreLabel.gradientTop = color;
			this._enemyHudComponents.ScoreLabel.gradientTop = color2;
			color = this.LeftScoreLabel.gradientBottom;
			color2 = this.RightScoreLabel.gradientBottom;
			this._allyHudComponents.ScoreLabel.gradientBottom = color;
			this._enemyHudComponents.ScoreLabel.gradientBottom = color2;
			color = this.LeftScoreLabel.effectColor;
			color2 = this.RightScoreLabel.effectColor;
			this._allyHudComponents.ScoreLabel.effectColor = color;
			this._enemyHudComponents.ScoreLabel.effectColor = color2;
			color = this.LeftScoreFrame.color;
			color2 = this.RightScoreFrame.color;
			this._allyHudComponents.ScoreLabelFrame.color = color;
			this._enemyHudComponents.ScoreLabelFrame.color = color2;
			color = this.LeftTeamGlowSprite.color;
			color2 = this.RightTeamGlowSprite.color;
			this._allyHudComponents.TeamGlowSprite.color = color;
			this._enemyHudComponents.TeamGlowSprite.color = color2;
			color = this.LeftTeamIconBase.color;
			color2 = this.RightTeamIconBase.color;
			this._allyHudComponents.TeamIconBase.color = color;
			this._enemyHudComponents.TeamIconBase.color = color2;
		}

		private void SetAllyAndEnemySides(bool allyTeamOnRightSide)
		{
			if (allyTeamOnRightSide)
			{
				this._allyHudComponents = new HudScoreController.HudScoreTeamComponents
				{
					ScoreSprites = this.RightColorSprites,
					ScoreFrames = this.RightColorFrames,
					ScoreLabel = this.RightScoreLabel,
					ScoreLabelFrame = this.RightScoreFrame,
					TeamGameObject = this.RightTeamGameObject,
					TeamGlowSprite = this.RightTeamGlowSprite,
					TeamIconBase = this.RightTeamIconBase,
					TeamIconSprite = this.RightTeamIconSprite,
					TeamTagLabel = this.RightTeamTagLabel,
					TeamCurrentUgcOwnerPublisherUserNameLabel = this.RightTeamCurrentUgcOwnerPublisherUserNameLabel
				};
				this._enemyHudComponents = new HudScoreController.HudScoreTeamComponents
				{
					ScoreSprites = this.LeftColorSprites,
					ScoreFrames = this.LeftColorFrames,
					ScoreLabel = this.LeftScoreLabel,
					ScoreLabelFrame = this.LeftScoreFrame,
					TeamGameObject = this.LeftTeamGameObject,
					TeamGlowSprite = this.LeftTeamGlowSprite,
					TeamIconBase = this.LeftTeamIconBase,
					TeamIconSprite = this.LeftTeamIconSprite,
					TeamTagLabel = this.LeftTeamTagLabel,
					TeamCurrentUgcOwnerPublisherUserNameLabel = this.LeftTeamCurrentUgcOwnerPublisherUserNameLabel
				};
			}
			else
			{
				this._allyHudComponents = new HudScoreController.HudScoreTeamComponents
				{
					ScoreSprites = this.LeftColorSprites,
					ScoreFrames = this.LeftColorFrames,
					ScoreLabel = this.LeftScoreLabel,
					ScoreLabelFrame = this.LeftScoreFrame,
					TeamGameObject = this.LeftTeamGameObject,
					TeamGlowSprite = this.LeftTeamGlowSprite,
					TeamIconBase = this.LeftTeamIconBase,
					TeamIconSprite = this.LeftTeamIconSprite,
					TeamTagLabel = this.LeftTeamTagLabel,
					TeamCurrentUgcOwnerPublisherUserNameLabel = this.LeftTeamCurrentUgcOwnerPublisherUserNameLabel
				};
				this._enemyHudComponents = new HudScoreController.HudScoreTeamComponents
				{
					ScoreSprites = this.RightColorSprites,
					ScoreFrames = this.RightColorFrames,
					ScoreLabel = this.RightScoreLabel,
					ScoreLabelFrame = this.RightScoreFrame,
					TeamGameObject = this.RightTeamGameObject,
					TeamGlowSprite = this.RightTeamGlowSprite,
					TeamIconBase = this.RightTeamIconBase,
					TeamIconSprite = this.RightTeamIconSprite,
					TeamTagLabel = this.RightTeamTagLabel,
					TeamCurrentUgcOwnerPublisherUserNameLabel = this.RightTeamCurrentUgcOwnerPublisherUserNameLabel
				};
			}
		}

		private void SetGroupTeamInfo(TeamKind teamKind)
		{
			Team groupTeam = this._teams.GetGroupTeam(teamKind);
			if (groupTeam == null)
			{
				return;
			}
			string anyTeamTagRestriction = this._teamNameRestriction.GetAnyTeamTagRestriction(groupTeam.CurrentUgmUserPlayerId, groupTeam.Tag);
			if (GameHubBehaviour.Hub.Players.CurrentPlayerData.Team == teamKind)
			{
				this._allyHudComponents.TeamIconSprite.SpriteName = groupTeam.ImageUrl;
				this._allyHudComponents.TeamTagLabel.text = string.Format("[{0}]", anyTeamTagRestriction);
				this._allyHudComponents.TeamGameObject.SetActive(true);
				this.FetchAndFillTeamUserGeneratedContentPublisherUserName(groupTeam, this._allyHudComponents.TeamCurrentUgcOwnerPublisherUserNameLabel);
			}
			else
			{
				this._enemyHudComponents.TeamIconSprite.SpriteName = groupTeam.ImageUrl;
				this._enemyHudComponents.TeamTagLabel.text = string.Format("[{0}]", anyTeamTagRestriction);
				this._enemyHudComponents.TeamGameObject.SetActive(true);
				this.FetchAndFillTeamUserGeneratedContentPublisherUserName(groupTeam, this._enemyHudComponents.TeamCurrentUgcOwnerPublisherUserNameLabel);
			}
		}

		public override void OnDestroy()
		{
			base.OnDestroy();
			GameHubBehaviour.Hub.BombManager.ListenToPhaseChange -= this.BombManagerOnPhaseChange;
			this._disposables.Dispose();
		}

		private void BombManagerOnPhaseChange(BombScoreboardState bombScoreBoardState)
		{
			this._currentMatchState = bombScoreBoardState;
			this.UpdateMatchScore();
		}

		private void UpdateMatchScore()
		{
			int num = GameHubBehaviour.Hub.BombManager.ScoreBoard.BombScoreRed;
			int num2 = GameHubBehaviour.Hub.BombManager.ScoreBoard.BombScoreBlue;
			if (GameHubBehaviour.Hub.Players.CurrentPlayerData.Team == TeamKind.Blue)
			{
				num = GameHubBehaviour.Hub.BombManager.ScoreBoard.BombScoreBlue;
				num2 = GameHubBehaviour.Hub.BombManager.ScoreBoard.BombScoreRed;
			}
			this._allyHudComponents.ScoreLabel.text = StringCaches.NonPaddedIntegers.Get(num);
			this._enemyHudComponents.ScoreLabel.text = StringCaches.NonPaddedIntegers.Get(num2);
			for (int i = 0; i < this._allyHudComponents.ScoreSprites.Length; i++)
			{
				this._allyHudComponents.ScoreSprites[i].alpha = ((num <= i) ? 0f : 1f);
				this._enemyHudComponents.ScoreSprites[i].alpha = ((num2 <= i) ? 0f : 1f);
			}
		}

		private void FetchAndFillTeamUserGeneratedContentPublisherUserName(Team team, UILabel label)
		{
			label.text = string.Empty;
			ObservableExtensions.Subscribe<string>(Observable.Do<string>(this._getDisplayablePublisherUserName.GetAsTeamUgcOwner(team.CurrentUgmUserUniversalId), delegate(string displayablePublisherUserName)
			{
				label.Text = displayablePublisherUserName;
			}));
		}

		private static readonly BitLogger Log = new BitLogger(typeof(HudScoreController));

		[Inject]
		private IMatchTeams _teams;

		[Inject]
		private ITeamNameRestriction _teamNameRestriction;

		[Inject]
		private IGetDisplayablePublisherUserName _getDisplayablePublisherUserName;

		[Header("[GUI components]")]
		public UI2DSprite[] LeftColorSprites;

		public UI2DSprite[] LeftColorFrames;

		public UILabel LeftScoreLabel;

		public UI2DSprite LeftScoreFrame;

		public UI2DSprite[] RightColorSprites;

		public UI2DSprite[] RightColorFrames;

		public UILabel RightScoreLabel;

		public UI2DSprite RightScoreFrame;

		public GameObject LeftTeamGameObject;

		public HMMUI2DDynamicSprite LeftTeamIconSprite;

		public UI2DSprite LeftTeamGlowSprite;

		public UI2DSprite LeftTeamIconBase;

		public UILabel LeftTeamTagLabel;

		public UILabel LeftTeamCurrentUgcOwnerPublisherUserNameLabel;

		public GameObject RightTeamGameObject;

		public HMMUI2DDynamicSprite RightTeamIconSprite;

		public UI2DSprite RightTeamGlowSprite;

		public UI2DSprite RightTeamIconBase;

		public UILabel RightTeamTagLabel;

		public UILabel RightTeamCurrentUgcOwnerPublisherUserNameLabel;

		private TimeSpan _time;

		private int _seconds;

		private int _roundDurationMillis;

		private BombScoreboardState _currentMatchState;

		private HudScoreController.HudScoreTeamComponents _allyHudComponents;

		private HudScoreController.HudScoreTeamComponents _enemyHudComponents;

		private readonly CompositeDisposable _disposables = new CompositeDisposable();

		public struct HudScoreTeamComponents
		{
			public UI2DSprite[] ScoreSprites;

			public UI2DSprite[] ScoreFrames;

			public UILabel ScoreLabel;

			public UI2DSprite ScoreLabelFrame;

			public GameObject TeamGameObject;

			public UILabel TeamTagLabel;

			public UI2DSprite TeamGlowSprite;

			public UI2DSprite TeamIconBase;

			public HMMUI2DDynamicSprite TeamIconSprite;

			public UILabel TeamCurrentUgcOwnerPublisherUserNameLabel;
		}
	}
}
