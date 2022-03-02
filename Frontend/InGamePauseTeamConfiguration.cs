using System;
using Plugins.Attributes.ReferenceByString;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	[CreateAssetMenu(menuName = "Scriptable Object/UI/InGamePauseTeamConfiguration")]
	public class InGamePauseTeamConfiguration : GameHubScriptableObject
	{
		public string TitleLabel;

		public Color TeamBackgroundColor;

		[ReferenceByName(typeof(Sprite))]
		public string SideFeedbackBackgroundSpriteName;
	}
}
