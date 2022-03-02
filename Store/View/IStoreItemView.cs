using System;
using HeavyMetalMachines.Presenting;

namespace HeavyMetalMachines.Store.View
{
	public interface IStoreItemView
	{
		IButton Button { get; }

		IDynamicImage MainImage { get; }

		IDynamicImage CategoryIconImage { get; }

		IDynamicImage CharacterIconImage { get; }

		ILabel NameLabel { get; }

		ILabel CategoryLabel { get; }

		ILabel DescriptionLabel { get; }

		ILabel SkinNameVariationLabel { get; }

		ILabel HardPriceLabel { get; }

		ILabel SoftPriceLabel { get; }

		IDynamicImage CharacterCarImage { get; }

		void Show();

		void Hide();

		void SetHasItem(bool hasItem);

		void SetHardPurchasable(bool isHardPurchasable);

		void SetSoftPurchasable(bool isSoftPurchasable);

		void SetSkinNameVariation(bool hasNameVariation);
	}
}
