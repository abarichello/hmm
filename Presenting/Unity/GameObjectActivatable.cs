using System;
using UnityEngine;

namespace HeavyMetalMachines.Presenting.Unity
{
	[Serializable]
	public class GameObjectActivatable : IActivatable, IValueHolder
	{
		public GameObjectActivatable(GameObject gameObject)
		{
			this._gameObject = gameObject;
		}

		public void SetActive(bool active)
		{
			if (this._gameObject == null)
			{
				return;
			}
			this._gameObject.SetActive(active);
		}

		bool IValueHolder.HasValue
		{
			get
			{
				return this._gameObject != null;
			}
		}

		[SerializeField]
		private GameObject _gameObject;
	}
}
