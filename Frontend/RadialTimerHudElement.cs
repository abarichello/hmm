using System;
using HeavyMetalMachines.Combat.GadgetScript;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	public class RadialTimerHudElement : MonoBehaviour
	{
		public void SetTimer(float totalTime, IHMMGadgetContext gadgetContext)
		{
			this._gadgetContext = gadgetContext;
			this._endTime = (long)this._gadgetContext.CurrentTime + (long)(totalTime * 1000f);
			if (!this._timerRunning)
			{
				this._totalTime = totalTime;
			}
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
				this._radialSprite.fillAmount = 1f - num / this._totalTime;
				if (num <= 0f)
				{
					this._timerRunning = false;
				}
			}
		}

		[SerializeField]
		private UI2DSprite _radialSprite;

		private long _endTime;

		private bool _timerRunning;

		private float _totalTime;

		private IHMMGadgetContext _gadgetContext;
	}
}
