using System;
using Assets.ClientApiObjects;
using HeavyMetalMachines.CharacterSelection.Rotation.DataTransferObjects;
using Pocketverse;

namespace HeavyMetalMachines.CharacterSelection.Rotation
{
	public class SkipSwordfishRotationWeekStorage : IRotationWeekStorage
	{
		public SkipSwordfishRotationWeekStorage(IConfigLoader configLoader, ICollectionScriptableObject collectionScriptableObject)
		{
			this._configLoader = configLoader;
			this._collectionScriptableObject = collectionScriptableObject;
		}

		public SerializableRotationWeek Get()
		{
			SerializableRotationWeek serializableRotationWeek = new SerializableRotationWeek();
			serializableRotationWeek.Configurations = new SerializableCharacterRotationConfigurationResult[1];
			serializableRotationWeek.Configurations[0] = new SerializableCharacterRotationConfigurationResult();
			serializableRotationWeek.Configurations[0].Arenas = new int[]
			{
				this._configLoader.GetIntValue(ConfigAccess.ArenaIndex)
			};
			serializableRotationWeek.Configurations[0].MinimumLevel = 0;
			string value = this._configLoader.GetValue(ConfigAccess.Rotation);
			if (string.IsNullOrEmpty(value))
			{
				serializableRotationWeek.Configurations[0].CharacterIds = new int[]
				{
					1,
					2,
					3,
					4,
					5,
					6,
					7,
					8,
					9,
					10,
					13,
					14,
					15,
					16,
					17,
					19,
					20
				};
			}
			else
			{
				serializableRotationWeek.Configurations[0].CharacterIds = Array.ConvertAll<string, int>(value.Split(new char[]
				{
					','
				}), new Converter<string, int>(this.StringToInt));
			}
			return serializableRotationWeek;
		}

		private int StringToInt(string s)
		{
			int result;
			int.TryParse(s, out result);
			return result;
		}

		public void Set(SerializableRotationWeek rotationWeek)
		{
			throw new NotImplementedException();
		}

		private Guid ConvertCharacterIdToGuid(int characterId)
		{
			return this._collectionScriptableObject.GetCharacterGuidId(characterId);
		}

		private readonly IConfigLoader _configLoader;

		private readonly ICollectionScriptableObject _collectionScriptableObject;
	}
}
