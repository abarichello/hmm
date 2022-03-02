using System;
using UnityEngine;
using Zenject;

namespace HeavyMetalMachines.Util
{
	public class MultiPlatformInstantiate : MonoBehaviour
	{
		private GameObject SelectPlatformGameObject()
		{
			return this._windowsPrefab;
		}

		public void Start()
		{
			this._container.InstantiatePrefab(this.SelectPlatformGameObject(), base.transform);
		}

		[Inject]
		private DiContainer _container;

		[SerializeField]
		private GameObject _windowsPrefab;
	}
}
