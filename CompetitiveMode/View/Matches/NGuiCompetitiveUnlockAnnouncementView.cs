using System;
using HeavyMetalMachines.Presenting;
using HeavyMetalMachines.Presenting.NGui;
using HeavyMetalMachines.Presenting.Unity;
using Hoplon.Input.UiNavigation;
using UnityEngine;
using Zenject;

namespace HeavyMetalMachines.CompetitiveMode.View.Matches
{
	public class NGuiCompetitiveUnlockAnnouncementView : MonoBehaviour, ICompetitiveUnlockAnnouncementView
	{
		public IActivatable Group
		{
			get
			{
				return this._group;
			}
		}

		public IAnimation ParentShowAnimation
		{
			get
			{
				return this._parentShowAnimation;
			}
		}

		public IAnimation ParentHideAnimation
		{
			get
			{
				return this._parentHideAnimation;
			}
		}

		public IAnimation ShowAnimation
		{
			get
			{
				return this._showAnimation;
			}
		}

		public IAnimation HideAnimation
		{
			get
			{
				return this._hideAnimation;
			}
		}

		public IAnimation IdleAnimation
		{
			get
			{
				return this._idleAnimation;
			}
		}

		public IButton ConfirmButton
		{
			get
			{
				return this._confirmButton;
			}
		}

		public IAnimation ShowConfirmButtonAnimation
		{
			get
			{
				return this._showConfirmButtonAnimation;
			}
		}

		public IUiNavigationGroupHolder UiNavigationGroupHolder
		{
			get
			{
				return this._uiNavigationGroupHolder;
			}
		}

		private void OnEnable()
		{
			this._viewProvider.Bind<ICompetitiveUnlockAnnouncementView>(this, null);
		}

		private void OnDisable()
		{
			this._viewProvider.Unbind<ICompetitiveUnlockAnnouncementView>(null);
		}

		[Inject]
		private IViewProvider _viewProvider;

		[SerializeField]
		private GameObjectActivatable _group;

		[SerializeField]
		private UiNavigationGroupHolder _uiNavigationGroupHolder;

		[SerializeField]
		private UnityAnimation _parentShowAnimation;

		[SerializeField]
		private UnityAnimation _parentHideAnimation;

		[SerializeField]
		private UnityAnimation _showAnimation;

		[SerializeField]
		private UnityAnimation _hideAnimation;

		[SerializeField]
		private UnityAnimation _idleAnimation;

		[SerializeField]
		private NGuiButton _confirmButton;

		[SerializeField]
		private UnityAnimation _showConfirmButtonAnimation;
	}
}
