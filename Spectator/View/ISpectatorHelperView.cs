using System;
using HeavyMetalMachines.Presenting;

namespace HeavyMetalMachines.Spectator.View
{
	public interface ISpectatorHelperView
	{
		ICanvas Canvas { get; }

		IButton CloseButton { get; }

		IActivatable KeyboardInputGroup { get; }

		IActivatable GamepadInputGroup { get; }

		void PlayInAnimation();

		void PlayOutAnimation();

		ISpriteImage SetNextMachineSelectionGampepadSprite { get; }

		ISpriteImage SetPreviousMachineSelectionGampepadSprite { get; }

		ISpriteImage SetBombSelectionGampepadSprite { get; }

		ISpriteImage SetZoomInGampepadSprite { get; }

		ISpriteImage SetZoomOutGampepadSprite { get; }

		ISpriteImage SetToogleOrbitalCameraGampepadSprite { get; }

		ISpriteImage SetToogleFreeCameraGampepadSprite { get; }

		ISpriteImage SetTeamBlueFixedCameraGampepadSprite { get; }

		ISpriteImage SetTeamRedFixedCameraGampepadSprite { get; }

		ISpriteImage SetCenterArenaFixedCameraGampepadSprite { get; }

		ISpriteImage SetFreeCameraMovementGampepadSprite { get; }

		ISpriteImage SetOrbitalCameraMovementGampepadSprite { get; }
	}
}
