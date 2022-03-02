using System;
using HeavyMetalMachines.Presenting;
using HeavyMetalMachines.Presenting.Unity;
using Hoplon.Input.UiNavigation;
using JetBrains.Annotations;
using UniRx;
using UnityEngine;
using Zenject;

namespace HeavyMetalMachines.Battlepass.View
{
	public class LegacyBattlepassViewWrapper : MonoBehaviour, IBattlepassView
	{
		public IUiNavigationGroupHolder UiNavigationGroupHolder
		{
			get
			{
				return this._uiNavigationGroupHolder;
			}
		}

		public IButton BackButton
		{
			get
			{
				return this._backButton;
			}
		}

		private void Awake()
		{
			this._viewProvider.Bind<IBattlepassView>(this, null);
		}

		private void OnDestroy()
		{
			this._viewProvider.Unbind<IBattlepassView>(null);
		}

		public IObservable<Unit> PlayInAnimation()
		{
			return this._legacyBattlepassView.AnimateShow();
		}

		public IObservable<Unit> PlayOutAnimation()
		{
			return this._legacyBattlepassView.AnimateHide();
		}

		[Inject]
		[UsedImplicitly]
		private IViewProvider _viewProvider;

		[SerializeField]
		private BattlepassComponent _battlepassComponent;

		[SerializeField]
		private UnityUiBattlepassView _legacyBattlepassView;

		[SerializeField]
		private UnityButton _backButton;

		[SerializeField]
		private UiNavigationGroupHolder _uiNavigationGroupHolder;
	}
}
