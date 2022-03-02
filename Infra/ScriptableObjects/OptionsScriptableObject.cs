using System;
using UnityEngine;

namespace HeavyMetalMachines.Infra.ScriptableObjects
{
	[CreateAssetMenu(menuName = "Scriptable Object/OptionsScriptableObject")]
	public class OptionsScriptableObject : ScriptableObject
	{
		[Header("[PlayerIndicator]")]
		public int PlayerIndicatorVisualMaxAlpha = 100;

		public int PlayerIndicatorVisualMinAlpha = 10;

		public float PlayerIndicatorMaxAlpha = 1f;

		public float PlayerIndicatorMinAlpha = 0.196f;

		public float PlayerIndicatorDefaultAlpha = 0.555f;

		[Header("[ObjectiveIndicator]")]
		[Header("Alpha")]
		public int ObjectiveIndicatorVisualMaxAlpha = 100;

		public int ObjectiveIndicatorVisualMinAlpha = 10;

		public float ObjectiveIndicatorMaxAlpha = 1f;

		public float ObjectiveIndicatorMinAlpha = 0.196f;

		[Tooltip("Slider Percentage")]
		public float ObjectiveIndicatorDefaultAlpha = 0.555f;

		[Header("Size")]
		public int ObjectiveIndicatorVisualMaxSize = 100;

		public int ObjectiveIndicatorVisualMinSize = 10;

		public float ObjectiveIndicatorMaxSize = 7f;

		public float ObjectiveIndicatorMinSize = 4f;

		[Tooltip("Slider Percentage")]
		public float ObjectiveIndicatorDefaultSize = 0.555f;

		[Header("Arrow Quantity")]
		public int ObjectiveIndicatorVisualMaxArrowQuantity = 10;

		public int ObjectiveIndicatorVisualMinArrowQuantity = 3;

		public int ObjectiveIndicatorMaxArrowQuantity = 10;

		public int ObjectiveIndicatorMinArrowQuantity = 3;

		[Tooltip("Slider Percentage")]
		public float ObjectiveIndicatorDefaultArrowQuantity = 1f;
	}
}
