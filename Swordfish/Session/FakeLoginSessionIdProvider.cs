using System;

namespace HeavyMetalMachines.Swordfish.Session
{
	public class FakeLoginSessionIdProvider : ILoginSessionIdProvider
	{
		public string GetToken()
		{
			return "fake_token";
		}
	}
}
