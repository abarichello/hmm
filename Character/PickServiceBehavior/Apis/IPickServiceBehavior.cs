using System;

namespace HeavyMetalMachines.Character.PickServiceBehavior.Apis
{
	public interface IPickServiceBehavior
	{
		bool SelectCharacter(byte sender, int characterId);

		PickResult PickCharacter(byte sender);

		void ConfirmSkin(byte sender, string characterGuid, string skinGuid);

		void SelectGrid(byte sender, int gridIndex);

		PickGridResult PickGrid(byte sender);
	}
}
