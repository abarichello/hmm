using System;

namespace HeavyMetalMachines.Frontend
{
	public interface ISpamFilter
	{
		bool IsSpam(string rawMessage, float currentUnscaledTime);
	}
}
