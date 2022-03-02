using System;
using HeavyMetalMachines.Presenting;
using HeavyMetalMachines.Presenting.Unity;
using UnityEngine;

namespace HeavyMetalMachines.Tournaments.Presenting.Detailing.Rankings
{
	public class TournamentRankingFinalSelectorView : MonoBehaviour, ITournamentRankingFinalSelectorView
	{
		public IToggle SelectionToggle
		{
			get
			{
				return this._selectionToggleUnity;
			}
		}

		public ILabel SelectionToggleLabel
		{
			get
			{
				return this._selectionToggleLabelUnity;
			}
		}

		[SerializeField]
		private UnityToggle _selectionToggleUnity;

		[SerializeField]
		private UnityLabel _selectionToggleLabelUnity;
	}
}
