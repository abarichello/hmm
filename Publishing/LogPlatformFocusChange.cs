using System;
using HeavyMetalMachines.Swordfish;
using Hoplon.Serialization;
using UniRx;

namespace HeavyMetalMachines.Publishing
{
	public class LogPlatformFocusChange : ILogPlatformFocusChange
	{
		public LogPlatformFocusChange(IClientBILogger clientBiLogger)
		{
			this._clientBiLogger = clientBiLogger;
		}

		public void Initialize()
		{
			ObservableExtensions.Subscribe<bool>(Observable.Do<bool>(Platform.Current.ObserveFocusChange(), new Action<bool>(this.LogFocusChange)));
		}

		private void LogFocusChange(bool isFocusLost)
		{
			LogPlatformFocusChange.LogPlatformFocusChangeBiLog logPlatformFocusChangeBiLog = new LogPlatformFocusChange.LogPlatformFocusChangeBiLog
			{
				Result = ((!isFocusLost) ? "Gain" : "Lost")
			};
			this._clientBiLogger.BILogClientMsg(108, logPlatformFocusChangeBiLog.Serialize(), false);
		}

		private readonly IClientBILogger _clientBiLogger;

		private class LogPlatformFocusChangeBiLog : JsonSerializeable<LogPlatformFocusChange.LogPlatformFocusChangeBiLog>
		{
			public string Result;
		}
	}
}
