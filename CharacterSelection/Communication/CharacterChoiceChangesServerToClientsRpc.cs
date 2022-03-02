using System;
using System.Collections.Generic;
using System.Linq;
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
	public class CharacterChoiceChangesServerToClientsRpc : MonoBehaviour, IListenForCharacterChoiceChanges, IBroadcastCharacterChoiceChanges, IBitComponent
	{
		public IObservable<CharacterChoice> Listen(MatchClient localClient)
		{
			return this._listenSubject;
		}

		public void Broadcast(CharacterChoice characterChoice, MatchClient[] clients)
		{
			IEnumerable<byte> source = from client in clients
			select this._matchPlayers.GetAnyByPlayerId(client.PlayerId).PlayerAddress;
			CharacterChoiceSerialized characterChoiceSerialized = new CharacterChoiceSerialized
			{
				CharacterChoice = characterChoice
			};
			this.DispatchReliable(source.ToArray<byte>()).CharacterChoiceReceived(characterChoiceSerialized);
		}

		[RemoteMethod]
		public void CharacterChoiceReceived(CharacterChoiceSerialized characterChoiceSerialized)
		{
			this._listenSubject.OnNext(characterChoiceSerialized.CharacterChoice);
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

		public ICharacterChoiceChangesServerToClientsRpcAsync Async()
		{
			return this.Async(0);
		}

		public ICharacterChoiceChangesServerToClientsRpcAsync Async(byte to)
		{
			if (this._async == null)
			{
				this._async = new CharacterChoiceChangesServerToClientsRpcAsync(this.OID);
			}
			return (to != 0) ? this._async.To(to) : this._async.To();
		}

		public ICharacterChoiceChangesServerToClientsRpcDispatch DispatchReliable(params byte[] to)
		{
			if (this._dispatch == null)
			{
				this._dispatch = new CharacterChoiceChangesServerToClientsRpcDispatch(this.OID);
			}
			this._dispatch.Reliable(true);
			return (to != null) ? this._dispatch.To(to) : this._dispatch.To();
		}

		public ICharacterChoiceChangesServerToClientsRpcDispatch Dispatch(params byte[] to)
		{
			if (this._dispatch == null)
			{
				this._dispatch = new CharacterChoiceChangesServerToClientsRpcDispatch(this.OID);
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
			this.CharacterChoiceReceived((CharacterChoiceSerialized)args[0]);
			return null;
		}

		[Inject]
		private IMatchPlayers _matchPlayers;

		private readonly Subject<CharacterChoice> _listenSubject = new Subject<CharacterChoice>();

		public const int StaticClassId = 1050;

		private Identifiable _identifiable;

		[ThreadStatic]
		private CharacterChoiceChangesServerToClientsRpcAsync _async;

		[ThreadStatic]
		private CharacterChoiceChangesServerToClientsRpcDispatch _dispatch;

		private IFuture _delayed;
	}
}
