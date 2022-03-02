using System;
using System.Linq;
using HeavyMetalMachines.CharacterSelection.Client.Infra;
using HeavyMetalMachines.CharacterSelection.Server.Infra;
using HeavyMetalMachines.CharacterSelection.Skins;
using HeavyMetalMachines.Matches;
using Hoplon.Logging;
using Pocketverse;
using UniRx;
using UnityEngine;
using Zenject;

namespace HeavyMetalMachines.CharacterSelection.Communication
{
	[RemoteClass]
	public class CharacterSelectionEquipSkinConfirmationRpc : MonoBehaviour, IListenForEquipSkinConfirmation, IBroadcastEquipSkinConfirmation, IBitComponent
	{
		private void Awake()
		{
			this._equipSkinConfirmationSubject = new Subject<EquipSkinConfirmation>();
			this._logger.Debug("Awake");
		}

		public void Send(EquipSkinConfirmation equipSkinConfirmation)
		{
			this._logger.DebugFormat("Sending SkinId={0}", new object[]
			{
				equipSkinConfirmation.SkinId
			});
		}

		public IObservable<EquipSkinConfirmation> ListenForEquipSkinConfirmations()
		{
			return this._equipSkinConfirmationSubject;
		}

		[RemoteMethod]
		public void SendEquipSkinConfirmations(bool success, long playerId, Guid characterId, Guid skinId)
		{
			MatchClient client = this._getCurrentMatch.GetIfExisting().Value.Clients.First((MatchClient c) => c.PlayerId == playerId);
			EquipSkinConfirmation equipSkinConfirmation = new EquipSkinConfirmation
			{
				Client = client,
				Success = success,
				CharacterId = characterId,
				SkinId = skinId
			};
			this._equipSkinConfirmationSubject.OnNext(equipSkinConfirmation);
		}

		public void Broadcast(EquipSkinConfirmation equipSkinConfirmation, MatchClient receiver)
		{
			byte playerAddress = this._matchPlayers.GetAnyByPlayerId(receiver.PlayerId).PlayerAddress;
			this.DispatchReliable(new byte[]
			{
				playerAddress
			}).SendEquipSkinConfirmations(equipSkinConfirmation.Success, receiver.PlayerId, equipSkinConfirmation.CharacterId, equipSkinConfirmation.SkinId);
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

		public ICharacterSelectionEquipSkinConfirmationRpcAsync Async()
		{
			return this.Async(0);
		}

		public ICharacterSelectionEquipSkinConfirmationRpcAsync Async(byte to)
		{
			if (this._async == null)
			{
				this._async = new CharacterSelectionEquipSkinConfirmationRpcAsync(this.OID);
			}
			return (to != 0) ? this._async.To(to) : this._async.To();
		}

		public ICharacterSelectionEquipSkinConfirmationRpcDispatch DispatchReliable(params byte[] to)
		{
			if (this._dispatch == null)
			{
				this._dispatch = new CharacterSelectionEquipSkinConfirmationRpcDispatch(this.OID);
			}
			this._dispatch.Reliable(true);
			return (to != null) ? this._dispatch.To(to) : this._dispatch.To();
		}

		public ICharacterSelectionEquipSkinConfirmationRpcDispatch Dispatch(params byte[] to)
		{
			if (this._dispatch == null)
			{
				this._dispatch = new CharacterSelectionEquipSkinConfirmationRpcDispatch(this.OID);
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
			this.SendEquipSkinConfirmations((bool)args[0], (long)args[1], (Guid)args[2], (Guid)args[3]);
			return null;
		}

		[Inject]
		private IMatchPlayers _matchPlayers;

		[Inject]
		private IGetCurrentMatch _getCurrentMatch;

		[Inject]
		private ILogger<CharacterSelectionEquipSkinConfirmationRpc> _logger;

		private Subject<EquipSkinConfirmation> _equipSkinConfirmationSubject;

		public const int StaticClassId = 1055;

		private Identifiable _identifiable;

		[ThreadStatic]
		private CharacterSelectionEquipSkinConfirmationRpcAsync _async;

		[ThreadStatic]
		private CharacterSelectionEquipSkinConfirmationRpcDispatch _dispatch;

		private IFuture _delayed;
	}
}
