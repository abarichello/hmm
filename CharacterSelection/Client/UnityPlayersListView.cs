using System;
using HeavyMetalMachines.CharacterSelection.Client.Presenting.Infra;
using HeavyMetalMachines.Presenting;
using UnityEngine;
using Zenject;

namespace HeavyMetalMachines.CharacterSelection.Client
{
	public class UnityPlayersListView : MonoBehaviour, IPlayersListView
	{
		private void OnEnable()
		{
			this._viewProvider.Bind<IPlayersListView>(this, null);
		}

		private void OnDisable()
		{
			this._viewProvider.Unbind<IPlayersListView>(null);
		}

		public IPlayersListItemView CreateAlliedItem()
		{
			return this._diContainer.InstantiatePrefabForComponent<IPlayersListItemView>(this._alliedViewPrefab, this._alliedViewsParent);
		}

		public IPlayersListItemView CreateEnemyItem()
		{
			return this._diContainer.InstantiatePrefabForComponent<IPlayersListItemView>(this._enemyViewPrefab, this._enemyViewsParent);
		}

		[Inject]
		private IViewProvider _viewProvider;

		[Inject]
		private DiContainer _diContainer;

		[SerializeField]
		private Transform _alliedViewsParent;

		[SerializeField]
		private Transform _enemyViewsParent;

		[SerializeField]
		private UnityPlayersListItemView _alliedViewPrefab;

		[SerializeField]
		private UnityPlayersListItemView _enemyViewPrefab;
	}
}
