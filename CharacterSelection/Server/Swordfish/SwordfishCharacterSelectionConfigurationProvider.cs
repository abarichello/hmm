using System;
using ClientAPI.Service.Interfaces;
using HeavyMetalMachines.CharacterSelection.Configuration;
using HeavyMetalMachines.CharacterSelection.DataTransferObjects;
using HeavyMetalMachines.CharacterSelection.Server.Infra;
using HeavyMetalMachines.Matches;
using HeavyMetalMachines.Swordfish;
using UniRx;

namespace HeavyMetalMachines.CharacterSelection.Server.Swordfish
{
	public class SwordfishCharacterSelectionConfigurationProvider : ICharacterSelectionConfigurationsProvider
	{
		public SwordfishCharacterSelectionConfigurationProvider(ICustomWS customWs)
		{
			this._customWs = customWs;
		}

		public IObservable<CharacterSelectionConfiguration> GetById(int configurationId)
		{
			return Observable.Select<SerializableCharacterSelectionConfiguration, CharacterSelectionConfiguration>(this._customWs.ExecuteAsObservable("GetCharacterSelectionConfigurationById", configurationId.ToString()), new Func<SerializableCharacterSelectionConfiguration, CharacterSelectionConfiguration>(this.Deserialize));
		}

		public IObservable<CharacterSelectionConfiguration> Get(MatchMode matchMode, int playerCountPerTeam)
		{
			SerializableCharacterSelectionConfigurationRequest serializableCharacterSelectionConfigurationRequest = new SerializableCharacterSelectionConfigurationRequest
			{
				MatchMode = matchMode,
				PlayerCountPerTeam = playerCountPerTeam
			};
			return Observable.Select<SerializableCharacterSelectionConfiguration, CharacterSelectionConfiguration>(this._customWs.ExecuteAsObservable("GetCharacterSelectionConfiguration", serializableCharacterSelectionConfigurationRequest.Serialize()), new Func<SerializableCharacterSelectionConfiguration, CharacterSelectionConfiguration>(this.Deserialize));
		}

		private CharacterSelectionConfiguration Deserialize(SerializableCharacterSelectionConfiguration serializedConfiguration)
		{
			if (serializedConfiguration == null)
			{
				return null;
			}
			return CharacterSelectionConvertions.ToModel(serializedConfiguration);
		}

		private readonly ICustomWS _customWs;
	}
}
