using System;

namespace HeavyMetalMachines.Server.Pick.Apis
{
	public interface IPickModeState
	{
		bool IsInitialized { get; }

		void Initialize();

		bool Update();
	}
}
