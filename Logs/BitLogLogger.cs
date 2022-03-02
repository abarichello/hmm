using System;
using Hoplon.Logging;
using Pocketverse;

namespace HeavyMetalMachines.Logs
{
	public class BitLogLogger<T> : ILogger<T>, ILogger
	{
		public BitLogLogger() : this(new BitLogger(typeof(T)))
		{
		}

		private BitLogLogger(BitLogger bitLogger)
		{
			this._bitLogger = bitLogger;
		}

		public static BitLogLogger<T> FromExistingBitLogger(BitLogger bitLogger)
		{
			return new BitLogLogger<T>(bitLogger);
		}

		public void Debug(object message)
		{
			this._bitLogger.Debug(message);
		}

		public void DebugFormat(string message, params object[] args)
		{
			this._bitLogger.DebugFormat(message, args);
		}

		public void DebugStackTrace(object message)
		{
			this._bitLogger.DebugStackTrace(message);
		}

		public void Info(object message)
		{
			this._bitLogger.Info(message);
		}

		public void InfoFormat(string message, params object[] args)
		{
			this._bitLogger.InfoFormat(message, args);
		}

		public void InfoStackTrace(object message)
		{
			this._bitLogger.InfoStackTrace(message);
		}

		public void Warn(object message)
		{
			this._bitLogger.Warn(message);
		}

		public void WarnFormat(string message, params object[] args)
		{
			this._bitLogger.WarnFormat(message, args);
		}

		public void WarnStackTrace(object message)
		{
			this._bitLogger.WarnStackTrace(message);
		}

		public void Error(object message)
		{
			this._bitLogger.ErrorStackTrace(message);
		}

		public void ErrorFormat(string message, params object[] args)
		{
			this._bitLogger.ErrorFormatStackTrace(message, args);
		}

		public void ErrorStackTrace(object message)
		{
			this._bitLogger.ErrorStackTrace(message);
		}

		public void Fatal(object message)
		{
			this._bitLogger.Fatal(message);
		}

		public void FatalFormat(string message, params object[] args)
		{
			this._bitLogger.FatalFormat(message, args);
		}

		public void FatalStackTrace(object message)
		{
			this._bitLogger.FatalStackTrace(message);
		}

		private readonly BitLogger _bitLogger;
	}
}
