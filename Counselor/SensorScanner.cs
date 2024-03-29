﻿using System;
using Hoplon.SensorSystem;

namespace HeavyMetalMachines.Counselor
{
	public class SensorScanner : IScanner, IObserver
	{
		public SensorScanner(SensorController context, string name)
		{
			this._bindingId = context.GetHash(name);
			this._context = context;
			this.notification = 0f;
		}

		public void UpdateContext(SensorController context)
		{
			if (this.notification > 0f)
			{
				float num;
				context.GetParameter(context.MainClockId, ref num);
				context.SetParameter(this._bindingId, num - this.notification);
			}
		}

		public void Notify(int id, bool isActive, int count)
		{
			float num;
			this._context.GetParameter(this._context.MainClockId, ref num);
			this.notification = num;
		}

		public void Reset()
		{
		}

		public int _bindingId;

		private float notification;

		private SensorController _context;
	}
}
