using System;
using HeavyMetalMachines.Presenting;
using UniRx;

namespace HeavyMetalMachines.Spectator.View
{
	public class SpectatorHelperFactory : ISpectatorHelperFactory
	{
		public SpectatorHelperFactory(IViewLoader viewLoader)
		{
			this._viewLoader = viewLoader;
		}

		public IObservable<Unit> LoadSpectatorHelper()
		{
			return this._viewLoader.LoadView("UI_ADD_Spectator_Help");
		}

		private const string SceneName = "UI_ADD_Spectator_Help";

		private readonly IViewLoader _viewLoader;
	}
}
