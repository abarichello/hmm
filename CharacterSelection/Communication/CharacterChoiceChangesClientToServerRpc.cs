using System;
using System.Linq;
using HeavyMetalMachines.CharacterSelection.Client.Picking;
using HeavyMetalMachines.CharacterSelection.Picking;
using HeavyMetalMachines.CharacterSelection.Server.Infra;
using HeavyMetalMachines.Match;
using HeavyMetalMachines.Matches;
using Pocketverse;
using UniRx;
using UnityEngine;
using Zenject;

namespace HeavyMetalMachines.CharacterSelection.Communication
{
	[RemoteClass]
	public class CharacterChoiceChangesClientToServerRpc : MonoBehaviour, ISendCharacterChoiceChanges, IListenForClientCharacterChoiceChanges, IBitComponent
	{
		public void Send(ClientCharacterChoice characterChoice)
		{
			this.DispatchReliable(new byte[0]).CharacterChoiceReceived(characterChoice.CharacterId);
		}

		[RemoteMethod]
		public void CharacterChoiceReceived(Guid characterId)
		{
			PlayerData player = this._matchPlayers.GetPlayerByAddress(this.Sender);
			MatchClient client2 = GetCurrentMatchExtensions.Get(this._getCurrentMatch).Clients.First((MatchClient client) => client.PlayerId == player.PlayerId);
			ClientCharacterChoice clientCharacterChoice = new ClientCharacterChoice
			{
				Client = client2,
				CharacterId = characterId
			};
			this._listenSubject.OnNext(clientCharacterChoice);
		}

		public IObservable<ClientCharacterChoice> ListenForClientCharacterChoiceChanges()
		{
			return this._listenSubject;
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

		public ICharacterChoiceChangesClientToServerRpcAsync Async()
		{
			return this.Async(0);
		}

		public ICharacterChoiceChangesClientToServerRpcAsync Async(byte to)
		{
			if (this._async == null)
			{
				this._async = new CharacterChoiceChangesClientToServerRpcAsync(this.OID);
			}
			return (to != 0) ? this._async.To(to) : this._async.To();
		}

		public ICharacterChoiceChangesClientToServerRpcDispatch DispatchReliable(params byte[] to)
		{
			if (this._dispatch == null)
			{
				this._dispatch = new CharacterChoiceChangesClientToServerRpcDispatch(this.OID);
			}
			this._dispatch.Reliable(true);
			return (to != null) ? this._dispatch.To(to) : this._dispatch.To();
		}

		public ICharacterChoiceChangesClientToServerRpcDispatch Dispatch(params byte[] to)
		{
			if (this._dispatch == null)
			{
				this._dispatch = new CharacterChoiceChangesClientToServerRpcDispatch(this.OID);
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
			if (methodId != 2)
			{
				throw new ScriptMethodNotFoundException(classId, (int)methodId);
			}
			this.CharacterChoiceReceived((Guid)args[0]);
			return null;
		}

		[Inject]
		private IMatchPlayers _matchPlayers;

		[Inject]
		private IGetCurrentMatch _getCurrentMatch;

		private readonly Subject<ClientCharacterChoice> _listenSubject = new Subject<ClientCharacterChoice>();

		public const int StaticClassId = 1049;

		private Identifiable _identifiable;

		[ThreadStatic]
		private CharacterChoiceChangesClientToServerRpcAsync _async;

		[ThreadStatic]
		private CharacterChoiceChangesClientToServerRpcDispatch _dispatch;

		private IFuture _delayed;
	}
}
