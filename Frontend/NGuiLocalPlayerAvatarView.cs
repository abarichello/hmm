using System;
using HeavyMetalMachines.PlayerTooltip.Presenting;
using HeavyMetalMachines.Presenting;
using HeavyMetalMachines.Presenting.Tooltip;
using UnityEngine;
using Zenject;

namespace HeavyMetalMachines.Frontend
{
	public class NGuiLocalPlayerAvatarView : MonoBehaviour, ILocalPlayerAvatarView
	{
		private void Awake()
		{
			this._viewProvider.Bind<ILocalPlayerAvatarView>(this, null);
		}

		private void OnDestroy()
		{
			this._viewProvider.Unbind<ILocalPlayerAvatarView>(null);
		}

		public ITooltipTrigger TooltipTrigger
		{
			get
			{
				return this._tooltipTrigger;
			}
		}

		[SerializeField]
		private NGuiNewTooltipTrigger _tooltipTrigger;

		[Inject]
		private IViewProvider _viewProvider;
	}
}
