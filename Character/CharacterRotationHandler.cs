using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using ClientAPI;
using HeavyMetalMachines.Infra.ScriptableObjects;
using HeavyMetalMachines.Match;
using HeavyMetalMachines.Swordfish;
using HeavyMetalMachines.Swordfish.Player;
using Pocketverse;

namespace HeavyMetalMachines.Character
{
	internal class CharacterRotationHandler
	{
		internal CharacterRotationHandler(ConfigLoader configLoader, SharedConfigs sharedConfigs, SwordfishConnection swordfishConnection, BattlepassProgressScriptableObject battlepassProgress)
		{
			this._configLoader = configLoader;
			this._sharedConfigs = sharedConfigs;
			this._swordfishConnection = swordfishConnection;
			this._battlepassProgress = battlepassProgress;
		}

		public bool IsRotationDisabled { get; private set; }

		public IEnumerator Initialize()
		{
			if (this._configLoader.GetBoolValue(ConfigAccess.NoRotation))
			{
				this.IsRotationDisabled = true;
				CharacterRotationHandler.Log.Warn("Rotation disabled!");
				yield break;
			}
			if (this._configLoader.GetBoolValue(ConfigAccess.SkipSwordfish))
			{
				this.SetRotationCharacters(this.GetDebugRotationWeek());
				CharacterRotationHandler.Log.Warn("Debug Rotation was set!");
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
			Week rotationCharacters = (Week)((JsonSerializeable<T>)week);
			this.SetRotationCharacters(rotationCharacters);
		}

		private void OnGetRotationError(object state, Exception exception)
		{
			CharacterRotationHandler.Log.Fatal("Failed to get current rotation week", exception);
			this.SetRotationCharacters(null);
		}

		public Week GetDebugRotationWeek()
		{
			Week week = new Week();
			week.VeteranLevel = this._configLoader.GetIntValue(ConfigAccess.RotationVeteranLevel);
			string value = this._configLoader.GetValue(ConfigAccess.RotationVeterans);
			string value2 = this._configLoader.GetValue(ConfigAccess.RotationRookies);
			Week week2 = week;
			string[] array = value.Split(new char[]
			{
				','
			});
			if (CharacterRotationHandler.<>f__mg$cache0 == null)
			{
				CharacterRotationHandler.<>f__mg$cache0 = new Converter<string, int>(CharacterRotationHandler.StringToInt);
			}
			week2.Veterans = Array.ConvertAll<string, int>(array, CharacterRotationHandler.<>f__mg$cache0);
			Week week3 = week;
			string[] array2 = value2.Split(new char[]
			{
				','
			});
			if (CharacterRotationHandler.<>f__mg$cache1 == null)
			{
				CharacterRotationHandler.<>f__mg$cache1 = new Converter<string, int>(CharacterRotationHandler.StringToInt);
			}
			week3.Rookies = Array.ConvertAll<string, int>(array2, CharacterRotationHandler.<>f__mg$cache1);
			return week;
		}

		private static int StringToInt(string s)
		{
			int result;
			int.TryParse(s, out result);
			return result;
		}

		public void SetRotationCharacters(Week currentRotationWeek)
		{
			this._currentRotation = currentRotationWeek;
			if (this._currentRotation == null || this._currentRotation.Rookies.Length < 3 || this._currentRotation.Veterans.Length < 3 || this._currentRotation.VeteranLevel <= 0)
			{
				CharacterRotationHandler.Log.WarnFormat("Disabling rotation week. json={0}", new object[]
				{
					(this._currentRotation != null) ? this._currentRotation.ToString() : "null"
				});
				this.IsRotationDisabled = true;
			}
		}

		public bool IsCharacterUnderRotationForPlayer(int charId, PlayerBag bag)
		{
			if (this.IsRotationDisabled || this._configLoader.GetIntValue(ConfigAccess.FastTestChar, -1) > -1)
			{
				return true;
			}
			int num = bag.AccountLevel(this._battlepassProgress.Progress, this._sharedConfigs.Battlepass);
			int[] array;
			if (num < this._currentRotation.VeteranLevel)
			{
				array = this._currentRotation.Rookies;
			}
			else
			{
				array = this._currentRotation.Veterans;
			}
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i] == charId)
				{
					return true;
				}
			}
			return false;
		}

		public int[] GetAvailableCharactersFor(PlayerData player, PlayerBag bag)
		{
			List<int> list = new List<int>();
			int[] ownedCharacters = player.GetOwnedCharacters();
			if (ownedCharacters != null)
			{
				for (int i = 0; i < ownedCharacters.Length; i++)
				{
					if (!list.Contains(ownedCharacters[i]))
					{
						list.Add(ownedCharacters[i]);
					}
				}
			}
			int num = bag.AccountLevel(player.BattlepassProgress, this._sharedConfigs.Battlepass);
			int[] array;
			if (num < this._currentRotation.VeteranLevel)
			{
				array = this._currentRotation.Rookies;
			}
			else
			{
				array = this._currentRotation.Veterans;
			}
			for (int j = 0; j < array.Length; j++)
			{
				if (!list.Contains(array[j]))
				{
					list.Add(array[j]);
				}
			}
			return list.ToArray();
		}

		private static readonly BitLogger Log = new BitLogger(typeof(CharacterRotationHandler));

		private readonly ConfigLoader _configLoader;

		private readonly SharedConfigs _sharedConfigs;

		private readonly SwordfishConnection _swordfishConnection;

		private readonly BattlepassProgressScriptableObject _battlepassProgress;

		private Week _currentRotation;

		[CompilerGenerated]
		private static Converter<string, int> <>f__mg$cache0;

		[CompilerGenerated]
		private static Converter<string, int> <>f__mg$cache1;
	}
}
