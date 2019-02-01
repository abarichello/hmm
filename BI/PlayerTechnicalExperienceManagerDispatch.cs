using System;
using Pocketverse;

namespace HeavyMetalMachines.BI
{
	public class PlayerTechnicalExperienceManagerDispatch : BaseRemoteStub<PlayerTechnicalExperienceManagerDispatch>, IPlayerTechnicalExperienceManagerDispatch, IDispatch
	{
		public PlayerTechnicalExperienceManagerDispatch(int guid) : base(guid)
		{
		}

		public void ReceivePlayerExperienceData(ExperienceDataSet data)
		{
			base.Dispatch(base.DispatchDestination(), base.OID, 1054, 12, base.IsReliable, new object[]
			{
				data
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
