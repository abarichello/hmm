using System;
using System.Collections.Generic;
using System.Diagnostics;
using HeavyMetalMachines.Combat;
using Pocketverse;
using Pocketverse.MuralContext;
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
			this._updater = new TimedUpdater(50, true, false);
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
				if (GameHubBehaviour.Hub.BombManager.CurrentBombGameState != BombScoreBoard.State.Replay && GameHubBehaviour.Hub.BombManager.CurrentBombGameState != BombScoreBoard.State.PreReplay && this.CurrentController.Inputs.HasPressedWeaponsDriveOrChat())
				{
					GameHubBehaviour.Hub.GuiScripts.AfkControllerGui.Close();
				}
				this.Dispatch(new byte[0]).ClientSendInput(this.CurrentController.Inputs);
				this.CurrentController.Reset();
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
		}

		internal void Register(PlayerController playerController, byte client)
		{
			this._players[client] = playerController;
		}

		public void OnCleanup(CleanupMessage msg)
		{
			this.CurrentController = null;
		}

		public override void OnBeforeSerialize()
		{
			base.OnBeforeSerialize();
			this.serializeHackClients = new List<byte>();
			this.serializeHackPlayerControllers = new List<PlayerController>();
			foreach (KeyValuePair<byte, PlayerController> keyValuePair in this._players)
			{
				this.serializeHackClients.Add(keyValuePair.Key);
				this.serializeHackPlayerControllers.Add(keyValuePair.Value);
			}
		}

		public override void OnAfterDeserialize()
		{
			base.OnAfterDeserialize();
			for (int i = 0; i < this.serializeHackClients.Count; i++)
			{
				this._players.Add(this.serializeHackClients[i], this.serializeHackPlayerControllers[i]);
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

		public object Invoke(int classId, short methodId, object[] args)
		{
			if (classId != 1019)
			{
				throw new Exception("Hierarchy in RemoteClass is not allowed!!! " + classId);
			}
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

		[SerializeField]
		private List<byte> serializeHackClients;

		[SerializeField]
		private List<PlayerController> serializeHackPlayerControllers;

		public const int StaticClassId = 1019;

		private Identifiable _identifiable;

		[ThreadStatic]
		private InputManagerAsync _async;

		[ThreadStatic]
		private InputManagerDispatch _dispatch;

		private IFuture _delayed;
	}
}
