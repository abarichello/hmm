using System;
using HeavyMetalMachines.CharacterSelection.Client.Presenting.Infra;
using HeavyMetalMachines.Extensions;
using HeavyMetalMachines.Presenting;
using HeavyMetalMachines.Presenting.Unity;
using Hoplon.Math;
using JetBrains.Annotations;
using UniRx;
using UnityEngine;

namespace HeavyMetalMachines.CharacterSelection.Client
{
	public class UnityBanCandidateView : MonoBehaviour, IBanCandidateView
	{
		public IActivatable MainGroup
		{
			get
			{
				return this._mainGroup;
			}
		}

		public ICanvasGroup CanvasGroup
		{
			get
			{
				return this._canvasGroup;
			}
		}

		public IDynamicImage PortraitImage
		{
			get
			{
				return this._portraitImage;
			}
		}

		public ILabel NameLabel
		{
			get
			{
				return this._nameLabel;
			}
		}

		public ILabel VoteCountLabel
		{
			get
			{
				return this._voteCountLabel;
			}
		}

		public IAnimation BanAnimation
		{
			get
			{
				return this._banAnimation;
			}
		}

		public IAnimation BanResetAnimation
		{
			get
			{
				return this._banResetAnimation;
			}
		}

		public IObservable<Unit> OnShouldMoveToSlot
		{
			get
			{
				return this._onShouldMoveToSlotSubject;
			}
		}

		public Vector2 ScreenPosition
		{
			get
			{
				return this._rectTransform.LocalToScreenPosition().ToHmmVector2();
			}
			set
			{
				this._rectTransform.anchoredPosition = this._parentRectTransform.ScreenToLocalPosition(value.ToUnityVector2());
			}
		}

		[UsedImplicitly]
		public void MoveToSlot()
		{
			this._onShouldMoveToSlotSubject.OnNext(Unit.Default);
		}

		[SerializeField]
		private RectTransform _rectTransform;

		[SerializeField]
		private RectTransform _parentRectTransform;

		[SerializeField]
		private GameObjectActivatable _mainGroup;

		[SerializeField]
		private UnityCanvasGroup _canvasGroup;

		[SerializeField]
		private UnityDynamicImage _portraitImage;

		[SerializeField]
		private UnityLabel _nameLabel;

		[SerializeField]
		private UnityLabel _voteCountLabel;

		[SerializeField]
		private UnityAnimation _banAnimation;

		[SerializeField]
		private UnityAnimation _banResetAnimation;

		private readonly Subject<Unit> _onShouldMoveToSlotSubject = new Subject<Unit>();
	}
}
