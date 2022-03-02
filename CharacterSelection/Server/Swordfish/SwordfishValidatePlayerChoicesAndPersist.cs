using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using ClientAPI;
using ClientAPI.Service.Interfaces;
using HeavyMetalMachines.CharacterSelection.DataTransferObjects;
using HeavyMetalMachines.CharacterSelection.Server.Infra;
using HeavyMetalMachines.Matches;
using HeavyMetalMachines.Swordfish;
using UniRx;

namespace HeavyMetalMachines.CharacterSelection.Server.Swordfish
{
	public class SwordfishValidatePlayerChoicesAndPersist : IValidatePlayerChoicesAndPersist
	{
		public SwordfishValidatePlayerChoicesAndPersist(IGetCurrentMatch getCurrentMatch, ICustomWS customWs)
		{
			this._getCurrentMatch = getCurrentMatch;
			this._customWs = customWs;
		}

		public IObservable<CharacterSelectionResult> Execute(CharacterSelectionResult currentResult)
		{
			IEnumerable<CharacterSelectionClientPick> playersPicks = from clientChoices in currentResult.Picks
			where !clientChoices.Client.IsBot
			select clientChoices;
			IEnumerable<CharacterSelectionClientPick> botsPicks = from clientChoices in currentResult.Picks
			where clientChoices.Client.IsBot
			select clientChoices;
			return Observable.Select<CharacterSelectionResult, CharacterSelectionResult>(Observable.Select<SerializableEquipSkinsResult, CharacterSelectionResult>(SwordfishObservable.FromStringSwordfishCall<SerializableEquipSkinsResult>(delegate(SwordfishClientApi.ParameterizedCallback<string> callback, SwordfishClientApi.ErrorCallback errorCallback)
			{
				SerializableEquipSkinsRequest serializableEquipSkinsRequest = this.ConvertToSerializableRequest(playersPicks);
				this._customWs.ExecuteCustomWSWithReturn(null, "EquipCharacterSkins", serializableEquipSkinsRequest.Serialize(), callback, errorCallback);
			}), new Func<SerializableEquipSkinsResult, CharacterSelectionResult>(this.ConvertToModelResult)), (CharacterSelectionResult playersValidateResult) => this.AddPicks(playersValidateResult, botsPicks));
		}

		private CharacterSelectionResult AddPicks(CharacterSelectionResult playersValidateResult, IEnumerable<CharacterSelectionClientPick> choicesToAdd)
		{
			return new CharacterSelectionResult
			{
				Picks = playersValidateResult.Picks.Concat(choicesToAdd).ToArray<CharacterSelectionClientPick>()
			};
		}

		private CharacterSelectionResult ConvertToModelResult(SerializableEquipSkinsResult serializableResult)
		{
			return new CharacterSelectionResult
			{
				Picks = serializableResult.Results.Select(new Func<SerializableEquipSkinResult, CharacterSelectionClientPick>(this.ConvertToModelClientChoices)).ToArray<CharacterSelectionClientPick>()
			};
		}

		private CharacterSelectionClientPick ConvertToModelClientChoices(SerializableEquipSkinResult serializableResult)
		{
			MatchClient client2 = GetCurrentMatchExtensions.Get(this._getCurrentMatch).Clients.First((MatchClient client) => SwordfishValidatePlayerChoicesAndPersist.CheckSerializableResultPlayerId(client, serializableResult.PlayerId));
			return new CharacterSelectionClientPick
			{
				Client = client2,
				CharacterId = serializableResult.CharacterGuid,
				SkinId = serializableResult.SkinGuid
			};
		}

		private static bool CheckSerializableResultPlayerId(MatchClient client, long playerId)
		{
			return client.PlayerId == playerId;
		}

		private SerializableEquipSkinsRequest ConvertToSerializableRequest(IEnumerable<CharacterSelectionClientPick> picks)
		{
			SerializableEquipSkinsRequest serializableEquipSkinsRequest = new SerializableEquipSkinsRequest();
			SerializableEquipSkinsRequest serializableEquipSkinsRequest2 = serializableEquipSkinsRequest;
			if (SwordfishValidatePlayerChoicesAndPersist.<>f__mg$cache0 == null)
			{
				SwordfishValidatePlayerChoicesAndPersist.<>f__mg$cache0 = new Func<CharacterSelectionClientPick, SerializableEquipSkinsRequestItem>(SwordfishValidatePlayerChoicesAndPersist.ConvertToRequestItem);
			}
			serializableEquipSkinsRequest2.Requests = picks.Select(SwordfishValidatePlayerChoicesAndPersist.<>f__mg$cache0).ToList<SerializableEquipSkinsRequestItem>();
			return serializableEquipSkinsRequest;
		}

		private static SerializableEquipSkinsRequestItem ConvertToRequestItem(CharacterSelectionClientPick clientChoices)
		{
			return new SerializableEquipSkinsRequestItem
			{
				PlayerId = clientChoices.Client.PlayerId,
				CharacterGuid = clientChoices.CharacterId,
				SkinGuid = clientChoices.SkinId
			};
		}

		private readonly IGetCurrentMatch _getCurrentMatch;

		private readonly ICustomWS _customWs;

		[CompilerGenerated]
		private static Func<CharacterSelectionClientPick, SerializableEquipSkinsRequestItem> <>f__mg$cache0;
	}
}
