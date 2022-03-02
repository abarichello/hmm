using System;
using HeavyMetalMachines.Presenting;
using HeavyMetalMachines.Presenting.Unity;
using UnityEngine;

namespace HeavyMetalMachines.CompetitiveMode.View.Prizes
{
	public class UnityCompetitiveRewardItemView : MonoBehaviour, ICompetitiveRewardItemView
	{
		public IDynamicImage ThumbnailImage
		{
			get
			{
				return this._thumbnailImage;
			}
		}

		public IToggle Toggle
		{
			get
			{
				return this._toggle;
			}
		}

		public void Destroy()
		{
			Object.Destroy(base.gameObject);
		}

		[SerializeField]
		private UnityDynamicRawImage _thumbnailImage;

		[SerializeField]
		private UnityToggle _toggle;
	}
}
