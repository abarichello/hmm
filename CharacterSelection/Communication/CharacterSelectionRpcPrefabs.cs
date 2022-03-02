using System;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.CharacterSelection.Communication
{
	public class CharacterSelectionRpcPrefabs : MonoBehaviour
	{
		[SerializeField]
		public Identifiable Identifiable;

		[SerializeField]
		public CharacterSelectionClientReadyRpc ClientReadyRpc;

		[SerializeField]
		public CharacterSelectionInitializationRpc InitializationRpc;

		[SerializeField]
		public CharacterSelectionBanVoteConfirmationRpc BanConfirmation;

		[SerializeField]
		public CharacterSelectionOthersBanVoteConfirmationRpc OthersBanConfirmation;

		[SerializeField]
		public CharacterSelectionBanStepResultRpc BanStepResult;

		[SerializeField]
		public CharacterSelectionPickConfirmationRpc PickConfirmation;

		[SerializeField]
		public CharacterSelectionOthersPickConfirmationRpc OthersPickConfirmation;

		[SerializeField]
		public CharacterSelectionPickStepResultRpc PickStepResult;

		[SerializeField]
		public CharacterSelectionStartRpc Start;

		[SerializeField]
		public CharacterSelectionResultRpc Result;

		[SerializeField]
		public CharacterChoiceChangesClientToServerRpc CharacterChoiceChangesClientToServer;

		[SerializeField]
		public CharacterChoiceChangesServerToClientsRpc CharacterChoiceChangesServerToClients;

		[SerializeField]
		public CharacterSelectionPickConfirmationRejectionRpc PickConfirmationRejection;

		[SerializeField]
		public CharacterSelectionClientsConnectionRpc ClientsConnection;

		[SerializeField]
		public CharacterSelectionEquipSkinRpc EquipSkin;

		[SerializeField]
		public CharacterSelectionEquipSkinConfirmationRpc EquipSkinConfirmations;
	}
}
