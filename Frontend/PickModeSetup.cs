using System;
using System.Collections.Generic;
using System.Linq;
using HeavyMetalMachines.Characters;
using HeavyMetalMachines.Combat;
using HeavyMetalMachines.Match;
using HeavyMetalMachines.Matchmaking.Configuration;
using HeavyMetalMachines.Social.FriendDataUpdater.Business;
using Hoplon.ToggleableFeatures;
using Hoplon.Unity.Loading;
using Pocketverse;
using UniRx;
using UnityEngine;
using Zenject;

namespace HeavyMetalMachines.Frontend
{
	public class PickModeSetup : GameState, ConfirmPickCallback.IConfirmPickCallbackListener, ConfirmSelectionCallback.IConfirmSelectionCallbackListener, PickTimeOutCallback.IPickTimeOutCallbackListener, ConfirmSkinCallback.IConfirmSkinCallbackListener, ConfirmGridSelectionCallback.IConfirmGridSelectionCallbackListener, ConfirmGridPickCallback.IConfirmGridPickCallbackListener
	{
		public bool NoCharChosen
		{
			get
			{
				return this.ClosedPilots == null || this.ClosedPilots.Count <= 0;
			}
		}

		public float GetTimer()
		{
			return (float)((double)this._timer - ((double)Time.realtimeSinceStartup - this._lasttimegottimerfromserver));
		}

		private List<int> _lockedInCompetitiveCharacterIds { get; set; }

		protected override void OnStateDisabled()
		{
			base.OnStateDisabled();
			this.PickModeGUI = null;
			if (this._friendDataUpdaterDisposable != null)
			{
				this._friendDataUpdaterDisposable.Dispose();
				this._friendDataUpdaterDisposable = null;
			}
		}

		private void OnDisable()
		{
			this.CountdownStarted = false;
			this.ClosedPilots.Clear();
			if (this.loadingToken != null)
			{
				Loading.Engine.UnloadToken(this.loadingToken);
			}
		}

		protected override void OnStateEnabled()
		{
			this._hub = GameHubBehaviour.Hub;
			this._gameCalled = false;
			this._counselorChoseSomeone = false;
			if (this._hub.User.IsReconnecting || GameHubBehaviour.Hub.Config.GetBoolValue(ConfigAccess.SkipSwordfish, false))
			{
				this._hub.Characters.ClientAskPickModeState();
			}
		}

		protected override IObservable<Unit> OnStateEnabledAsync()
		{
			return Observable.Do<Unit>(this.GetCompetitiveLockedCharacters(), delegate(Unit _)
			{
				this.InitializeFriendDataUpdater();
			});
		}

		private void InitializeFriendDataUpdater()
		{
			this._localFriendDataUpdater = this._diContainer.ResolveId<ILocalFriendDataUpdater>(1);
			this._friendDataUpdaterDisposable = ObservableExtensions.Subscribe<Unit>(this._localFriendDataUpdater.ExecuteIndefinitely(), delegate(Unit onNext)
			{
				PickModeSetup.Log.Debug("SwordfishLocalFriendDataService onNext");
			}, delegate(Exception onError)
			{
				PickModeSetup.Log.ErrorFormat("SwordfishLocalFriendDataService Error={0}", new object[]
				{
					onError
				});
			});
		}

		private IObservable<Unit> GetCompetitiveLockedCharacters()
		{
			bool flag = GameHubBehaviour.Hub.Match.Kind == 3;
			if (flag)
			{
				return Observable.AsUnitObservable<QueueConfiguration>(Observable.Do<QueueConfiguration>(Observable.First<QueueConfiguration>(this._diContainer.Resolve<IGetThenObserveCompetitiveQueueConfiguration>().GetThenObserve()), new Action<QueueConfiguration>(this.CacheCompetitiveQueueLockedCharacters)));
			}
			this._lockedInCompetitiveCharacterIds = new List<int>(0);
			return Observable.ReturnUnit();
		}

		private void CacheCompetitiveQueueLockedCharacters(QueueConfiguration queueConfiguration)
		{
			this._lockedInCompetitiveCharacterIds = (from data in queueConfiguration.LockedCharacters
			select data.Id).ToList<int>();
		}

		protected override void OnMyLevelLoaded()
		{
			this.PickModeGUI = (PickModeGUI)GameHubBehaviour.Hub.State.CurrentSceneStateData.StateGuiController;
			this.PickModeGUI.LockedInCompetitiveCharacterIds = this._lockedInCompetitiveCharacterIds;
			this.GetTimeFromServer();
			this.PickModeGUI.PopulateCharacterGrid();
			PickModeSetup.CallDelayedEvents<ConfirmSelectionCallback>(ref this._delayedConfirmCharacterSelectionCallbacks, new Action<ConfirmSelectionCallback>(this.OnConfirmSelectionCallback));
			PickModeSetup.CallDelayedEvents<ConfirmPickCallback>(ref this._delayedConfirmCharacterPickCallbacks, new Action<ConfirmPickCallback>(this.OnConfirmPickCallback));
			PickModeSetup.CallDelayedEvents<ConfirmSkinCallback>(ref this._delayedConfirmSkinPickCallbacks, new Action<ConfirmSkinCallback>(this.OnConfirmSkinCallback));
			PickModeSetup.CallDelayedEvents<ConfirmGridSelectionCallback>(ref this._delayedConfirmGridSelectionCallbacks, new Action<ConfirmGridSelectionCallback>(this.OnConfirmGridSelectionCallback));
			PickModeSetup.CallDelayedEvents<ConfirmGridPickCallback>(ref this._delayedConfirmGridPickCallbacks, new Action<ConfirmGridPickCallback>(this.OnConfirmGridPickCallback));
			PickModeSetup.CallDelayedEvents<PickTimeOutCallback>(ref this._delayedTimeoutCallbacks, new Action<PickTimeOutCallback>(this.OnPickTimeOutCallback));
		}

		private static void CallDelayedEvents<T>(ref List<T> delayedEventList, Action<T> functionToCall) where T : Mural.IMuralMessage
		{
			for (int i = 0; i < delayedEventList.Count; i++)
			{
				T obj = delayedEventList[i];
				functionToCall(obj);
			}
			delayedEventList.Clear();
		}

		public void GetTimeFromServer()
		{
			IFuture<float> pickTime = this._hub.Characters.Async().GetPickTime();
			pickTime.WhenDone(delegate(IFuture fut)
			{
				if (fut.CancelWasCalled || fut.HasException || this.CountdownStarted)
				{
					return;
				}
				this._timer = (float)fut.Result;
				this._lasttimegottimerfromserver = (double)Time.realtimeSinceStartup;
			});
		}

		public void SelectCharacter(int characterId)
		{
			this._hub.Characters.DispatchReliable(new byte[0]).SelectCharacter(characterId);
		}

		public void OnConfirmSelectionCallback(ConfirmSelectionCallback evt)
		{
			if (!this.PickModeGUI)
			{
				this._delayedConfirmCharacterSelectionCallbacks.Add(evt);
				return;
			}
			PickModeSetup.Log.DebugFormat("OnConfirmSelectionCallback received. address={0} charId={1} message={2}", new object[]
			{
				evt.PlayerAddress,
				evt.PilotId,
				evt.Message
			});
			this.PickModeGUI.OnServerConfirmCharacterSelection((int)evt.PlayerAddress, evt.PilotId);
		}

		public void ConfirmPick(int characterId)
		{
			if (this.CheckIfAlreadyPicked(characterId))
			{
				this.PickModeGUI.ShowFeedbackWindow("CHARACTER_ALREADY_PICKED", new object[0]);
				PickModeSetup.Log.ErrorFormat("Could not pick, character already picked. PilotId = {0}", new object[]
				{
					characterId
				});
				return;
			}
			IFuture<int> future = this._hub.Characters.Async().PickCharacter();
			future.WhenDone(delegate(IFuture fut)
			{
				if (!fut.CancelWasCalled && !fut.HasException && (int)fut.Result == 1)
				{
					return;
				}
				PickResult pickResult = (PickResult)fut.Result;
				switch (pickResult)
				{
				case PickResult.CharacterNotOwned:
					this.PickModeGUI.ShowFeedbackWindow("CHARACTER_NOT_OWNED", new object[0]);
					return;
				case PickResult.CharacterAlreadyPicked:
					this.PickModeGUI.ShowFeedbackWindow("CHARACTER_ALREADY_PICKED", new object[0]);
					return;
				case PickResult.PickPhaseOver:
					this.PickModeGUI.ShowFeedbackWindow("PICK_PHASE_OVER", new object[0]);
					return;
				}
				this.PickModeGUI.ShowFeedbackWindow("PICK_ERROR", new object[]
				{
					pickResult.ToString()
				});
				PickModeSetup.Log.ErrorFormat("Unknow Future result on confirm Pick. PickResult: {0}, Exception: {1}", new object[]
				{
					pickResult,
					fut.ExceptionThrowed
				});
			});
		}

		private bool CheckIfAlreadyPicked(int characterId)
		{
			return this.ClosedPilots.Contains(characterId);
		}

		public void OnConfirmPickCallback(ConfirmPickCallback evt)
		{
			if (!this.PickModeGUI)
			{
				this._delayedConfirmCharacterPickCallbacks.Add(evt);
				return;
			}
			PickModeSetup.Log.DebugFormat("OnConfirmPickCallback received. address={0} charId={1} teamKind={2}", new object[]
			{
				evt.PlayerAddress,
				evt.CharacterId,
				evt.TeamKind
			});
			if (evt.TeamKind == (int)this._hub.Players.CurrentPlayerData.Team)
			{
				this.ClosedPilots.Add(evt.CharacterId);
			}
			if (evt.PlayerAddress == this._hub.Players.CurrentPlayerData.PlayerAddress)
			{
				this.PickModeGUI.OnServerConfirmMyPick(evt.CharacterId, evt.LastSkin);
				return;
			}
			this.PickModeGUI.OnServerConfirmSomePlayerPick((int)evt.PlayerAddress, evt.TeamKind, evt.CharacterId);
		}

		public void ConfirmSkin(Guid characterGuid, Guid skinGuid)
		{
			this._hub.Characters.DispatchReliable(new byte[0]).ConfirmSkin(characterGuid.ToString(), skinGuid.ToString());
		}

		public void OnConfirmSkinCallback(ConfirmSkinCallback evt)
		{
			if (!evt.Success)
			{
				return;
			}
			if (!this.PickModeGUI)
			{
				this._delayedConfirmSkinPickCallbacks.Add(evt);
				return;
			}
			this.PickModeGUI.OnServerConfirmSkin(evt);
		}

		public void SelectGrid(int gridIndex)
		{
			this._hub.Characters.DispatchReliable(new byte[0]).SelectGrid(gridIndex);
		}

		public void OnConfirmGridSelectionCallback(ConfirmGridSelectionCallback evt)
		{
			if (!this.PickModeGUI)
			{
				this._delayedConfirmGridSelectionCallbacks.Add(evt);
				return;
			}
			PickModeSetup.Log.Debug("OnConfirmGridSelectionCallback received");
			this.PickModeGUI.OnConfirmGridSelection(evt.PlayerAddress, evt.GridIndex);
		}

		public void PickGrid()
		{
			IFuture<int> future = this._hub.Characters.Async().PickGrid();
			future.WhenDone(delegate(IFuture fut)
			{
				if (!fut.CancelWasCalled && !fut.HasException && (PickGridResult)fut.Result == PickGridResult.Ok)
				{
					return;
				}
				PickGridResult pickGridResult = (PickGridResult)fut.Result;
				if (pickGridResult != PickGridResult.InvalidGridIndex)
				{
					if (pickGridResult != PickGridResult.GridAlreadyPicked)
					{
						this.PickModeGUI.ShowFeedbackWindow("GRID_PICK_ERROR", new object[]
						{
							pickGridResult.ToString()
						});
						PickModeSetup.Log.ErrorFormat("Unknow Future result on confirm Grid Pick. GridPickResult: {0}, Exception: {1}", new object[]
						{
							pickGridResult,
							fut.ExceptionThrowed
						});
					}
					else
					{
						this.PickModeGUI.ShowFeedbackWindow("GRID_ALREADY_PICKED", new object[0]);
					}
				}
				else
				{
					this.PickModeGUI.ShowFeedbackWindow("INVALID_GRID_INDEX", new object[0]);
				}
			});
		}

		public void OnConfirmGridPickCallback(ConfirmGridPickCallback evt)
		{
			if (!this.PickModeGUI)
			{
				this._delayedConfirmGridPickCallbacks.Add(evt);
				return;
			}
			PickModeSetup.Log.Debug("OnConfirmGridPickCallback received");
			this.PickModeGUI.OnConfirmGridPick(evt.PlayerAddress, evt.GridIndex);
		}

		private float DeltaTimeSincePick()
		{
			return Time.realtimeSinceStartup - this._realTimeOnPick;
		}

		private void Update()
		{
			MatchData.MatchState state = this._hub.Match.State;
			if (state != MatchData.MatchState.CharacterPick)
			{
				if (state == MatchData.MatchState.MatchStarted || state == MatchData.MatchState.PreMatch)
				{
					if (this.GetTimer() < -3f || this.DeltaTimeSincePick() > 3f)
					{
						this.GoToLoading();
					}
				}
			}
			else
			{
				this._realTimeOnPick = Time.realtimeSinceStartup;
			}
		}

		private void GoToLoading()
		{
			if (this._gameCalled)
			{
				return;
			}
			this._gameCalled = true;
			HudWindowManager.Instance.CloseAll();
			base.GoToState(this.loadingState, false);
			if (this.PickModeGUI)
			{
				this.PickModeGUI.PlayEndPickScreenAudio();
			}
		}

		public void OnPickTimeOutCallback(PickTimeOutCallback evt)
		{
			if (!this.PickModeGUI)
			{
				this._delayedTimeoutCallbacks.Add(evt);
				return;
			}
			PickModeSetup.Log.DebugFormat("OnPickTimeOutCallback received. time={0}", new object[]
			{
				evt.CustomizationTime
			});
			this._timer = evt.CustomizationTime;
			this._lasttimegottimerfromserver = (double)Time.realtimeSinceStartup;
			this.CountdownStarted = true;
			this.PickModeGUI.OnCountdownStarted();
		}

		private void OnApplicationQuit()
		{
			GameHubBehaviour.Hub.Server.ClientSendPlayerDisconnectInfo();
		}

		private static readonly BitLogger Log = new BitLogger(typeof(PickModeSetup));

		[Inject]
		private DiContainer _diContainer;

		[Inject]
		private IIsFeatureToggled _isFeatureToggled;

		private ILocalFriendDataUpdater _localFriendDataUpdater;

		private IDisposable _friendDataUpdaterDisposable;

		private HMMHub _hub;

		public PickModeGUI PickModeGUI;

		public List<int> ClosedPilots = new List<int>();

		public LoadingState loadingState;

		private LoadingToken loadingToken;

		private float _timer;

		private double _lasttimegottimerfromserver;

		private PickModeSetup.CounselorChosen _chosen;

		private bool _counselorChoseSomeone;

		private List<ConfirmPickCallback> _delayedConfirmCharacterPickCallbacks = new List<ConfirmPickCallback>();

		private List<ConfirmSelectionCallback> _delayedConfirmCharacterSelectionCallbacks = new List<ConfirmSelectionCallback>();

		private List<ConfirmSkinCallback> _delayedConfirmSkinPickCallbacks = new List<ConfirmSkinCallback>();

		private List<ConfirmGridSelectionCallback> _delayedConfirmGridSelectionCallbacks = new List<ConfirmGridSelectionCallback>();

		private List<ConfirmGridPickCallback> _delayedConfirmGridPickCallbacks = new List<ConfirmGridPickCallback>();

		private List<PickTimeOutCallback> _delayedTimeoutCallbacks = new List<PickTimeOutCallback>();

		private bool _endScreenAudioPlayed;

		private float _realTimeOnPick;

		private bool _gameCalled;

		[NonSerialized]
		public bool CountdownStarted;

		private struct CounselorChosen
		{
			public int Character;

			public Guid Skin;
		}
	}
}
