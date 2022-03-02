using System;
using HeavyMetalMachines.Characters;

namespace HeavyMetalMachines.Announcer
{
	public class FakeGetCharacterData : IGetCharacterData
	{
		public CharacterData[] GetAll()
		{
			throw new NotImplementedException();
		}

		public CharacterData Get(Guid characterId)
		{
			return new CharacterData
			{
				Id = characterId,
				NameDraft = "Mustang",
				RoundLookLeftIconName = "Mustang",
				RoundLookRightIconName = "Mustang",
				SquaredLookLeftIconName = "Mustang",
				SquaredLookRightIconName = "Mustang"
			};
		}
	}
}
