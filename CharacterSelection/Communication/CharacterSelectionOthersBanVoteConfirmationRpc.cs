using System;
using System.Collections.Generic;
using System.Linq;
using HeavyMetalMachines.CharacterSelection.Banning;
using HeavyMetalMachines.CharacterSelection.Client.Picking;
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
	public class CharacterSelectionOthersBanVoteConfirmationRpc : MonoBehaviour, IListenForBanVoteConfirmations, IBroadcastBanVoteConfirmation, IBitComponent
	{
		private void Awake()
		{
			this._listenSubject = new Subject<ServerBanVoteConfirmation>();
		}

		public void Broadcast(ServerBanVoteConfirmation voter, MatchClient[] receivers)
		{
			ServerBanVoteSerializable client2 = new ServerBanVoteSerializable
			{
				Data = voter
			};
			IEnumerable<byte> source = from client in receivers
			select this._matchPlayers.GetAnyByPlayerId(client.PlayerId).PlayerAddress;
			this.DispatchReliable(source.ToArray<byte>()).BroadcastBanVoteConfirmation(client2);
		}

		public IObservable<ServerBanVoteConfirmation> Listen(MatchClient localClient)
		{
			return this._listenSubject;
		}

		[RemoteMethod]
		public void BroadcastBanVoteConfirmation(ServerBanVoteSerializable client)
		{
			this._listenSubject.OnNext(client.Data);
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

		public ICharacterSelectionOthersBanVoteConfirmationRpcAsync Async()
		{
			return this.Async(0);
		}

		public ICharacterSelectionOthersBanVoteConfirmationRpcAsync Async(byte to)
		{
			if (this._async == null)
			{
				this._async = new CharacterSelectionOthersBanVoteConfirmationRpcAsync(this.OID);
			}
			return (to != 0) ? this._async.To(to) : this._async.To();
		}

		public ICharacterSelectionOthersBanVoteConfirmationRpcDispatch DispatchReliable(params byte[] to)
		{
			if (this._dispatch == null)
			{
				this._dispatch = new CharacterSelectionOthersBanVoteConfirmationRpcDispatch(this.OID);
			}
			this._dispatch.Reliable(true);
			return (to != null) ? this._dispatch.To(to) : this._dispatch.To();
		}

		public ICharacterSelectionOthersBanVoteConfirmationRpcDispatch Dispatch(params byte[] to)
		{
			if (this._dispatch == null)
			{
				this._dispatch = new CharacterSelectionOthersBanVoteConfirmationRpcDispatch(this.OID);
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
			if (methodId != 4)
			{
				throw new ScriptMethodNotFoundException(classId, (int)methodId);
			}
			this.BroadcastBanVoteConfirmation((ServerBanVoteSerializable)args[0]);
			return null;
		}

		[Inject]
		private IMatchPlayers _matchPlayers;

		private Subject<ServerBanVoteConfirmation> _listenSubject;

		public const int StaticClassId = 1058;

		private Identifiable _identifiable;

		[ThreadStatic]
		private CharacterSelectionOthersBanVoteConfirmationRpcAsync _async;

		[ThreadStatic]
		private CharacterSelectionOthersBanVoteConfirmationRpcDispatch _dispatch;

		private IFuture _delayed;
	}
}
