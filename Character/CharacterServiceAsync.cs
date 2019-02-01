using System;
using Pocketverse;

namespace HeavyMetalMachines.Character
{
	public class CharacterServiceAsync : BaseRemoteStub<CharacterServiceAsync>, ICharacterServiceAsync, IAsync
	{
		public CharacterServiceAsync(int guid) : base(guid)
		{
		}

		public IFuture<bool> SelectCharacter(int characterId)
		{
			IFuture<bool> future = new Future<bool>();
			base.ExecuteAsync(future, base.AsyncDestination(), base.CallbackTimeoutMillis, base.OID, 1051, 10, new object[]
			{
				characterId
			});
			return future;
		}

		public IFuture ConfirmSelection(byte playerAddress, int characterId)
		{
			IFuture<object> future = new Future<object>();
			base.ExecuteAsync(future, base.AsyncDestination(), base.CallbackTimeoutMillis, base.OID, 1051, 21, new object[]
			{
				playerAddress,
				characterId
			});
			return future;
		}

		public IFuture<int> PickCharacter()
		{
			IFuture<int> future = new Future<int>();
			base.ExecuteAsync(future, base.AsyncDestination(), base.CallbackTimeoutMillis, base.OID, 1051, 22, new object[0]);
			return future;
		}

		public IFuture ConfirmPick(byte playerAddress, int teamID, int characterId, Guid lastSkin)
		{
			IFuture<object> future = new Future<object>();
			base.ExecuteAsync(future, base.AsyncDestination(), base.CallbackTimeoutMillis, base.OID, 1051, 26, new object[]
			{
				playerAddress,
				teamID,
				characterId,
				lastSkin
			});
			return future;
		}

		public IFuture ConfirmSkin(string characterGuid, string skinGuid)
		{
			IFuture<object> future = new Future<object>();
			base.ExecuteAsync(future, base.AsyncDestination(), base.CallbackTimeoutMillis, base.OID, 1051, 27, new object[]
			{
				characterGuid,
				skinGuid
			});
			return future;
		}

		public IFuture ConfirmSkinCallback(byte playerAddress, int teamID, bool success, string skinGuid)
		{
			IFuture<object> future = new Future<object>();
			base.ExecuteAsync(future, base.AsyncDestination(), base.CallbackTimeoutMillis, base.OID, 1051, 30, new object[]
			{
				playerAddress,
				teamID,
				success,
				skinGuid
			});
			return future;
		}

		public IFuture<float> GetPickTime()
		{
			IFuture<float> future = new Future<float>();
			base.ExecuteAsync(future, base.AsyncDestination(), base.CallbackTimeoutMillis, base.OID, 1051, 31, new object[0]);
			return future;
		}

		public IFuture ServerSendPickModeStateToPlayer()
		{
			IFuture<object> future = new Future<object>();
			base.ExecuteAsync(future, base.AsyncDestination(), base.CallbackTimeoutMillis, base.OID, 1051, 33, new object[0]);
			return future;
		}

		public IFuture PickTimeOutClient(float customizationTime)
		{
			IFuture<object> future = new Future<object>();
			base.ExecuteAsync(future, base.AsyncDestination(), base.CallbackTimeoutMillis, base.OID, 1051, 47, new object[]
			{
				customizationTime
			});
			return future;
		}

		public IFuture SelectGrid(int gridIndex)
		{
			IFuture<object> future = new Future<object>();
			base.ExecuteAsync(future, base.AsyncDestination(), base.CallbackTimeoutMillis, base.OID, 1051, 49, new object[]
			{
				gridIndex
			});
			return future;
		}

		public IFuture ConfirmGridSelection(byte playerAddress, int gridIndex)
		{
			IFuture<object> future = new Future<object>();
			base.ExecuteAsync(future, base.AsyncDestination(), base.CallbackTimeoutMillis, base.OID, 1051, 50, new object[]
			{
				playerAddress,
				gridIndex
			});
			return future;
		}

		public IFuture<int> PickGrid()
		{
			IFuture<int> future = new Future<int>();
			base.ExecuteAsync(future, base.AsyncDestination(), base.CallbackTimeoutMillis, base.OID, 1051, 51, new object[0]);
			return future;
		}

		public IFuture ConfirmGridPick(byte playerAddress, int gridIndex, Guid skinSelected)
		{
			IFuture<object> future = new Future<object>();
			base.ExecuteAsync(future, base.AsyncDestination(), base.CallbackTimeoutMillis, base.OID, 1051, 54, new object[]
			{
				playerAddress,
				gridIndex,
				skinSelected
			});
			return future;
		}

		public IFuture ClientSendCounselorActivation(bool counselorActivation)
		{
			IFuture<object> future = new Future<object>();
			base.ExecuteAsync(future, base.AsyncDestination(), base.CallbackTimeoutMillis, base.OID, 1051, 55, new object[]
			{
				counselorActivation
			});
			return future;
		}

		int IAsync.get_CallbackTimeoutMillis()
		{
			return base.CallbackTimeoutMillis;
		}

		void IAsync.set_CallbackTimeoutMillis(int value)
		{
			base.CallbackTimeoutMillis = value;
		}
	}
}
