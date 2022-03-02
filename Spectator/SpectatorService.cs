using System;
using Assets.Standard_Assets.Scripts.HMM.PlotKids;

namespace HeavyMetalMachines.Spectator
{
	public class SpectatorService : ISpectatorService
	{
		public bool IsSpectating
		{
			get
			{
				return SpectatorController.IsSpectating;
			}
		}

		public void SetFixedCamera(ICameraAngle angle)
		{
			SingletonMonoBehaviour<SpectatorController>.Instance.SetFixedCamera(angle);
		}

		public CameraZoomLevel GetCurrentZoomLevel()
		{
			return SpectatorController.ZoomLevel;
		}
	}
}
