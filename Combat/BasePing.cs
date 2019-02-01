using System;
using System.Collections.Generic;
using HeavyMetalMachines.Event;
using HeavyMetalMachines.Frontend;
using HeavyMetalMachines.HMMChat;
using HeavyMetalMachines.Match;
using NewParticleSystem;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat
{
	public class BasePing : GameHubBehaviour
	{
		protected static GameGui GameGui
		{
			get
			{
				GameGui result;
				if ((result = BasePing._gameGui) == null)
				{
					result = (BasePing._gameGui = GameHubBehaviour.Hub.State.Current.GetStateGuiController<GameGui>());
				}
				return result;
			}
		}

		protected virtual void Awake()
		{
			GameHubBehaviour.Hub.Events.Players.CurrentPlayerCreatedCallback += this.OnCurrentPlayerCreatedBaseConfig;
			this._minimapPingsUpdateDelay = new TimedUpdater(this.MiniMapPingRefreshRateMillis, true, false);
			this._updateMinimapPingsList = new List<BasePing.EventPing3D>(5);
		}

		protected virtual void OnDestroy()
		{
			if (GameHubBehaviour.Hub != null && GameHubBehaviour.Hub.Events != null && GameHubBehaviour.Hub.Events.Players != null)
			{
				GameHubBehaviour.Hub.Events.Players.CurrentPlayerCreatedCallback -= this.OnCurrentPlayerCreatedBaseConfig;
			}
		}

		private void OnCurrentPlayerCreatedBaseConfig(PlayerEvent data)
		{
			BasePing._gameGui = null;
			this._gameState = (GameHubBehaviour.Hub.State.Current as Game);
			if (this._gameState != null)
			{
				this._gameState.OnGameOver += this.GameOver;
			}
			for (int i = 0; i < this.Pings.Count; i++)
			{
				BasePing.EventPing3D eventPing3D = this.Pings[i];
				if (eventPing3D.Prefab3D)
				{
					GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(eventPing3D.Prefab3D, Vector3.zero, Quaternion.identity);
					eventPing3D.Instance3D = gameObject.GetComponentInChildren<HoplonParticleSystem>();
					eventPing3D.Instance3D.transform.parent.parent = GameHubBehaviour.Hub.Drawer.Effects;
					Vector3 zero = Vector3.zero;
					zero.y = 0.1f;
					eventPing3D.Instance3D.transform.parent.localPosition = zero;
				}
			}
			this.OnPlayerCreated(data);
		}

		protected virtual bool OnPlayerCreated(PlayerEvent data)
		{
			return true;
		}

		protected virtual void GameOver(MatchData.MatchState matchWinner)
		{
			BasePing._gameGui = null;
			if (this._gameState != null)
			{
				this._gameState.OnGameOver -= this.GameOver;
			}
			for (int i = 0; i < this.Pings.Count; i++)
			{
				if (this.Pings[i].Instance3D && this.Pings[i].Instance3D.transform.parent)
				{
					UnityEngine.Object.Destroy(this.Pings[i].Instance3D.transform.parent.gameObject);
					this.Pings[i].Instance3D = null;
				}
			}
			this._updateMinimapPingsList.Clear();
		}

		protected virtual void Update()
		{
			if (!this.ShowMiniMapPing || this._updateMinimapPingsList == null || this._updateMinimapPingsList.Count <= 0 || this._minimapPingsUpdateDelay.ShouldHalt())
			{
				return;
			}
			for (int i = 0; i < this._updateMinimapPingsList.Count; i++)
			{
				BasePing.EventPing3D eventPing3D = this._updateMinimapPingsList[i];
				if (eventPing3D.Instance3D)
				{
					if (!eventPing3D.Instance3D.IsPlaying)
					{
						this._updateMinimapPingsList.RemoveAt(i);
						i--;
					}
				}
			}
		}

		protected virtual void Ping(Transform newParenTransform, int kind, bool refreshMinimapPingPosition, byte playerAddress)
		{
			if (this._lastPingKind != 0)
			{
				this.StopPingEvent(this._lastPingKind);
			}
			this._lastPingKind = kind;
			BasePing.EventPing3D pingInstance = this.GetPingInstance(kind);
			if (pingInstance == null)
			{
				return;
			}
			if (!string.IsNullOrEmpty(pingInstance.PingDraftText))
			{
				string msg = Language.Get(pingInstance.PingDraftText, TranslationSheets.HUDChat);
				GameHubBehaviour.Hub.Chat.SetupPlayerMessage(false, msg, playerAddress, ChatService.ChatMessageKind.PlayerNotification);
			}
			if (pingInstance.Instance3D)
			{
				pingInstance.Instance3D.transform.parent.parent = newParenTransform;
				Vector3 zero = Vector3.zero;
				zero.y = 0.1f;
				pingInstance.Instance3D.transform.parent.localPosition = zero;
				pingInstance.Instance3D.Play();
			}
			if (this.ShowMiniMapPing && pingInstance.PrefabMiniMapIcon && refreshMinimapPingPosition)
			{
				this._updateMinimapPingsList.Add(pingInstance);
			}
		}

		protected virtual void StopPingEvent(int kind)
		{
			BasePing.EventPing3D pingInstance = this.GetPingInstance(kind);
			if (pingInstance == null)
			{
				return;
			}
			if (pingInstance.Instance3D)
			{
				pingInstance.Instance3D.Stop();
			}
			this._updateMinimapPingsList.Remove(pingInstance);
		}

		protected virtual BasePing.EventPing3D GetPingInstance(int kind)
		{
			for (int i = 0; i < this.Pings.Count; i++)
			{
				BasePing.EventPing3D eventPing3D = this.Pings[i];
				if (eventPing3D.Kind == kind)
				{
					return eventPing3D;
				}
			}
			if (this.Pings.Count <= 0)
			{
				return null;
			}
			return this.Pings[0];
		}

		public bool ShowMiniMapPing;

		public int MiniMapPingRefreshRateMillis = 100;

		public List<BasePing.EventPing3D> Pings;

		private TimedUpdater _minimapPingsUpdateDelay;

		private List<BasePing.EventPing3D> _updateMinimapPingsList;

		private Game _gameState;

		private static GameGui _gameGui;

		private int _lastPingKind;

		[Serializable]
		public class EventPing3D
		{
			public int Kind;

			public GameObject Prefab3D;

			public GameObject PrefabMiniMapIcon;

			public Color Color;

			public string PingDraftText;

			[NonSerialized]
			public HoplonParticleSystem Instance3D;
		}
	}
}
