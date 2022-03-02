using System;
using System.Collections.Generic;
using System.Diagnostics;
using HeavyMetalMachines.DataTransferObjects.Progression;
using HeavyMetalMachines.Horta.View;
using HeavyMetalMachines.Match;
using HeavyMetalMachines.Playback;
using HeavyMetalMachines.Playback.Snapshot;
using Hoplon.ToggleableFeatures;
using Pocketverse;
using UniRx;
using UnityEngine;
using Zenject;

namespace HeavyMetalMachines.Frontend
{
	public class HORTAComponent : GameHubObject
	{
		public HORTAComponent(IPlayback playback, IGameTime time, IHORTATimelinePresenter timelinePresenter, IConfigLoader config, List<IFeatureSnapshot> snapshots, IToggleFeature toggleFeature, DiContainer container)
		{
			this._container = container;
			this._playback = playback;
			this._timelinePresenter = timelinePresenter;
			this._hortaTime = (time as HORTATime);
			this._hortaPlayback = (playback as HORTAPlayback);
			this._timeController = new HORTATimeController(this._hortaTime, this._hortaPlayback);
			this._snapshots = snapshots;
			this._config = config;
			this._toggleFeature = toggleFeature;
			this.DefaultFile = this._config.GetValue(ConfigAccess.HORTAFile);
		}

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event Action<string, string> OnVersionMismatched;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event Action<Exception> OnReadFileException;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event Action<IMatchInformation> OnMatchFileLoaded;

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event Action OnMatchReadyToPlay;

		public IMatchInformation MatchInfo { get; private set; }

		public HORTATime HORTAClock
		{
			get
			{
				return this._hortaTime;
			}
		}

		public ITimelineController TimelineController
		{
			get
			{
				return this._timeController;
			}
		}

		public HORTATimeController TimeControl
		{
			get
			{
				return this._timeController;
			}
		}

		public HORTAPlayback Playback
		{
			get
			{
				return this._hortaPlayback;
			}
		}

		public bool Enabled { get; set; }

		public void LoadFile(string path)
		{
			IMatchInformation matchInformation;
			try
			{
				MatchFile.ReadFile(path, out matchInformation);
			}
			catch (Exception obj)
			{
				if (this.OnReadFileException != null)
				{
					this.OnReadFileException(obj);
				}
				return;
			}
			if (string.Compare(matchInformation.Version, "Release.15.00.250", StringComparison.InvariantCulture) != 0 && this.OnVersionMismatched != null)
			{
				this.OnVersionMismatched(matchInformation.Version, "Release.15.00.250");
			}
			HORTAComponent.Log.DebugFormat("Match={0} ver={1} data={2} stts={3} kfrms={4}", new object[]
			{
				matchInformation.MatchId,
				matchInformation.Version,
				matchInformation.Data.ToString(),
				matchInformation.States,
				matchInformation.KeyFrames
			});
			if (this.OnMatchFileLoaded != null)
			{
				this.OnMatchFileLoaded(matchInformation);
			}
			this.MatchInfo = matchInformation;
		}

		public bool Play()
		{
			HORTAComponent.Log.DebugFormat("HORTA PLAY! Match={0}", new object[]
			{
				(this.MatchInfo != null) ? this.MatchInfo.MatchId.ToString() : "null"
			});
			if (this.MatchInfo == null || this.OnMatchReadyToPlay == null)
			{
				return false;
			}
			this.Behaviour = this._container.InstantiateComponent<HORTABehaviour>(GameHubObject.Hub.gameObject);
			this.StatePlayback = this._container.InstantiateComponent<HORTAStatePlayback>(GameHubObject.Hub.gameObject);
			this.Net = this._container.InstantiateComponent<HORTANetwork>(GameHubObject.Hub.gameObject);
			this.HORTAClock.Reset();
			this._oldNet = GameHubObject.Hub.Net;
			GameHubObject.Hub.Net = this.Net;
			this.Behaviour.Init(this, this.StatePlayback, (HORTAState)GameHubObject.Hub.State.Current, GameHubObject.Hub.Match.State, this._hortaTime);
			GameHubObject.Hub.Match = this.MatchInfo.Data;
			GameHubObject.Hub.Match.State = MatchData.MatchState.PreMatch;
			this._playback.SetBuffer(this.MatchInfo.KeyFrames);
			this.StatePlayback.Init(this, this.MatchInfo.States);
			GameHubObject.Hub.Players.AddNarrator(new PlayerData(GameHubObject.Hub.User.UserSF.PublisherUserId, 0, (!GameHubObject.Hub.Match.LevelIsTutorial()) ? TeamKind.Blue : TeamKind.Red, this.Net.GetMyAddress(), 0, false, 0, true, new BattlepassProgress()));
			GameHubObject.Hub.User.ConnectToHORTA(GameHubObject.Hub.Match);
			this.StatePlayback.RunInitialStates();
			this._playback.Init();
			this.PreParseFrames();
			if (this.OnMatchReadyToPlay != null)
			{
				this.OnMatchReadyToPlay();
			}
			HORTAComponent.Log.DebugFormat("All set go HORTA go!", new object[0]);
			return true;
		}

		private void PreParseFrames()
		{
			IndexedMatchBufferReader indexedMatchBufferReader = new IndexedMatchBufferReader(this.MatchInfo.KeyFrames);
			FrameCheck check = (IFrame x) => true;
			int lastFrameTime = 0;
			IFrame frame;
			while (indexedMatchBufferReader.ReadNext(check, out frame))
			{
				IFeatureSnapshot featureSnapshot = this._snapshots.Find((IFeatureSnapshot x) => x.Kind == (FrameKind)frame.Type);
				if (featureSnapshot != null)
				{
					featureSnapshot.AddFrame(frame);
				}
				lastFrameTime = frame.Time;
			}
			this.HORTAClock.SetLastFrameTime(lastFrameTime);
		}

		public void EndGame()
		{
			this.Behaviour.CallEndGame();
		}

		public void CleanUp()
		{
			this.MatchInfo = null;
			Object.Destroy(this.Behaviour);
			Object.Destroy(this.StatePlayback);
			Object.Destroy(this.Net);
			for (int i = 0; i < this._snapshots.Count; i++)
			{
				this._snapshots[i].Clear();
			}
			this._timeController.Reset();
			this.HORTAClock.Reset();
			GameHubObject.Hub.Net = this._oldNet;
		}

		public void ShowTimelineWindow()
		{
			ObservableExtensions.Subscribe<Unit>(Observable.ContinueWith<Unit, Unit>(this._timelinePresenter.Initialize(this._timeController), (Unit _) => this._timelinePresenter.Show()));
		}

		public void DisposeTimelineWindow()
		{
			ObservableExtensions.Subscribe<Unit>(this._timelinePresenter.Dispose());
		}

		public void HideTimelineWindow()
		{
			ObservableExtensions.Subscribe<Unit>(Observable.ContinueWith<Unit, Unit>(this._timelinePresenter.Hide(), (Unit _) => this._timelinePresenter.Dispose()));
		}

		public void ToggleTimelinePresenterVisibility()
		{
			this._timelinePresenter.ToggleVisibility();
		}

		private readonly IPlayback _playback;

		private readonly HORTATime _hortaTime;

		private readonly IHORTATimelinePresenter _timelinePresenter;

		private readonly HORTATimeController _timeController;

		private readonly HORTAPlayback _hortaPlayback;

		private readonly IConfigLoader _config;

		private readonly List<IFeatureSnapshot> _snapshots;

		private readonly IToggleFeature _toggleFeature;

		public static readonly BitLogger Log = new BitLogger(typeof(HORTAComponent));

		public HORTAStatePlayback StatePlayback;

		public HORTABehaviour Behaviour;

		public HORTANetwork Net;

		private Network _oldNet;

		private DiContainer _container;

		public int LastFrameTime;

		public readonly string DefaultFile;
	}
}
