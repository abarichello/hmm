using System;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	public class FloodFilter : ISpamFilter
	{
		public FloodFilter(int spamMessageCountThreshold, int spamBlockedDuration, int spamAllowedDuration)
		{
			this._spamMessageCountThreshold = spamMessageCountThreshold;
			this._spamBlockedDuration = spamBlockedDuration;
			this._spamAllowedDuration = spamAllowedDuration;
			this._timeAllowedToSend = 0f;
			this._repeatedMessageCount = 0;
			this._timeAllowedToSpam = float.MaxValue;
		}

		public bool IsSpam(string rawMessage, float currentUnscaledTime)
		{
			if (this._timeAllowedToSend > currentUnscaledTime)
			{
				return true;
			}
			if (this._timeAllowedToSpam < currentUnscaledTime)
			{
				this._repeatedMessageCount = 0;
			}
			if (this._repeatedMessageCount >= this._spamMessageCountThreshold)
			{
				this.Block();
				return true;
			}
			this._repeatedMessageCount++;
			if (this._repeatedMessageCount == 1)
			{
				this._timeAllowedToSpam = currentUnscaledTime + (float)this._spamAllowedDuration;
			}
			return false;
		}

		private void Block()
		{
			this._timeAllowedToSend = Time.unscaledTime + (float)this._spamBlockedDuration;
			this._repeatedMessageCount = 0;
			this._timeAllowedToSpam = float.MaxValue;
		}

		private int _repeatedMessageCount;

		private readonly int _spamMessageCountThreshold;

		private readonly int _spamBlockedDuration;

		private readonly int _spamAllowedDuration;

		private float _timeAllowedToSend;

		private float _timeAllowedToSpam;
	}
}
