using System;

namespace HeavyMetalMachines.CharacterSelection.Communication
{
	public class CharacterSelectionRpcStorage
	{
		public CharacterSelectionClientReadyRpc ClientReady { get; set; }

		public CharacterSelectionInitializationRpc Initialization { get; set; }

		public CharacterSelectionBanVoteConfirmationRpc BanConfirmation { get; set; }

		public CharacterSelectionOthersBanVoteConfirmationRpc OthersBanVoteConfirmation { get; set; }

		public CharacterSelectionBanStepResultRpc BanStepResult { get; set; }

		public CharacterSelectionPickConfirmationRpc PickConfirmation { get; set; }

		public CharacterSelectionEquipSkinRpc SkinEquipped { get; set; }

		public CharacterSelectionEquipSkinConfirmationRpc EquippedSkinsConfirmations { get; set; }

		public CharacterSelectionOthersPickConfirmationRpc OthersPickConfirmation { get; set; }

		public CharacterSelectionPickStepResultRpc PickStepResult { get; set; }

		public CharacterSelectionStartRpc Start { get; set; }

		public CharacterSelectionResultRpc Result { get; set; }

		public CharacterChoiceChangesClientToServerRpc CharacterChoiceChangesClientToServer { get; set; }

		public CharacterChoiceChangesServerToClientsRpc CharacterChoiceChangesServerToClients { get; set; }

		public CharacterSelectionPickConfirmationRejectionRpc PickConfirmationRejection { get; set; }

		public CharacterSelectionClientsConnectionRpc ClientsConnection { get; set; }
	}
}
