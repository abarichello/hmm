using System;
using HeavyMetalMachines.Login.DataTransferObjects;

namespace HeavyMetalMachines.Login
{
	public class GenerateClientLoginRequest : IGenerateClientLoginRequest
	{
		public SerializableClientLoginRequest Generate()
		{
			return new SerializableClientLoginRequest
			{
				ClientVersion = "Release.15.00.250"
			};
		}
	}
}
