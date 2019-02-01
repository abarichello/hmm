using System;
using HeavyMetalMachines.Character;
using HeavyMetalMachines.Frontend;
using Pocketverse;

namespace HeavyMetalMachines.Automated
{
	public class FastTestChar : GameHubBehaviour
	{
		private void Start()
		{
		}

		private void OnEnable()
		{
			GameHubBehaviour.Hub.State.getGameState(GameState.GameStateKind.Pick).ListenToStateSceneLevelLoaded += this.ListenToStateSceneLevelLoaded;
		}

		private void OnDisable()
		{
			GameHubBehaviour.Hub.State.getGameState(GameState.GameStateKind.Pick).ListenToStateSceneLevelLoaded -= this.ListenToStateSceneLevelLoaded;
		}

		private void ListenToStateSceneLevelLoaded()
		{
			this.TryToPick((PickModeSetup)GameHubBehaviour.Hub.State.Current);
		}

		private void TryToPick(PickModeSetup pick)
		{
			CharacterInfo validChar = this.GetValidChar();
			pick.SelectCharacter(validChar.CharacterId);
			pick.ConfirmPick(validChar.CharacterId);
			pick.ConfirmSkin(validChar.CharacterItemTypeGuid, Guid.Empty);
		}

		private CharacterInfo GetValidChar()
		{
			return GameHubBehaviour.Hub.InventoryColletion.GetCharacterInfoByCharacterId(GameHubBehaviour.Hub.Config.GetIntValue(ConfigAccess.FastTestChar));
		}

		private static readonly BitLogger Log = new BitLogger(typeof(FastTestChar));
	}
}
