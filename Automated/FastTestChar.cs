using System;
using Assets.ClientApiObjects;
using Assets.ClientApiObjects.Components;
using HeavyMetalMachines.Frontend;
using Pocketverse;

namespace HeavyMetalMachines.Automated
{
	public class FastTestChar : GameHubBehaviour
	{
		private void Start()
		{
			FastTestChar.Log.DebugFormat("Added FastTestChar!", new object[0]);
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
			IItemType validChar = this.GetValidChar();
			CharacterItemTypeComponent component = validChar.GetComponent<CharacterItemTypeComponent>();
			FastTestChar.Log.Debug("TryingToPick");
			pick.SelectCharacter(component.CharacterId);
			pick.ConfirmPick(component.CharacterId);
			pick.ConfirmSkin(validChar.Id, Guid.Empty);
		}

		private IItemType GetValidChar()
		{
			int intValue = GameHubBehaviour.Hub.Config.GetIntValue(ConfigAccess.FastTestChar);
			IItemType result;
			GameHubBehaviour.Hub.InventoryColletion.AllCharactersByCharacterId.TryGetValue(intValue, out result);
			return result;
		}

		private static readonly BitLogger Log = new BitLogger(typeof(FastTestChar));
	}
}
