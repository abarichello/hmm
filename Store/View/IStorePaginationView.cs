using System;
using System.Collections.Generic;
using HeavyMetalMachines.Presenting;

namespace HeavyMetalMachines.Store.View
{
	public interface IStorePaginationView
	{
		void CreatePageToogles(int numberOfPages);

		IButton NextPageButton { get; }

		IButton PreviousPageButton { get; }

		List<IPageButtonToggle> PageToggles { get; }

		void RefreshPageToggles();
	}
}
