using System;
using UnityEngine;

namespace HeavyMetalMachines.EndMatch.Battlepass
{
	public class EndMatchBattlepassObjectiveMissionSlot : MonoBehaviour
	{
		public Transform ObjectiveAnchorTransform
		{
			get
			{
				return this._objectiveAnchorTransform;
			}
		}

		[SerializeField]
		public UILabel DescriptionLabel;

		[SerializeField]
		public UITexture ProgressSliderTexture;

		[SerializeField]
		public UILabel ProgressLabel;

		[SerializeField]
		public GameObject SeparatorObject;

		[SerializeField]
		private Transform _objectiveAnchorTransform;
	}
}
