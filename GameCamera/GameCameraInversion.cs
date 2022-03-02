using System;
using HeavyMetalMachines.Arena;
using UnityEngine;

namespace HeavyMetalMachines.GameCamera
{
	[Serializable]
	public class GameCameraInversion : IGameCameraInversion
	{
		public float ScreenSpaceAngle
		{
			get
			{
				return -90f - this.CameraInversionAngleY;
			}
		}

		public Vector3 InversionAngle
		{
			get
			{
				return new Vector3(0f, this.CameraInversionAngleY, this.CameraInversionAngle);
			}
		}

		public void SetupArena(IGameArenaInfo config)
		{
			this.CameraInversionTeamAAngleY = (float)config.CameraInversionTeamAAngleY;
			this.CameraInversionTeamBAngleY = (float)config.CameraInversionTeamBAngleY;
		}

		public bool CameraInverted
		{
			get
			{
				return this._cameraInversionIsInverted;
			}
			set
			{
				this._cameraInversionIsInverted = value;
				this.CameraInversionAngleY = ((!value) ? this.CameraInversionTeamBAngleY : this.CameraInversionTeamAAngleY);
			}
		}

		public float CameraInversionAngle = 45f;

		public float CameraInversionAngleY = 300f;

		public float CameraInversionTeamAAngleY = 135f;

		public float CameraInversionTeamBAngleY = 135f;

		public bool _cameraInversionIsInverted;
	}
}
