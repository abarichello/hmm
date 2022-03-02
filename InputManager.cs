using System;
using System.Collections.Generic;
using System.Diagnostics;
using HeavyMetalMachines.BI;
using HeavyMetalMachines.Infra.Context;
using HeavyMetalMachines.Infra.DependencyInjection.Attributes;
using HeavyMetalMachines.Input.NoInputDetection;
using HeavyMetalMachines.Input.NoInputDetection.Infra;
using Pocketverse;
using Pocketverse.MuralContext;
using UniRx;
using UnityEngine;

namespace HeavyMetalMachines
{
	[RemoteClass]
	public class InputManager : GameHubBehaviour, ICleanupListener, ISerializationCallbackReceiver, IBitComponent
	{
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event Action OnSendInput;

		private void Awake()
		{
			this._updater = new TimedUpdater(50, true, true);
		}

		private void Start()
		{
			if (GameHubBehaviour.Hub.Net.IsServer())
			{
				this._storage.Configuration.InputNotificationTicks = TimeSpan.FromSeconds(6.0).Ticks;
				ObservableExtensions.Subscribe<PlayerInputFrozeDetection>(Observable.Do<PlayerInputFrozeDetection>(this._inputFrozeDetection.ObservePlayerInputFroze(), delegate(PlayerInputFrozeDetection detection)
				{
					MatchLogWriter.PlayerInputFroze(detection.PlayerId, this._inputFrozeDetection.GetLastReceivedInputTimeSeconds(detection.PlayerId));
				}));
			}
		}

		private void LateUpdate()
		{
			if (GameHubBehaviour.Hub.Net.IsClient() && GameHubBehaviour.Hub.Net.IsConnected())
			{
				if (!this.CurrentController)
				{
					return;
				}
				if (this.OnSendInput != null)
				{
					this.OnSendInput();
				}
				if (this._updater.ShouldHalt())
				{
					if (!this.CurrentController.Inputs.HasGadgetInputChanged() && !this.CurrentController.Inputs.Respawn && !this.CurrentController.Inputs.HasPauseButtonPressed())
					{
						return;
					}
					this._updater.Reset();
				}
				if (GameHubBehaviour.Hub.BombManager.CurrentBombGameState != BombScoreboardState.Replay && GameHubBehaviour.Hub.BombManager.CurrentBombGameState != BombScoreboardState.PreReplay && this.CurrentController.Inputs.HasPressedAnyAntiAfkInput())
				{
					GameHubBehaviour.Hub.GuiScripts.AfkControllerGui.Close();
				}
				this.Dispatch(new byte[0]).ClientSendInput(this.CurrentController.Inputs);
				this.CurrentController.Reset();
			}
			if (GameHubBehaviour.Hub.Net.IsServer())
			{
				this._inputFrozeDetection.Update();
			}
		}

		[RemoteMethod]
		private void ClientSendInput(PlayerController.InputMap inputs)
		{
			PlayerController playerController;
			if (!this._players.TryGetValue(this.Sender, out playerController))
			{
				return;
			}
			playerController.ExecInput(inputs);
			this._inputFrozeDetection.StoreInputReceived(playerController.Combat.Player.PlayerId);
		}

		internal void Register(PlayerController playerController, byte client)
		{
			this._players[client] = playerController;
		}

		public void OnCleanup(CleanupMessage msg)
		{
			this.CurrentController = null;
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

		public IInputManagerAsync Async()
		{
			return this.Async(0);
		}

		public IInputManagerAsync Async(byte to)
		{
			if (this._async == null)
			{
				this._async = new InputManagerAsync(this.OID);
			}
			return (to != 0) ? this._async.To(to) : this._async.To();
		}

		public IInputManagerDispatch DispatchReliable(params byte[] to)
		{
			if (this._dispatch == null)
			{
				this._dispatch = new InputManagerDispatch(this.OID);
			}
			this._dispatch.Reliable(true);
			return (to != null) ? this._dispatch.To(to) : this._dispatch.To();
		}

		public IInputManagerDispatch Dispatch(params byte[] to)
		{
			if (this._dispatch == null)
			{
				this._dispatch = new InputManagerDispatch(this.OID);
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
			this.ClientSendInput((PlayerController.InputMap)args[0]);
			return null;
		}

		private long _nextUpdate;

		private Dictionary<byte, PlayerController> _players = new Dictionary<byte, PlayerController>(10);

		public PlayerController CurrentController;

		private TimedUpdater _updater;

		[InjectOnServer]
		private IServerPlayerInputFrozeDetection _inputFrozeDetection;

		[InjectOnServer]
		private IServerPlayerInputFrozeConfigurationStorage _storage;

		public const int StaticClassId = 1019;

		private Identifiable _identifiable;

		[ThreadStatic]
		private InputManagerAsync _async;

		[ThreadStatic]
		private InputManagerDispatch _dispatch;

		private IFuture _delayed;
	}
}
