using System;

namespace HeavyMetalMachines.VFX
{
	public class QuestionHintContent : BaseHintContent
	{
		public QuestionHintContent(string textContent, float timeoutSeconds, Action approveAction, Action refuseAction, bool executeRefuseActionOnTimeout = true) : base(textContent, timeoutSeconds, true, null, "SystemMessage")
		{
			this._textContent = textContent;
			this._timeoutSeconds = timeoutSeconds;
			this._approveAction = approveAction;
			this._refuseAction = refuseAction;
			this._executeRefuseActionOnTimeout = executeRefuseActionOnTimeout;
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

		public bool ExecuteRefuseActionOnTimeout
		{
			get
			{
				return this._executeRefuseActionOnTimeout;
			}
		}

		private readonly Action _approveAction;

		private readonly Action _refuseAction;

		private readonly bool _executeRefuseActionOnTimeout;
	}
}
