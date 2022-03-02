using System;
using System.Collections;
using HeavyMetalMachines.Infra.Context;
using HeavyMetalMachines.Match;
using Pocketverse;
using Pocketverse.MuralContext;
using UniRx;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	public class HORTABehaviour : GameHubBehaviour
	{
		public bool Running { get; set; }

		internal void Init(HORTAComponent comp, HORTAStatePlayback statePlayback, HORTAState state, MatchData.MatchState finalState, HORTATime time)
		{
			this.Component = comp;
			this.StatePlayback = statePlayback;
			this.State = state;
			this.FinalState = finalState;
			this._quitPressedTime = -1f;
			this._time = time;
			this._availabilityDisposable = ObservableExtensions.Subscribe<bool>(this.Component.TimeControl.ObserveAvailability(), delegate(bool available)
			{
				this._timelineAvailable = available;
			});
		}

		private void Awake()
		{
			this.Running = false;
		}

		private void Update()
		{
			this._time.Update();
			this.ProcessInputs();
			if (!this.Running)
			{
				return;
			}
			if (GameHubBehaviour.Hub.BombManager.ScoreBoard.CurrentState == BombScoreboardState.Replay && Mathf.Max(GameHubBehaviour.Hub.BombManager.ScoreBoard.BombScoreBlue, GameHubBehaviour.Hub.BombManager.ScoreBoard.BombScoreRed) >= GameHubBehaviour.Hub.BombManager.Rules.BombScoreTarget)
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
			HORTABehaviour.Log.DebugFormat("Match OVER={0}", new object[]
			{
				GameHubBehaviour.Hub.GameTime.MatchTimer.GetTimeSeconds()
			});
			GameHubBehaviour.Hub.Match.State = this.FinalState;
			GameHubBehaviour.Hub.Server.ClientSetInfo(GameHubBehaviour.Hub.Match);
			this.Running = false;
			yield break;
		}

		public void QuitPlayback()
		{
			HORTABehaviour.Log.Debug("Back to HORTA");
			this.Component.CleanUp();
			base.StopAllCoroutines();
			Mural.PostAll(default(CleanupMessage), typeof(ICleanupListener));
			GameHubBehaviour.Hub.State.GotoState(this.State, false);
		}

		private void ProcessInputs()
		{
			if (this._timelineAvailable)
			{
				if (Input.GetKeyDown(102))
				{
					HORTABehaviour.Log.DebugFormat("Toggle FastForward", new object[0]);
					this.Component.TimeControl.IncreaseSpeed();
				}
				if (Input.GetKeyDown(122))
				{
					HORTABehaviour.Log.DebugFormat("Toggle SlowMotion", new object[0]);
					this.Component.TimeControl.DecreaseSpeed();
				}
				if (Input.GetKeyDown(120))
				{
					HORTABehaviour.Log.DebugFormat("Restore time/Pause", new object[0]);
					if (this.Component.TimeControl.IsPlaying)
					{
						this.Component.TimeControl.Pause();
					}
					else
					{
						this.Component.TimeControl.Play();
					}
				}
			}
			if (Input.GetKeyDown(116))
			{
				this.Component.ToggleTimelinePresenterVisibility();
			}
		}

		public static readonly BitLogger Log = new BitLogger(typeof(HORTABehaviour));

		public HORTAStatePlayback StatePlayback;

		public HORTAComponent Component;

		public HORTAState State;

		private MatchData.MatchState FinalState;

		private HORTATime _time;

		private float _quitPressedTime;

		private bool _timelineAvailable;

		private IDisposable _availabilityDisposable;
	}
}
