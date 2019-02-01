using System;

namespace HeavyMetalMachines.Frontend
{
	public interface IHudWindow
	{
		bool CanBeHiddenByEscKey();

		void ChangeWindowVisibility(bool targetVisibleState);

		bool IsWindowVisible();

		int GetDepth();

		bool CanOpen();

		bool IsStackableWithType(Type type);
	}
}
