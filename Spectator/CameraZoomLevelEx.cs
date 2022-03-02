using System;

namespace HeavyMetalMachines.Spectator
{
	public static class CameraZoomLevelEx
	{
		public static CameraZoomLevel Further(this CameraZoomLevel level)
		{
			if (level == CameraZoomLevel.Close)
			{
				return CameraZoomLevel.Near;
			}
			if (level != CameraZoomLevel.Near)
			{
				return CameraZoomLevel.Far;
			}
			return CameraZoomLevel.Game;
		}

		public static CameraZoomLevel Nearer(this CameraZoomLevel level)
		{
			if (level == CameraZoomLevel.Far)
			{
				return CameraZoomLevel.Game;
			}
			if (level != CameraZoomLevel.Game)
			{
				return CameraZoomLevel.Close;
			}
			return CameraZoomLevel.Near;
		}
	}
}
