using System;
using System.Collections;
using System.Runtime.CompilerServices;
using HeavyMetalMachines.Combat;
using HeavyMetalMachines.GameCamera;
using HeavyMetalMachines.GameCamera.Behaviour;
using HeavyMetalMachines.Infra.Context;
using HeavyMetalMachines.Match;
using HeavyMetalMachines.PostProcessing;
using Pocketverse;
using Pocketverse.MuralContext;
using UnityEngine;
using Zenject;

namespace HeavyMetalMachines.Playback
{
	public class GamePlayback : GameHubBehaviour, ICleanupListener, IPlayback
	{
		public bool IsRunningReplay
		{
			get
			{
				return this.State == GamePlayback.PlaybackState.Replay;
			}
		}

		private int KeyFrameTimeComparison(int frameId1, int frameId2)
		{
			IFrame frame = this._matchBuffer.GetFrame(frameId1);
			IFrame frame2 = this._matchBuffer.GetFrame(frameId2);
			int num = (frame != null) ? frame.Time : 0;
			int value = (frame2 != null) ? frame2.Time : 0;
			return num.CompareTo(value);
		}

		private static bool CheckFrameOlderThanCurrentTime(IFrame frame)
		{
			return frame.Time <= GameHubBehaviour.Hub.GameTime.GetPlaybackTime();
		}

		private static bool CheckFrameNewerThanCurrentTime(IFrame frame)
		{
			return frame.Time > GameHubBehaviour.Hub.GameTime.GetPlaybackTime();
		}

		private void CreateBuffer()
		{
			if (this._matchBuffer == null)
			{
				this._matchBuffer = new MemoryMatchBuffer(52428800, 131072);
				this._bufferReader = new IndexedMatchBufferReader(this._matchBuffer);
			}
		}

		public void SetBuffer(IMatchBuffer buffer)
		{
			if (this._matchBuffer != null)
			{
				this._matchBuffer.Close();
			}
			this._matchBuffer = buffer;
			this._bufferReader = new IndexedMatchBufferReader(this._matchBuffer);
		}

		private void Start()
		{
			if (GameHubBehaviour.Hub.Net.IsServer() && !GameHubBehaviour.Hub.Net.IsTest())
			{
				base.enabled = false;
				return;
			}
			this._defaultContext = new DefaultProcessContext(new Func<int>(GameHubBehaviour.Hub.GameTime.GetPlaybackTime));
			this._simpleContext = new NoQueueProcessContext(new Func<int>(GameHubBehaviour.Hub.GameTime.GetPlaybackTime));
			this.CreateBuffer();
			GameHubBehaviour.Hub.BombManager.ListenToBombDelivery += this.OnBombDelivery;
			GameHubBehaviour.Hub.BombManager.ListenToPhaseChange += this.OnPhaseChange;
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

		private void OnPhaseChange(BombScoreboardState state)
		{
			if (state != BombScoreboardState.Replay)
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
			GamePlayback.Log.InfoFormat("Replay Tag Start={0}", new object[]
			{
				causerId
			});
			if (this._runningReplay)
			{
				GamePlayback.Log.FatalFormat("Already running replay={0}", new object[]
				{
					causerId
				});
				return;
			}
			this._runningReplay = true;
			this.State = GamePlayback.PlaybackState.Replay;
			this.RewindTime(GameHubBehaviour.Hub.BombManager.Rules.ReplayTimeSeconds);
			GamePlayback.Log.Info("Replay Tag Rewinded");
			BombVisualController instance = BombVisualController.GetInstance();
			if (instance == null)
			{
				return;
			}
			this._bombScoreCamera.FollowBomb(instance.transform);
			Func<bool> condition = () => GameHubBehaviour.Hub.BombManager.ScoreBoard.CurrentState == BombScoreboardState.Replay;
			PostProcessingState postProcessingState = this._postProcessing.Request("Replay", condition, true);
			if (postProcessingState == null)
			{
				return;
			}
			postProcessingState.Enabled = true;
			postProcessingState.CRTMonitor.Enabled = true;
			postProcessingState.CRTMonitor.Parameters.LineStrength = 0.5f;
		}

		private IEnumerator EndReplayCoroutine()
		{
			if (!this._runningReplay)
			{
				GamePlayback.Log.FatalFormat("Not running replay={0}", new object[]
				{
					this._lastBombDeliver
				});
				yield break;
			}
			yield return base.StartCoroutine(UnityUtils.WaitForSecondsRealTime(0.5f));
			GamePlayback.Log.Info("Replay Tag Restoring");
			this.RestoreTime();
			this.Play();
			this._bombScoreCamera.StopBehaviour();
			GamePlayback.Log.Info("Replay Tag End");
			this._runningReplay = false;
			yield break;
		}

		private void RestoreTime()
		{
			GameHubBehaviour.Hub.GameTime.RewindedTimeMillis = 0;
			this._currentProcessors = this._processorFactory.GetProvider(OperationKind.Playback);
			for (;;)
			{
				IMatchBufferReader bufferReader = this._bufferReader;
				if (GamePlayback.<>f__mg$cache0 == null)
				{
					GamePlayback.<>f__mg$cache0 = new FrameCheck(GamePlayback.CheckFrameOlderThanCurrentTime);
				}
				IFrame frame;
				if (!bufferReader.ReadNext(GamePlayback.<>f__mg$cache0, out frame))
				{
					break;
				}
				this.ProcessKeyFrame(frame);
			}
			this._currentProcessors = this._processorFactory.GetProvider(OperationKind.Playback);
		}

		private void RewindTime(float timeSeconds)
		{
			GameHubBehaviour.Hub.GameTime.RewindedTimeMillis += (int)(timeSeconds * 1000f);
			this._defaultContext.ClearQueue();
			this._currentProcessors = this._processorFactory.GetProvider(OperationKind.ReplayRewind);
			IFrame frame = this._bufferReader.Current;
			while (frame != null)
			{
				this.ProcessKeyFrame(frame);
				IMatchBufferReader bufferReader = this._bufferReader;
				if (GamePlayback.<>f__mg$cache1 == null)
				{
					GamePlayback.<>f__mg$cache1 = new FrameCheck(GamePlayback.CheckFrameNewerThanCurrentTime);
				}
				if (!bufferReader.ReadPrevious(GamePlayback.<>f__mg$cache1, out frame))
				{
					IL_88:
					this._defaultContext.ExecutionQueue.Sort(new Comparison<int>(this.KeyFrameTimeComparison));
					this._currentProcessors = this._processorFactory.GetProvider(OperationKind.ReplayExecutionQueue);
					for (int i = 0; i < this._defaultContext.ExecutionQueue.Count; i++)
					{
						IFrame frame2 = this._matchBuffer.GetFrame(this._defaultContext.ExecutionQueue[i]);
						if (frame2 == null)
						{
							GamePlayback.Log.DebugFormat("Frame={0} is null on execution queue rewind", new object[]
							{
								this._defaultContext.ExecutionQueue[i]
							});
						}
						else
						{
							this.ProcessKeyFrame(frame2);
						}
					}
					this._currentProcessors = this._processorFactory.GetProvider(OperationKind.ReplayPlayback);
					this._bufferReader.ReadPrevious((IFrame x) => true, out frame);
					return;
				}
			}
			goto IL_88;
		}

		private void Update()
		{
			this.UpdatePlayback();
		}

		public void AddStateKeyFrame(byte keyFrameType, int frameId, int previousFrameId, int time, byte[] data)
		{
			if (GameHubBehaviour.Hub.Net.IsServer())
			{
				GamePlayback.Log.ErrorFormat("AddStateKeyFrame should not be called SERVER side - KeyFrameType:[{0}] - FrameId:[{1}] - PreviousFrameId:[{2}] - Time:[{3}]", new object[]
				{
					keyFrameType,
					frameId,
					previousFrameId,
					time
				});
				return;
			}
			this._matchBuffer.AddFrame(keyFrameType, frameId, previousFrameId, time, data);
			IFrame frame2;
			if (this.State == GamePlayback.PlaybackState.Replay)
			{
				IFrame frame = this._matchBuffer.GetFrame(frameId);
				ProcessFrame processor = this._processorFactory.GetProvider(OperationKind.ArrivalDuringReplay).GetProcessor((FrameKind)frame.Type);
				processor(frame, this._simpleContext);
			}
			else if (this._bufferReader.ReadNext((IFrame x) => true, out frame2))
			{
				this.ProcessKeyFrame(frame2);
			}
		}

		public void Init()
		{
			GamePlayback.Log.Debug("Init");
			this.State = GamePlayback.PlaybackState.Ready;
			this._currentProcessors = this._processorFactory.GetProvider(OperationKind.Playback);
		}

		public void Play()
		{
			if (this.State == GamePlayback.PlaybackState.RecordAndPlay)
			{
				return;
			}
			this.State = GamePlayback.PlaybackState.RecordAndPlay;
			this._currentProcessors = this._processorFactory.GetProvider(OperationKind.Playback);
			this.UpdatePlayback();
		}

		public void Stop()
		{
			GamePlayback.Log.Debug("Stop");
			if (this._matchBuffer != null)
			{
				this._matchBuffer.Close();
			}
			this._bufferReader = new IndexedMatchBufferReader(this._matchBuffer);
			this.State = GamePlayback.PlaybackState.Stopped;
			GameHubBehaviour.Hub.GameTime.RewindedTimeMillis = 0;
			this._runningReplay = false;
		}

		public void UpdatePlayback()
		{
			if (this.State == GamePlayback.PlaybackState.Stopped || this._matchBuffer == null)
			{
				return;
			}
			int num = 0;
			do
			{
				IMatchBufferReader bufferReader = this._bufferReader;
				if (GamePlayback.<>f__mg$cache2 == null)
				{
					GamePlayback.<>f__mg$cache2 = new FrameCheck(GamePlayback.CheckFrameOlderThanCurrentTime);
				}
				IFrame frame;
				if (!bufferReader.ReadNext(GamePlayback.<>f__mg$cache2, out frame))
				{
					break;
				}
				this.ProcessKeyFrame(frame);
			}
			while (++num <= 100);
		}

		private void ProcessKeyFrame(IFrame frame)
		{
			if (this._currentProcessors == null)
			{
				GamePlayback.Log.WarnFormat("Current Processors is null: FrameId:[{0}], PreviousFrameId:[{1}], FrameType:[{2}], FrameTime:[{3}]", new object[]
				{
					frame.FrameId,
					frame.PreviousFrameId,
					frame.Type,
					frame.Time
				});
				return;
			}
			ProcessFrame processor = this._currentProcessors.GetProcessor((FrameKind)frame.Type);
			if (processor != null)
			{
				processor(frame, this._defaultContext);
				return;
			}
			GamePlayback.Log.WarnFormat("Processor not found for type={0}. Skipping this keyframe.", new object[]
			{
				frame.Type
			});
		}

		public void OnCleanup(CleanupMessage msg)
		{
			if (this._matchBuffer is MemoryMatchBuffer)
			{
				GamePlayback.Log.DebugFormat("Keyframes={0}", new object[]
				{
					((MemoryMatchBuffer)this._matchBuffer).FrameCount
				});
			}
			this.Stop();
			base.StopAllCoroutines();
			GameHubBehaviour.Hub.GameTime.RewindedTimeMillis = 0;
			if (this._matchBuffer != null)
			{
				this._matchBuffer.Close();
			}
			this._bufferReader = new IndexedMatchBufferReader(this._matchBuffer);
		}

		private static readonly BitLogger Log = new BitLogger(typeof(GamePlayback));

		private DefaultProcessContext _defaultContext;

		private NoQueueProcessContext _simpleContext;

		[Inject]
		private IGamePostProcessing _postProcessing;

		[Inject]
		private IFrameProcessorFactory _processorFactory;

		[Inject]
		private IGameCamera _gameCamera;

		[Inject]
		private IBombScoreCameraBehaviour _bombScoreCamera;

		private IFrameProcessorProvider _currentProcessors;

		public const int MaxEventsPerFrame = 100;

		private GamePlayback.PlaybackState State;

		private int _lastBombDeliver;

		private IMatchBuffer _matchBuffer;

		private IMatchBufferReader _bufferReader;

		private bool _runningReplay;

		[CompilerGenerated]
		private static FrameCheck <>f__mg$cache0;

		[CompilerGenerated]
		private static FrameCheck <>f__mg$cache1;

		[CompilerGenerated]
		private static FrameCheck <>f__mg$cache2;

		private enum PlaybackState
		{
			Stopped,
			Ready,
			RecordAndPlay,
			Replay
		}
	}
}
