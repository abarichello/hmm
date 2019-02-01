using System;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Car
{
	[CreateAssetMenu(menuName = "Scriptable Object/PlayerIndicator/PlayerIndicatorBorderAnimationConfig")]
	public class PlayerIndicatorBorderAnimationConfig : GameHubScriptableObject
	{
		public AnimationCurve AnimationCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

		public float StartingAlpha;

		public float StartingSize;

		public float DurationInSeconds;
	}
}
