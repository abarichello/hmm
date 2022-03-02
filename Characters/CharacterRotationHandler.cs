using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ClientAPI;
using HeavyMetalMachines.CharacterSelection.Rotation;
using HeavyMetalMachines.CharacterSelection.Rotation.DataTransferObjects;
using HeavyMetalMachines.DataTransferObjects.Player;
using HeavyMetalMachines.Infra.ScriptableObjects;
using HeavyMetalMachines.Match;
using HeavyMetalMachines.Swordfish;
using Hoplon.Serialization;
using Pocketverse;

namespace HeavyMetalMachines.Characters
{
	internal class CharacterRotationHandler
	{
		internal CharacterRotationHandler(IConfigLoader configLoader, SharedConfigs sharedConfigs, SwordfishConnection swordfishConnection, BattlepassProgressScriptableObject battlepassProgress, IRotationWeekStorage rotationWeekStorage, IGetPlayerRotation getPlayerRotation)
		{
			this._getPlayerRotation = getPlayerRotation;
			this._rotationWeekStorage = rotationWeekStorage;
			this._configLoader = configLoader;
			this._sharedConfigs = sharedConfigs;
			this._swordfishConnection = swordfishConnection;
			this._battlepassProgress = battlepassProgress;
		}

		public bool IsRotationDisabled { get; private set; }

		public IEnumerator Initialize()
		{
			if (this._configLoader.GetBoolValue(ConfigAccess.SkipSwordfish))
			{
				this.IsRotationDisabled = true;
				yield break;
			}
			if (this._configLoader.GetIntValue(ConfigAccess.FastTestChar, -1) > -1)
			{
				this.IsRotationDisabled = true;
				yield break;
			}
			while (!this._swordfishConnection.Connected)
			{
				yield return null;
			}
			CharacterCustomWS.GetRotation(new SwordfishClientApi.ParameterizedCallback<string>(this.OnRotationTaken), new SwordfishClientApi.ErrorCallback(this.OnGetRotationError));
			yield break;
		}

		private void OnRotationTaken(object state, string week)
		{
			this._rotationWeekStorage.Set((SerializableRotationWeek)((JsonSerializeable<!0>)week));
		}

		private void OnGetRotationError(object state, Exception exception)
		{
			CharacterRotationHandler.Log.Fatal("Failed to get current rotation week", exception);
		}

		public bool IsCharacterUnderRotationForPlayer(int charId, PlayerBag bag)
		{
			if (this._configLoader.GetBoolValue(ConfigAccess.SkipSwordfish))
			{
				this.IsRotationDisabled = true;
			}
			if (this._configLoader.GetIntValue(ConfigAccess.FastTestChar, -1) > -1)
			{
				this.IsRotationDisabled = true;
			}
			if (this.IsRotationDisabled)
			{
				return true;
			}
			int playerLevel = bag.AccountLevel(this._battlepassProgress.Progress, this._sharedConfigs.Battlepass);
			IEnumerable<int> charactersIdsFromRotation = this.GetCharactersIdsFromRotation(playerLevel);
			return charactersIdsFromRotation.Contains(charId);
		}

		public IEnumerable<int> GetAvailableCharactersIdsFor(PlayerData player, PlayerBag bag)
		{
			IEnumerable<int> ownedCharactersIds = this.GetOwnedCharactersIds(player);
			int playerLevel = bag.AccountLevel(player.BattlepassProgress, this._sharedConfigs.Battlepass);
			IEnumerable<int> charactersIdsFromRotation = this.GetCharactersIdsFromRotation(playerLevel);
			return ownedCharactersIds.Union(charactersIdsFromRotation);
		}

		private IEnumerable<int> GetOwnedCharactersIds(PlayerData player)
		{
			return player.GetOwnedCharacters().Distinct<int>();
		}

		private IEnumerable<int> GetCharactersIdsFromRotation(int playerLevel)
		{
			return this._getPlayerRotation.GetAvailableCharactersIdsFor(playerLevel);
		}

		private static readonly BitLogger Log = new BitLogger(typeof(CharacterRotationHandler));

		private readonly IConfigLoader _configLoader;

		private readonly SharedConfigs _sharedConfigs;

		private readonly SwordfishConnection _swordfishConnection;

		private readonly BattlepassProgressScriptableObject _battlepassProgress;

		private IRotationWeekStorage _rotationWeekStorage;

		private IGetPlayerRotation _getPlayerRotation;
	}
}
