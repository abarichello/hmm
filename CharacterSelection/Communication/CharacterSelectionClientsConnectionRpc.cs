using System;
using System.Collections.Generic;
using System.Linq;
using HeavyMetalMachines.CharacterSelection.Client.Infra;
using HeavyMetalMachines.CharacterSelection.Communication.SerializedData;
using HeavyMetalMachines.CharacterSelection.Server.Infra;
using HeavyMetalMachines.Matches;
using Pocketverse;
using UniRx;
using UnityEngine;
using Zenject;

namespace HeavyMetalMachines.CharacterSelection.Communication
{
	[RemoteClass]
	public class CharacterSelectionClientsConnectionRpc : MonoBehaviour, IListenForClientsConnection, IBroadcastClientsConnection, IBitComponent
	{
		public IObservable<MatchClient> OnClientDisconnected(MatchClient localClient)
		{
			return this._onClientDisconnectedSubject;
		}

		public IObservable<MatchClient> OnClientReconnected(MatchClient localClient)
		{
			return this._onClientReconnectedSubject;
		}

		public void BroadcastClientDisconnected(MatchClient disconnectedClient, MatchClient[] recipientClients)
		{
			IEnumerable<byte> source = from client in recipientClients
			select this._matchPlayers.GetAnyByPlayerId(client.PlayerId).PlayerAddress;
			MatchClientSerializable client2 = new MatchClientSerializable
			{
				Data = disconnectedClient
			};
			this.DispatchReliable(source.ToArray<byte>()).BroadcastClientDisconnectedRemote(client2);
		}

		public void BroadcastClientReconnected(MatchClient reconnectedClient, MatchClient[] recipientClients)
		{
			IEnumerable<byte> source = from client in recipientClients
			select this._matchPlayers.GetAnyByPlayerId(client.PlayerId).PlayerAddress;
			MatchClientSerializable client2 = new MatchClientSerializable
			{
				Data = reconnectedClient
			};
			this.DispatchReliable(source.ToArray<byte>()).BroadcastClientReconnectedRemote(client2);
		}

		[RemoteMethod]
		public void BroadcastClientDisconnectedRemote(MatchClientSerializable client)
		{
			this._onClientDisconnectedSubject.OnNext(client.Data);
		}

		[RemoteMethod]
		public void BroadcastClientReconnectedRemote(MatchClientSerializable client)
		{
			this._onClientReconnectedSubject.OnNext(client.Data);
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

		public ICharacterSelectionClientsConnectionRpcAsync Async()
		{
			return this.Async(0);
		}

		public ICharacterSelectionClientsConnectionRpcAsync Async(byte to)
		{
			if (this._async == null)
			{
				this._async = new CharacterSelectionClientsConnectionRpcAsync(this.OID);
			}
			return (to != 0) ? this._async.To(to) : this._async.To();
		}

		public ICharacterSelectionClientsConnectionRpcDispatch DispatchReliable(params byte[] to)
		{
			if (this._dispatch == null)
			{
				this._dispatch = new CharacterSelectionClientsConnectionRpcDispatch(this.OID);
			}
			this._dispatch.Reliable(true);
			return (to != null) ? this._dispatch.To(to) : this._dispatch.To();
		}

		public ICharacterSelectionClientsConnectionRpcDispatch Dispatch(params byte[] to)
		{
			if (this._dispatch == null)
			{
				this._dispatch = new CharacterSelectionClientsConnectionRpcDispatch(this.OID);
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
			if (methodId == 5)
			{
				this.BroadcastClientDisconnectedRemote((MatchClientSerializable)args[0]);
				return null;
			}
			if (methodId != 6)
			{
				throw new ScriptMethodNotFoundException(classId, (int)methodId);
			}
			this.BroadcastClientReconnectedRemote((MatchClientSerializable)args[0]);
			return null;
		}

		[Inject]
		private IMatchPlayers _matchPlayers;

		private readonly Subject<MatchClient> _onClientDisconnectedSubject = new Subject<MatchClient>();

		private readonly Subject<MatchClient> _onClientReconnectedSubject = new Subject<MatchClient>();

		public const int StaticClassId = 1054;

		private Identifiable _identifiable;

		[ThreadStatic]
		private CharacterSelectionClientsConnectionRpcAsync _async;

		[ThreadStatic]
		private CharacterSelectionClientsConnectionRpcDispatch _dispatch;

		private IFuture _delayed;
	}
}
