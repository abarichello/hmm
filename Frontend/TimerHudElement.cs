using System;
using HeavyMetalMachines.Combat.GadgetScript;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	public class TimerHudElement : MonoBehaviour
	{
		public void SetTimer(long endTime, IHMMGadgetContext gadgetContext)
		{
			this._endTime = endTime;
			this._gadgetContext = gadgetContext;
			if (this._endTime > (long)this._gadgetContext.CurrentTime)
			{
				this._timerRunning = true;
			}
		}

		private void Update()
		{
			if (this._timerRunning)
			{
				float num = (float)(this._endTime - (long)this._gadgetContext.CurrentTime) * 0.001f;
				float num2 = Mathf.Floor((num - Mathf.Floor(num)) * 10f);
				this._secondsLabel.text = Mathf.Floor(num).ToString();
				this._millisecondsLabel.text = string.Format(".{0}", num2);
				if (num <= 0f)
				{
					this._timerRunning = false;
					this._secondsLabel.text = " ";
					if (null != this._millisecondsLabel)
					{
						this._millisecondsLabel.text = " ";
					}
				}
			}
		}

		[SerializeField]
		private UILabel _secondsLabel;

		[SerializeField]
		private UILabel _millisecondsLabel;

		private long _endTime;

		private IHMMGadgetContext _gadgetContext;

		private bool _timerRunning;
	}
}
