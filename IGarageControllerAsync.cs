using System;
using Pocketverse;

namespace HeavyMetalMachines
{
	public interface IGarageControllerAsync : IAsync
	{
		IFuture ServerPlayerOpenGadgetShop();

		IFuture ServerPlayerCloseGadgetShop();

		IFuture ServerBuyActivate(int gadgetKind);

		IFuture ServerBuyUpgrade(int gadgetKind, string upgradeName);

		IFuture ServerSellUpgrade(int gadgetKind, string upgradeName);

		IFuture ServerSelectInstance(string instanceName);
	}
}
