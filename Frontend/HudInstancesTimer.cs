using System;
using System.Collections;
using FMod;
using HeavyMetalMachines.Match;
using HeavyMetalMachines.Utils;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	public class HudInstancesTimer : GameHubBehaviour
	{
		public void Show(HudInstancesTimer.RoundType roundType)
		{
			if (roundType != HudInstancesTimer.RoundType.First)
			{
				if (roundType != HudInstancesTimer.RoundType.Win)
				{
					if (roundType == HudInstancesTimer.RoundType.Lose)
					{
						this.TitleLabel.text = Language.Get("INSTANCES_ROUNDLOST_STATUS", TranslationSheets.Instances);
						this.TimeTitleLabel.text = Language.Get("INSTANCES_ROUNDLOST_PICK", TranslationSheets.Instances);
					}
				}
				else
				{
					this.TitleLabel.text = Language.Get("INSTANCES_ROUNDWON_STATUS", TranslationSheets.Instances);
					this.TimeTitleLabel.text = Language.Get("INSTANCES_ROUNDWON_PICK", TranslationSheets.Instances);
				}
			}
			else
			{
				this.TitleLabel.text = Language.Get("INSTANCES_STARTMATCH_STATUS", TranslationSheets.Instances);
				this.TimeTitleLabel.text = Language.Get("INSTANCES_STARTMATCH_PICK", TranslationSheets.Instances);
			}
			int num = GameHubBehaviour.Hub.BombManager.ScoreBoard.BombScoreRed;
			int num2 = GameHubBehaviour.Hub.BombManager.ScoreBoard.BombScoreBlue;
			if (GameHubBehaviour.Hub.Players.CurrentPlayerData.Team == TeamKind.Blue)
			{
				num = GameHubBehaviour.Hub.BombManager.ScoreBoard.BombScoreBlue;
				num2 = GameHubBehaviour.Hub.BombManager.ScoreBoard.BombScoreRed;
			}
			TeamKind currentPlayerTeam = GameHubBehaviour.Hub.Players.CurrentPlayerTeam;
			GameArenaInfo gameArenaInfo = GameHubBehaviour.Hub.ArenaConfig.Arenas[GameHubBehaviour.Hub.Match.ArenaIndex];
			bool flag = currentPlayerTeam == gameArenaInfo.TugOfWarFlipTeam;
			if (flag)
			{
				this.RightTeamScoreLabel.text = num.ToString("0");
				this.RightTeamScoreLabel.color = GUIColorsInfo.Instance.BlueTeamColor;
				this.LeftTeamScoreLabel.text = num2.ToString("0");
				this.LeftTeamScoreLabel.color = GUIColorsInfo.Instance.RedTeamColor;
			}
			else
			{
				this.RightTeamScoreLabel.text = num2.ToString("0");
				this.RightTeamScoreLabel.color = GUIColorsInfo.Instance.RedTeamColor;
				this.LeftTeamScoreLabel.text = num.ToString("0");
				this.LeftTeamScoreLabel.color = GUIColorsInfo.Instance.BlueTeamColor;
			}
			this.StartRoundTimeCounter();
			base.gameObject.SetActive(true);
			if (!this._didAnimation)
			{
				this._didAnimation = true;
				GUIUtils.PlayAnimation(this.ScoreInOutAnimation, false, 1f, string.Empty);
			}
		}

		public void Hide()
		{
			this.StopRoundTimeCounter();
			base.gameObject.SetActive(false);
			if (!HudInstancesController.IsInShopState())
			{
				this._didAnimation = false;
			}
		}

		public void HideAnimating()
		{
			this.StopRoundTimeCounter();
			if (!base.gameObject.activeInHierarchy)
			{
				return;
			}
			base.StartCoroutine(this.AnimateHideCoroutine());
		}

		private IEnumerator AnimateHideCoroutine()
		{
			GUIUtils.PlayAnimation(this.ScoreInOutAnimation, true, 1f, string.Empty);
			while (this.ScoreInOutAnimation.isPlaying)
			{
				yield return null;
				if (!base.gameObject.activeInHierarchy)
				{
					yield break;
				}
			}
			this.Hide();
			yield break;
		}

		private void StartRoundTimeCounter()
		{
			this._updateStartRoundTime = true;
			this.TimeToCloseSmallLabel.gameObject.SetActive(false);
		}

		private void StopRoundTimeCounter()
		{
			this._updateStartRoundTime = false;
			this.TimeToCloseLabel.text = "0";
			this.TimeToCloseSmallLabel.text = ".0";
		}

		public float GetScoreBoardRemainingTimeInSec()
		{
			return (float)(GameHubBehaviour.Hub.BombManager.ScoreBoard.Timeout - (long)GameHubBehaviour.Hub.GameTime.GetPlaybackTime()) / 1000f;
		}

		protected void Update()
		{
			if (this._updateStartRoundTime)
			{
				float num = this.GetScoreBoardRemainingTimeInSec();
				if (num < 0f)
				{
					num = 0f;
				}
				this._timerPreviousCounter = this._timerCounter;
				this._timerCounter = Mathf.FloorToInt(num);
				this.TimeToCloseLabel.text = this._timerCounter.ToString("#0");
				if (num <= 5f && this._timerCounter != this._timerPreviousCounter)
				{
					FMODAudioManager.PlayOneShotAt(this._countdownAudio, Vector3.zero, 0);
				}
				if (num < 10f)
				{
					if (!this.TimeToCloseSmallLabel.gameObject.activeSelf)
					{
						this.TimeToCloseSmallLabel.gameObject.SetActive(true);
					}
					float num2 = Mathf.Clamp(num - (float)this._timerCounter, 0f, 0.9f);
					this.TimeToCloseSmallLabel.text = num2.ToString("#.0");
				}
			}
		}

		[SerializeField]
		private UILabel TitleLabel;

		[SerializeField]
		private UILabel LeftTeamScoreLabel;

		[SerializeField]
		private UILabel RightTeamScoreLabel;

		[SerializeField]
		private UILabel TimeTitleLabel;

		[SerializeField]
		private UILabel TimeToCloseLabel;

		[SerializeField]
		private UILabel TimeToCloseSmallLabel;

		[SerializeField]
		private Animation ScoreInOutAnimation;

		[SerializeField]
		private FMODAsset _countdownAudio;

		private bool _updateStartRoundTime;

		private bool _didAnimation;

		private int _timerCounter = int.MaxValue;

		private int _timerPreviousCounter = int.MaxValue;

		public enum RoundType
		{
			First,
			Win,
			Lose
		}
	}
}
