using System;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	public class MatchBlocker
	{
		public void BlockPlayer()
		{
			this._playerBlockedTimeInSec = Time.unscaledTime + 180f;
		}

		public bool IsBlocked()
		{
			return Time.unscaledTime < this._playerBlockedTimeInSec;
		}

		private static readonly BitLogger Log = new BitLogger(typeof(MatchBlocker));

		public const int MaxBlockedTimeInSec = 180;

		private float _playerBlockedTimeInSec = -1f;
	}
}
