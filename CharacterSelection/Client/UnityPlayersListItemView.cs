using System;
using HeavyMetalMachines.CharacterSelection.Client.Presenting.Infra;
using HeavyMetalMachines.Presenting;
using HeavyMetalMachines.Presenting.Unity;
using UnityEngine;

namespace HeavyMetalMachines.CharacterSelection.Client
{
	public class UnityPlayersListItemView : MonoBehaviour, IPlayersListItemView
	{
		public void EnableGrayScale()
		{
			Color color = this._characterIconImage.Color;
			color.R = 0f;
			this._characterIconImage.Color = color;
		}

		public void DisableGrayScale()
		{
			Color color = this._characterIconImage.Color;
			color.R = 1f;
			this._characterIconImage.Color = color;
		}

		public ILabel NameLabel
		{
			get
			{
				return this._nameLabel;
			}
		}

		public IDynamicImage CharacterIconImage
		{
			get
			{
				return this._characterIconImage;
			}
		}

		public IActivatable LocalPlayerIndicator
		{
			get
			{
				return this._localPlayerIndicator;
			}
		}

		public IActivatable TimerGroup
		{
			get
			{
				return this._timerGroup;
			}
		}

		public ILabel TimerLabel
		{
			get
			{
				return this._timerLabel;
			}
		}

		public ILabel CharacterNameLabel
		{
			get
			{
				return this._characterNameLabel;
			}
		}

		public ILabel VotedLabel
		{
			get
			{
				return this._votedLabel;
			}
		}

		public ILabel VotedCharacterLabel
		{
			get
			{
				return this._votedCharacterLabel;
			}
		}

		public IActivatable PublisherIndicator
		{
			get
			{
				return this._publisherIndicator;
			}
		}

		public IDynamicImage PublisherIconImage
		{
			get
			{
				return this._publisherIconImage;
			}
		}

		public ILabel PublisherUserNameLabel
		{
			get
			{
				return this._publisherUserNameLabel;
			}
		}

		public IEventEmitter EventEmitter
		{
			get
			{
				return this._eventEmitter;
			}
		}

		[SerializeField]
		private UnityLabel _nameLabel;

		[SerializeField]
		private UnityDynamicImage _characterIconImage;

		[SerializeField]
		private UnityDynamicImage _publisherIconImage;

		[SerializeField]
		private GameObjectActivatable _localPlayerIndicator;

		[SerializeField]
		private GameObjectActivatable _timerGroup;

		[SerializeField]
		private GameObjectActivatable _publisherIndicator;

		[SerializeField]
		private UnityLabel _timerLabel;

		[SerializeField]
		private UnityLabel _characterNameLabel;

		[SerializeField]
		private UnityLabel _votedLabel;

		[SerializeField]
		private UnityLabel _votedCharacterLabel;

		[SerializeField]
		private UnityLabel _publisherUserNameLabel;

		[SerializeField]
		private UnityEventEmitter _eventEmitter;
	}
}
