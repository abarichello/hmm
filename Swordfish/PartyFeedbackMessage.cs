using System;
using ClientAPI.MessageHub;

namespace HeavyMetalMachines.Swordfish
{
	public class PartyFeedbackMessage : Message
	{
		public PartyFeedbackMessage() : base("PartyFeedbackMessage")
		{
		}

		protected override void ReadBody(string body)
		{
			this.FeedbackKind = (PartyFeedbackEnum)Convert.ToInt32(body);
		}

		protected override string WriteBody()
		{
			int feedbackKind = (int)this.FeedbackKind;
			return feedbackKind.ToString();
		}

		public PartyFeedbackEnum FeedbackKind;
	}
}
