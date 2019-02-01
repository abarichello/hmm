using System;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat
{
	public class BombTimeoutGUI : GameHubBehaviour
	{
		private void Start()
		{
			GameHubBehaviour.Hub.BombManager.ListenToMatchUpdate += this.Refresh;
			this.Refresh();
		}

		private void OnDestroy()
		{
			if (GameHubBehaviour.Hub)
			{
				GameHubBehaviour.Hub.BombManager.ListenToMatchUpdate -= this.Refresh;
			}
		}

		private void Refresh()
		{
			this.Info.TimeLabel.transform.parent.gameObject.SetActive(false);
			this.Info.Timeout = 0L;
			this._lastSecondsUiUpdate = -1;
			bool flag = false;
			BombScoreBoard.State currentBombGameState = GameHubBehaviour.Hub.BombManager.CurrentBombGameState;
			if (currentBombGameState == BombScoreBoard.State.PreBomb)
			{
				this.Info.BombIconSprite.enabled = false;
				this.Info.Timeout = GameHubBehaviour.Hub.BombManager.ScoreBoard.Timeout;
				this.Info.ScoreBoard = true;
				this.Info.Running = true;
				this.Info.EnemyTeamLabel.enabled = false;
				this.Info.MyTeamLabel.enabled = false;
				this.Info.PassLabel.enabled = false;
				this.Info.PassSprite.enabled = false;
				this.Info.TimeLabel.transform.parent.gameObject.SetActive(true);
				flag = true;
			}
			this.RootPanel.gameObject.SetActive(flag);
			if (flag)
			{
				this.Table.Reposition();
			}
		}

		private void Update()
		{
			if (!this.RootPanel.gameObject.activeSelf || this._timeoutUpdater.ShouldHalt())
			{
				return;
			}
			this.UpdateTimeoutLabel(this.Info);
			this.UpdatePassLabel(this.Info);
		}

		private void UpdatePassLabel(BombTimeoutGUI.TimeoutInfo timeoutInfo)
		{
			Color color;
			int textInt;
			if (timeoutInfo.PassTimeout <= (long)GameHubBehaviour.Hub.GameTime.GetPlaybackTime())
			{
				color = Color.green;
				textInt = 0;
			}
			else
			{
				color = Color.red;
				textInt = Mathf.CeilToInt(Convert.ToSingle(TimeSpan.FromMilliseconds((double)(timeoutInfo.PassTimeout - (long)GameHubBehaviour.Hub.GameTime.GetPlaybackTime())).TotalSeconds));
			}
			timeoutInfo.PassLabel.SetTextInt(textInt);
			timeoutInfo.PassSprite.color = color;
		}

		private void UpdateTimeoutLabel(BombTimeoutGUI.TimeoutInfo timeoutInfo)
		{
			if (timeoutInfo.Timeout <= 0L || !timeoutInfo.Running)
			{
				return;
			}
			TimeSpan timeSpan = TimeSpan.FromMilliseconds((double)(timeoutInfo.Timeout - (long)((!timeoutInfo.ScoreBoard) ? 0 : GameHubBehaviour.Hub.GameTime.GetPlaybackTime())));
			if (timeSpan.TotalSeconds < 0.0 || this._lastSecondsUiUpdate == timeSpan.Seconds)
			{
				return;
			}
			this._lastSecondsUiUpdate = timeSpan.Seconds;
			timeoutInfo.TimeLabel.text = string.Format("{0:00}:{1:00}", timeSpan.TotalMinutes, timeSpan.Seconds);
		}

		public static readonly BitLogger Log = new BitLogger(typeof(BombTimeoutGUI));

		public UIPanel RootPanel;

		public UITable Table;

		public BombTimeoutGUI.TimeoutInfo Info;

		public Sprite BombIconFriendly;

		public Sprite BombIconEnemy;

		public Sprite BombIconNeutral;

		private TimedUpdater _timeoutUpdater = new TimedUpdater
		{
			PeriodMillis = 500
		};

		private int _lastSecondsUiUpdate = -1;

		[Serializable]
		public class TimeoutInfo
		{
			public UILabel TimeLabel;

			public UI2DSprite BombIconSprite;

			public long Timeout;

			public UILabel MyTeamLabel;

			public UILabel EnemyTeamLabel;

			public UILabel PassLabel;

			public UI2DSprite PassSprite;

			public long PassTimeout;

			[HideInInspector]
			public bool ScoreBoard;

			[HideInInspector]
			public bool Running;
		}
	}
}
