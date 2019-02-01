using System;
using HeavyMetalMachines.Options;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat.Gadget
{
	[Serializable]
	public class UpgradeInfo
	{
		public string LocalizedName
		{
			get
			{
				return Language.Get(this.ContentName, TranslationSheets.CharactersMatchInfo);
			}
		}

		public string LocalizedDescription
		{
			get
			{
				string text = Language.Get(this.Description, TranslationSheets.Instances);
				ControlOptions.ControlActionInputType controlActionInputType = (!ControlOptions.IsUsingControllerJoystick(GameHubBehaviour.Hub)) ? ControlOptions.ControlActionInputType.Primary : ControlOptions.ControlActionInputType.Secondary;
				if (this.DescriptionActionKey != null)
				{
					int num = this.DescriptionActionKey.Length;
					if (num != 1)
					{
						if (num != 2)
						{
							if (num == 3)
							{
								text = string.Format(text, ControlOptions.GetTextlocalized(this.DescriptionActionKey[0], controlActionInputType), ControlOptions.GetTextlocalized(this.DescriptionActionKey[1], controlActionInputType), ControlOptions.GetTextlocalized(this.DescriptionActionKey[2], controlActionInputType));
							}
						}
						else
						{
							text = string.Format(text, ControlOptions.GetTextlocalized(this.DescriptionActionKey[0], controlActionInputType), ControlOptions.GetTextlocalized(this.DescriptionActionKey[1], controlActionInputType));
						}
					}
					else
					{
						text = string.Format(text, ControlOptions.GetTextlocalized(this.DescriptionActionKey[0], controlActionInputType));
					}
				}
				return text;
			}
		}

		public InstanceCategory Category;

		public string Name;

		public string ContentName;

		public ControlAction[] DescriptionActionKey;

		public string Description;

		public string DescriptionSummary;

		public string Tag;

		[Tooltip("The order in which the instance will be displayed in the instances selection screen.")]
		public int GuiOrderIndex;

		public string[] LevelNames;

		public int[] LevelPrices;

		public ExternalUpgrade[] ExternalUpgrades;
	}
}
