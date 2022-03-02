using System;
using System.Collections.Generic;
using HeavyMetalMachines.CharacterSelection.Banning;
using HeavyMetalMachines.CharacterSelection.Configuration;
using HeavyMetalMachines.CharacterSelection.Skins;
using HeavyMetalMachines.CharacterSelection.State;
using HeavyMetalMachines.Matches;
using HeavyMetalMachines.Matches.API;
using Pocketverse;

namespace HeavyMetalMachines.CharacterSelection.Communication.SerializedData
{
	[Serializable]
	public class InitializationDataSerialized : IBitStreamSerializable
	{
		public InitializationDataSerialized()
		{
		}

		public InitializationDataSerialized(CharacterSelectionInitializationData data)
		{
			this.Data = data;
		}

		public CharacterSelectionInitializationData Data { get; private set; }

		public void WriteToBitStream(BitStream bs)
		{
			this.WriteConfiguration(this.Data.Configuration, bs);
			this.WriteState(this.Data.State, bs);
			this.WriteCharactersAvailability(this.Data.CharactersAvailability, bs);
		}

		private void WriteConfiguration(CharacterSelectionConfiguration configuration, BitStream bs)
		{
			bs.WriteLong(configuration.WaitForClientsReadyTimeout.Ticks);
			bs.WriteLong(configuration.StartupDuration.Ticks);
			bs.WriteLong(configuration.WrapupDuration.Ticks);
			bs.WriteBool(configuration.HidePickConfirmationsFromAdversaries);
			bs.WriteBool(configuration.AllowPickingDuplicatedCharactersBetweenTeams);
			bs.WriteLong(configuration.BanStage.BanResultDuration.Ticks);
			bs.WriteCompressedInt(configuration.BanStage.Steps.Length);
			foreach (BanStepConfiguration banStepConfiguration in configuration.BanStage.Steps)
			{
				bs.WriteCompressedInt(banStepConfiguration.BanStepIndex);
				bs.WriteLong(banStepConfiguration.Duration.Ticks);
			}
			bs.WriteCompressedInt(configuration.PickStage.Steps.Length);
			foreach (PickStepConfiguration pickStepConfiguration in configuration.PickStage.Steps)
			{
				bs.WriteCompressedInt(pickStepConfiguration.PickStepIndex);
				bs.WriteLong(pickStepConfiguration.Duration.Ticks);
				bs.WriteCompressedShort(pickStepConfiguration.PicksPerTeam);
				bs.WriteCompressedInt(pickStepConfiguration.PickingSlots.Length);
				foreach (CharacterSelectionSlot slot in pickStepConfiguration.PickingSlots)
				{
					CharacterSelectionSlotSerialized value = new CharacterSelectionSlotSerialized
					{
						Slot = slot
					};
					bs.WriteBitSerializable<CharacterSelectionSlotSerialized>(value);
				}
			}
		}

		private void WriteState(CharacterSelectionState state, BitStream bs)
		{
			bool flag = state.Step.Type == 5;
			bs.WriteBool(flag);
			if (flag)
			{
				CharacterSelectionPickStep characterSelectionPickStep = (CharacterSelectionPickStep)state.Step;
				bs.WriteCompressedInt(characterSelectionPickStep.PickingSlots.Length);
				foreach (CharacterSelectionSlot characterSelectionSlot in characterSelectionPickStep.PickingSlots)
				{
					bs.WriteCompressedInt(characterSelectionSlot.Index);
					bs.WriteCompressedInt(characterSelectionSlot.Team);
				}
			}
			bs.WriteCompressedInt(state.Step.Type);
			bs.WriteCompressedInt(state.Step.Index);
			bs.WriteLong(state.Step.RemainingDuration.Ticks);
			this.WriteTeamState(state.Team1State, bs);
			this.WriteTeamState(state.Team2State, bs);
			this.WriteBanStepResults(state.BanStepResults, bs);
		}

		private void WriteBanStepResults(BanStepResult[] banStepResults, BitStream bs)
		{
			bs.WriteCompressedInt(banStepResults.Length);
			foreach (BanStepResult banStepResult in banStepResults)
			{
				bs.WriteCompressedInt(banStepResult.BanStepIndex);
				bs.WriteCompressedInt(banStepResult.BanCandidates.Count);
				foreach (BanCandidate banCandidate in banStepResult.BanCandidates)
				{
					bs.WriteGuid(banCandidate.CharacterId);
					bs.WriteBool(banCandidate.IsBanned);
					bs.WriteCompressedInt(banCandidate.Votes);
					bs.WriteByte(banCandidate.Team);
				}
			}
		}

		private void WriteTeamState(TeamState teamState, BitStream bs)
		{
			bs.WriteCompressedInt(teamState.PlayerStates.Length);
			foreach (PlayerState playerState in teamState.PlayerStates)
			{
				bs.WriteLong(playerState.Client.PlayerId);
				bs.WriteBool(playerState.Client.IsBot);
				bs.WriteCompressedInt(playerState.Client.BotId);
				bs.WriteCompressedInt(playerState.Slot.Index);
				bs.WriteByte(playerState.Slot.Team);
				bs.WriteBool(playerState.IsConnected);
				this.WriteNullableGuid(playerState.PickedCharacterId, bs);
				this.WriteNullableGuid(playerState.CurrentSelectedCharacterId, bs);
				bs.WriteCompressedInt(playerState.BanStepVotes.Length);
				foreach (BanStepVote banStepVote in playerState.BanStepVotes)
				{
					bs.WriteBool(banStepVote.HasVoted);
					this.WriteNullableGuid(banStepVote.VotedCharacterId, bs);
				}
				bs.WriteCompressedInt(playerState.EquippedSkins.Length);
				foreach (CharacterEquippedSkin characterEquippedSkin in playerState.EquippedSkins)
				{
					bs.WriteGuid(characterEquippedSkin.CharacterId);
					bs.WriteGuid(characterEquippedSkin.SkinId);
				}
			}
		}

		private void WriteCharactersAvailability(CharactersAvailability availability, BitStream bs)
		{
			bs.WriteCompressedInt(availability.ForPlayers.Length);
			foreach (PlayerCharactersAvailability playerAvailability in availability.ForPlayers)
			{
				this.WritePlayerCharactersAvailability(playerAvailability, bs);
			}
			bs.WriteCompressedInt(availability.BannedByModeCharacterIds.Length);
			foreach (Guid value in availability.BannedByModeCharacterIds)
			{
				bs.WriteGuid(value);
			}
		}

		private void WritePlayerCharactersAvailability(PlayerCharactersAvailability playerAvailability, BitStream bs)
		{
			bs.WriteLong(playerAvailability.Client.PlayerId);
			bs.WriteBool(playerAvailability.Client.IsBot);
			bs.WriteCompressedInt(playerAvailability.Client.BotId);
			bs.WriteCompressedInt(playerAvailability.OwnedCharacterIds.Length);
			foreach (Guid value in playerAvailability.OwnedCharacterIds)
			{
				bs.WriteGuid(value);
			}
			bs.WriteCompressedInt(playerAvailability.AvailableFromRotationCharacterIds.Length);
			foreach (Guid value2 in playerAvailability.AvailableFromRotationCharacterIds)
			{
				bs.WriteGuid(value2);
			}
		}

		private void WriteNullableGuid(Guid? guid, BitStream bs)
		{
			bs.WriteBool(guid != null);
			if (guid != null)
			{
				bs.WriteGuid(guid.Value);
			}
		}

		private void WriteResult(CharacterSelectionResult result, BitStream bs)
		{
			if (result == null)
			{
				bs.WriteBool(false);
				return;
			}
			bs.WriteBool(true);
			CharacterSelectionResultSerialized value = new CharacterSelectionResultSerialized
			{
				Data = result
			};
			bs.WriteBitSerializable<CharacterSelectionResultSerialized>(value);
		}

		public void ReadFromBitStream(BitStream bs)
		{
			this.Data = new CharacterSelectionInitializationData();
			this.Data.Configuration = this.ReadConfiguration(bs);
			this.Data.State = this.ReadState(bs);
			this.Data.CharactersAvailability = this.ReadCharactersAvailability(bs);
		}

		private CharacterSelectionConfiguration ReadConfiguration(BitStream bs)
		{
			CharacterSelectionConfiguration characterSelectionConfiguration = new CharacterSelectionConfiguration();
			characterSelectionConfiguration.WaitForClientsReadyTimeout = this.ReadTimeSpan(bs);
			characterSelectionConfiguration.StartupDuration = this.ReadTimeSpan(bs);
			characterSelectionConfiguration.WrapupDuration = this.ReadTimeSpan(bs);
			characterSelectionConfiguration.HidePickConfirmationsFromAdversaries = bs.ReadBool();
			characterSelectionConfiguration.AllowPickingDuplicatedCharactersBetweenTeams = bs.ReadBool();
			characterSelectionConfiguration.BanStage = new BanStageConfiguration();
			characterSelectionConfiguration.BanStage.BanResultDuration = this.ReadTimeSpan(bs);
			int num = bs.ReadCompressedInt();
			if (num > 1048576)
			{
				InitializationDataSerialized._log.ErrorStackTrace(string.Format("ReadConfiguration={0}", new object[0]));
			}
			characterSelectionConfiguration.BanStage.Steps = new BanStepConfiguration[num];
			for (int i = 0; i < characterSelectionConfiguration.BanStage.Steps.Length; i++)
			{
				BanStepConfiguration banStepConfiguration = new BanStepConfiguration();
				banStepConfiguration.BanStepIndex = bs.ReadCompressedInt();
				banStepConfiguration.Duration = this.ReadTimeSpan(bs);
				characterSelectionConfiguration.BanStage.Steps[i] = banStepConfiguration;
			}
			characterSelectionConfiguration.PickStage = new PickStageConfiguration();
			characterSelectionConfiguration.PickStage.Steps = new PickStepConfiguration[bs.ReadCompressedInt()];
			for (int j = 0; j < characterSelectionConfiguration.PickStage.Steps.Length; j++)
			{
				PickStepConfiguration pickStepConfiguration = new PickStepConfiguration();
				pickStepConfiguration.PickStepIndex = bs.ReadCompressedInt();
				pickStepConfiguration.Duration = this.ReadTimeSpan(bs);
				pickStepConfiguration.PicksPerTeam = bs.ReadCompressedShort();
				pickStepConfiguration.PickingSlots = new CharacterSelectionSlot[bs.ReadCompressedInt()];
				for (int k = 0; k < pickStepConfiguration.PickingSlots.Length; k++)
				{
					CharacterSelectionSlotSerialized characterSelectionSlotSerialized = new CharacterSelectionSlotSerialized();
					bs.ReadBitSerializable<CharacterSelectionSlotSerialized>(ref characterSelectionSlotSerialized);
					pickStepConfiguration.PickingSlots[k] = characterSelectionSlotSerialized.Slot;
				}
				characterSelectionConfiguration.PickStage.Steps[j] = pickStepConfiguration;
			}
			return characterSelectionConfiguration;
		}

		private CharacterSelectionState ReadState(BitStream bs)
		{
			CharacterSelectionState characterSelectionState = new CharacterSelectionState();
			bool flag = bs.ReadBool();
			if (flag)
			{
				CharacterSelectionPickStep characterSelectionPickStep = new CharacterSelectionPickStep();
				int num = bs.ReadCompressedInt();
				characterSelectionPickStep.PickingSlots = new CharacterSelectionSlot[num];
				for (int i = 0; i < characterSelectionPickStep.PickingSlots.Length; i++)
				{
					characterSelectionPickStep.PickingSlots[i] = new CharacterSelectionSlot(bs.ReadCompressedInt(), bs.ReadCompressedInt());
				}
				characterSelectionState.Step = characterSelectionPickStep;
			}
			else
			{
				characterSelectionState.Step = new CharacterSelectionStep();
			}
			characterSelectionState.Step.Type = bs.ReadCompressedInt();
			characterSelectionState.Step.Index = bs.ReadCompressedInt();
			characterSelectionState.Step.RemainingDuration = this.ReadTimeSpan(bs);
			characterSelectionState.Team1State = this.ReadTeamState(bs);
			characterSelectionState.Team2State = this.ReadTeamState(bs);
			characterSelectionState.BanStepResults = this.ReadBanStepResults(bs);
			return characterSelectionState;
		}

		private BanStepResult[] ReadBanStepResults(BitStream bs)
		{
			BanStepResult[] array = new BanStepResult[bs.ReadCompressedInt()];
			for (int i = 0; i < array.Length; i++)
			{
				BanStepResult banStepResult = new BanStepResult();
				banStepResult.BanStepIndex = bs.ReadCompressedInt();
				int num = bs.ReadCompressedInt();
				banStepResult.BanCandidates = new List<BanCandidate>(num);
				for (int j = 0; j < num; j++)
				{
					BanCandidate banCandidate = new BanCandidate();
					banCandidate.CharacterId = bs.ReadGuid();
					banCandidate.IsBanned = bs.ReadBool();
					banCandidate.Votes = bs.ReadCompressedInt();
					banCandidate.Team = bs.ReadByte();
					banStepResult.BanCandidates.Add(banCandidate);
				}
				array[i] = banStepResult;
			}
			return array;
		}

		private TeamState ReadTeamState(BitStream bs)
		{
			TeamState teamState = new TeamState();
			teamState.PlayerStates = new PlayerState[bs.ReadCompressedInt()];
			for (int i = 0; i < teamState.PlayerStates.Length; i++)
			{
				PlayerState playerState = new PlayerState();
				MatchClient client = default(MatchClient);
				client.PlayerId = bs.ReadLong();
				client.IsBot = bs.ReadBool();
				client.BotId = bs.ReadCompressedInt();
				playerState.Client = client;
				playerState.Slot = new CharacterSelectionSlot(bs.ReadCompressedInt(), bs.ReadByte());
				playerState.IsConnected = bs.ReadBool();
				playerState.PickedCharacterId = this.ReadNullableGuid(bs);
				playerState.CurrentSelectedCharacterId = this.ReadNullableGuid(bs);
				playerState.BanStepVotes = new BanStepVote[bs.ReadCompressedInt()];
				for (int j = 0; j < playerState.BanStepVotes.Length; j++)
				{
					playerState.BanStepVotes[j] = new BanStepVote();
					playerState.BanStepVotes[j].HasVoted = bs.ReadBool();
					playerState.BanStepVotes[j].VotedCharacterId = this.ReadNullableGuid(bs);
				}
				playerState.EquippedSkins = new CharacterEquippedSkin[bs.ReadCompressedInt()];
				for (int k = 0; k < playerState.EquippedSkins.Length; k++)
				{
					Guid characterId = bs.ReadGuid();
					Guid skinId = bs.ReadGuid();
					playerState.EquippedSkins[k] = new CharacterEquippedSkin
					{
						CharacterId = characterId,
						SkinId = skinId
					};
				}
				teamState.PlayerStates[i] = playerState;
			}
			return teamState;
		}

		private CharactersAvailability ReadCharactersAvailability(BitStream bs)
		{
			CharactersAvailability charactersAvailability = new CharactersAvailability();
			charactersAvailability.ForPlayers = new PlayerCharactersAvailability[bs.ReadCompressedInt()];
			for (int i = 0; i < charactersAvailability.ForPlayers.Length; i++)
			{
				charactersAvailability.ForPlayers[i] = this.ReadPlayerCharactersAvailability(bs);
			}
			charactersAvailability.BannedByModeCharacterIds = new Guid[bs.ReadCompressedInt()];
			for (int j = 0; j < charactersAvailability.BannedByModeCharacterIds.Length; j++)
			{
				charactersAvailability.BannedByModeCharacterIds[j] = bs.ReadGuid();
			}
			return charactersAvailability;
		}

		private PlayerCharactersAvailability ReadPlayerCharactersAvailability(BitStream bs)
		{
			PlayerCharactersAvailability playerCharactersAvailability = new PlayerCharactersAvailability();
			MatchClient client = default(MatchClient);
			client.PlayerId = bs.ReadLong();
			client.IsBot = bs.ReadBool();
			client.BotId = bs.ReadCompressedInt();
			playerCharactersAvailability.Client = client;
			playerCharactersAvailability.OwnedCharacterIds = new Guid[bs.ReadCompressedInt()];
			for (int i = 0; i < playerCharactersAvailability.OwnedCharacterIds.Length; i++)
			{
				playerCharactersAvailability.OwnedCharacterIds[i] = bs.ReadGuid();
			}
			playerCharactersAvailability.AvailableFromRotationCharacterIds = new Guid[bs.ReadCompressedInt()];
			for (int j = 0; j < playerCharactersAvailability.AvailableFromRotationCharacterIds.Length; j++)
			{
				playerCharactersAvailability.AvailableFromRotationCharacterIds[j] = bs.ReadGuid();
			}
			return playerCharactersAvailability;
		}

		private CharacterSelectionResult ReadResult(BitStream bs)
		{
			if (!bs.ReadBool())
			{
				return null;
			}
			CharacterSelectionResultSerialized characterSelectionResultSerialized = new CharacterSelectionResultSerialized();
			bs.ReadBitSerializable<CharacterSelectionResultSerialized>(ref characterSelectionResultSerialized);
			return characterSelectionResultSerialized.Data;
		}

		private TimeSpan ReadTimeSpan(BitStream bs)
		{
			return new TimeSpan(bs.ReadLong());
		}

		private Guid? ReadNullableGuid(BitStream bs)
		{
			if (!bs.ReadBool())
			{
				return null;
			}
			return new Guid?(bs.ReadGuid());
		}

		private static readonly BitLogger _log = new BitLogger(typeof(InitializationDataSerialized));
	}
}
