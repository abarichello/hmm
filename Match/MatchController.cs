using System;
using System.Collections.Generic;
using HeavyMetalMachines.Announcer;
using HeavyMetalMachines.Arena;
using HeavyMetalMachines.BI;
using HeavyMetalMachines.BI.Players;
using HeavyMetalMachines.Combat;
using HeavyMetalMachines.Event;
using HeavyMetalMachines.Infra.DependencyInjection.Attributes;
using Pocketverse;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace HeavyMetalMachines.Match
{
	public class MatchController : GameHubBehaviour
	{
		public bool WarmupDone
		{
			get
			{
				return this._warmup00Done;
			}
		}

		public static bool IsValidPoint(Vector3 point, float[] depthLayers)
		{
			Vector2 vector;
			vector..ctor(point.x, point.z);
			for (int i = 0; i < depthLayers.Length; i++)
			{
				if (Physics2D.OverlapPoint(vector, 1, depthLayers[i], depthLayers[i]) != null)
				{
					return true;
				}
			}
			return false;
		}

		public Vector3 GetClosestValidPoint(Vector3 position, Vector3 lastValidPosition, float[] depthLayers, float offset)
		{
			if (MatchController.IsValidPoint(position, depthLayers))
			{
				return position;
			}
			Vector3 zero = Vector3.zero;
			float num = float.PositiveInfinity;
			Vector2 vector;
			vector..ctor(position.x, position.z);
			Vector2 vector2;
			vector2..ctor(lastValidPosition.x, lastValidPosition.z);
			ContactFilter2D contactFilter2D = default(ContactFilter2D);
			contactFilter2D.useDepth = true;
			RaycastHit2D[] array = new RaycastHit2D[1];
			for (int i = 0; i < depthLayers.Length; i++)
			{
				contactFilter2D.minDepth = depthLayers[i];
				contactFilter2D.maxDepth = depthLayers[i];
				int num2 = Physics2D.Linecast(vector, vector2, contactFilter2D, array);
				if (num2 > 0 && array[i].distance < num)
				{
					num = array[i].distance;
					Vector2 vector3 = array[i].point;
					Vector2 vector4 = array[i].normal * -offset;
					vector3 += vector4;
					zero..ctor(vector3.x, 0f, vector3.y);
				}
			}
			return zero;
		}

		public Vector3 GetClosestValidPoint(Vector3 point, float[] depthLayers, float offset = 0f)
		{
			if (MatchController.IsValidPoint(point, depthLayers))
			{
				return point;
			}
			Vector3 zero = Vector3.zero;
			Vector2 vector;
			vector..ctor(point.x, point.z);
			float positiveInfinity = float.PositiveInfinity;
			GameObject gameObject = new GameObject();
			Collider2D collider2D = gameObject.AddComponent<CircleCollider2D>();
			gameObject.transform.position = vector;
			for (int i = 0; i < depthLayers.Length; i++)
			{
				for (int j = 0; j < this.ArenaColliders.Length; j++)
				{
					if (Mathf.Abs(this.ArenaColliders[j].transform.position.z - depthLayers[i]) < 1E-45f)
					{
						ColliderDistance2D colliderDistance2D = Physics2D.Distance(this.ArenaColliders[j], collider2D);
						if (colliderDistance2D.distance < positiveInfinity)
						{
							Vector2 vector2 = colliderDistance2D.pointA + colliderDistance2D.normal * offset;
							zero..ctor(vector2.x, 0f, vector2.y);
						}
					}
				}
			}
			Object.Destroy(gameObject);
			return zero;
		}

		public bool CanStartMatch
		{
			get
			{
				return this._canStartMatch || GameHubBehaviour.Hub.Players.Players.Count <= 0;
			}
			set
			{
				if (!this._canStartMatch && value)
				{
					this._canStartMatch = true;
					this._countdownStartMatchTimeMilis = GameHubBehaviour.Hub.GameTime.GetPlaybackTime();
					GameHubBehaviour.Hub.BombManager.ScoreBoard.SetTimeout((long)GameHubBehaviour.Hub.GameTime.GetPlaybackTime(), this._warmupMillis);
				}
			}
		}

		public void SetWarmup(int seconds)
		{
			this.WarmupSeconds = seconds;
			this._warmupMillis = this.WarmupSeconds * 1000;
		}

		private void Awake()
		{
			SceneManager.sceneLoaded += new UnityAction<Scene, LoadSceneMode>(this.OnSceneLoaded);
			if (!GameHubBehaviour.Hub)
			{
				return;
			}
			GameHubBehaviour.Hub.MatchMan = this;
			this.ApplyArenaConfig();
			this.PreLoadMatch();
		}

		private void ApplyArenaConfig()
		{
			IGameArenaInfo currentArena = GameHubBehaviour.Hub.ArenaConfig.GetCurrentArena();
			this.SetWarmup(currentArena.WarmupTimeSeconds);
		}

		private void PreLoadMatch()
		{
		}

		private void OnDestroy()
		{
			if (GameHubBehaviour.Hub)
			{
				GameHubBehaviour.Hub.MatchMan = null;
				if (GameHubBehaviour.Hub.Net.IsClient())
				{
					GameHubBehaviour.Hub.Swordfish.MatchBI.ClientOnMatchEnded();
				}
			}
			SceneManager.sceneLoaded -= new UnityAction<Scene, LoadSceneMode>(this.OnSceneLoaded);
		}

		private void OnSceneLoaded(Scene scene, LoadSceneMode sceneMode)
		{
			if (!GameHubBehaviour.Hub)
			{
				return;
			}
			GameHubBehaviour.Hub.Swordfish.MatchBI.OnMatchLoaded();
			if (GameHubBehaviour.Hub.Net.IsClient())
			{
				return;
			}
			this._warmupStartDone = GameHubBehaviour.Hub.Match.LevelIsTutorial();
		}

		private void Start()
		{
			if (GameHubBehaviour.Hub.Match.LevelIsTutorial())
			{
				GameHubBehaviour.Hub.Swordfish.MatchBI.ServerOnMatchStarted();
			}
		}

		private void Update()
		{
			if (!GameHubBehaviour.Hub || GameHubBehaviour.Hub.Net.IsClient())
			{
				return;
			}
			if (this.WarmupDone || !this.CanStartMatch)
			{
				return;
			}
			if (this._updater.ShouldHalt())
			{
				return;
			}
			int num = GameHubBehaviour.Hub.GameTime.GetPlaybackTime() - this._countdownStartMatchTimeMilis;
			if (!this._warmupStartDone)
			{
				this._warmupStartDone = true;
				this.ThrowAnnounceEvent(AnnouncerLog.AnnouncerEventKinds.BeginningWarmup);
			}
			if (!this._warmup30Done && this._warmupMillis >= 30000 && this._warmupMillis - num <= 30000)
			{
				this._warmup30Done = true;
				this.ThrowAnnounceEvent(AnnouncerLog.AnnouncerEventKinds.Beginning30);
			}
			if (!this._warmup10Done && this._warmupMillis >= 10000 && this._warmupMillis - num <= 10000)
			{
				this._warmup10Done = true;
				this.ThrowAnnounceEvent(AnnouncerLog.AnnouncerEventKinds.Beginning10);
				if (GameHubBehaviour.Hub.Match.LevelIsTutorial())
				{
					GameHubBehaviour.Hub.BombManager.WarmupAlmostDone();
				}
			}
			if (!this._warmup05Done && this._warmupMillis >= 5000 && this._warmupMillis - num <= 5000)
			{
				this._warmup05Done = true;
				this.ThrowAnnounceEvent(AnnouncerLog.AnnouncerEventKinds.Beginning05);
			}
			if (!this._warmup00Done && this._warmupMillis - num <= 0)
			{
				this._warmup00Done = true;
				GameHubBehaviour.Hub.Match.State = MatchData.MatchState.MatchStarted;
				this._afkManager.ResetValues();
				GameHubBehaviour.Hub.Swordfish.MatchBI.ServerOnMatchStarted();
				GameHubBehaviour.Hub.Server.SpreadInfo();
				this.ThrowAnnounceEvent(AnnouncerLog.AnnouncerEventKinds.Beginning00);
				MatchLogWriter.WriteMatchCustomizationsBI();
			}
		}

		public void EndMatch(TeamKind winner)
		{
			MatchData match = GameHubBehaviour.Hub.Match;
			if (winner != TeamKind.Red)
			{
				if (winner != TeamKind.Blue)
				{
					match.State = MatchData.MatchState.Nothing;
				}
				else
				{
					match.State = MatchData.MatchState.MatchOverBluWins;
				}
			}
			else
			{
				match.State = MatchData.MatchState.MatchOverRedWins;
			}
			GameHubBehaviour.Hub.GameTime.SetTimeScale(1f);
			GameHubBehaviour.Hub.Server.SpreadInfo();
			foreach (Identifiable identifiable in GameHubBehaviour.Hub.ObjectCollection.GetObjects().Values)
			{
				CombatObject component = identifiable.GetComponent<CombatObject>();
				if (component != null)
				{
					component.Attributes.ForceInvincible = true;
				}
			}
			List<PlayerEndGamePresence.PresenceData> playerPresenceList = MatchLogWriter.WritePlayersPresence();
			if (GameHubBehaviour.Hub.Config.GetBoolValue(ConfigAccess.EnableHoplonTTEvent))
			{
				EndMatchPresenceUpdater.UpdateEndMatchPresence(playerPresenceList);
			}
			Mural.PostAll(new MatchController.GameOverMessage
			{
				State = match.State
			}, typeof(MatchController.GameOverMessage.IGameOverListener));
		}

		private void ThrowAnnounceEvent(AnnouncerLog.AnnouncerEventKinds logKind)
		{
			AnnouncerEvent content = new AnnouncerEvent
			{
				AnnouncerEventKind = logKind
			};
			GameHubBehaviour.Hub.Events.TriggerEvent(content);
		}

		public static readonly BitLogger Log = new BitLogger(typeof(MatchController));

		[InjectOnServer]
		private IAFKManager _afkManager;

		public PolygonCollider2D[] ArenaColliders;

		[HideInInspector]
		public int WarmupSeconds;

		private int _warmupMillis;

		private bool _warmupStartDone;

		private bool _warmup30Done;

		private bool _warmup10Done;

		private bool _warmup05Done;

		private bool _warmup00Done;

		private int _countdownStartMatchTimeMilis;

		private bool _canStartMatch;

		private TimedUpdater _updater = new TimedUpdater
		{
			PeriodMillis = 500
		};

		public struct GameOverMessage : Mural.IMuralMessage
		{
			public string Message
			{
				get
				{
					return "OnGameOver";
				}
			}

			public const string Msg = "OnGameOver";

			public MatchData.MatchState State;

			public interface IGameOverListener
			{
				void OnGameOver(MatchController.GameOverMessage msg);
			}
		}
	}
}
