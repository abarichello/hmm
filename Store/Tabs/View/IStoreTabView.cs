using System;
using System.Collections.Generic;
using HeavyMetalMachines.Store.View;
using Hoplon.Input.UiNavigation;
using Hoplon.Input.UiNavigation.AxisSelector;
using UniRx;

namespace HeavyMetalMachines.Store.Tabs.View
{
	public interface IStoreTabView
	{
		List<IStoreItemView> StoreItems { get; }

		IUiNavigationSubGroupHolder UiNavigationSubGroupHolder { get; }

		IUiNavigationAxisSelector UiNavigationAxisSelector { get; }

		[Obsolete]
		void Show();

		[Obsolete]
		void Hide();

		void Reposition();

		IObservable<Unit> AnimateShow();

		IObservable<Unit> AnimateHide();
	}
}
