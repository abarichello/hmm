using System;
using System.Linq;
using HeavyMetalMachines.CharacterSelection.Client.Picking;
using HeavyMetalMachines.CharacterSelection.Communication.SerializedData;
using HeavyMetalMachines.CharacterSelection.Server.Infra;
using HeavyMetalMachines.CharacterSelection.State;
using HeavyMetalMachines.Match;
using HeavyMetalMachines.Matches;
using Hoplon.Assertions;
using Pocketverse;
using UniRx;
using UnityEngine;
using Zenject;

namespace HeavyMetalMachines.CharacterSelection.Communication
{
	[RemoteClass]
	public class CharacterSelectionInitializationRpc : MonoBehaviour, ISendInitializationData, IListenForInitializationData, IBitComponent
	{
		private void Awake()
		{
			this._receivedInitializationDataSubject = new Subject<CharacterSelectionInitializationData>();
		}

		public void Send(CharacterSelectionInitializationData data, MatchClient matchClient)
		{
			PlayerData anyByPlayerId = this._matchPlayers.GetAnyByPlayerId(matchClient.PlayerId);
			this.DispatchReliable(new byte[]
			{
				anyByPlayerId.PlayerAddress
			}).SendInitializationData(new InitializationDataSerialized(data));
		}

		[RemoteMethod]
		public void SendInitializationData(InitializationDataSerialized dataSerialized)
		{
			Assert.IsNotNull<InitializationDataSerialized>(dataSerialized, "Received null CharacterSelectionInitializationData.");
			Match match = GetCurrentMatchExtensions.Get(this._getCurrentMatch);
			PlayerState[] playerStates = dataSerialized.Data.State.Team1State.PlayerStates;
			for (int i = 0; i < playerStates.Length; i++)
			{
				PlayerState state = playerStates[i];
				state.Client = match.Clients.First((MatchClient client) => client == state.Client);
			}
			PlayerState[] playerStates2 = dataSerialized.Data.State.Team2State.PlayerStates;
			for (int j = 0; j < playerStates2.Length; j++)
			{
				PlayerState state = playerStates2[j];
				state.Client = match.Clients.First((MatchClient client) => client == state.Client);
			}
			this._receivedInitializationDataSubject.OnNext(dataSerialized.Data);
		}

		public IObservable<CharacterSelectionInitializationData> Listen(MatchClient localClient)
		{
			return this._receivedInitializationDataSubject;
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

		public ICharacterSelectionInitializationRpcAsync Async()
		{
			return this.Async(0);
		}

		public ICharacterSelectionInitializationRpcAsync Async(byte to)
		{
			if (this._async == null)
			{
				this._async = new CharacterSelectionInitializationRpcAsync(this.OID);
			}
			return (to != 0) ? this._async.To(to) : this._async.To();
		}

		public ICharacterSelectionInitializationRpcDispatch DispatchReliable(params byte[] to)
		{
			if (this._dispatch == null)
			{
				this._dispatch = new CharacterSelectionInitializationRpcDispatch(this.OID);
			}
			this._dispatch.Reliable(true);
			return (to != null) ? this._dispatch.To(to) : this._dispatch.To();
		}

		public ICharacterSelectionInitializationRpcDispatch Dispatch(params byte[] to)
		{
			if (this._dispatch == null)
			{
				this._dispatch = new CharacterSelectionInitializationRpcDispatch(this.OID);
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
			if (methodId != 3)
			{
				throw new ScriptMethodNotFoundException(classId, (int)methodId);
			}
			this.SendInitializationData((InitializationDataSerialized)args[0]);
			return null;
		}

		[Inject]
		private IMatchPlayers _matchPlayers;

		[Inject]
		private IGetCurrentMatch _getCurrentMatch;

		private Subject<CharacterSelectionInitializationData> _receivedInitializationDataSubject;

		public const int StaticClassId = 1057;

		private Identifiable _identifiable;

		[ThreadStatic]
		private CharacterSelectionInitializationRpcAsync _async;

		[ThreadStatic]
		private CharacterSelectionInitializationRpcDispatch _dispatch;

		private IFuture _delayed;
	}
}
