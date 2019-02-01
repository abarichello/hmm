using System;
using Assets.ClientApiObjects;

namespace HeavyMetalMachines.Battlepass
{
	public interface IBattlepassPremiumShopView
	{
		void SetVisibility(bool isVisible);

		bool IsVisible();

		void Setup(ItemTypeScriptableObject[] packages, TimeSpan remainingTime);

		void SetInteractability(bool isInteractible);
	}
}
