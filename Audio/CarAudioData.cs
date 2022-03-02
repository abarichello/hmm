using System;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Audio
{
	public class CarAudioData : GameHubScriptableObject
	{
		public void Preload()
		{
			if (this.Engine_Player != null)
			{
				this.Engine_Player.Preload();
			}
			if (this.Engine_Others != null)
			{
				this.Engine_Others.Preload();
			}
			if (this.Hitted_Player != null)
			{
				this.Hitted_Player.Preload();
			}
			if (this.Hitted_Others != null)
			{
				this.Hitted_Others.Preload();
			}
		}

		public AudioEventAsset Engine_Player;

		public AudioEventAsset Engine_Others;

		public AudioEventAsset Hitted_Player;

		public AudioEventAsset Hitted_Others;

		public AnimationCurve[] RpmGearCurves;

		public AnimationCurve RpmTurningCurve;

		public AnimationCurve RpmGridCurve;

		public AnimationCurve RpmDriftTurningCurve;

		public AnimationCurve RpmGainBySpeedCurve;

		public float[] gearShiftRPM;

		public float rpmGearShiftPercent;

		public float gearUpDelay = 0.5f;

		public float gearDownDelay = 0.5f;

		public float driftDelay = 0.5f;

		public float releaseTime = 1f;

		public float idleRPM;

		public float minRPM = 2000f;

		public float maxRPM = 10000f;

		public float rpmGain = 0.05f;

		public float driftRPMGain = 0.1f;

		public float MaxDriftRPMGain = 1500f;

		public int rpmFloor;

		public float gearShitRPMLoss = 2000f;

		public float minDriftSpeed = 5f;
	}
}
