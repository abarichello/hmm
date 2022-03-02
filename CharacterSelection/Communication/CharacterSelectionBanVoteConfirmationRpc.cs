using System;
using System.Linq;
using HeavyMetalMachines.CharacterSelection.Banning;
using HeavyMetalMachines.CharacterSelection.Client.Picking;
using HeavyMetalMachines.CharacterSelection.Server.Banning;
using HeavyMetalMachines.Match;
using HeavyMetalMachines.Matches;
using Pocketverse;
using UniRx;
using UnityEngine;
using Zenject;

namespace HeavyMetalMachines.CharacterSelection.Communication
{
	[RemoteClass]
	public class CharacterSelectionBanVoteConfirmationRpc : MonoBehaviour, ISendBanVoteConfirmation, IListenForBanVoteConfirmation, IBitComponent
	{
		private void Awake()
		{
			this._sendSubject = new Subject<ClientVoteBanConfirmation>();
		}

		public void Send(ClientVoteBanConfirmation confirmation)
		{
			this.DispatchReliable(new byte[0]).SendBanConfirmation(confirmation.CharacterId);
		}

		public IObservable<ClientVoteBanConfirmation> ListenForBanVoteConfirmations()
		{
			return this._sendSubject;
		}

		[RemoteMethod]
		public void SendBanConfirmation(Guid characterId)
		{
			PlayerData player = this._matchPlayers.GetPlayerByAddress(this.Sender);
			MatchClient client2 = GetCurrentMatchExtensions.Get(this._getCurrentMatch).Clients.First((MatchClient client) => client.PlayerId == player.PlayerId);
			ClientVoteBanConfirmation clientVoteBanConfirmation = new ClientVoteBanConfirmation
			{
				CharacterId = characterId,
				Client = client2
			};
			this._sendSubject.OnNext(clientVoteBanConfirmation);
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

		public ICharacterSelectionBanVoteConfirmationRpcAsync Async()
		{
			return this.Async(0);
		}

		public ICharacterSelectionBanVoteConfirmationRpcAsync Async(byte to)
		{
			if (this._async == null)
			{
				this._async = new CharacterSelectionBanVoteConfirmationRpcAsync(this.OID);
			}
			return (to != 0) ? this._async.To(to) : this._async.To();
		}

		public ICharacterSelectionBanVoteConfirmationRpcDispatch DispatchReliable(params byte[] to)
		{
			if (this._dispatch == null)
			{
				this._dispatch = new CharacterSelectionBanVoteConfirmationRpcDispatch(this.OID);
			}
			this._dispatch.Reliable(true);
			return (to != null) ? this._dispatch.To(to) : this._dispatch.To();
		}

		public ICharacterSelectionBanVoteConfirmationRpcDispatch Dispatch(params byte[] to)
		{
			if (this._dispatch == null)
			{
				this._dispatch = new CharacterSelectionBanVoteConfirmationRpcDispatch(this.OID);
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
			this.SendBanConfirmation((Guid)args[0]);
			return null;
		}

		[Inject]
		private IMatchPlayers _matchPlayers;

		[Inject]
		private IGetCurrentMatch _getCurrentMatch;

		private Subject<ClientVoteBanConfirmation> _sendSubject;

		public const int StaticClassId = 1052;

		private Identifiable _identifiable;

		[ThreadStatic]
		private CharacterSelectionBanVoteConfirmationRpcAsync _async;

		[ThreadStatic]
		private CharacterSelectionBanVoteConfirmationRpcDispatch _dispatch;

		private IFuture _delayed;
	}
}
