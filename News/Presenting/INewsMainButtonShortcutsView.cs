using System;
using HeavyMetalMachines.Presenting;

namespace HeavyMetalMachines.News.Presenting
{
	public interface INewsMainButtonShortcutsView
	{
		IActivatable ShowShortcutActivatable { get; }

		IActivatable HideShortcutActivatable { get; }

		void UpdateShortcuts();
	}
}
