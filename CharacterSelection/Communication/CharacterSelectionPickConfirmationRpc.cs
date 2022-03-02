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
	public class CharacterSelectionPickConfirmationRpc : MonoBehaviour, IListenForPickConfirmation, ISendPickConfirmation, IBitComponent
	{
		private void Awake()
		{
			this._pickConfirmedSubject = new Subject<ClientPickConfirmation>();
		}

		public void Send(ClientPickConfirmation pickConfirmation)
		{
			this.DispatchReliable(new byte[0]).SendPickConfirmation(pickConfirmation.CharacterId);
		}

		public IObservable<ClientPickConfirmation> ListenForPickConfirmations()
		{
			return this._pickConfirmedSubject;
		}

		[RemoteMethod]
		public void SendPickConfirmation(Guid characterId)
		{
			PlayerData player = this._matchPlayers.GetPlayerByAddress(this.Sender);
			MatchClient client2 = GetCurrentMatchExtensions.Get(this._getCurrentMatch).Clients.First((MatchClient client) => client.PlayerId == player.PlayerId);
			ClientPickConfirmation clientPickConfirmation = new ClientPickConfirmation
			{
				CharacterId = characterId,
				Client = client2
			};
			this._pickConfirmedSubject.OnNext(clientPickConfirmation);
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

		public ICharacterSelectionPickConfirmationRpcAsync Async()
		{
			return this.Async(0);
		}

		public ICharacterSelectionPickConfirmationRpcAsync Async(byte to)
		{
			if (this._async == null)
			{
				this._async = new CharacterSelectionPickConfirmationRpcAsync(this.OID);
			}
			return (to != 0) ? this._async.To(to) : this._async.To();
		}

		public ICharacterSelectionPickConfirmationRpcDispatch DispatchReliable(params byte[] to)
		{
			if (this._dispatch == null)
			{
				this._dispatch = new CharacterSelectionPickConfirmationRpcDispatch(this.OID);
			}
			this._dispatch.Reliable(true);
			return (to != null) ? this._dispatch.To(to) : this._dispatch.To();
		}

		public ICharacterSelectionPickConfirmationRpcDispatch Dispatch(params byte[] to)
		{
			if (this._dispatch == null)
			{
				this._dispatch = new CharacterSelectionPickConfirmationRpcDispatch(this.OID);
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
			this.SendPickConfirmation((Guid)args[0]);
			return null;
		}

		[Inject]
		private IMatchPlayers _matchPlayers;

		[Inject]
		private IGetCurrentMatch _getCurrentMatch;

		private Subject<ClientPickConfirmation> _pickConfirmedSubject;

		public const int StaticClassId = 1061;

		private Identifiable _identifiable;

		[ThreadStatic]
		private CharacterSelectionPickConfirmationRpcAsync _async;

		[ThreadStatic]
		private CharacterSelectionPickConfirmationRpcDispatch _dispatch;

		private IFuture _delayed;
	}
}
