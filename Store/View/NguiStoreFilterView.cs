using System;
using HeavyMetalMachines.Presenting;
using HeavyMetalMachines.Presenting.NGui;
using HeavyMetalMachines.Presenting.Unity;
using UnityEngine;

namespace HeavyMetalMachines.Store.View
{
	public class NguiStoreFilterView : MonoBehaviour, IStoreFilterView
	{
		public IActivatable MainActivatable
		{
			get
			{
				return this._mainGameObjectActivatable;
			}
		}

		public IButton LeftButton
		{
			get
			{
				return this._leftButton;
			}
		}

		public IButton RightButton
		{
			get
			{
				return this._rightButton;
			}
		}

		public ILabel CurrentFilterLabel
		{
			get
			{
				return this._currentFilterLabel;
			}
		}

		public IActivatable ArrowLeftActivatable
		{
			get
			{
				return this._arrowLeftGameObjectActivatable;
			}
		}

		public IActivatable ArrowRightActivatable
		{
			get
			{
				return this._arrowRightGameObjectActivatable;
			}
		}

		public IActivatable BorderActivatable
		{
			get
			{
				return this._borderGameObjectActivatable;
			}
		}

		[SerializeField]
		private GameObjectActivatable _mainGameObjectActivatable;

		[SerializeField]
		private NGuiButton _leftButton;

		[SerializeField]
		private NGuiButton _rightButton;

		[SerializeField]
		private NGuiLabel _currentFilterLabel;

		[SerializeField]
		private GameObjectActivatable _arrowLeftGameObjectActivatable;

		[SerializeField]
		private GameObjectActivatable _arrowRightGameObjectActivatable;

		[SerializeField]
		private GameObjectActivatable _borderGameObjectActivatable;
	}
}
