using System;
using System.Collections.Generic;
using Pocketverse;

namespace HeavyMetalMachines.Frontend
{
	public class ScreenFeedback : GameHubScriptableObject
	{
		private void OnEnable()
		{
			for (int i = 0; i < this.Feedbacks.Count; i++)
			{
				this.FeedbacksDic.Add(this.Feedbacks[i].Name, this.Feedbacks[i].Name);
			}
		}

		public List<ScreenFeedback.Feedback> Feedbacks = new List<ScreenFeedback.Feedback>();

		public Dictionary<string, string> FeedbacksDic = new Dictionary<string, string>();

		[Serializable]
		public class Feedback
		{
			public string Name;

			public string FeedbackText;
		}
	}
}
