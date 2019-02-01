using System;

namespace HeavyMetalMachines.Frontend
{
	public interface IGroupMemberRemovalLocker
	{
		void SetLockingInterval(int interval);

		void StartPreventMemberRemoval();

		void InterruptPreventMemberRemoval();
	}
}
