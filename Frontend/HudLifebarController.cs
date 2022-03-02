using System;
using HeavyMetalMachines.Combat;
using HeavyMetalMachines.Infra.Context;
using HeavyMetalMachines.Match;
using HeavyMetalMachines.Utils;
using Pocketverse;
using Zenject;

namespace HeavyMetalMachines.Frontend
{
	public class HudLifebarController : GameHubBehaviour, PlayerBuildComplete.IPlayerBuildCompleteListener
	{
		public void Awake()
		{
			ObjectPoolUtils.CreateInjectedObjectPool<HudLifebarPlayerObject>(this.HudLifebarPlayerObjectReference, out this._hudLifebarPlayerObjects, this.HudLifebarSettings.PlayerMaxPool, this._container, 1, null);
			for (int i = 0; i < this._hudLifebarPlayerObjects.Length; i++)
			{
				this._hudLifebarPlayerObjects[i].CreatePool();
			}
			GameHubBehaviour.Hub.BombManager.ListenToPhaseChange += this.OnBombPhaseChanged;
			GameHubBehaviour.Hub.Options.Game.OnCounselorActiveChanged += this.OnCounselorActiveChanged;
		}

		public void OnDestroy()
		{
			GameHubBehaviour.Hub.BombManager.ListenToPhaseChange -= this.OnBombPhaseChanged;
			GameHubBehaviour.Hub.Options.Game.OnCounselorActiveChanged -= this.OnCounselorActiveChanged;
		}

		private void OnBombPhaseChanged(BombScoreboardState bombState)
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
			if (playerOrBotsByObjectId.IsCurrentPlayer && GameHubBehaviour.Hub.Options.Game.CounselorActive)
			{
				this.HudLifebarCounselorReference.Setup(hudLifebarPlayerObject.CounselorParentGameObject.transform);
				this.HudLifebarCounselorReference.gameObject.SetActive(true);
			}
		}

		public void OnCounselorActiveChanged()
		{
			this.HudLifebarCounselorReference.gameObject.SetActive(GameHubBehaviour.Hub.Options.Game.CounselorActive);
			if (GameHubBehaviour.Hub.Options.Game.CounselorActive)
			{
				this.HudLifebarCounselorReference.Setup(this.HudLifebarPlayerObjectReference.CounselorParentGameObject.transform);
			}
		}

		public void SetAllPlayersVisibility(bool visibility)
		{
			for (int i = 0; i < this._hudLifebarPlayerObjects.Length; i++)
			{
				HudLifebarPlayerObject hudLifebarPlayerObject = this._hudLifebarPlayerObjects[i];
				if (!(hudLifebarPlayerObject.CombatObject == null))
				{
					hudLifebarPlayerObject.SetVisibility(visibility);
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
				if (!(hudLifebarPlayerObject.CombatObject != null) || hudLifebarPlayerObject.CombatObject.Id.ObjId == lifebarOwnerId)
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

		public HudLifebarPlayerObject GetLifebarObject(int combatId)
		{
			for (int i = 0; i < this._hudLifebarPlayerObjects.Length; i++)
			{
				HudLifebarPlayerObject hudLifebarPlayerObject = this._hudLifebarPlayerObjects[i];
				if (hudLifebarPlayerObject.CombatObject.Id.ObjId == combatId)
				{
					return hudLifebarPlayerObject;
				}
			}
			return null;
		}

		private static readonly BitLogger Log = new BitLogger(typeof(HudLifebarController));

		public HudLifebarSettings HudLifebarSettings;

		public HudLifebarPlayerObject HudLifebarPlayerObjectReference;

		public HudLifebarCounselor HudLifebarCounselorReference;

		private HudLifebarPlayerObject[] _hudLifebarPlayerObjects;

		[Inject]
		private DiContainer _container;
	}
}
