using System;
using System.Collections.Generic;
using HeavyMetalMachines.Bank;
using HeavyMetalMachines.Characters;
using HeavyMetalMachines.Combat;
using HeavyMetalMachines.Event;
using HeavyMetalMachines.Match;
using Pocketverse;
using Pocketverse.MuralContext;
using UnityEngine;

namespace HeavyMetalMachines.BotAI
{
	public class BotAISpawnManager : BaseSpawnManager
	{
		public override bool CarCreationFinished
		{
			get
			{
				return this.FirstSpawnFinished || (this.ObjectList.Count == 0 && !GameHubBehaviour.Hub.Match.LevelIsTutorial());
			}
		}

		protected override List<PlayerData> ObjectList
		{
			get
			{
				return GameHubBehaviour.Hub.Players.Bots;
			}
		}

		protected override PlayerEvent CreateSpawnData()
		{
			return new BotAIEvent();
		}

		protected override PlayerCarFactory GetFactory()
		{
			return this.Factory;
		}

		protected override void CreatePlayer(PlayerData bot)
		{
			int objId = bot.PlayerCarId;
			BotAISpawnManager.Log.InfoFormat("Creating bot character={1} for={0} ObjectId={2} Botname={3}", new object[]
			{
				bot.PlayerAddress,
				bot.GetCharacter(),
				objId,
				bot.Name
			});
			Transform transform = (!base.Spawn) ? base.transform : base.Spawn.GetStart(bot);
			PlayerEvent spawnData = this.CreateSpawnData();
			spawnData.TargetId = objId;
			spawnData.CauserId = -1;
			spawnData.Direction = transform.forward;
			spawnData.Location = transform.position;
			spawnData.SourceEventId = -1;
			spawnData.PlayerAddress = bot.PlayerAddress;
			spawnData.EventKind = PlayerEvent.Kind.Create;
			IFuture<Identifiable> fut = new Future<Identifiable>();
			this.GetFactory().OrderObject(objId, fut);
			Vector3 spawnPos = transform.position;
			Quaternion spawnRot = transform.rotation;
			this.BuildQueue.Add(objId, fut);
			fut.WhenDone(delegate(IFuture future)
			{
				BotAISpawnManager.Log.DebugFormat("CreateBot: factory ordered: ", new object[]
				{
					this.name
				});
				bot.CharacterInstance = fut.Result;
				bot.CharacterInstance.transform.position = spawnPos;
				bot.CharacterInstance.transform.rotation = spawnRot;
				SpawnController component = bot.CharacterInstance.GetComponent<SpawnController>();
				if (component)
				{
					if (GameHubBehaviour.Hub.Match.LevelIsTutorial())
					{
						component.SpawnPosition = ((!this.Spawn) ? this.transform : this.Spawn.GetSpawn(bot));
						component.StartPosition = component.SpawnPosition;
					}
					else
					{
						component.StartPosition = ((!this.Spawn) ? this.transform : this.Spawn.GetStart(bot));
						component.SpawnPosition = ((!this.Spawn) ? this.transform : this.Spawn.GetSpawn(bot));
					}
					component.Player = bot;
					component.RespawnTimeMillis = this.RespawnTimeSeconds * 1000;
					this.Players.Add(component);
				}
				this.CreatedPlayers++;
				if (!this.FirstSpawnFinished && this.CreatedPlayers >= this.ObjectList.Count)
				{
					this.FirstSpawnFinished = true;
					this.CallListenToAllPlayersSpawned();
				}
				if (GameHubBehaviour.Hub.Net.IsClient())
				{
					Mural.PostAll(new PlayerBuildComplete(objId, fut.Result), typeof(PlayerBuildComplete.IPlayerBuildCompleteListener));
					Mural.PostDeep(new MyPlayerBuildComplete(fut.Result), fut.Result);
					this.BuildQueue.Remove(objId);
					this.Respawn(spawnData, -1);
					return;
				}
				if (GameHubBehaviour.Hub.Match.LevelIsTutorial())
				{
					BotAIGoalManager component2 = bot.CharacterInstance.GetComponent<BotAIGoalManager>();
					component2.enabled = false;
					component2.TutorialDisabled = true;
				}
				PlayerController component3 = bot.CharacterInstance.GetComponent<PlayerController>();
				if (component3)
				{
					GameHubBehaviour.Hub.Input.Register(component3, bot.PlayerAddress);
				}
				PlayerStats component4 = bot.CharacterInstance.GetComponent<PlayerStats>();
				if (component4)
				{
					GameHubBehaviour.Hub.ScrapBank.RegisterPlayer(objId, component4);
				}
				this.BuildQueue.Remove(objId);
				this.Respawn(spawnData, -1);
			});
		}

		protected override void PreRespawn(PlayerEvent data, int eventId)
		{
			base.PreRespawn(data, eventId);
			for (int i = 0; i < this.Players.Count; i++)
			{
				if (this.Players[i].Player.PlayerCarId == data.TargetId)
				{
					base.ForceGadgetActivation(this.Players[i], GameHubBehaviour.Hub.BombManager.BombMovement.transform.position, GameHubBehaviour.Hub.BombManager.BombMovement.transform.forward);
					break;
				}
			}
		}

		public void BotForceUnspawn(PlayerData bot)
		{
			PlayerEvent playerEvent = this.CreateSpawnData();
			playerEvent.EventKind = PlayerEvent.Kind.Unspawn;
			playerEvent.Reason = SpawnReason.None;
			playerEvent.CauserId = -1;
			playerEvent.TargetId = bot.PlayerCarId;
			playerEvent.SourceEventId = -1;
			playerEvent.Location = Vector3.zero;
			playerEvent.PlayerAddress = bot.PlayerAddress;
			playerEvent.Assists = new List<int>();
			GameHubBehaviour.Hub.Events.TriggerEvent(playerEvent);
		}

		public override void OnCleanup(CleanupMessage msg)
		{
			base.OnCleanup(msg);
			this.FirstSpawnFinished = false;
		}

		private static readonly BitLogger Log = new BitLogger(typeof(BotAISpawnManager));

		public BotAICarFactory Factory;
	}
}
