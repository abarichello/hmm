using System;
using HeavyMetalMachines.CharacterSelection.Banning;
using HeavyMetalMachines.CharacterSelection.Client.Picking;
using HeavyMetalMachines.CharacterSelection.Communication.SerializedData;
using HeavyMetalMachines.CharacterSelection.Server.Banning;
using HeavyMetalMachines.Matches;
using Pocketverse;
using UniRx;
using UnityEngine;
using Zenject;

namespace HeavyMetalMachines.CharacterSelection.Communication
{
	[RemoteClass]
	public class CharacterSelectionBanStepResultRpc : MonoBehaviour, IBroadcastBanStepResult, IListenForBanStepResult, IBitComponent
	{
		[RemoteMethod]
		public void ReceiveResult(BanStepResultSerializable banStepResult)
		{
			this._listenSubject.OnNext(banStepResult.Data);
		}

		public void Broadcast(BanStepResult banStepResult, MatchClient[] clients)
		{
			BanStepResultSerializable banStepResult2 = new BanStepResultSerializable
			{
				Data = banStepResult
			};
			this.DispatchReliable(this._addressGroupHelper.GetGroup(0)).ReceiveResult(banStepResult2);
		}

		public IObservable<BanStepResult> Listen(MatchClient localClient)
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

		public ICharacterSelectionBanStepResultRpcAsync Async()
		{
			return this.Async(0);
		}

		public ICharacterSelectionBanStepResultRpcAsync Async(byte to)
		{
			if (this._async == null)
			{
				this._async = new CharacterSelectionBanStepResultRpcAsync(this.OID);
			}
			return (to != 0) ? this._async.To(to) : this._async.To();
		}

		public ICharacterSelectionBanStepResultRpcDispatch DispatchReliable(params byte[] to)
		{
			if (this._dispatch == null)
			{
				this._dispatch = new CharacterSelectionBanStepResultRpcDispatch(this.OID);
			}
			this._dispatch.Reliable(true);
			return (to != null) ? this._dispatch.To(to) : this._dispatch.To();
		}

		public ICharacterSelectionBanStepResultRpcDispatch Dispatch(params byte[] to)
		{
			if (this._dispatch == null)
			{
				this._dispatch = new CharacterSelectionBanStepResultRpcDispatch(this.OID);
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
			if (methodId != 1)
			{
				throw new ScriptMethodNotFoundException(classId, (int)methodId);
			}
			this.ReceiveResult((BanStepResultSerializable)args[0]);
			return null;
		}

		[Inject]
		private AddressGroupHelper _addressGroupHelper;

		private readonly Subject<BanStepResult> _listenSubject = new Subject<BanStepResult>();

		public const int StaticClassId = 1051;

		private Identifiable _identifiable;

		[ThreadStatic]
		private CharacterSelectionBanStepResultRpcAsync _async;

		[ThreadStatic]
		private CharacterSelectionBanStepResultRpcDispatch _dispatch;

		private IFuture _delayed;
	}
}
