using System;
using HeavyMetalMachines.Match;
using Pocketverse;
using Zenject;

namespace HeavyMetalMachines.Combat.Business
{
	public class CombatControllerStorageInitializer : GameHubObject, ICombatControllerStorageInitializer
	{
		public void Initialize()
		{
			if (!GameHubObject.Hub.Events.Players.CarCreationFinished || !GameHubObject.Hub.Events.Bots.CarCreationFinished)
			{
				return;
			}
			for (int i = 0; i < GameHubObject.Hub.Players.PlayersAndBots.Count; i++)
			{
				PlayerData playerData = GameHubObject.Hub.Players.PlayersAndBots[i];
				CombatController component = playerData.CharacterInstance.GetComponent<CombatController>();
				this._combatControllerStorage.Set(playerData.CharacterInstance.ObjId, component);
			}
		}

		[Inject]
		private ICombatControllerStorage _combatControllerStorage;
	}
}
