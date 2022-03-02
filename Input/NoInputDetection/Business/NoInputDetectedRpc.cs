using System;
using Hoplon.Logging;
using Pocketverse;
using UniRx;
using UnityEngine;
using Zenject;

namespace HeavyMetalMachines.Input.NoInputDetection.Business
{
	[RemoteClass]
	public class NoInputDetectedRpc : GameHubBehaviour, INoInputDetectedRpc, IBitComponent
	{
		public IObservable<byte> OnPlayerInputDisconnection
		{
			get
			{
				return this._onPlayerInputDisconnection;
			}
		}

		private void Awake()
		{
			Object.DontDestroyOnLoad(this);
		}

		[RemoteMethod]
		public void ReceiveNoInputDetectedMessage(byte playerAddress)
		{
			this._log.InfoFormat("Received player lost input={0}", new object[]
			{
				this.Sender
			});
			this._onPlayerInputDisconnection.OnNext(this.Sender);
		}

		public void SendNoInputDetectedMessage()
		{
			if (GameHubBehaviour.Hub == null)
			{
				return;
			}
			if (GameHubBehaviour.Hub.Players == null)
			{
				return;
			}
			if (GameHubBehaviour.Hub.Players.CurrentPlayerData == null)
			{
				return;
			}
			this._log.InfoFormat("Sending player lost input", new object[0]);
			this.DispatchReliable(new byte[0]).ReceiveNoInputDetectedMessage(GameHubBehaviour.Hub.Players.CurrentPlayerData.PlayerAddress);
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

		public INoInputDetectedRpcAsync Async()
		{
			return this.Async(0);
		}

		public INoInputDetectedRpcAsync Async(byte to)
		{
			if (this._async == null)
			{
				this._async = new NoInputDetectedRpcAsync(this.OID);
			}
			return (to != 0) ? this._async.To(to) : this._async.To();
		}

		public INoInputDetectedRpcDispatch DispatchReliable(params byte[] to)
		{
			if (this._dispatch == null)
			{
				this._dispatch = new NoInputDetectedRpcDispatch(this.OID);
			}
			this._dispatch.Reliable(true);
			return (to != null) ? this._dispatch.To(to) : this._dispatch.To();
		}

		public INoInputDetectedRpcDispatch Dispatch(params byte[] to)
		{
			if (this._dispatch == null)
			{
				this._dispatch = new NoInputDetectedRpcDispatch(this.OID);
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
			this.ReceiveNoInputDetectedMessage((byte)args[0]);
			return null;
		}

		private readonly Subject<byte> _onPlayerInputDisconnection = new Subject<byte>();

		[Inject]
		private ILogger<NoInputDetectedRpc> _log;

		public const int StaticClassId = 1024;

		private Identifiable _identifiable;

		[ThreadStatic]
		private NoInputDetectedRpcAsync _async;

		[ThreadStatic]
		private NoInputDetectedRpcDispatch _dispatch;

		private IFuture _delayed;
	}
}
