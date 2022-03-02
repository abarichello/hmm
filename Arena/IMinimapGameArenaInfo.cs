using System;
using HeavyMetalMachines.Match;
using UnityEngine;

namespace HeavyMetalMachines.Arena
{
	public interface IMinimapGameArenaInfo
	{
		string MinimapTextureName { get; }

		int MinimapScale { get; }

		Vector2 MinimapTextureSize { get; }

		int MinimapTextureYOffset { get; }

		int MinimapTextureXOffset { get; }

		int MapSize { get; }

		int TeamBlueAngleY { get; }

		int TeamRedAngleY { get; }

		Vector2 IconPositionOffset { get; }

		int TeamBlueArrowRotation { get; }

		int TeamRedArrowRotation { get; }

		Vector2 TeamBlueBasePoint { get; }

		Vector2 TeamRedBasePoint { get; }

		Vector2[] TeamBlueDeadZone { get; }

		Vector2[] TeamRedDeadZone { get; }

		TeamKind BaseFlipTeam { get; }

		bool HideWhenInDeadZone { get; }

		float MinimapCarriedBombOffset { get; }
	}
}
