using System;
using System.Text;
using ClientAPI;
using ClientAPI.Service.Interfaces;
using Hoplon.Logging;

namespace HeavyMetalMachines.ReportSystem.Infra
{
	public class MarkAsReadPlayerFeedbacks : IMarkAsReadPlayerFeedbacks
	{
		public MarkAsReadPlayerFeedbacks(IPlayerFeedback feedbackApi, ILogger<MarkAsReadPlayerFeedbacks> log)
		{
			this._feedbackApi = feedbackApi;
			this._log = log;
		}

		public void Mark(long[] feedbackIds)
		{
			this._feedbackApi.MarkListFeedbackAsInformed(feedbackIds, feedbackIds, new SwordfishClientApi.Callback(this.OnSuccess), new SwordfishClientApi.ErrorCallback(this.OnError));
		}

		private void OnError(object state, Exception exception)
		{
			this._log.ErrorFormat("Failed to mark feedback as read id={0} exception={1} Stack={2}", new object[]
			{
				this.StateToString(state),
				exception.Message,
				exception.StackTrace
			});
		}

		private void OnSuccess(object state)
		{
			this._log.InfoFormat("Marked feedback={0} as read.", new object[]
			{
				this.StateToString(state)
			});
		}

		private string StateToString(object state)
		{
			long[] array = state as long[];
			if (array == null)
			{
				return "[ null ]";
			}
			StringBuilder stringBuilder = new StringBuilder("[");
			for (int i = 0; i < array.Length; i++)
			{
				stringBuilder.Append(" ");
				stringBuilder.Append(array[i]);
			}
			stringBuilder.Append(" ]");
			return stringBuilder.ToString();
		}

		private readonly IPlayerFeedback _feedbackApi;

		private readonly ILogger<MarkAsReadPlayerFeedbacks> _log;
	}
}
