using System;
using System.Collections;
using HeavyMetalMachines.Combat;
using HeavyMetalMachines.Match;
using Pocketverse;
using Pocketverse.MuralContext;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	public class HORTABehaviour : GameHubBehaviour
	{
		public bool Running { get; set; }

		internal void Init(HORTAComponent comp, HORTAStatePlayback statePlayback, HORTAState state, MatchData.MatchState finalState)
		{
			this.Component = comp;
			this.StatePlayback = statePlayback;
			this.State = state;
			this.FinalState = finalState;
			this._quitPressedTime = -1f;
		}

		private void Awake()
		{
			this.Running = false;
			this._ended = false;
		}

		private void Update()
		{
			this.ProccessInputs();
			if (!this.Running)
			{
				return;
			}
			if (GameHubBehaviour.Hub.BombManager.ScoreBoard.CurrentState == BombScoreBoard.State.Replay && Mathf.Max(GameHubBehaviour.Hub.BombManager.ScoreBoard.BombScoreBlue, GameHubBehaviour.Hub.BombManager.ScoreBoard.BombScoreRed) >= GameHubBehaviour.Hub.BombManager.Rules.BombScoreTarget)
			{
				this.CallEndGame();
			}
		}

		public void CallEndGame()
		{
			base.StartCoroutine(this.EndGame());
		}

		private IEnumerator EndGame()
		{
			yield return new WaitForSeconds(GameHubBehaviour.Hub.BombManager.Rules.ReplayTimeSeconds);
			GameHubBehaviour.Hub.Match.State = this.FinalState;
			GameHubBehaviour.Hub.Server.ClientSetInfo(GameHubBehaviour.Hub.Match);
			this.Running = false;
			this._ended = true;
			yield break;
		}

		private void QuitPlayback()
		{
			this.Component.CleanUp();
			Mural.PostAll(default(CleanupMessage), typeof(ICleanupListener));
			GameHubBehaviour.Hub.State.GotoState(this.State, false);
		}

		private void ProccessInputs()
		{
			if (this._ended)
			{
				if (Input.GetKeyDown(KeyCode.Q))
				{
					this.QuitPlayback();
					return;
				}
				return;
			}
			else
			{
				if (Input.GetKeyDown(KeyCode.Q))
				{
					this._quitPressedTime = Time.realtimeSinceStartup;
				}
				if (this._quitPressedTime > 0f && Time.realtimeSinceStartup - this._quitPressedTime > 5f)
				{
					this.QuitPlayback();
					return;
				}
				if (Input.GetKeyUp(KeyCode.Q))
				{
					this._quitPressedTime = -1f;
				}
				if (Input.GetKeyDown(KeyCode.P))
				{
					this.Component.HORTAClock.TogglePause();
				}
				if (Input.GetKeyDown(KeyCode.F))
				{
					this.Component.HORTAClock.ToggleFastForward();
				}
				if (Input.GetKeyDown(KeyCode.S))
				{
					this.Component.HORTAClock.ToggleSlowMotion();
				}
				if (Input.GetKeyDown(KeyCode.D))
				{
					this.Component.HORTAClock.ToggleOffAny();
				}
				return;
			}
		}

		public static readonly BitLogger Log = new BitLogger(typeof(HORTABehaviour));

		public HORTAStatePlayback StatePlayback;

		public HORTAComponent Component;

		public HORTAState State;

		private MatchData.MatchState FinalState;

		private bool _ended;

		private float _quitPressedTime;
	}
}
