using System;
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
	public class CharacterSelectionPickStepResultRpc : MonoBehaviour, IBroadcastPickStepResult, IListenForPickStepResult, IBitComponent
	{
		public void Broadcast(PickStepResult pickStepResult, MatchClient[] clients)
		{
			PickStepResultSerialized pickStepResult2 = new PickStepResultSerialized
			{
				Data = pickStepResult
			};
			this.DispatchReliable(this._addressGroupHelper.GetGroup(0)).ReceiveResult(pickStepResult2);
		}

		public IObservable<PickStepResult> Listen(MatchClient localClient)
		{
			return this._listenSubject;
		}

		[RemoteMethod]
		public void ReceiveResult(PickStepResultSerialized pickStepResult)
		{
			this._listenSubject.OnNext(pickStepResult.Data);
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

		public ICharacterSelectionPickStepResultRpcAsync Async()
		{
			return this.Async(0);
		}

		public ICharacterSelectionPickStepResultRpcAsync Async(byte to)
		{
			if (this._async == null)
			{
				this._async = new CharacterSelectionPickStepResultRpcAsync(this.OID);
			}
			return (to != 0) ? this._async.To(to) : this._async.To();
		}

		public ICharacterSelectionPickStepResultRpcDispatch DispatchReliable(params byte[] to)
		{
			if (this._dispatch == null)
			{
				this._dispatch = new CharacterSelectionPickStepResultRpcDispatch(this.OID);
			}
			this._dispatch.Reliable(true);
			return (to != null) ? this._dispatch.To(to) : this._dispatch.To();
		}

		public ICharacterSelectionPickStepResultRpcDispatch Dispatch(params byte[] to)
		{
			if (this._dispatch == null)
			{
				this._dispatch = new CharacterSelectionPickStepResultRpcDispatch(this.OID);
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
			this.ReceiveResult((PickStepResultSerialized)args[0]);
			return null;
		}

		[Inject]
		private AddressGroupHelper _addressGroupHelper;

		private readonly Subject<PickStepResult> _listenSubject = new Subject<PickStepResult>();

		public const int StaticClassId = 1062;

		private Identifiable _identifiable;

		[ThreadStatic]
		private CharacterSelectionPickStepResultRpcAsync _async;

		[ThreadStatic]
		private CharacterSelectionPickStepResultRpcDispatch _dispatch;

		private IFuture _delayed;
	}
}
