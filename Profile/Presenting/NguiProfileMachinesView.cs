using System;
using System.Collections.Generic;
using HeavyMetalMachines.Characters;
using HeavyMetalMachines.Frontend;
using HeavyMetalMachines.Presenting;
using HeavyMetalMachines.Presenting.NGui;
using HeavyMetalMachines.Presenting.Unity;
using HeavyMetalMachines.Social.Profile.Presenting;
using HeavyMetalMachines.ToggleableFeatures;
using Hoplon.Input.UiNavigation;
using Hoplon.ToggleableFeatures;
using UnityEngine;
using Zenject;

namespace HeavyMetalMachines.Profile.Presenting
{
	public class NguiProfileMachinesView : MonoBehaviour, IProfileMachinesView
	{
		public IUiNavigationSubGroupHolder UiNavigationSubGroupHolder
		{
			get
			{
				return this._uiNavigationSubGroupHolder;
			}
		}

		public IActivatable Group
		{
			get
			{
				return this._group;
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

		private void Awake()
		{
			if (!this._isFeatureToggled.Check(Features.ProfileRefactor))
			{
				Object.Destroy(this);
				return;
			}
			ActivatableExtensions.Deactivate(this._group);
			this.PrepareCharacterViews();
			this._viewProvider.Bind<IProfileMachinesView>(this, null);
		}

		private void OnDestroy()
		{
			this._viewProvider.Unbind<IProfileMachinesView>(null);
		}

		public List<IProfileCharacterView> GetCharacterViews()
		{
			return this._characterViews;
		}

		private void PrepareCharacterViews()
		{
			List<NguiProfileCharacterView> list = new List<NguiProfileCharacterView>
			{
				this._characterViewPrefab
			};
			int characterCount = this.GetCharacterCount();
			for (int i = 0; i < characterCount - 1; i++)
			{
				NguiProfileCharacterView item = Object.Instantiate<NguiProfileCharacterView>(this._characterViewPrefab, this._characterViewParent);
				list.Add(item);
			}
			this._characterViewParentGrid.Reposition();
			foreach (NguiProfileCharacterView nguiProfileCharacterView in list)
			{
				NguiProfileMachinesView.PrepareCharacterItemView(nguiProfileCharacterView);
				this._characterViews.Add(nguiProfileCharacterView);
			}
		}

		private static void PrepareCharacterItemView(NguiProfileCharacterView characterView)
		{
			characterView.GetComponentInChildren<GUIEventListener>().enabled = false;
			characterView.gameObject.SetActive(true);
		}

		private int GetCharacterCount()
		{
			return this._playerCharactersStorage.PlayerCharacters.Length;
		}

		[Header("Group")]
		[SerializeField]
		private UiNavigationSubGroupHolder _uiNavigationSubGroupHolder;

		[SerializeField]
		private GameObjectActivatable _group;

		[SerializeField]
		private UnityAnimation _showAnimation;

		[SerializeField]
		private UnityAnimation _hideAnimation;

		[Header("Machines")]
		[SerializeField]
		private Transform _characterViewParent;

		[SerializeField]
		private NguiGrid _characterViewParentGrid;

		[SerializeField]
		private NguiProfileCharacterView _characterViewPrefab;

		[Inject]
		private IViewProvider _viewProvider;

		[Inject]
		private IIsFeatureToggled _isFeatureToggled;

		[Inject]
		private IPlayerCharactersStorage _playerCharactersStorage;

		private readonly List<IProfileCharacterView> _characterViews = new List<IProfileCharacterView>();
	}
}
