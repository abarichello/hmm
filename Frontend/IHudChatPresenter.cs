using System;
using UniRx;

namespace HeavyMetalMachines.Frontend
{
	public interface IHudChatPresenter
	{
		bool Visible { get; }

		IObservable<bool> VisibilityChanged();

		void AddChatMessage(string text);
	}
}
