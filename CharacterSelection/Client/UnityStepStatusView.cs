using System;
using HeavyMetalMachines.CharacterSelection.Client.Presenting.Infra;
using HeavyMetalMachines.Presenting;
using HeavyMetalMachines.Presenting.Unity;
using UnityEngine;
using Zenject;

namespace HeavyMetalMachines.CharacterSelection.Client
{
	public class UnityStepStatusView : MonoBehaviour, IStepStatusView
	{
		public IEventEmitter EventEmitter
		{
			get
			{
				return this._eventEmitter;
			}
		}

		public ILabel StatusLabel
		{
			get
			{
				return this._statusLabel;
			}
		}

		public ILabel TimerLabel
		{
			get
			{
				return this._timerLabel;
			}
		}

		public Color AllyPlayerNameColor
		{
			get
			{
				return this._allyPlayerNameColor.ToHmmColor();
			}
		}

		public Color OpponentPlayerNameColor
		{
			get
			{
				return this._opponentPlayerNameColor.ToHmmColor();
			}
		}

		public IProgressBar AlliedTimerProgressBar
		{
			get
			{
				return this._alliedTimerProgressBar;
			}
		}

		public IProgressBar OpponentTimerProgressBar
		{
			get
			{
				return this._opponentTimerProgressBar;
			}
		}

		private void OnEnable()
		{
			this._viewProvider.Bind<IStepStatusView>(this, null);
		}

		private void OnDisable()
		{
			this._viewProvider.Unbind<IStepStatusView>(null);
		}

		[Inject]
		private IViewProvider _viewProvider;

		[SerializeField]
		private UnityEventEmitter _eventEmitter;

		[SerializeField]
		private UnityLabel _statusLabel;

		[SerializeField]
		private UnityLabel _timerLabel;

		[SerializeField]
		private Color _allyPlayerNameColor;

		[SerializeField]
		private Color _opponentPlayerNameColor;

		[SerializeField]
		private UnityImageProgressBar _alliedTimerProgressBar;

		[SerializeField]
		private UnityImageProgressBar _opponentTimerProgressBar;
	}
}
