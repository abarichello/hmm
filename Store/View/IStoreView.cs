using System;
using HeavyMetalMachines.Presenting;
using Hoplon.Input.UiNavigation;
using UniRx;

namespace HeavyMetalMachines.Store.View
{
	public interface IStoreView
	{
		IButton BackButton { get; }

		IUiNavigationGroupHolder UiNavigationGroupHolder { get; }

		IObservable<Unit> Initialize();

		IObservable<Unit> AnimateShow();

		IObservable<Unit> AnimateHide();
	}
}
