using System;
using UnityEngine;

namespace HeavyMetalMachines.Frontend.Apis
{
	public interface IScreenResolutionController
	{
		Vector2 GetClientWindowDimensions();

		Vector2 GetClientWindowOffset();

		Resolution GetCurrentResolution();
	}
}
