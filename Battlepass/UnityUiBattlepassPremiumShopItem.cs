using System;
using Assets.ClientApiObjects;
using Assets.ClientApiObjects.Components;
using UnityEngine;
using UnityEngine.UI;

namespace HeavyMetalMachines.Battlepass
{
	public class UnityUiBattlepassPremiumShopItem : MonoBehaviour, IBattlepassPremiumShopItem
	{
		public void Setup(ItemTypeScriptableObject package)
		{
			PremiumItemTypeComponent component = package.GetComponent<PremiumItemTypeComponent>();
			this._title.text = string.Format(Language.Get(component.TitleDraft, TranslationSheets.Battlepass), component.LevelCount);
			for (int i = 0; i < this._bulletPoints.Length; i++)
			{
				this._bulletPoints[i].text = string.Format(Language.Get(component.DescriptionDrafts[i], TranslationSheets.Battlepass), component.LevelCount);
			}
			this._value.text = package.ReferenceHardPrice.ToString();
		}

		[SerializeField]
		private Text _title;

		[SerializeField]
		private Text[] _bulletPoints;

		[SerializeField]
		private Text _value;
	}
}
