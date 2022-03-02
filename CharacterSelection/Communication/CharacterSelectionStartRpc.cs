using System;
using HeavyMetalMachines.CharacterSelection.Client.Picking;
using HeavyMetalMachines.CharacterSelection.Server.Infra;
using HeavyMetalMachines.Matches;
using Pocketverse;
using UniRx;
using UnityEngine;
using Zenject;

namespace HeavyMetalMachines.CharacterSelection.Communication
{
	[RemoteClass]
	public class CharacterSelectionStartRpc : MonoBehaviour, IBroadcastCharacterSelectionStarted, IListenForCharacterSelectionToStart, IBitComponent
	{
		private void Awake()
		{
			this._listenSubject = new Subject<Unit>();
		}

		public void Broadcast(MatchClient[] clients)
		{
			this.DispatchReliable(this._addressGroupHelper.GetGroup(0)).SendStarted();
		}

		public IObservable<Unit> Listen(MatchClient localClient)
		{
			return this._listenSubject;
		}

		[RemoteMethod]
		public void SendStarted()
		{
			this._listenSubject.OnNext(Unit.Default);
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

		public ICharacterSelectionStartRpcAsync Async()
		{
			return this.Async(0);
		}

		public ICharacterSelectionStartRpcAsync Async(byte to)
		{
			if (this._async == null)
			{
				this._async = new CharacterSelectionStartRpcAsync(this.OID);
			}
			return (to != 0) ? this._async.To(to) : this._async.To();
		}

		public ICharacterSelectionStartRpcDispatch DispatchReliable(params byte[] to)
		{
			if (this._dispatch == null)
			{
				this._dispatch = new CharacterSelectionStartRpcDispatch(this.OID);
			}
			this._dispatch.Reliable(true);
			return (to != null) ? this._dispatch.To(to) : this._dispatch.To();
		}

		public ICharacterSelectionStartRpcDispatch Dispatch(params byte[] to)
		{
			if (this._dispatch == null)
			{
				this._dispatch = new CharacterSelectionStartRpcDispatch(this.OID);
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
			this.SendStarted();
			return null;
		}

		[Inject]
		private AddressGroupHelper _addressGroupHelper;

		private Subject<Unit> _listenSubject;

		public const int StaticClassId = 1064;

		private Identifiable _identifiable;

		[ThreadStatic]
		private CharacterSelectionStartRpcAsync _async;

		[ThreadStatic]
		private CharacterSelectionStartRpcDispatch _dispatch;

		private IFuture _delayed;
	}
}
