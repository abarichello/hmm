using System;
using System.Collections.Generic;
using HeavyMetalMachines.BI;
using UniRx;

namespace HeavyMetalMachines.Swordfish
{
	public class ClientButtonBILogger : IClientButtonBILogger
	{
		public ClientButtonBILogger(IClientBILogger logger)
		{
			this._logger = logger;
			this._pressedButtons = new HashSet<ButtonNameInstance>();
			this._logButtonStream = new Subject<ButtonNameInstance>();
			ObservableExtensions.Subscribe<ButtonNameInstance>(Observable.Do<ButtonNameInstance>(Observable.Delay<ButtonNameInstance>(Observable.Do<ButtonNameInstance>(Observable.Where<ButtonNameInstance>(this._logButtonStream, new Func<ButtonNameInstance, bool>(this.ButtonWasNotLoggedRecently)), new Action<ButtonNameInstance>(this.LogAndAddButtonToRecentLogs)), this.WaitTime), new Action<ButtonNameInstance>(this.RemoveButtonFromRecentLogs)));
		}

		private bool ButtonWasNotLoggedRecently(ButtonNameInstance button)
		{
			return !this._pressedButtons.Contains(button);
		}

		private void LogAndAddButtonToRecentLogs(ButtonNameInstance button)
		{
			this._logger.BILogClientMsg(69, string.Format("ButtonName={0}", button.Description), true);
			this._pressedButtons.Add(button);
		}

		private void RemoveButtonFromRecentLogs(ButtonNameInstance button)
		{
			this._pressedButtons.Remove(button);
		}

		public void LogButtonClick(ButtonNameInstance button)
		{
			this._logButtonStream.OnNext(button);
		}

		private IClientBILogger _logger;

		private Subject<ButtonNameInstance> _logButtonStream;

		private HashSet<ButtonNameInstance> _pressedButtons;

		private readonly TimeSpan WaitTime = TimeSpan.FromSeconds(1.0);
	}
}
