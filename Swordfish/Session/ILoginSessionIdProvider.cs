using System;

namespace HeavyMetalMachines.Swordfish.Session
{
	public interface ILoginSessionIdProvider
	{
		string GetToken();
	}
}
