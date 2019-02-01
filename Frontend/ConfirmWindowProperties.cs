using System;

namespace HeavyMetalMachines.Frontend
{
	[Serializable]
	public class ConfirmWindowProperties
	{
		public bool IsTextOnly()
		{
			return this.OnConfirm == null && this.OnRefuse == null && this.OnOk == null;
		}

		public Guid Guid;

		public string TileText = string.Empty;

		public string QuestionText = string.Empty;

		public string HintText = string.Empty;

		public string ConfirmButtonText = string.Empty;

		public string RefuseButtonText = string.Empty;

		public string CheckboxText = string.Empty;

		public string OkButtonText = string.Empty;

		public bool CheckboxInitialState;

		public bool IsStackable = true;

		public bool EnableLoadGameObject;

		public bool EnableItemErrorGameObject;

		public bool ClockTextFormat;

		public float CountDownTime;

		public float BackgroundAlpha = -1f;

		public Action OnConfirm;

		public Action OnRefuse;

		public Action OnOk;

		public Action OnTimeOut;

		public Func<bool> TimerPausePredicate;
	}
}
