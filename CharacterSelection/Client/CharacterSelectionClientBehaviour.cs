using System;
using HeavyMetalMachines.CharacterSelection.Client.Presenting.API;
using UniRx;
using UnityEngine;
using Zenject;

namespace HeavyMetalMachines.CharacterSelection.Client
{
	public class CharacterSelectionClientBehaviour : MonoBehaviour
	{
		private void Start()
		{
			DisposableExtensions.AddTo<IDisposable>(ObservableExtensions.Subscribe<Unit>(this._executeLocalClientCharacterSelection.Execute()), this);
		}

		[Inject]
		private IExecuteLocalClientCharacterSelection _executeLocalClientCharacterSelection;
	}
}
