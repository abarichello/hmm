using System;
using HeavyMetalMachines.Presenting;
using HeavyMetalMachines.Presenting.Unity;
using UnityEngine;

namespace HeavyMetalMachines.News.Presenting
{
	public class NewsSelectorToggleView : MonoBehaviour, INewsSelectorView
	{
		public IActivatable Activatable
		{
			get
			{
				return this._activatable;
			}
		}

		public IToggle Toggle
		{
			get
			{
				return this._toggle;
			}
		}

		[SerializeField]
		private GameObjectActivatable _activatable;

		[SerializeField]
		private UnityToggle _toggle;
	}
}
