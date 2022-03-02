using System;
using Assets.ClientApiObjects;

namespace HeavyMetalMachines.Store.Presenter
{
	public interface IStoreItemPresenter
	{
		IItemType ItemType { get; }

		void Setup(IItemType itemType);

		void Dispose();

		void Show();
	}
}
