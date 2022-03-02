using System;
using HeavyMetalMachines.CharacterSelection.Client.Presenting.Infra;
using HeavyMetalMachines.Extensions;
using HeavyMetalMachines.Presenting;
using HeavyMetalMachines.Presenting.Unity;
using Hoplon.Math;
using UnityEngine;

namespace HeavyMetalMachines.CharacterSelection.Client
{
	public class UnityBannedCharacterSlotView : MonoBehaviour, IBannedCharacterSlotView
	{
		public IActivatable MainGroup
		{
			get
			{
				return this._mainGroup;
			}
		}

		public ICanvasGroup CharacterGroup
		{
			get
			{
				return this._characterGroup;
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

		public IAnimation ShowBanAnimation
		{
			get
			{
				return this._showBanAnimation;
			}
		}

		public Vector2 ScreenPosition
		{
			get
			{
				return this._rectTransform.LocalToScreenPosition().ToHmmVector2();
			}
		}

		[SerializeField]
		private GameObjectActivatable _mainGroup;

		[SerializeField]
		private UnityCanvasGroup _characterGroup;

		[SerializeField]
		private RectTransform _rectTransform;

		[SerializeField]
		private UnityDynamicImage _portraitImage;

		[SerializeField]
		private UnityLabel _nameLabel;

		[SerializeField]
		private UnityAnimation _showBanAnimation;
	}
}
