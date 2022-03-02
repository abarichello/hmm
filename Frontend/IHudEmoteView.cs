using System;
using HeavyMetalMachines.Presenting;

namespace HeavyMetalMachines.Frontend
{
	public interface IHudEmoteView
	{
		IAnimation AnimationIn { get; }

		IAnimation AnimationOut { get; }

		ITextureMappingUpdater SpritesheetAnimator { get; }
	}
}
