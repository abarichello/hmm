using System;
using HeavyMetalMachines.Customizations.Skins;

namespace HeavyMetalMachines.Customization.Skins
{
	public interface ISkinRarityProvider
	{
		bool TryGetSkinRarityInfo(TierKind skinTierKind, out SkinRarityInfo foundRarityInfo);
	}
}
