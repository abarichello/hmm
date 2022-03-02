using System;
using HeavyMetalMachines.Presenting;
using HeavyMetalMachines.Presenting.NGui;
using HeavyMetalMachines.Presenting.Unity;
using UnityEngine;
using Zenject;

namespace HeavyMetalMachines.Friends.GUI
{
	public class NguiFriendTooltipRankView : MonoBehaviour, IFriendTooltipRankView
	{
		public IActivatable MainGroup
		{
			get
			{
				return this._mainGroup;
			}
		}

		public IActivatable LoadingActivatable
		{
			get
			{
				return this._loadingActivatable;
			}
		}

		public IDynamicImage RankDynamicImage
		{
			get
			{
				return this._rankDynamicImage;
			}
		}

		public IActivatable RankImageActivatable
		{
			get
			{
				return this._rankImageActivatable;
			}
		}

		public ILabel RankLabel
		{
			get
			{
				return this._rankLabel;
			}
		}

		private void Awake()
		{
			this._viewProvider.Bind<IFriendTooltipRankView>(this, null);
		}

		private void OnDestroy()
		{
			this._viewProvider.Unbind<IFriendTooltipRankView>(null);
		}

		[Inject]
		private IViewProvider _viewProvider;

		[SerializeField]
		private GameObjectActivatable _mainGroup;

		[SerializeField]
		private GameObjectActivatable _loadingActivatable;

		[SerializeField]
		private NGuiDynamicImage _rankDynamicImage;

		[SerializeField]
		private GameObjectActivatable _rankImageActivatable;

		[SerializeField]
		private NGuiLabel _rankLabel;
	}
}
