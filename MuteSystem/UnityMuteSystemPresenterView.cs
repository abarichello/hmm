using System;
using HeavyMetalMachines.Frontend;
using HeavyMetalMachines.Presenting;
using HeavyMetalMachines.Presenting.Unity;
using Hoplon.Input.UiNavigation;
using Hoplon.Input.UiNavigation.AxisSelector;
using UnityEngine;
using Zenject;

namespace HeavyMetalMachines.MuteSystem
{
	public class UnityMuteSystemPresenterView : MonoBehaviour, IMuteSystemPresenterView
	{
		private void OnEnable()
		{
			this._viewProvider.Bind<IMuteSystemPresenterView>(this, null);
		}

		private void OnDisable()
		{
			this._viewProvider.Unbind<IMuteSystemPresenterView>(null);
		}

		public ICanvas MainCanvas
		{
			get
			{
				return this._mainCanvas;
			}
		}

		public ICanvasGroup MainCanvasGroup
		{
			get
			{
				return this._mainCanvasGroup;
			}
		}

		public IButton ExitButton
		{
			get
			{
				return this._exitButton;
			}
		}

		public IAnimator ContainerAnimator
		{
			get
			{
				return this._containerAnimator;
			}
		}

		public IUiNavigationGroupHolder UiNavigationGroupHolder
		{
			get
			{
				return this._uiNavigationGroupHolder;
			}
		}

		public IUiNavigationAxisSelector UiNavigationAxisSelector
		{
			get
			{
				return this._uiNavigationAxisSelector;
			}
		}

		public IMuteSystemSubtitleView MuteSystemSubtitleView
		{
			get
			{
				return this._unityMuteSystemSubtitleView;
			}
		}

		public IMuteSystemPlayerSlotView AddPlayerSlot()
		{
			this._addedPlayerCount++;
			if (this._addedPlayerCount == 1)
			{
				return this._playerSlotView;
			}
			return Object.Instantiate<UnityMuteSystemPlayerSlotView>(this._playerSlotView, this._playerSlotView.transform.parent);
		}

		public void AddSeparator(int index)
		{
			this._addedSeparatorCount++;
			Transform transform = (this._addedSeparatorCount != 1) ? Object.Instantiate<Transform>(this._separatorTransform, this._separatorTransform.transform.parent) : this._separatorTransform;
			transform.gameObject.SetActive(true);
			transform.SetSiblingIndex(this._playerSlotView.transform.GetSiblingIndex() + index);
		}

		public float GetInAnimationLength()
		{
			return this._inAnimationClip.length;
		}

		public float GetOutAnimationLength()
		{
			return this._outAnimationClip.length;
		}

		public void TryToSelect(IButton selectable, IMuteSystemPlayerSlotView playerSlotView)
		{
			playerSlotView.TryToSelect(selectable, this._uiNavigationAxisSelector);
		}

		public void ShowNarratorTitle(int index)
		{
			this._narratorTitleTransform.gameObject.SetActive(true);
			this._narratorTitleTransform.SetSiblingIndex(this._playerSlotView.transform.GetSiblingIndex() + index - 1);
		}

		[Inject]
		private IViewProvider _viewProvider;

		[SerializeField]
		private UnityCanvas _mainCanvas;

		[SerializeField]
		private UnityCanvasGroup _mainCanvasGroup;

		[SerializeField]
		private UnityButton _exitButton;

		[SerializeField]
		private UnityAnimator _containerAnimator;

		[SerializeField]
		private AnimationClip _inAnimationClip;

		[SerializeField]
		private AnimationClip _outAnimationClip;

		[SerializeField]
		private UiNavigationGroupHolder _uiNavigationGroupHolder;

		[SerializeField]
		private UiNavigationAxisSelector _uiNavigationAxisSelector;

		[SerializeField]
		private UnityMuteSystemSubtitleView _unityMuteSystemSubtitleView;

		[SerializeField]
		private UnityMuteSystemPlayerSlotView _playerSlotView;

		[SerializeField]
		private Transform _separatorTransform;

		[SerializeField]
		private Transform _narratorTitleTransform;

		private int _addedPlayerCount;

		private int _addedSeparatorCount;
	}
}
