using System;
using Pocketverse;

namespace HeavyMetalMachines.BI
{
	public class PlayerTechnicalExperienceManagerAsync : BaseRemoteStub<PlayerTechnicalExperienceManagerAsync>, IPlayerTechnicalExperienceManagerAsync, IAsync
	{
		public PlayerTechnicalExperienceManagerAsync(int guid) : base(guid)
		{
		}

		public IFuture ReceivePlayerExperienceData(ExperienceDataSet data)
		{
			IFuture<object> future = new Future<object>();
			base.ExecuteAsync(future, base.AsyncDestination(), base.CallbackTimeoutMillis, base.OID, 1079, 12, new object[]
			{
				data
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
