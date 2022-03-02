using System;
using HeavyMetalMachines.CharacterSelection.Client.Presenting.Infra;
using HeavyMetalMachines.Presenting;
using HeavyMetalMachines.Presenting.Unity;
using UnityEngine;
using Zenject;

namespace HeavyMetalMachines.CharacterSelection.Client
{
	public class UnityHintsView : MonoBehaviour, IHintsView
	{
		public ILabel InfoLabel
		{
			get
			{
				return this._infoLabel;
			}
		}

		public IEventEmitter EventEmitter
		{
			get
			{
				return this._eventEmitter;
			}
		}

		private void OnEnable()
		{
			this._viewProvider.Bind<IHintsView>(this, null);
		}

		private void OnDisable()
		{
			this._viewProvider.Unbind<IHintsView>(null);
		}

		[Inject]
		private IViewProvider _viewProvider;

		[SerializeField]
		private UnityLabel _infoLabel;

		[SerializeField]
		private UnityEventEmitter _eventEmitter;
	}
}
