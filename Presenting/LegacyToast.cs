using System;
using HeavyMetalMachines.VFX;
using UniRx;

namespace HeavyMetalMachines.Presenting
{
	public class LegacyToast : IToast
	{
		public LegacyToast()
		{
			this._panelController = SingletonMonoBehaviour<PanelController>.Instance;
		}

		public IObservable<Unit> Show(string message)
		{
			return Observable.Defer<Unit>(delegate()
			{
				this._panelController.SendSystemMessage(message, "SystemMessage", false, false, StackableHintKind.None, HintColorScheme.System);
				return Observable.ReturnUnit();
			});
		}

		private readonly PanelController _panelController;
	}
}
