using System;
using HeavyMetalMachines.Combat.Gadget;
using UnityEngine;

namespace HeavyMetalMachines.Render
{
	public abstract class BaseActivatableGadgetFeedback : MonoBehaviour, IActivatableGadgetFeedback
	{
		protected abstract void OnActivate();

		protected abstract void OnDeactivate();

		public bool IsActive
		{
			get
			{
				return this._isActive;
			}
			set
			{
				this._isActive = value;
				if (this._isActive)
				{
					this.OnActivate();
				}
				else
				{
					this.OnDeactivate();
				}
			}
		}

		public GadgetSlot Slot
		{
			get
			{
				return this._slot;
			}
			set
			{
				this._slot = value;
			}
		}

		[SerializeField]
		private GadgetSlot _slot;

		[SerializeField]
		private bool _isActive;
	}
}
