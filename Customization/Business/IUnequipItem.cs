using System;
using Assets.ClientApiObjects;

namespace HeavyMetalMachines.Customization.Business
{
	public interface IUnequipItem
	{
		void Setup(IItemType itemType);
	}
}
