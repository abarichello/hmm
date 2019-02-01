using System;
using Pocketverse;

namespace HeavyMetalMachines
{
	public interface IGarageControllerDispatch : IDispatch
	{
		void ServerPlayerOpenGadgetShop();

		void ServerPlayerCloseGadgetShop();

		void ServerBuyActivate(int gadgetKind);

		void ServerBuyUpgrade(int gadgetKind, string upgradeName);

		void ServerSellUpgrade(int gadgetKind, string upgradeName);

		void ServerSelectInstance(string instanceName);
	}
}
