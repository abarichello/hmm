using System;
using UnityEngine;

namespace HeavyMetalMachines.Combat.Gadget
{
	[Serializable]
	public class BombGadgetInfo : BasicLinkInfo
	{
		public override Type GadgetType()
		{
			return typeof(BombGadget);
		}

		public float TotalPowerHoldTime
		{
			get
			{
				return this.DropTime + this.HoldStartTime + this.HoldPowerUpTime + this.MaxPowerTime;
			}
		}

		public float TotalPowerUpTime
		{
			get
			{
				return this.HoldStartTime + this.HoldPowerUpTime;
			}
		}

		[Header("[BombGadget]")]
		public ModifierInfo[] WarmupModifiers;

		public FXInfo GrabberMiss;

		public ModifierInfo[] GrabberMissOnHitBombModifiers;

		public float MinimumGrabberUseTime;

		public float GrabberCancelWarmupLifetime;

		public FXInfo GrabberCancelWarmup;

		public ModifierInfo[] LinkCancelWarmupModifiers;

		public float MinimumLinkedTime;

		public FXInfo BombLinkCreated;

		public float BombLinkCreatedLifetime;

		public ModifierInfo[] BombLinkCreatedModifiers;

		public float SpinningSpeedThreshold;

		public string SpinningSpeedThresholdUpgrade;

		public float SpinningTimeActivation;

		public string SpinningTimeActivationUpgrade;

		public FXInfo SpinningEffect;

		public ModifierInfo[] SpinningModifiers;

		public FXInfo SpinningReleaseEffect;

		public FXInfo SpinningReleaseMaxPowerEffect;

		public ModifierInfo[] SpinningReleaseModifiers;

		public float SpinningReleaseLifetime;

		public string SpinningReleaseLifetimeUpgrade;

		public float SpinningTimeDeactivation;

		public string SpinningTimeDeactivationUpgrade;

		public float JammedRange;

		public FXInfo GrabberJammerEffect;

		public FXInfo JammedEffect;

		public ModifierInfo[] JammedModifiers;

		public float PushBackAngle;

		public float PushBackForce;

		public BombGadgetInfo.PushBackType PushBack;

		public float LeewayOffsetAngle;

		public float LeewayOffset;

		public bool ProgressiveLeeway;

		public FXInfo LinkBrokenEffect;

		public ModifierInfo[] LinkBrokenModifiers;

		public float LinkBrokenLifeTime;

		public FXInfo BombHoldWarmupEffect;

		public ModifierInfo[] BombHoldWarmupModifiers;

		public FXInfo BombHolderEffect;

		public ModifierInfo[] BombHolderModifiers;

		[Tooltip("Time you can hold the bomb and just drop it, old style")]
		public float DropTime;

		[Tooltip("Time between start holding and start powering up, when you can't drop the bomb yet")]
		public float HoldStartTime;

		[Tooltip("Time where bomb powers up after holding, the rate of power gained is given by HoldPowerPerSecond")]
		public float HoldPowerUpTime;

		[Tooltip("Amount of power added per second held")]
		public float HoldPowerPerSecond;

		[Tooltip("Time bomb still holds after max power is achieved")]
		public float MaxPowerTime;

		public enum PushBackType
		{
			Accelerate,
			DeaccelerateOnly,
			SetSpeed
		}
	}
}
