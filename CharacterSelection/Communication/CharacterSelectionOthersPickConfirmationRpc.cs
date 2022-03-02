using System;
using System.Collections.Generic;
using System.Linq;
using HeavyMetalMachines.CharacterSelection.Client.Picking;
using HeavyMetalMachines.CharacterSelection.Communication.SerializedData;
using HeavyMetalMachines.CharacterSelection.Picking;
using HeavyMetalMachines.CharacterSelection.Server.Infra;
using HeavyMetalMachines.Matches;
using Pocketverse;
using UniRx;
using UnityEngine;
using Zenject;

namespace HeavyMetalMachines.CharacterSelection.Communication
{
	[RemoteClass]
	public class CharacterSelectionOthersPickConfirmationRpc : MonoBehaviour, IBroadcastPickConfirmation, IListenForPickConfirmations, IBitComponent
	{
		private void Awake()
		{
			this._listenSubject = new Subject<PickConfirmation>();
		}

		public void Broadcast(PickConfirmation pickConfirmation, MatchClient[] clients)
		{
			IEnumerable<byte> source = from client in clients
			select this._matchPlayers.GetAnyByPlayerId(client.PlayerId).PlayerAddress;
			PickConfirmationSerialized pickConfirmation2 = new PickConfirmationSerialized
			{
				PickConfirmation = pickConfirmation
			};
			this.DispatchReliable(source.ToArray<byte>()).BroadcastPickConfirmation(pickConfirmation2);
		}

		public IObservable<PickConfirmation> Listen(MatchClient localClient)
		{
			return this._listenSubject;
		}

		[RemoteMethod]
		public void BroadcastPickConfirmation(PickConfirmationSerialized pickConfirmation)
		{
			this._listenSubject.OnNext(pickConfirmation.PickConfirmation);
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

		public ICharacterSelectionOthersPickConfirmationRpcAsync Async()
		{
			return this.Async(0);
		}

		public ICharacterSelectionOthersPickConfirmationRpcAsync Async(byte to)
		{
			if (this._async == null)
			{
				this._async = new CharacterSelectionOthersPickConfirmationRpcAsync(this.OID);
			}
			return (to != 0) ? this._async.To(to) : this._async.To();
		}

		public ICharacterSelectionOthersPickConfirmationRpcDispatch DispatchReliable(params byte[] to)
		{
			if (this._dispatch == null)
			{
				this._dispatch = new CharacterSelectionOthersPickConfirmationRpcDispatch(this.OID);
			}
			this._dispatch.Reliable(true);
			return (to != null) ? this._dispatch.To(to) : this._dispatch.To();
		}

		public ICharacterSelectionOthersPickConfirmationRpcDispatch Dispatch(params byte[] to)
		{
			if (this._dispatch == null)
			{
				this._dispatch = new CharacterSelectionOthersPickConfirmationRpcDispatch(this.OID);
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
			this.BroadcastPickConfirmation((PickConfirmationSerialized)args[0]);
			return null;
		}

		[Inject]
		private IMatchPlayers _matchPlayers;

		private Subject<PickConfirmation> _listenSubject;

		public const int StaticClassId = 1059;

		private Identifiable _identifiable;

		[ThreadStatic]
		private CharacterSelectionOthersPickConfirmationRpcAsync _async;

		[ThreadStatic]
		private CharacterSelectionOthersPickConfirmationRpcDispatch _dispatch;

		private IFuture _delayed;
	}
}
