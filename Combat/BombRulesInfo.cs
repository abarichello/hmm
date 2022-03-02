using System;
using HeavyMetalMachines.Combat.Gadget;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat
{
	public class BombRulesInfo : GameHubScriptableObject
	{
		public int BombScoreTarget
		{
			get
			{
				return this._bombScoreTarget;
			}
			set
			{
				this._bombScoreTarget = value;
			}
		}

		public float ShopDelayBeforeOpenSeconds = 2f;

		[HideInInspector]
		public float ShopPhaseSeconds;

		[HideInInspector]
		public float ReplayDelaySeconds = 1.5f;

		public float ReplayTimeSeconds = 10f;

		public int PreBombDurationSeconds = 10;

		public BombInfo BombInfo;

		[SerializeField]
		private int _bombScoreTarget = 3;

		public float TimeToCheckDisputeFinished = 1f;

		public bool SlowMotionEnabled = true;

		public float SlowMotionSeconds = 0.3f;

		public float SlowMotionTimeScaleRate = 0.3f;

		public int MaxDistanceToActivateZoom;

		public int GrabFeedbackMaxBombDistance = 100;

		public GadgetInfo Weapon;

		public float GridCursorSpeedUp;

		public float GridCursorSpeedDown;

		public AnimationCurve GridCursorCurveUp;

		public AnimationCurve GridCursorCurveDown;

		public int GridMaxDiffClientServer;

		public float BombSaveTimeSeconds = 1f;
	}
}
