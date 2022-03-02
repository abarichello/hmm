using System;
using HeavyMetalMachines.Presenting;
using HeavyMetalMachines.Presenting.NGui;
using HeavyMetalMachines.Social.Profile.Presenting;
using UnityEngine;

namespace HeavyMetalMachines.Profile.Presenting
{
	public class NguiProfileStatisticView : MonoBehaviour, IProfileStatisticView
	{
		public ISpriteImage IconImage
		{
			get
			{
				return this._iconImage;
			}
		}

		public ILabel TitleLabel
		{
			get
			{
				return this._titleLabel;
			}
		}

		public ILabel TotalValueLabel
		{
			get
			{
				return this._totalValueLabel;
			}
		}

		public ILabel AverageValueLabel
		{
			get
			{
				return this._averageValueLabel;
			}
		}

		[SerializeField]
		private NGuiImage _iconImage;

		[SerializeField]
		private NGuiLabel _titleLabel;

		[SerializeField]
		private NGuiLabel _totalValueLabel;

		[SerializeField]
		private NGuiLabel _averageValueLabel;
	}
}
