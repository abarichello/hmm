using System;

namespace HeavyMetalMachines.VFX
{
	public class QuestionHintContent : BaseHintContent
	{
		public QuestionHintContent(string textContent, float timeoutSeconds, Action approveAction, Action refuseAction) : base(textContent, timeoutSeconds, true, null, "SystemMessage")
		{
			this._textContent = textContent;
			this._timeoutSeconds = timeoutSeconds;
			this._approveAction = approveAction;
			this._refuseAction = refuseAction;
		}

		public Action ApproveAction
		{
			get
			{
				return this._approveAction;
			}
		}

		public Action RefuseAction
		{
			get
			{
				return this._refuseAction;
			}
		}

		private Action _approveAction;

		private Action _refuseAction;
	}
}
