using System;

namespace HeavyMetalMachines.Spectator.View
{
	public interface ISpectatorHelperPresenter
	{
		void Initialize(ISpectatorHelperView view);

		void Dispose();
	}
}
