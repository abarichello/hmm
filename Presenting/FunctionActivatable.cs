using System;

namespace HeavyMetalMachines.Presenting
{
	public class FunctionActivatable : IActivatable, IValueHolder
	{
		public FunctionActivatable(Action activateAction, Action deactivateAction)
		{
			this._activateAction = activateAction;
			this._deactivateAction = deactivateAction;
		}

		private void Activate()
		{
			this._activateAction();
		}

		private void Deactivate()
		{
			this._deactivateAction();
		}

		public void SetActive(bool active)
		{
			if (active)
			{
				this.Activate();
			}
			else
			{
				this.Deactivate();
			}
		}

		public bool HasValue
		{
			get
			{
				return this._activateAction != null && this._deactivateAction != null;
			}
		}

		private readonly Action _activateAction;

		private readonly Action _deactivateAction;
	}
}
