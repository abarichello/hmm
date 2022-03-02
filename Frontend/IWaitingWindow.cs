using System;

namespace HeavyMetalMachines.Frontend
{
	public interface IWaitingWindow
	{
		void Show(bool showLabel, Type type);

		void Show(Type type);

		void Hide(Type type);
	}
}
