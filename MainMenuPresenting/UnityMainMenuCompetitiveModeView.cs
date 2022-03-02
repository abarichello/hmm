using System;
using HeavyMetalMachines.Infra.DependencyInjection.Attributes;
using HeavyMetalMachines.Presenting;
using HeavyMetalMachines.Presenting.NGui;
using JetBrains.Annotations;
using UnityEngine;

namespace HeavyMetalMachines.MainMenuPresenting
{
	public class UnityMainMenuCompetitiveModeView : MonoBehaviour, IMainMenuCompetitiveModeView
	{
		public IButton OpenCompetitiveModeButton
		{
			get
			{
				return this._openRankingButton;
			}
		}

		public IActivatable OpenRankingButtonActivatable
		{
			get
			{
				return new FunctionActivatable(delegate()
				{
					this._openRankingButtonParent.SetActive(true);
				}, delegate()
				{
					this._openRankingButtonParent.SetActive(false);
				});
			}
		}

		private void Awake()
		{
			this._viewProvider.Bind<IMainMenuCompetitiveModeView>(this, null);
		}

		private void OnDestroy()
		{
			this._viewProvider.Unbind<IMainMenuCompetitiveModeView>(null);
		}

		[SerializeField]
		private NGuiButton _openRankingButton;

		[SerializeField]
		private GameObject _openRankingButtonParent;

		[InjectOnClient]
		[UsedImplicitly]
		private IViewProvider _viewProvider;
	}
}
