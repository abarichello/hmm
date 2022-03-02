using System;
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
	public class CharacterSelectionResultRpc : MonoBehaviour, IBroadcastCharacterSelectionResult, IListenForCharacterSelectionResult, IBitComponent
	{
		public void Broadcast(CharacterSelectionResult result, MatchClient[] clients)
		{
			CharacterSelectionResultSerialized result2 = new CharacterSelectionResultSerialized
			{
				Data = result
			};
			this.DispatchReliable(this._addressGroupHelper.GetGroup(0)).ReceiveResult(result2);
		}

		public IObservable<CharacterSelectionResult> Listen(MatchClient localClient)
		{
			return this._listenSubject;
		}

		[RemoteMethod]
		public void ReceiveResult(CharacterSelectionResultSerialized result)
		{
			this._listenSubject.OnNext(result.Data);
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

		public ICharacterSelectionResultRpcAsync Async()
		{
			return this.Async(0);
		}

		public ICharacterSelectionResultRpcAsync Async(byte to)
		{
			if (this._async == null)
			{
				this._async = new CharacterSelectionResultRpcAsync(this.OID);
			}
			return (to != 0) ? this._async.To(to) : this._async.To();
		}

		public ICharacterSelectionResultRpcDispatch DispatchReliable(params byte[] to)
		{
			if (this._dispatch == null)
			{
				this._dispatch = new CharacterSelectionResultRpcDispatch(this.OID);
			}
			this._dispatch.Reliable(true);
			return (to != null) ? this._dispatch.To(to) : this._dispatch.To();
		}

		public ICharacterSelectionResultRpcDispatch Dispatch(params byte[] to)
		{
			if (this._dispatch == null)
			{
				this._dispatch = new CharacterSelectionResultRpcDispatch(this.OID);
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
			this.ReceiveResult((CharacterSelectionResultSerialized)args[0]);
			return null;
		}

		[Inject]
		private AddressGroupHelper _addressGroupHelper;

		private readonly Subject<CharacterSelectionResult> _listenSubject = new Subject<CharacterSelectionResult>();

		public const int StaticClassId = 1063;

		private Identifiable _identifiable;

		[ThreadStatic]
		private CharacterSelectionResultRpcAsync _async;

		[ThreadStatic]
		private CharacterSelectionResultRpcDispatch _dispatch;

		private IFuture _delayed;
	}
}
