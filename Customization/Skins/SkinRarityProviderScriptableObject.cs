using System;
using HeavyMetalMachines.Customizations.Skins;
using UnityEngine;

namespace HeavyMetalMachines.Customization.Skins
{
	public class SkinRarityProviderScriptableObject : ScriptableObject, ISkinRarityProvider
	{
		public bool TryGetSkinRarityInfo(TierKind skinTierKind, out SkinRarityInfo foundRarityInfo)
		{
			for (int i = 0; i < this._skinRarityInfo.Length; i++)
			{
				SkinRarityInfo skinRarityInfo = this._skinRarityInfo[i];
				if (skinRarityInfo.TierKind == skinTierKind)
				{
					foundRarityInfo = skinRarityInfo;
					return true;
				}
			}
			foundRarityInfo = default(SkinRarityInfo);
			return false;
		}

		[SerializeField]
		private SkinRarityInfo[] _skinRarityInfo;
	}
}
