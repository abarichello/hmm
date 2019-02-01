using System;
using UnityEngine;

namespace HeavyMetalMachines.Server
{
	public class BotPickData
	{
		public BotPickData(float minSelectionDelay, float maxSelectionDelay, float minConfirmationDelay, float maxConfirmationDelay)
		{
			this.DesiredPick = -1;
			this._minSelectionDelay = minSelectionDelay;
			this._maxSelectionDelay = maxSelectionDelay;
			this._minConfirmationDelay = minConfirmationDelay;
			this._maxConfirmationDelay = maxConfirmationDelay;
		}

		public int DesiredPick { get; set; }

		public float Delay { get; private set; }

		public void SetupRandomSelectionDelay()
		{
			this.Delay = UnityEngine.Random.Range(this._minSelectionDelay, this._maxSelectionDelay);
		}

		public void SetupRandomConfirmationDelay()
		{
			this.Delay = UnityEngine.Random.Range(this._minConfirmationDelay, this._maxConfirmationDelay);
		}

		public void DecrementeDelay(float value)
		{
			this.Delay -= value;
		}

		public bool IsWaiting(float currPickTime)
		{
			return currPickTime > 0f && this.Delay > 0f;
		}

		private readonly float _minSelectionDelay;

		private readonly float _maxSelectionDelay;

		private readonly float _minConfirmationDelay;

		private readonly float _maxConfirmationDelay;
	}
}
