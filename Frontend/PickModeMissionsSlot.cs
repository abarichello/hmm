using System;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	public class PickModeMissionsSlot : GameHubBehaviour
	{
		public void Setup(bool isEnabled, int currentValue, int totalValue, string description, bool isRare)
		{
			this.MissionProgressBar.value = ((totalValue != 0) ? ((float)currentValue / (float)totalValue) : 0f);
			this.NumberLabel.text = string.Format("{0}/{1}", currentValue, totalValue);
			this.DescriptionLabel.text = description;
			this.ProgressBarSprite.color = ((!isRare) ? this.NormalColor : this.RareColor);
			if (isEnabled)
			{
				this.Enable();
			}
			else
			{
				this.Disable();
			}
		}

		public void Disable()
		{
			this.BaseGameObject.SetActive(true);
			this.MissionProgressBar.gameObject.SetActive(false);
			this.DescriptionGroupGameObject.SetActive(false);
		}

		public void Enable()
		{
			this.BaseGameObject.SetActive(true);
			this.MissionProgressBar.gameObject.SetActive(true);
			this.DescriptionGroupGameObject.SetActive(true);
		}

		[SerializeField]
		protected Color NormalColor;

		[SerializeField]
		protected Color RareColor;

		[SerializeField]
		protected UI2DSprite ProgressBarSprite;

		[SerializeField]
		protected GameObject BaseGameObject;

		[SerializeField]
		protected UIProgressBar MissionProgressBar;

		[SerializeField]
		protected GameObject DescriptionGroupGameObject;

		[SerializeField]
		protected UILabel NumberLabel;

		[SerializeField]
		protected UILabel DescriptionLabel;
	}
}
