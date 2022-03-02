using System;
using HeavyMetalMachines.Presenting;
using HeavyMetalMachines.Presenting.NGui;
using HeavyMetalMachines.Presenting.Unity;
using UnityEngine;

namespace HeavyMetalMachines.CompetitiveMode.View.Loading
{
	public class NguiLoadingVersusPlayerRankView : MonoBehaviour, ILoadingVersusPlayerRankView
	{
		public IActivatable RankGroup
		{
			get
			{
				return this._rankGroup;
			}
		}

		public IDynamicImage RankDynamicImage
		{
			get
			{
				return this._rankDynamicImage;
			}
		}

		[SerializeField]
		private GameObjectActivatable _rankGroup;

		[SerializeField]
		private NGuiDynamicImage _rankDynamicImage;
	}
}
