using System;
using HeavyMetalMachines.Presenting;

namespace HeavyMetalMachines.Store.View
{
	public interface IPageButtonToggle
	{
		void SetActive(bool active);

		IButton PageButton { get; }

		int PageIndex { get; set; }

		ILabel PageLabel { get; }

		IToggle Toggle { get; }
	}
}
