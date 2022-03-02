using System;
using HeavyMetalMachines.Audio;
using HeavyMetalMachines.Event;
using HeavyMetalMachines.Match;
using Pocketverse;

namespace HeavyMetalMachines.Combat
{
	[RemoteClass]
	public class PlayerPing : BasePing, IBitComponent
	{
		[RemoteMethod]
		public void ServerCreatePing(int pingKind)
		{
			PingEvent pingEvent = new PingEvent();
			pingEvent.PingKind = pingKind;
			PlayerData playerByAddress = GameHubBehaviour.Hub.Players.GetPlayerByAddress(this.Sender);
			pingEvent.Owner = playerByAddress.PlayerCarId;
			pingEvent.Team = playerByAddress.Team;
			GameHubBehaviour.Hub.Events.TriggerEvent(pingEvent);
		}

		public void Trigger(PingEvent pingKind, int eventId)
		{
			if (GameHubBehaviour.Hub.Net.IsServer())
			{
				return;
			}
			CombatObject combat = CombatRef.GetCombat(pingKind.Owner);
			if (!combat)
			{
				return;
			}
			this.Ping(combat.Transform, pingKind.PingKind, true, combat.Player.PlayerAddress);
			VoiceOverController voiceOverController = combat.Player.CharacterInstance.GetComponentHub<CarComponentHub>().VoiceOverController;
			if (voiceOverController != null)
			{
				voiceOverController.PlayPing((PlayerPing.PlayerPingKind)pingKind.PingKind);
			}
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

		public IPlayerPingAsync Async()
		{
			return this.Async(0);
		}

		public IPlayerPingAsync Async(byte to)
		{
			if (this._async == null)
			{
				this._async = new PlayerPingAsync(this.OID);
			}
			return (to != 0) ? this._async.To(to) : this._async.To();
		}

		public IPlayerPingDispatch DispatchReliable(params byte[] to)
		{
			if (this._dispatch == null)
			{
				this._dispatch = new PlayerPingDispatch(this.OID);
			}
			this._dispatch.Reliable(true);
			return (to != null) ? this._dispatch.To(to) : this._dispatch.To();
		}

		public IPlayerPingDispatch Dispatch(params byte[] to)
		{
			if (this._dispatch == null)
			{
				this._dispatch = new PlayerPingDispatch(this.OID);
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
			this.ServerCreatePing((int)args[0]);
			return null;
		}

		public int FloodIntervalMillis = 100;

		public const int StaticClassId = 1077;

		private Identifiable _identifiable;

		[ThreadStatic]
		private PlayerPingAsync _async;

		[ThreadStatic]
		private PlayerPingDispatch _dispatch;

		private IFuture _delayed;

		public enum PlayerPingKind
		{
			None,
			ProtectTheBomb,
			GoodGame,
			OnMyWay,
			Thanks,
			CountMeOut,
			GoodLuckHaveFun,
			LetMeGetThebomb,
			Sorry,
			GetTheBomb,
			IWillDropTheBomb
		}
	}
}
