using System;
using HeavyMetalMachines.ReportSystem.Infra;
using Hoplon.Logging;
using Pocketverse.Util;

namespace HeavyMetalMachines.SkipSwordfish
{
	public class SkipSwordfishMarkAsReadFeedback : IMarkAsReadPlayerFeedbacks
	{
		public SkipSwordfishMarkAsReadFeedback(ILogger<SkipSwordfishMarkAsReadFeedback> log)
		{
			this._log = log;
		}

		public void Mark(long[] feedbackIds)
		{
			this._log.InfoFormat("Marking as read={0}", new object[]
			{
				Arrays.ToStringWithComma(feedbackIds)
			});
		}

		private readonly ILogger<SkipSwordfishMarkAsReadFeedback> _log;
	}
}
