using System;
using System.Collections;
using System.Collections.Generic;
using HeavyMetalMachines.Combat;
using HeavyMetalMachines.Match;
using HeavyMetalMachines.PostProcessing;
using Pocketverse;
using Pocketverse.MuralContext;
using UnityEngine;

namespace HeavyMetalMachines
{
	public class PlaybackSystem : GameHubBehaviour, ICleanupListener
	{
		public static PlaybackSystem Instance
		{
			get
			{
				if (PlaybackSystem._instance == null)
				{
					PlaybackSystem._instance = new PlaybackSystem();
				}
				return PlaybackSystem._instance;
			}
		}

		public static bool IsRunningReplay
		{
			get
			{
				return PlaybackSystem._instance.State == PlaybackSystem.PlaybackState.Replay;
			}
		}

		private int KeyFrameTimeComparison(IFrame x, IFrame y)
		{
			return x.Time.CompareTo(y.Time);
		}

		private bool CheckFrameOlderThanCurrentTime(IFrame frame)
		{
			return frame.Time <= GameHubBehaviour.Hub.GameTime.GetPlaybackTime();
		}

		private bool CheckFrameNewerThanCurrentTime(IFrame frame)
		{
			return frame.Time > GameHubBehaviour.Hub.GameTime.GetPlaybackTime();
		}

		private void Awake()
		{
			PlaybackSystem._instance = this;
		}

		private void CreateBuffer()
		{
			if (this._matchBuffer == null)
			{
				this._matchBuffer = new MemoryMatchBuffer(52428800, 131072);
			}
		}

		public void SetBuffer(IMatchBuffer buffer)
		{
			if (this._matchBuffer != null)
			{
				this._matchBuffer.Close();
			}
			this._matchBuffer = buffer;
		}

		private void Start()
		{
			if (GameHubBehaviour.Hub.Net.IsServer() && !GameHubBehaviour.Hub.Net.IsTest())
			{
				PlaybackSystem.Log.Error("Server should not have an instance of playback system around!");
				return;
			}
			this.CreateBuffer();
			GameHubBehaviour.Hub.BombManager.ListenToBombDelivery += this.OnBombDelivery;
			GameHubBehaviour.Hub.BombManager.ListenToPhaseChange += this.OnPhaseChange;
		}

		private void CheckStreams()
		{
			if (this._matchBuffer is MemoryMatchBuffer)
			{
				((MemoryMatchBuffer)this._matchBuffer).CheckStreams();
			}
		}

		private void OnDestroy()
		{
			if (GameHubBehaviour.Hub.Net.IsServer())
			{
				return;
			}
			GameHubBehaviour.Hub.BombManager.ListenToBombDelivery -= this.OnBombDelivery;
			GameHubBehaviour.Hub.BombManager.ListenToPhaseChange -= this.OnPhaseChange;
		}

		private void OnBombDelivery(int causerid, TeamKind scoredTeam, Vector3 deliveryPosition)
		{
			this._lastBombDeliver = causerid;
		}

		private void OnPhaseChange(BombScoreBoard.State state)
		{
			if (state != BombScoreBoard.State.Replay)
			{
				if (this._runningReplay)
				{
					base.StartCoroutine(this.EndReplayCoroutine());
				}
			}
			else
			{
				this.StartReplay(this._lastBombDeliver);
			}
		}

		private void StartReplay(int causerId)
		{
			PlaybackSystem.Log.InfoFormat("Replay Tag Start={0}", new object[]
			{
				causerId
			});
			if (this._runningReplay)
			{
				PlaybackSystem.Log.FatalFormat("Already running replay={0}", new object[]
				{
					causerId
				});
				return;
			}
			this._runningReplay = true;
			PlaybackSystem.Log.Info("Replay Tag Rewinded");
			PlaybackSystem._instance.State = PlaybackSystem.PlaybackState.Replay;
			this.RewindTime(GameHubBehaviour.Hub.BombManager.Rules.ReplayTimeSeconds);
			BombVisualController instance = BombVisualController.GetInstance(false);
			if (null != instance)
			{
				CarCamera.Singleton.LockPan = true;
				Func<bool> condition = () => GameHubBehaviour.Hub.BombManager.ScoreBoard.CurrentState == BombScoreBoard.State.Replay;
				CarCamera.Singleton.SetTarget("Replay", condition, instance.transform, true, true, false);
				PostProcessingState postProcessingState = CarCamera.Singleton.postProcessing.Request("Replay", condition, true);
				if (postProcessingState != null)
				{
					postProcessingState.Enabled = true;
					postProcessingState.CRTMonitor.Enabled = true;
					postProcessingState.CRTMonitor.Parameters.LineStrength = 0.5f;
				}
			}
		}

		private IEnumerator EndReplayCoroutine()
		{
			if (!this._runningReplay)
			{
				PlaybackSystem.Log.FatalFormat("Not running replay={0}", new object[]
				{
					this._lastBombDeliver
				});
				yield break;
			}
			yield return base.StartCoroutine(UnityUtils.WaitForSecondsRealTime(0.5f));
			PlaybackSystem.Log.Info("Replay Tag Restoring");
			this.RestoreTime();
			PlaybackSystem.PlayAndRecordPlayback();
			CarCamera.Singleton.LockPan = false;
			PlaybackSystem.Log.Info("Replay Tag End");
			this._runningReplay = false;
			yield break;
		}

		private void RestoreTime()
		{
			GameHubBehaviour.Hub.GameTime.RewindedTimeMillis = 0;
			IFrame frame;
			while (this._matchBuffer.ReadNext(new FrameCheck(this.CheckFrameOlderThanCurrentTime), out frame))
			{
				this.ProcessKeyFrame(frame);
			}
		}

		private void RewindTime(float timeSeconds)
		{
			GameHubBehaviour.Hub.GameTime.RewindedTimeMillis += (int)(timeSeconds * 1000f);
			this._rewindingKeyFrames.Clear();
			IFrame frame = this._matchBuffer.Current;
			while (frame != null)
			{
				IKeyFrameParser frameParser = PlaybackManager.GetFrameParser((KeyFrameType)frame.Type);
				if (frameParser != null)
				{
					bool flag = frameParser.RewindProcess(frame);
					if (flag && frame.PreviousFrameId != -1)
					{
						IFrame frame2 = this._matchBuffer.GetFrame(frame.PreviousFrameId);
						if (frame2 != null && frame2.Time < GameHubBehaviour.Hub.GameTime.GetPlaybackTime())
						{
							this._rewindingKeyFrames.Add(frame2);
						}
					}
				}
				if (!this._matchBuffer.ReadPrevious(new FrameCheck(this.CheckFrameNewerThanCurrentTime), out frame))
				{
					IL_C8:
					this._rewindingKeyFrames.Sort(new Comparison<IFrame>(this.KeyFrameTimeComparison));
					for (int i = 0; i < this._rewindingKeyFrames.Count; i++)
					{
						this.ProcessKeyFrame(this._rewindingKeyFrames[i]);
					}
					return;
				}
			}
			goto IL_C8;
		}

		private void Update()
		{
			this.UpdatePlayback();
			this.CheckStreams();
		}

		public void AddStateKeyFrame(KeyFrameType keyFrameType, int frameId, int previousFrameId, int time, byte[] data)
		{
			if (GameHubBehaviour.Hub.Net.IsServer())
			{
				PlaybackSystem.Log.ErrorFormat("AddStateKeyFrame should not be called SERVER side - KeyFrameType:[{0}] - FrameId:[{1}] - PreviousFrameId:[{2}] - Time:[{3}]", new object[]
				{
					keyFrameType,
					frameId,
					previousFrameId,
					time
				});
				return;
			}
			this._matchBuffer.AddFrame((byte)keyFrameType, frameId, previousFrameId, time, data);
			if (this.State == PlaybackSystem.PlaybackState.RecordAndPlay)
			{
				IFrame frame;
				if (this._matchBuffer.ReadNext((IFrame x) => true, out frame))
				{
					this.ProcessKeyFrame(frame);
				}
			}
		}

		public static PlaybackSystem.PlaybackState GetCurrentState()
		{
			return PlaybackSystem.Instance.State;
		}

		public static void PlayAndRecordPlayback()
		{
			if (PlaybackSystem._instance.State == PlaybackSystem.PlaybackState.RecordAndPlay)
			{
				return;
			}
			PlaybackSystem._instance.State = PlaybackSystem.PlaybackState.RecordAndPlay;
			PlaybackSystem._instance.UpdatePlayback();
		}

		public static void Stop()
		{
			if (PlaybackSystem._instance._matchBuffer != null)
			{
				PlaybackSystem._instance._matchBuffer.Close();
			}
			PlaybackSystem._instance.State = PlaybackSystem.PlaybackState.Stopped;
			GameHubBehaviour.Hub.GameTime.RewindedTimeMillis = 0;
			PlaybackSystem._instance._runningReplay = false;
		}

		public void UpdatePlayback()
		{
			if (this.State == PlaybackSystem.PlaybackState.Stopped || this._matchBuffer == null)
			{
				return;
			}
			int num = 0;
			IFrame frame;
			while (this._matchBuffer.ReadNext(new FrameCheck(this.CheckFrameOlderThanCurrentTime), out frame))
			{
				this.ProcessKeyFrame(frame);
				if (++num > 100)
				{
					break;
				}
			}
		}

		private void ProcessKeyFrame(IFrame frame)
		{
			IKeyFrameParser frameParser = PlaybackManager.GetFrameParser((KeyFrameType)frame.Type);
			if (frameParser != null)
			{
				frameParser.Process(frame.GetReadData());
				return;
			}
			PlaybackSystem.Log.WarnFormat("Can't process unknown KeyFrameType:{0}. Skipping this keyframe.", new object[]
			{
				frame.Type
			});
		}

		public void OnCleanup(CleanupMessage msg)
		{
			if (this._matchBuffer is MemoryMatchBuffer)
			{
			}
			PlaybackSystem.Stop();
			base.StopAllCoroutines();
			GameHubBehaviour.Hub.GameTime.RewindedTimeMillis = 0;
		}

		private static readonly BitLogger Log = new BitLogger(typeof(PlaybackSystem));

		public const int MaxEventsPerFrame = 100;

		public PlaybackSystem.PlaybackState State;

		private int _lastBombDeliver;

		private IMatchBuffer _matchBuffer;

		private List<IFrame> _rewindingKeyFrames = new List<IFrame>();

		private static PlaybackSystem _instance;

		private bool _runningReplay;

		public enum PlaybackState
		{
			Stopped,
			RecordAndPlay,
			Play,
			Replay
		}
	}
}
