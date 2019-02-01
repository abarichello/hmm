using System;
using HeavyMetalMachines.Combat;
using HeavyMetalMachines.Match;
using HeavyMetalMachines.Utils;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	public class HudLifebarController : GameHubBehaviour, PlayerBuildComplete.IPlayerBuildCompleteListener
	{
		public void Awake()
		{
			ObjectPoolUtils.CreateObjectPool<HudLifebarPlayerObject>(this.HudLifebarPlayerObjectReference, out this._hudLifebarPlayerObjects, this.HudLifebarSettings.PlayerMaxPool);
			for (int i = 0; i < this._hudLifebarPlayerObjects.Length; i++)
			{
				this._hudLifebarPlayerObjects[i].CreatePool();
			}
			GameHubBehaviour.Hub.BombManager.ListenToPhaseChange += this.OnBombPhaseChanged;
			if (GameHubBehaviour.Hub.Match.LevelIsTutorial())
			{
				ObjectPoolUtils.CreateObjectPool<HudLifebarCreepObject>(this.HudLifebarCreepObjectReference, out this._hudLifebarCreepObjects, this.HudLifebarSettings.CreepMaxPool);
				for (int j = 0; j < this._hudLifebarCreepObjects.Length; j++)
				{
					this._hudLifebarCreepObjects[j].CreatePool();
				}
				GameHubBehaviour.Hub.Events.Creeps.ListenToCreepSpawn += this.OnCreepSpawn;
			}
			else
			{
				UnityEngine.Object.Destroy(this.HudLifebarCreepObjectReference.transform.parent.gameObject);
			}
		}

		public void OnDestroy()
		{
			GameHubBehaviour.Hub.BombManager.ListenToPhaseChange -= this.OnBombPhaseChanged;
			GameHubBehaviour.Hub.Events.Creeps.ListenToCreepSpawn -= this.OnCreepSpawn;
		}

		private void OnCreepSpawn(CreepController creepController)
		{
			HudLifebarCreepObject hudLifebarCreepObject = null;
			if (!ObjectPoolUtils.TryToGetFreeObject<HudLifebarCreepObject>(this._hudLifebarCreepObjects, ref hudLifebarCreepObject))
			{
				HudLifebarController.Log.ErrorFormat("No lifebar available for creep: {0}", new object[]
				{
					creepController.Combat.Id.ObjId
				});
				return;
			}
			hudLifebarCreepObject.Setup(creepController.Combat);
			hudLifebarCreepObject.gameObject.SetActive(true);
		}

		private void OnBombPhaseChanged(BombScoreBoard.State bombState)
		{
			if (!GameHubBehaviour.Hub.GuiScripts.AfkControllerGui.IsWindowVisible())
			{
				this.SetAllPlayersVisibility(true);
			}
		}

		public void OnPlayerBuildComplete(PlayerBuildComplete evt)
		{
			HudLifebarPlayerObject hudLifebarPlayerObject = null;
			if (!ObjectPoolUtils.TryToGetFreeObject<HudLifebarPlayerObject>(this._hudLifebarPlayerObjects, ref hudLifebarPlayerObject))
			{
				HudLifebarController.Log.ErrorFormat("No lifebar available for player: {0}", new object[]
				{
					evt.Id
				});
				return;
			}
			PlayerData playerOrBotsByObjectId = GameHubBehaviour.Hub.Players.GetPlayerOrBotsByObjectId(evt.Id);
			CombatObject bitComponent = playerOrBotsByObjectId.CharacterInstance.GetBitComponent<CombatObject>();
			hudLifebarPlayerObject.Setup(bitComponent);
			hudLifebarPlayerObject.gameObject.SetActive(true);
			if (playerOrBotsByObjectId.IsCurrentPlayer)
			{
				this.HudLifebarCounselorReference.Setup(hudLifebarPlayerObject.CounselorParentGameObject.transform);
				this.HudLifebarCounselorReference.gameObject.SetActive(true);
			}
		}

		public void SetAllPlayersVisibility(bool visibility)
		{
			for (int i = 0; i < this._hudLifebarPlayerObjects.Length; i++)
			{
				HudLifebarPlayerObject hudLifebarPlayerObject = this._hudLifebarPlayerObjects[i];
				if (!(hudLifebarPlayerObject.CombatObject == null))
				{
					hudLifebarPlayerObject.SetVisible(visibility);
				}
			}
		}

		public void SetCurrentPlayerVisibility(int combatId, bool visible)
		{
			for (int i = 0; i < this._hudLifebarPlayerObjects.Length; i++)
			{
				HudLifebarPlayerObject hudLifebarPlayerObject = this._hudLifebarPlayerObjects[i];
				if (hudLifebarPlayerObject.CombatObject.Id.ObjId == combatId)
				{
					hudLifebarPlayerObject.SetCanBeVisible(visible);
					break;
				}
			}
		}

		public void SetAttachedLifebarVisibility(int lifebarOwnerId, int attachedObjectId, bool visible)
		{
			for (int i = 0; i < this._hudLifebarPlayerObjects.Length; i++)
			{
				HudLifebarPlayerObject hudLifebarPlayerObject = this._hudLifebarPlayerObjects[i];
				if (hudLifebarPlayerObject.CombatObject.Id.ObjId == lifebarOwnerId)
				{
					hudLifebarPlayerObject.SetAttachedGroupVisibility(attachedObjectId, visible);
				}
			}
		}

		public void HackPlayerLifebarToEP(int combatId, bool unhack)
		{
			for (int i = 0; i < this._hudLifebarPlayerObjects.Length; i++)
			{
				HudLifebarPlayerObject hudLifebarPlayerObject = this._hudLifebarPlayerObjects[i];
				if (hudLifebarPlayerObject.CombatObject.Id.ObjId == combatId)
				{
					hudLifebarPlayerObject.HackToEP(!unhack);
					break;
				}
			}
		}

		private static readonly BitLogger Log = new BitLogger(typeof(HudLifebarController));

		public HudLifebarSettings HudLifebarSettings;

		public HudLifebarPlayerObject HudLifebarPlayerObjectReference;

		public HudLifebarCreepObject HudLifebarCreepObjectReference;

		public HudLifebarCounselor HudLifebarCounselorReference;

		private HudLifebarPlayerObject[] _hudLifebarPlayerObjects;

		private HudLifebarCreepObject[] _hudLifebarCreepObjects;
	}
}
