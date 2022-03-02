using System;
using HeavyMetalMachines.Matches;
using UniRx;
using UnityEngine;
using Zenject;

namespace HeavyMetalMachines.CharacterSelection.Client
{
	public class EditorLocalClientSelectionInput : MonoBehaviour
	{
		public IObservable<Unit> OnConnectionToggled
		{
			get
			{
				return this._onConnectionToggledSubject;
			}
		}

		public IObservable<int> OnBotForceDisconnect
		{
			get
			{
				return this._onBotForceDisconnectSubject;
			}
		}

		public IObservable<int> OnBotForceReconnect
		{
			get
			{
				return this._onBotForceReconnectSubject;
			}
		}

		private void Update()
		{
			if (Input.GetKeyUp(289))
			{
				this._onConnectionToggledSubject.OnNext(Unit.Default);
			}
			if (Input.GetKeyUp(290))
			{
				this._observeClientsConnection.SimulateClientDisconnected(MatchClient.AsBot(2, 0));
				this._onBotForceDisconnectSubject.OnNext(2);
			}
			if (Input.GetKeyUp(291))
			{
				this._onBotForceReconnectSubject.OnNext(2);
			}
		}

		private readonly Subject<Unit> _onConnectionToggledSubject = new Subject<Unit>();

		private readonly Subject<int> _onBotForceDisconnectSubject = new Subject<int>();

		private readonly Subject<int> _onBotForceReconnectSubject = new Subject<int>();

		[Inject]
		private EditorCharacterSelectionClientInstaller.ManualObserveClientsConnection _observeClientsConnection;
	}
}
