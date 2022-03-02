using System;
using System.Collections.Generic;
using System.Diagnostics;
using HeavyMetalMachines.Infra.Context;
using HeavyMetalMachines.Match;
using HeavyMetalMachines.Swordfish.API;
using Pocketverse;
using Pocketverse.MuralContext;
using Zenject;

namespace HeavyMetalMachines.BI
{
	[RemoteClass]
	public class PlayerTechnicalExperienceManager : GameHubBehaviour, ICleanupListener, ClientDisconnectMessage.IClientDisconnectListener, ClientReconnectMessage.IClientReconnectListener, IBitComponent
	{
		private void Awake()
		{
			this._updater = new TimedUpdater(1000, false, true);
			GameHubBehaviour.Hub.PlayerExperienceBI = this;
		}

		public void OnClientReconnect(ClientReconnectMessage msg)
		{
			PlayerData playerByAddress = GameHubBehaviour.Hub.Players.GetPlayerByAddress(msg.Session.ConnectionId);
			if (playerByAddress == null || playerByAddress.IsNarrator)
			{
				return;
			}
			this.AddPlayer(playerByAddress.UserId, playerByAddress.PlayerAddress);
		}

		public void AddPlayer(string uid, byte address)
		{
			if (this.ExperienceDatas.ContainsKey(address))
			{
				return;
			}
			this.ExperienceDatas.Add(address, new PlayerTechnicalExperienceManager.PlayerExperienceData
			{
				PlayerId = uid
			});
		}

		public void OnClientDisconnect(ClientDisconnectMessage msg)
		{
			this.OnDisconnect(msg.Session.ConnectionId);
		}

		private void OnDisconnect(byte sender)
		{
			PlayerTechnicalExperienceManager.PlayerExperienceData playerExperienceData;
			if (!this.ExperienceDatas.TryGetValue(sender, out playerExperienceData))
			{
				return;
			}
			playerExperienceData.SavePlayerExperienceOnDisconnect();
		}

		public ExperienceDataSet ClientData
		{
			get
			{
				return this._clientData;
			}
		}

		public void SetFreezeState(bool isFreezing)
		{
			this._isFrozenThisFrame = (this._isFrozenThisFrame || isFreezing);
		}

		private void DetectFreeze()
		{
			if (!this._wasPreviouslyFrozen && this._isFrozenThisFrame)
			{
				this._freezeTimer.Start();
			}
			else if (this._wasPreviouslyFrozen && !this._isFrozenThisFrame)
			{
				this._freezeTimer.Stop();
				this.AddFreeze(this._freezeTimer.ElapsedMilliseconds);
				this._freezeTimer.Reset();
			}
			this._wasPreviouslyFrozen = this._isFrozenThisFrame;
			this._isFrozenThisFrame = false;
		}

		private void AddFreeze(long millis)
		{
			this._clientData.FreezeAcc = this._clientData.FreezeAcc + millis;
			this._clientData.FreezeCount = this._clientData.FreezeCount + 1;
			if (this._clientData.FreezeCount % 10 == 1)
			{
				PlayerTechnicalExperienceManager.Log.DebugFormat("Current fcount={0} facc={1}", new object[]
				{
					this._clientData.FreezeCount,
					this._clientData.FreezeAcc
				});
			}
			if (this._clientData.FreezeCount <= this._configLoader.GetIntValue(ConfigAccess.FreezeLogCount))
			{
				this._swordfishLogProvider.GetSwordfishLog().BILogClientMsg(92, string.Format("MatchTime={0} FreezeTime={1}", this._gameTime.MatchTimer.GetTimeSeconds(), millis), false);
			}
		}

		public void OnCleanup(CleanupMessage msg)
		{
			if (this._clientData.FreezeCount > 0)
			{
				PlayerTechnicalExperienceManager.Log.InfoFormat("Last fcount={0} facc={1}", new object[]
				{
					this._clientData.FreezeCount,
					this._clientData.FreezeAcc
				});
			}
			this._wasPreviouslyFrozen = false;
			this._isFrozenThisFrame = false;
			this._clientData = default(ExperienceDataSet);
			this._freezeTimer.Reset();
		}

		private void Update()
		{
			if (GameHubBehaviour.Hub.Net.IsServer() || !GameHubBehaviour.Hub.Net.IsConnected() || GameHubBehaviour.Hub.User.IsNarrator)
			{
				return;
			}
			if (this._scoreBoard.CurrentState != BombScoreboardState.BombDelivery && this._scoreBoard.CurrentState != BombScoreboardState.PreReplay)
			{
				if (this._wasPreviouslyFrozen)
				{
					this._isFrozenThisFrame = false;
					this.DetectFreeze();
				}
				return;
			}
			this.DetectFreeze();
			if (this._updater.ShouldHalt())
			{
				return;
			}
			this.SendPlayerExperienceData();
		}

		public void SendPlayerExperienceData()
		{
			this.Dispatch(new byte[0]).ReceivePlayerExperienceData(this._clientData);
		}

		[RemoteMethod]
		private void ReceivePlayerExperienceData(ExperienceDataSet data)
		{
			PlayerTechnicalExperienceManager.PlayerExperienceData playerExperienceData;
			if (!this.ExperienceDatas.TryGetValue(this.Sender, out playerExperienceData))
			{
				return;
			}
			playerExperienceData.SetCurrent(data);
		}

		private int OID
		{
			get
			{
				if (!this._identifiable)
				{
					this._identifiable = base.GetComponent<Identifiable>();
				}
				return this._identifiable.ObjId;
			}
		}

		public byte Sender { get; set; }

		public IPlayerTechnicalExperienceManagerAsync Async()
		{
			return this.Async(0);
		}

		public IPlayerTechnicalExperienceManagerAsync Async(byte to)
		{
			if (this._async == null)
			{
				this._async = new PlayerTechnicalExperienceManagerAsync(this.OID);
			}
			return (to != 0) ? this._async.To(to) : this._async.To();
		}

		public IPlayerTechnicalExperienceManagerDispatch DispatchReliable(params byte[] to)
		{
			if (this._dispatch == null)
			{
				this._dispatch = new PlayerTechnicalExperienceManagerDispatch(this.OID);
			}
			this._dispatch.Reliable(true);
			return (to != null) ? this._dispatch.To(to) : this._dispatch.To();
		}

		public IPlayerTechnicalExperienceManagerDispatch Dispatch(params byte[] to)
		{
			if (this._dispatch == null)
			{
				this._dispatch = new PlayerTechnicalExperienceManagerDispatch(this.OID);
			}
			this._dispatch.Reliable(false);
			return (to != null) ? this._dispatch.To(to) : this._dispatch.To();
		}

		protected IFuture Delayed
		{
			get
			{
				return this._delayed;
			}
		}

		protected void Delay(IFuture future)
		{
			this._delayed = future;
		}

		public object Invoke(int classId, short methodId, object[] args, BitStream bitstream = null)
		{
			this._delayed = null;
			if (methodId != 12)
			{
				throw new ScriptMethodNotFoundException(classId, (int)methodId);
			}
			this.ReceivePlayerExperienceData((ExperienceDataSet)args[0]);
			return null;
		}

		private static readonly BitLogger Log = new BitLogger(typeof(PlayerTechnicalExperienceManager));

		[Inject]
		private ISwordfishLogProvider _swordfishLogProvider;

		[Inject]
		private IGameTime _gameTime;

		[Inject]
		private IConfigLoader _configLoader;

		[Inject]
		private IScoreBoard _scoreBoard;

		public Dictionary<byte, PlayerTechnicalExperienceManager.PlayerExperienceData> ExperienceDatas = new Dictionary<byte, PlayerTechnicalExperienceManager.PlayerExperienceData>();

		private ExperienceDataSet _clientData;

		private Stopwatch _freezeTimer = new Stopwatch();

		private bool _wasPreviouslyFrozen;

		private bool _isFrozenThisFrame;

		private TimedUpdater _updater;

		public const int StaticClassId = 1079;

		private Identifiable _identifiable;

		[ThreadStatic]
		private PlayerTechnicalExperienceManagerAsync _async;

		[ThreadStatic]
		private PlayerTechnicalExperienceManagerDispatch _dispatch;

		private IFuture _delayed;

		public class PlayerExperienceData
		{
			public ExperienceDataSet Total
			{
				get
				{
					return this._accumulated + this._current;
				}
			}

			public void SetCurrent(ExperienceDataSet cur)
			{
				if (cur > this._current)
				{
					this._current = cur;
				}
			}

			public void SavePlayerExperienceOnDisconnect()
			{
				this._accumulated += this._current;
				this._current = default(ExperienceDataSet);
			}

			private ExperienceDataSet _current;

			private ExperienceDataSet _accumulated;

			public string PlayerId;
		}
	}
}
