using System;
using System.Linq;
using HeavyMetalMachines.CharacterSelection.Client.Picking;
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
	public class CharacterSelectionClientReadyRpc : MonoBehaviour, IListenForClientReady, ISendLocalPlayerReady, IBitComponent
	{
		private void Awake()
		{
			this._listenSubject = new Subject<MatchClient>();
		}

		public void Send(MatchClient client)
		{
			this.DispatchReliable(new byte[0]).SendReady();
		}

		public IObservable<MatchClient> ListenForClientsReady()
		{
			return this._listenSubject;
		}

		[RemoteMethod]
		public void SendReady()
		{
			PlayerData player = this._matchPlayers.GetPlayerByAddress(this.Sender);
			MatchClient matchClient = GetCurrentMatchExtensions.Get(this._getCurrentMatch).Clients.First((MatchClient client) => client.PlayerId == player.PlayerId);
			this._listenSubject.OnNext(matchClient);
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

		public ICharacterSelectionClientReadyRpcAsync Async()
		{
			return this.Async(0);
		}

		public ICharacterSelectionClientReadyRpcAsync Async(byte to)
		{
			if (this._async == null)
			{
				this._async = new CharacterSelectionClientReadyRpcAsync(this.OID);
			}
			return (to != 0) ? this._async.To(to) : this._async.To();
		}

		public ICharacterSelectionClientReadyRpcDispatch DispatchReliable(params byte[] to)
		{
			if (this._dispatch == null)
			{
				this._dispatch = new CharacterSelectionClientReadyRpcDispatch(this.OID);
			}
			this._dispatch.Reliable(true);
			return (to != null) ? this._dispatch.To(to) : this._dispatch.To();
		}

		public ICharacterSelectionClientReadyRpcDispatch Dispatch(params byte[] to)
		{
			if (this._dispatch == null)
			{
				this._dispatch = new CharacterSelectionClientReadyRpcDispatch(this.OID);
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
			this.SendReady();
			return null;
		}

		[Inject]
		private IMatchPlayers _matchPlayers;

		[Inject]
		private IGetCurrentMatch _getCurrentMatch;

		private Subject<MatchClient> _listenSubject;

		public const int StaticClassId = 1053;

		private Identifiable _identifiable;

		[ThreadStatic]
		private CharacterSelectionClientReadyRpcAsync _async;

		[ThreadStatic]
		private CharacterSelectionClientReadyRpcDispatch _dispatch;

		private IFuture _delayed;
	}
}
