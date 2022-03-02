using System;
using System.Collections;
using System.Collections.Generic;
using HeavyMetalMachines.Combat;
using HeavyMetalMachines.Frontend;
using HeavyMetalMachines.GameCamera.Behaviour;
using HeavyMetalMachines.Infra.Context;
using HeavyMetalMachines.Match;
using HeavyMetalMachines.Playback.Snapshot;
using HeavyMetalMachines.PostProcessing;
using Pocketverse;
using Pocketverse.MuralContext;
using UnityEngine;
using Zenject;

namespace HeavyMetalMachines.Playback
{
	public class HORTAPlayback : GameHubBehaviour, ICleanupListener, IPlayback
	{
		public bool IsRunningReplay
		{
			get
			{
				return this.State == HORTAPlayback.PlaybackState.Replay;
			}
		}

		private int KeyFrameTimeComparison(int frameId1, int frameId2)
		{
			IFrame frame = this._matchBuffer.GetFrame(frameId1);
			IFrame frame2 = this._matchBuffer.GetFrame(frameId2);
			return frame.Time.CompareTo(frame2.Time);
		}

		private bool CheckFrameOlderThanCurrentTime(IFrame frame)
		{
			return frame.Time <= this._gameTime.GetPlaybackTime();
		}

		private bool CheckFrameOlderThanPlaybackPlusReplayTime(IFrame frame)
		{
			return frame.Time <= this._hortaComponent.HORTAClock.GetSynchTime();
		}

		private bool CheckFrameNewerThanCurrentTime(IFrame frame)
		{
			return frame.Time > this._gameTime.GetPlaybackTime();
		}

		private bool CheckFrameOnTimeZero(IFrame frame)
		{
			return frame.Time == 0;
		}

		public void SetBuffer(IMatchBuffer buffer)
		{
			if (this._matchBuffer != null)
			{
				this._matchBuffer.Close();
			}
			this._matchBuffer = buffer;
			this._bufferReader = new IndexedMatchBufferReader(this._matchBuffer);
			this._replayReader = new IndexedMatchBufferReader(this._matchBuffer);
		}

		private void Start()
		{
			this._defaultContext = new DefaultProcessContext(new Func<int>(this._gameTime.GetPlaybackTime));
			this._simpleContext = new NoQueueProcessContext(new Func<int>(this._gameTime.GetPlaybackTime));
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
			HORTAPlayback.Log.InfoFormat("Replay Tag Start={0}", new object[]
			{
				causerId
			});
			if (this._runningReplay)
			{
				HORTAPlayback.Log.FatalFormat("Already running replay={0}", new object[]
				{
					causerId
				});
				return;
			}
			this._runningReplay = true;
			this.State = HORTAPlayback.PlaybackState.Replay;
			this.RewindTime(GameHubBehaviour.Hub.BombManager.Rules.ReplayTimeSeconds);
			HORTAPlayback.Log.Info("Replay Tag Rewinded");
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
				HORTAPlayback.Log.FatalFormat("Not running replay={0}", new object[]
				{
					this._lastBombDeliver
				});
				yield break;
			}
			yield return base.StartCoroutine(UnityUtils.WaitForSecondsRealTime(0.5f));
			HORTAPlayback.Log.Info("Replay Tag Restoring");
			this.RestoreTime();
			this.Play();
			this._bombScoreCamera.StopBehaviour();
			HORTAPlayback.Log.Info("Replay Tag End");
			this._runningReplay = false;
			yield break;
		}

		private void RestoreTime()
		{
			this._gameTime.RewindedTimeMillis = 0;
			this._currentProcessors = this._processorFactory.GetProvider(OperationKind.Playback);
			IFrame frame;
			while (this._bufferReader.ReadNext(new FrameCheck(this.CheckFrameOlderThanCurrentTime), out frame))
			{
				this.ProcessKeyFrame(frame);
			}
			this._currentProcessors = this._processorFactory.GetProvider(OperationKind.Playback);
		}

		private void RewindTime(float timeSeconds)
		{
			this._gameTime.RewindedTimeMillis += (int)(timeSeconds * 1000f);
			this._defaultContext.ClearQueue();
			this._currentProcessors = this._processorFactory.GetProvider(OperationKind.ReplayRewind);
			this._replayReader.SetIndex(this._bufferReader.CurrentIndex);
			IFrame frame = this._bufferReader.Current;
			while (frame != null)
			{
				this.ProcessKeyFrame(frame);
				if (!this._bufferReader.ReadPrevious(new FrameCheck(this.CheckFrameNewerThanCurrentTime), out frame))
				{
					IL_89:
					this._defaultContext.ExecutionQueue.Sort(new Comparison<int>(this.KeyFrameTimeComparison));
					this._currentProcessors = this._processorFactory.GetProvider(OperationKind.ReplayExecutionQueue);
					for (int i = 0; i < this._defaultContext.ExecutionQueue.Count; i++)
					{
						IFrame frame2 = this._matchBuffer.GetFrame(this._defaultContext.ExecutionQueue[i]);
						this.ProcessKeyFrame(frame2);
					}
					this._currentProcessors = this._processorFactory.GetProvider(OperationKind.ReplayPlayback);
					this._bufferReader.ReadPrevious((IFrame x) => true, out frame);
					return;
				}
			}
			goto IL_89;
		}

		private void Update()
		{
			this.UpdatePlayback();
			if (this._runningReplay)
			{
				this.UpdateReplayFrames();
			}
		}

		public void AddStateKeyFrame(byte keyFrameType, int frameId, int previousFrameId, int time, byte[] data)
		{
			HORTAPlayback.Log.ErrorFormatStackTrace("HORTA Should never add keyframes this way!", new object[0]);
		}

		public void Init()
		{
			HORTAPlayback.Log.Debug("Init");
			this.State = HORTAPlayback.PlaybackState.Ready;
			this._currentProcessors = this._processorFactory.GetProvider(OperationKind.Playback);
			IFrame frame;
			while (this._bufferReader.ReadNext(new FrameCheck(this.CheckFrameOnTimeZero), out frame))
			{
				ProcessFrame processor = this._currentProcessors.GetProcessor((FrameKind)frame.Type);
				processor(frame, this._simpleContext);
			}
		}

		public void Play()
		{
			if (this.State == HORTAPlayback.PlaybackState.RecordAndPlay)
			{
				return;
			}
			this.State = HORTAPlayback.PlaybackState.RecordAndPlay;
			this._currentProcessors = this._processorFactory.GetProvider(OperationKind.Playback);
			this.UpdatePlayback();
		}

		public void Stop()
		{
			HORTAPlayback.Log.Debug("Stop");
			if (this._matchBuffer != null)
			{
				this._matchBuffer.Close();
			}
			this._bufferReader = new IndexedMatchBufferReader(this._matchBuffer);
			this.State = HORTAPlayback.PlaybackState.Stopped;
			this._gameTime.RewindedTimeMillis = 0;
			this._runningReplay = false;
		}

		public void UpdatePlayback()
		{
			if (this.State == HORTAPlayback.PlaybackState.Stopped || this._matchBuffer == null)
			{
				return;
			}
			int num = 0;
			IFrame frame;
			while (this._bufferReader.ReadNext(new FrameCheck(this.CheckFrameOlderThanCurrentTime), out frame))
			{
				this.ProcessKeyFrame(frame);
				if (++num > 100)
				{
					break;
				}
			}
		}

		private void UpdateReplayFrames()
		{
			int num = 0;
			IFrame frame;
			while (this._replayReader.ReadNext(new FrameCheck(this.CheckFrameOlderThanPlaybackPlusReplayTime), out frame))
			{
				ProcessFrame processor = this._processorFactory.GetProvider(OperationKind.ArrivalDuringReplay).GetProcessor((FrameKind)frame.Type);
				processor(frame, this._simpleContext);
				if (++num > 100)
				{
					break;
				}
			}
		}

		private void ProcessKeyFrame(IFrame frame)
		{
			ProcessFrame processor = this._currentProcessors.GetProcessor((FrameKind)frame.Type);
			if (processor != null)
			{
				processor(frame, this._defaultContext);
				return;
			}
			HORTAPlayback.Log.WarnFormat("Processor not found for type={0}. Skipping this keyframe.", new object[]
			{
				frame.Type
			});
		}

		public void OnCleanup(CleanupMessage msg)
		{
			if (this._matchBuffer is MemoryMatchBuffer)
			{
				HORTAPlayback.Log.DebugFormat("Keyframes={0}", new object[]
				{
					((MemoryMatchBuffer)this._matchBuffer).FrameCount
				});
			}
			this.Stop();
			base.StopAllCoroutines();
			this._gameTime.RewindedTimeMillis = 0;
			if (this._matchBuffer != null)
			{
				this._matchBuffer.Close();
			}
			this._bufferReader = new IndexedMatchBufferReader(this._matchBuffer);
		}

		public void FixTime(int originalTime)
		{
			this._bombScoreCamera.StopBehaviour();
			if (this._runningReplay)
			{
				this.RestoreTime();
				base.StopAllCoroutines();
				this._runningReplay = false;
			}
			IFrame frame = this._bufferReader.Current;
			int synchTime = this._gameTime.GetSynchTime();
			HORTAPlayback.Log.DebugFormat("Fixing time={0} current={1} playback={2} original={3}", new object[]
			{
				synchTime,
				frame.Time,
				this._gameTime.GetPlaybackTime(),
				originalTime
			});
			if (originalTime < synchTime)
			{
				HORTAPlayback.Log.ErrorFormat("OH NO!! No fast forward implemented from={0} to={1}", new object[]
				{
					frame.Time,
					synchTime
				});
				return;
			}
			HORTAFastRewind hortafastRewind = new HORTAFastRewind(this._bufferReader, this._processorFactory, this._matchBuffer);
			hortafastRewind.Execute(synchTime, originalTime, this._snapshots);
		}

		private void FastForward()
		{
			IFrame frame3 = this._bufferReader.Current;
			int playbackTime = this._gameTime.GetPlaybackTime();
			this._defaultContext.ClearQueue();
			this._currentProcessors = this._processorFactory.GetProvider(OperationKind.FastForward);
			FrameCheck check = (IFrame frame) => frame.Time < playbackTime;
			do
			{
				this.ProcessKeyFrame(frame3);
				this._bufferReader.ReadNext(check, out frame3);
			}
			while (frame3 != null);
			this._defaultContext.ExecutionQueue.Sort(new Comparison<int>(this.KeyFrameTimeComparison));
			this._currentProcessors = this._processorFactory.GetProvider(OperationKind.Playback);
			int i = 0;
			for (int j = 0; j < this._defaultContext.ExecutionQueue.Count; j++)
			{
				IFrame frame2 = this._matchBuffer.GetFrame(this._defaultContext.ExecutionQueue[j]);
				while (i < this._defaultContext.ActionQueue.Count && this._defaultContext.ActionQueue.Keys[i] <= frame2.Time)
				{
					this._defaultContext.ActionQueue.Values[i]();
					i++;
				}
				this.ProcessKeyFrame(frame2);
			}
			while (i < this._defaultContext.ActionQueue.Count)
			{
				this._defaultContext.ActionQueue.Values[i++]();
			}
		}

		private static readonly BitLogger Log = new BitLogger(typeof(HORTAPlayback));

		private DefaultProcessContext _defaultContext;

		private NoQueueProcessContext _simpleContext;

		[Inject]
		private IFrameProcessorFactory _processorFactory;

		[Inject]
		private HORTAComponent _hortaComponent;

		[Inject]
		private IGameTime _gameTime;

		[Inject]
		private List<IFeatureSnapshot> _snapshots;

		[Inject]
		private IGamePostProcessing _postProcessing;

		[Inject]
		private IBombScoreCameraBehaviour _bombScoreCamera;

		private IFrameProcessorProvider _currentProcessors;

		public const int MaxEventsPerFrame = 100;

		private HORTAPlayback.PlaybackState State;

		private int _lastBombDeliver;

		private IMatchBuffer _matchBuffer;

		private IndexedMatchBufferReader _bufferReader;

		private IndexedMatchBufferReader _replayReader;

		private bool _runningReplay;

		private enum PlaybackState
		{
			Stopped,
			Ready,
			RecordAndPlay,
			Replay
		}
	}
}
