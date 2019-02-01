using System;
using System.Collections;
using ClientAPI.Objects;
using FMod;
using HeavyMetalMachines.Audio.Music;
using HeavyMetalMachines.Combat;
using HeavyMetalMachines.Match;
using HeavyMetalMachines.Utils;
using HeavyMetalMachines.VFX;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	public class HudScoreController : HudWindow
	{
		public void Awake()
		{
			this.TimerLabel.text = "00:00";
			this._overtimeCountdownWidgetAlpha = this.OvertimeCountdownAnimation.GetComponent<NGUIWidgetAlpha>();
			base.SetWindowVisibility(false);
			this.ApplyArenaConfig();
		}

		private void OnEnable()
		{
			GameHubBehaviour.Hub.BombManager.ListenToOvertimeStarted += this.OnOvertimeStarted;
		}

		private void OnDisable()
		{
			GameHubBehaviour.Hub.BombManager.ListenToOvertimeStarted -= this.OnOvertimeStarted;
		}

		public void Start()
		{
			GameHubBehaviour.Hub.BombManager.ListenToPhaseChange += this.BombManagerOnPhaseChange;
			this._currentMatchState = GameHubBehaviour.Hub.BombManager.CurrentBombGameState;
			this.UpdateMatchScore();
			this.LeftTeamGameObject.SetActive(false);
			this.RightTeamGameObject.SetActive(false);
			TeamUtils.GetGroupTeamAsync(GameHubBehaviour.Hub, TeamKind.Blue, delegate(Team team)
			{
				this.SetGroupTeamInfo(TeamKind.Blue, team);
			}, delegate(Exception exception)
			{
				HudScoreController.Log.Error(string.Format("Error on GetGroupTeamAsync [{0}]. Exception:{1}", TeamKind.Blue, exception));
			});
			TeamUtils.GetGroupTeamAsync(GameHubBehaviour.Hub, TeamKind.Red, delegate(Team team)
			{
				this.SetGroupTeamInfo(TeamKind.Red, team);
			}, delegate(Exception exception)
			{
				HudScoreController.Log.Error(string.Format("Error on GetGroupTeamAsync [{0}]. Exception:{1}", TeamKind.Red, exception));
			});
			this._isTutorial = GameHubBehaviour.Hub.Match.LevelIsTutorial();
			if (this._isTutorial)
			{
				this._timeTutorialIcon.SetActive(true);
				this.TimerLabel.gameObject.SetActive(false);
			}
		}

		private void ApplyArenaConfig()
		{
			TeamKind currentPlayerTeam = GameHubBehaviour.Hub.Players.CurrentPlayerTeam;
			GameArenaInfo gameArenaInfo = GameHubBehaviour.Hub.ArenaConfig.Arenas[GameHubBehaviour.Hub.Match.ArenaIndex];
			this._roundDurationMillis = (int)(gameArenaInfo.RoundTimeSeconds * 1000f);
			TimeSpan timeSpan = new TimeSpan(0, 0, 0, 0, this._roundDurationMillis);
			this._roundDurationString = TimeUtils.FormatTime(timeSpan);
			bool flag = currentPlayerTeam == gameArenaInfo.TugOfWarFlipTeam;
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
					TeamTagLabel = this.RightTeamTagLabel
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
					TeamTagLabel = this.LeftTeamTagLabel
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
					TeamTagLabel = this.LeftTeamTagLabel
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
					TeamTagLabel = this.RightTeamTagLabel
				};
			}
		}

		private void SetGroupTeamInfo(TeamKind teamKind, Team team)
		{
			if (team == null)
			{
				return;
			}
			if (GameHubBehaviour.Hub.Players.CurrentPlayerData.Team == teamKind)
			{
				this._allyHudComponents.TeamIconSprite.SpriteName = team.ImageUrl;
				this._allyHudComponents.TeamTagLabel.text = string.Format("[{0}]", team.Tag);
				this._allyHudComponents.TeamGameObject.SetActive(true);
			}
			else
			{
				this._enemyHudComponents.TeamIconSprite.SpriteName = team.ImageUrl;
				this._enemyHudComponents.TeamTagLabel.text = string.Format("[{0}]", team.Tag);
				this._enemyHudComponents.TeamGameObject.SetActive(true);
			}
		}

		public override void OnDestroy()
		{
			base.OnDestroy();
			GameHubBehaviour.Hub.BombManager.ListenToPhaseChange -= this.BombManagerOnPhaseChange;
		}

		public void Update()
		{
			if (GameHubBehaviour.Hub == null || GameHubBehaviour.Hub.MatchMan == null)
			{
				return;
			}
			this.TimerUpdate();
		}

		private void BombManagerOnPhaseChange(BombScoreBoard.State bombScoreBoardState)
		{
			this._currentMatchState = bombScoreBoardState;
			this.UpdateMatchScore();
			BombScoreBoard.State currentMatchState = this._currentMatchState;
			if (currentMatchState != BombScoreBoard.State.PreReplay)
			{
				if (currentMatchState != BombScoreBoard.State.Replay)
				{
					if (currentMatchState == BombScoreBoard.State.PreBomb)
					{
						this._isInOvertime = false;
						this.TimerLabel.text = this._roundDurationString;
					}
				}
				else
				{
					this._overtimeCountdownWidgetAlpha.alpha = 1f;
					this.TimerLabel.text = "00:00";
				}
			}
			else
			{
				this.UpdateMatchScore();
				if (!this._hideFillAnimationScheduled)
				{
					this.HideOvertimeFillAnimation();
				}
			}
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
			this._allyHudComponents.ScoreLabel.text = num.ToString();
			this._enemyHudComponents.ScoreLabel.text = num2.ToString();
			for (int i = 0; i < this._allyHudComponents.ScoreSprites.Length; i++)
			{
				this._allyHudComponents.ScoreSprites[i].alpha = ((num <= i) ? 0f : 1f);
				this._enemyHudComponents.ScoreSprites[i].alpha = ((num2 <= i) ? 0f : 1f);
			}
		}

		private void TimerUpdate()
		{
			if (this._isInOvertime || this._currentMatchState != BombScoreBoard.State.BombDelivery || this._isTutorial)
			{
				return;
			}
			int num = GameHubBehaviour.Hub.GameTime.MatchTimer.GetTime();
			int num2 = num - GameHubBehaviour.Hub.BombManager.ScoreBoard.RoundStartTimeMillis;
			if (num2 >= this._roundDurationMillis)
			{
				num = 0;
			}
			else
			{
				num = this._roundDurationMillis - num2;
			}
			this._time = TimeSpan.FromMilliseconds((double)num);
			if (this._seconds != this._time.Seconds)
			{
				this._seconds = this._time.Seconds;
				this.TimerLabel.text = TimeUtils.FormatTime(this._time);
				if (this._time.Minutes == 0 && this._seconds >= 0 && this._seconds < 10 && this._countdownAudioAsset != null)
				{
					FMODAudioManager.PlayOneShotAt(this._countdownAudioAsset, Vector3.zero, 0);
				}
			}
			GameArenaInfo currentArena = GameHubBehaviour.Hub.ArenaConfig.GetCurrentArena();
			if (num < currentArena.TimeBeforeOvertime && num > 0)
			{
				if (this.OvertimeFillAnimation.gameObject.activeSelf)
				{
					float x = Mathf.Clamp01((float)num / 10000f);
					this.OvertimeFillSprite.transform.localScale = new Vector3(x, 1f, 1f);
				}
				else
				{
					this._hideFillAnimationScheduled = false;
					this.OvertimeFillAnimation.gameObject.SetActive(true);
					this.OvertimeFillSprite.transform.localScale = Vector3.one;
					this.OvertimeFillAnimation.Play("PreOvertimeInAnimation");
					this.OvertimeCountdownAnimation.Play();
					MusicManager.PlayMusic(MusicManager.State.PreOvertime);
				}
			}
		}

		private void HideOvertimeFillAnimation()
		{
			if (this._hideFillAnimationScheduled)
			{
				return;
			}
			this._hideFillAnimationScheduled = true;
			this.OvertimeFillAnimation.Stop();
			this.OvertimeFillAnimation.Play("PreOvertimeOutAnimation");
			float length = this.OvertimeFillAnimation["PreOvertimeOutAnimation"].length;
			this.OvertimeCountdownAnimation.Stop();
			this.OvertimeCountdownAnimation.transform.localScale = Vector3.one;
			this._overtimeCountdownWidgetAlpha.alpha = 0f;
			base.StartCoroutine(this.HideOvertimeFillAnimationCoroutine(length));
		}

		private IEnumerator HideOvertimeFillAnimationCoroutine(float delay)
		{
			yield return new WaitForSeconds(delay);
			this.OvertimeFillAnimation.gameObject.SetActive(false);
			yield break;
		}

		private void OnOvertimeStarted()
		{
			this._isInOvertime = true;
			this.HideOvertimeFillAnimation();
		}

		private static readonly BitLogger Log = new BitLogger(typeof(HudScoreController));

		[Header("[GUI components]")]
		public UILabel TimerLabel;

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

		public GameObject RightTeamGameObject;

		public HMMUI2DDynamicSprite RightTeamIconSprite;

		public UI2DSprite RightTeamGlowSprite;

		public UI2DSprite RightTeamIconBase;

		public UILabel RightTeamTagLabel;

		public UI2DSprite OvertimeFillSprite;

		public Animation OvertimeFillAnimation;

		public Animation OvertimeCountdownAnimation;

		private NGUIWidgetAlpha _overtimeCountdownWidgetAlpha;

		private bool _hideFillAnimationScheduled;

		private TimeSpan _time;

		private int _seconds;

		private int _roundDurationMillis;

		private string _roundDurationString;

		private bool _isInOvertime;

		private BombScoreBoard.State _currentMatchState;

		private bool _isTutorial;

		[SerializeField]
		private GameObject _timeTutorialIcon;

		[SerializeField]
		private FMODAsset _countdownAudioAsset;

		private HudScoreController.HudScoreTeamComponents _allyHudComponents;

		private HudScoreController.HudScoreTeamComponents _enemyHudComponents;

		private const string TIME_ZERO = "00:00";

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
		}
	}
}
