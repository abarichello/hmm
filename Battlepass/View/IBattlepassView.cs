using System;
using HeavyMetalMachines.Presenting;
using Hoplon.Input.UiNavigation;
using UniRx;

namespace HeavyMetalMachines.Battlepass.View
{
	public interface IBattlepassView
	{
		IUiNavigationGroupHolder UiNavigationGroupHolder { get; }

		IButton BackButton { get; }

		IObservable<Unit> PlayInAnimation();

		IObservable<Unit> PlayOutAnimation();
	}
}
