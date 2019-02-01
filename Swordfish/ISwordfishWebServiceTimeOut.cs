using System;

namespace HeavyMetalMachines.Swordfish
{
	internal interface ISwordfishWebServiceTimeOut
	{
		bool CloseGame();

		string TimeOutMessage();

		void TimeOut();
	}
}
