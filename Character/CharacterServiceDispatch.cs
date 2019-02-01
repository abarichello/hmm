using System;
using Pocketverse;

namespace HeavyMetalMachines.Character
{
	public class CharacterServiceDispatch : BaseRemoteStub<CharacterServiceDispatch>, ICharacterServiceDispatch, IDispatch
	{
		public CharacterServiceDispatch(int guid) : base(guid)
		{
		}

		public void SelectCharacter(int characterId)
		{
			base.Dispatch(base.DispatchDestination(), base.OID, 1051, 10, base.IsReliable, new object[]
			{
				characterId
			});
		}

		public void ConfirmSelection(byte playerAddress, int characterId)
		{
			base.Dispatch(base.DispatchDestination(), base.OID, 1051, 21, base.IsReliable, new object[]
			{
				playerAddress,
				characterId
			});
		}

		public void PickCharacter()
		{
			base.Dispatch(base.DispatchDestination(), base.OID, 1051, 22, base.IsReliable, new object[0]);
		}

		public void ConfirmPick(byte playerAddress, int teamID, int characterId, Guid lastSkin)
		{
			base.Dispatch(base.DispatchDestination(), base.OID, 1051, 26, base.IsReliable, new object[]
			{
				playerAddress,
				teamID,
				characterId,
				lastSkin
			});
		}

		public void ConfirmSkin(string characterGuid, string skinGuid)
		{
			base.Dispatch(base.DispatchDestination(), base.OID, 1051, 27, base.IsReliable, new object[]
			{
				characterGuid,
				skinGuid
			});
		}

		public void ConfirmSkinCallback(byte playerAddress, int teamID, bool success, string skinGuid)
		{
			base.Dispatch(base.DispatchDestination(), base.OID, 1051, 30, base.IsReliable, new object[]
			{
				playerAddress,
				teamID,
				success,
				skinGuid
			});
		}

		public void GetPickTime()
		{
			base.Dispatch(base.DispatchDestination(), base.OID, 1051, 31, base.IsReliable, new object[0]);
		}

		public void ServerSendPickModeStateToPlayer()
		{
			base.Dispatch(base.DispatchDestination(), base.OID, 1051, 33, base.IsReliable, new object[0]);
		}

		public void PickTimeOutClient(float customizationTime)
		{
			base.Dispatch(base.DispatchDestination(), base.OID, 1051, 47, base.IsReliable, new object[]
			{
				customizationTime
			});
		}

		public void SelectGrid(int gridIndex)
		{
			base.Dispatch(base.DispatchDestination(), base.OID, 1051, 49, base.IsReliable, new object[]
			{
				gridIndex
			});
		}

		public void ConfirmGridSelection(byte playerAddress, int gridIndex)
		{
			base.Dispatch(base.DispatchDestination(), base.OID, 1051, 50, base.IsReliable, new object[]
			{
				playerAddress,
				gridIndex
			});
		}

		public void PickGrid()
		{
			base.Dispatch(base.DispatchDestination(), base.OID, 1051, 51, base.IsReliable, new object[0]);
		}

		public void ConfirmGridPick(byte playerAddress, int gridIndex, Guid skinSelected)
		{
			base.Dispatch(base.DispatchDestination(), base.OID, 1051, 54, base.IsReliable, new object[]
			{
				playerAddress,
				gridIndex,
				skinSelected
			});
		}

		public void ClientSendCounselorActivation(bool counselorActivation)
		{
			base.Dispatch(base.DispatchDestination(), base.OID, 1051, 55, base.IsReliable, new object[]
			{
				counselorActivation
			});
		}

		int IDispatch.get_CallbackTimeoutMillis()
		{
			return base.CallbackTimeoutMillis;
		}

		void IDispatch.set_CallbackTimeoutMillis(int value)
		{
			base.CallbackTimeoutMillis = value;
		}

		bool IDispatch.get_IsReliable()
		{
			return base.IsReliable;
		}

		void IDispatch.set_IsReliable(bool value)
		{
			base.IsReliable = value;
		}
	}
}
