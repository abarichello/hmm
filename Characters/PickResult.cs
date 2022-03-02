using System;

namespace HeavyMetalMachines.Characters
{
	public enum PickResult
	{
		None,
		Ok,
		IsClient,
		PlayerNotFound,
		CharacterNotFound,
		CharacterNotOwned,
		CharacterAlreadyPicked,
		PickPhaseOver,
		LastCharacterFallback
	}
}
