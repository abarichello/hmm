using System;
using HeavyMetalMachines.CharacterSelection.Client.Presenting.Infra;
using HeavyMetalMachines.Presenting;
using HeavyMetalMachines.Presenting.Unity;
using UnityEngine;
using Zenject;

namespace HeavyMetalMachines.CharacterSelection.Client
{
	public class UnityModelView : MonoBehaviour, IModelView
	{
		public IEventEmitter EventEmitter
		{
			get
			{
				return this._eventEmitter;
			}
		}

		private void OnEnable()
		{
			this._viewProvider.Bind<IModelView>(this, null);
		}

		private void OnDisable()
		{
			this._viewProvider.Unbind<IModelView>(null);
		}

		[Inject]
		private IViewProvider _viewProvider;

		[SerializeField]
		private UnityEventEmitter _eventEmitter;
	}
}
