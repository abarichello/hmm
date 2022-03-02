using System;
using HeavyMetalMachines.Localization;

namespace HeavyMetalMachines.Frontend
{
	public class SpamFilter : ISpamFilter
	{
		public SpamFilter(int spamMessageCountThreshold, int spamBlockedChatDuration)
		{
			this._spamMessageCountThreshold = spamMessageCountThreshold;
			this._spamBlockedChatDuration = spamBlockedChatDuration;
		}

		public bool IsSpam(string rawMessage, float currentUnscaledTime)
		{
			if (currentUnscaledTime < this._timeAllowedToSendMessages)
			{
				return true;
			}
			if (this._repeatedMessageCount >= this._spamMessageCountThreshold)
			{
				this._repeatedMessageCount = 0;
			}
			else if (string.Equals(this._lastSentMessage, rawMessage, StringComparison.OrdinalIgnoreCase))
			{
				this._repeatedMessageCount++;
				if (this._repeatedMessageCount >= this._spamMessageCountThreshold)
				{
					this._timeAllowedToSendMessages = currentUnscaledTime + (float)this._spamBlockedChatDuration;
					return true;
				}
			}
			else
			{
				this._repeatedMessageCount = 0;
				this._lastSentMessage = rawMessage;
			}
			return false;
		}

		public string GetSpamBlockMessage(float currentUnscaledTime)
		{
			int num = (int)Math.Ceiling((double)(this._timeAllowedToSendMessages - currentUnscaledTime));
			return Language.GetFormatted("CHAT_MESSAGE_SPAM_BLOCK", TranslationContext.Chat, new object[]
			{
				num
			});
		}

		private int _repeatedMessageCount;

		private float _timeAllowedToSendMessages;

		private string _lastSentMessage = string.Empty;

		private readonly int _spamMessageCountThreshold;

		private readonly int _spamBlockedChatDuration;
	}
}
