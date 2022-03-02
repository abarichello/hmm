using System;
using System.Linq;
using HeavyMetalMachines.CharacterSelection.Client.Infra;
using HeavyMetalMachines.CharacterSelection.Server.Infra;
using HeavyMetalMachines.CharacterSelection.Skins;
using HeavyMetalMachines.Match;
using HeavyMetalMachines.Matches;
using Hoplon.Logging;
using Pocketverse;
using UniRx;
using UnityEngine;
using Zenject;

namespace HeavyMetalMachines.CharacterSelection.Communication
{
	[RemoteClass]
	public class CharacterSelectionEquipSkinRpc : MonoBehaviour, IListenForSkinEquips, ISendSkinEquipped, IBitComponent
	{
		private void Awake()
		{
			this._equipSkinRequestSubject = new Subject<EquipSkinRequest>();
			this._logger.Debug("Awake");
		}

		public void Send(EquipSkinRequest equipSkinRequest)
		{
			this._logger.DebugFormat("Sending SkinId={0}", new object[]
			{
				equipSkinRequest.SkinId
			});
			this.DispatchReliable(new byte[0]).SendEquipSkinRequest(equipSkinRequest.SkinId);
		}

		public IObservable<EquipSkinRequest> ListenForEquipSkinRequests()
		{
			return this._equipSkinRequestSubject;
		}

		[RemoteMethod]
		public void SendEquipSkinRequest(Guid skinId)
		{
			PlayerData player = this._matchPlayers.GetPlayerByAddress(this.Sender);
			MatchClient matchClient = GetCurrentMatchExtensions.Get(this._getCurrentMatch).Clients.First((MatchClient client) => client.PlayerId == player.PlayerId);
			this._logger.DebugFormat("Received SkinId={0} PlayerId={1} Client={2}", new object[]
			{
				skinId,
				player.PlayerId,
				matchClient
			});
			EquipSkinRequest equipSkinRequest = new EquipSkinRequest
			{
				SkinId = skinId,
				Client = matchClient
			};
			this._equipSkinRequestSubject.OnNext(equipSkinRequest);
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

		public ICharacterSelectionEquipSkinRpcAsync Async()
		{
			return this.Async(0);
		}

		public ICharacterSelectionEquipSkinRpcAsync Async(byte to)
		{
			if (this._async == null)
			{
				this._async = new CharacterSelectionEquipSkinRpcAsync(this.OID);
			}
			return (to != 0) ? this._async.To(to) : this._async.To();
		}

		public ICharacterSelectionEquipSkinRpcDispatch DispatchReliable(params byte[] to)
		{
			if (this._dispatch == null)
			{
				this._dispatch = new CharacterSelectionEquipSkinRpcDispatch(this.OID);
			}
			this._dispatch.Reliable(true);
			return (to != null) ? this._dispatch.To(to) : this._dispatch.To();
		}

		public ICharacterSelectionEquipSkinRpcDispatch Dispatch(params byte[] to)
		{
			if (this._dispatch == null)
			{
				this._dispatch = new CharacterSelectionEquipSkinRpcDispatch(this.OID);
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
			this.SendEquipSkinRequest((Guid)args[0]);
			return null;
		}

		[Inject]
		private IMatchPlayers _matchPlayers;

		[Inject]
		private IGetCurrentMatch _getCurrentMatch;

		[Inject]
		private ILogger<CharacterSelectionEquipSkinRpc> _logger;

		private Subject<EquipSkinRequest> _equipSkinRequestSubject;

		public const int StaticClassId = 1056;

		private Identifiable _identifiable;

		[ThreadStatic]
		private CharacterSelectionEquipSkinRpcAsync _async;

		[ThreadStatic]
		private CharacterSelectionEquipSkinRpcDispatch _dispatch;

		private IFuture _delayed;
	}
}
