using System;

namespace HeavyMetalMachines.Tutorial.UnityUI
{
	public interface ITutorialComponentVisibility
	{
		bool CanBeVisible { get; set; }

		void Show();

		void Hide();
	}
}
