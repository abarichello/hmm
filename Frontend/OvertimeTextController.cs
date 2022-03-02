using System;
using HeavyMetalMachines.Arena;
using HeavyMetalMachines.Localization;
using Pocketverse;
using UnityEngine;
using UnityEngine.UI;

namespace HeavyMetalMachines.Frontend
{
	public class OvertimeTextController : GameHubBehaviour
	{
		private void Awake()
		{
			this._isClient = (GameHubBehaviour.Hub && GameHubBehaviour.Hub.Net.IsClient());
			if (!this._isClient)
			{
				base.gameObject.SetActive(false);
				return;
			}
			string text = Language.Get(this.OvertimeDraft, TranslationContext.Hud);
			this.OvertimeLabel.text = text;
			this.OvertimeAnimLabel.text = text;
			this._overtimeAnimation = base.GetComponent<Animation>();
			GameHubBehaviour.Hub.BombManager.ListenToOvertimeStarted += this.OnOvertimeStarted;
			IGameArenaInfo currentArena = GameHubBehaviour.Hub.ArenaConfig.GetCurrentArena();
			this._overtimeDuration = (int)(currentArena.OvertimeDurationSeconds * 1000f);
			this._roundDuration = (int)(currentArena.RoundTimeSeconds * 1000f);
		}

		private void OnDestroy()
		{
			if (this._isClient)
			{
				GameHubBehaviour.Hub.BombManager.ListenToOvertimeStarted -= this.OnOvertimeStarted;
			}
		}

		private void Update()
		{
			if (!this._isInOvertime)
			{
				return;
			}
			if (GameHubBehaviour.Hub.BombManager.ScoreBoard.IsInOvertime)
			{
				int time = GameHubBehaviour.Hub.GameTime.MatchTimer.GetTime();
				float num = (float)(time - this._currentRoundStartTime - this._roundDuration);
				float num2 = Mathf.Clamp01(num / (float)this._overtimeDuration);
				this._overtimeAnimationState.normalizedSpeed = 1f + num2 * this.MaxIdleSpeed;
				return;
			}
			this._isInOvertime = false;
			this._overtimeAnimation.Stop();
			this.SetElementsActive(false);
		}

		private void OnOvertimeStarted()
		{
			this.SetElementsActive(true);
			this._overtimeAnimation.Play("OvertimeInAnimation");
			this._overtimeAnimationState = this._overtimeAnimation.PlayQueued("OvertimeIdleAnimation");
			this._overtimeAnimationState.normalizedSpeed = 1f;
			this._currentRoundStartTime = GameHubBehaviour.Hub.BombManager.ScoreBoard.RoundStartTimeMillis;
			this._isInOvertime = true;
		}

		private void SetElementsActive(bool active)
		{
			for (int i = 0; i < this.OvertimeElements.Length; i++)
			{
				this.OvertimeElements[i].SetActive(active);
			}
		}

		public void SetVisibility(bool visibility)
		{
			if (!this._isInOvertime)
			{
				return;
			}
			this.SetElementsActive(visibility);
		}

		public GameObject[] OvertimeElements;

		public float MaxIdleSpeed = 5f;

		public string OvertimeDraft;

		public Text OvertimeLabel;

		public Text OvertimeAnimLabel;

		private bool _isClient;

		private bool _isInOvertime;

		private int _overtimeDuration;

		private int _roundDuration;

		private int _currentRoundStartTime;

		private Animation _overtimeAnimation;

		private AnimationState _overtimeAnimationState;

		private const string IN_ANIMATION_NAME = "OvertimeInAnimation";

		private const string IDLE_ANIMATION_NAME = "OvertimeIdleAnimation";
	}
}
