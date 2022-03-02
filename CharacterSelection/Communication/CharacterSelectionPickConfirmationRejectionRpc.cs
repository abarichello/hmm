using System;
using HeavyMetalMachines.CharacterSelection.Client.Picking;
using HeavyMetalMachines.CharacterSelection.Communication.SerializedData;
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
	public class CharacterSelectionPickConfirmationRejectionRpc : MonoBehaviour, ISendPickConfirmationRejection, IListenForPickConfirmationRejection, IBitComponent
	{
		public void Send(MatchClient client, PickConfirmationRejectionReason reason)
		{
			PlayerData anyByPlayerId = this._matchPlayers.GetAnyByPlayerId(client.PlayerId);
			this.DispatchReliable(new byte[]
			{
				anyByPlayerId.PlayerAddress
			}).RejectionSent(new PickConfirmationRejectionSerialized
			{
				Reason = reason
			});
		}

		[RemoteMethod]
		private void RejectionSent(PickConfirmationRejectionSerialized pickConfirmationRejection)
		{
			this._listenSubject.OnNext(pickConfirmationRejection.Reason);
		}

		public IObservable<PickConfirmationRejectionReason> Listen(MatchClient localClient)
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

		public ICharacterSelectionPickConfirmationRejectionRpcAsync Async()
		{
			return this.Async(0);
		}

		public ICharacterSelectionPickConfirmationRejectionRpcAsync Async(byte to)
		{
			if (this._async == null)
			{
				this._async = new CharacterSelectionPickConfirmationRejectionRpcAsync(this.OID);
			}
			return (to != 0) ? this._async.To(to) : this._async.To();
		}

		public ICharacterSelectionPickConfirmationRejectionRpcDispatch DispatchReliable(params byte[] to)
		{
			if (this._dispatch == null)
			{
				this._dispatch = new CharacterSelectionPickConfirmationRejectionRpcDispatch(this.OID);
			}
			this._dispatch.Reliable(true);
			return (to != null) ? this._dispatch.To(to) : this._dispatch.To();
		}

		public ICharacterSelectionPickConfirmationRejectionRpcDispatch Dispatch(params byte[] to)
		{
			if (this._dispatch == null)
			{
				this._dispatch = new CharacterSelectionPickConfirmationRejectionRpcDispatch(this.OID);
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
			this.RejectionSent((PickConfirmationRejectionSerialized)args[0]);
			return null;
		}

		[Inject]
		private IMatchPlayers _matchPlayers;

		private readonly Subject<PickConfirmationRejectionReason> _listenSubject = new Subject<PickConfirmationRejectionReason>();

		public const int StaticClassId = 1060;

		private Identifiable _identifiable;

		[ThreadStatic]
		private CharacterSelectionPickConfirmationRejectionRpcAsync _async;

		[ThreadStatic]
		private CharacterSelectionPickConfirmationRejectionRpcDispatch _dispatch;

		private IFuture _delayed;
	}
}
