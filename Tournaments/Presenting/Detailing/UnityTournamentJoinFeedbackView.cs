using System;
using HeavyMetalMachines.Presenting;
using HeavyMetalMachines.Presenting.Unity;
using UnityEngine;

namespace HeavyMetalMachines.Tournaments.Presenting.Detailing
{
	public class UnityTournamentJoinFeedbackView : MonoBehaviour, ITournamentJoinFeedbackView
	{
		public ILabel ReasonLabel
		{
			get
			{
				return this._reasonLabel;
			}
		}

		public void Destroy()
		{
			Object.Destroy(base.gameObject);
		}

		[SerializeField]
		private UnityLabel _reasonLabel;
	}
}
