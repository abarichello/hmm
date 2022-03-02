using System;
using HeavyMetalMachines.Infra.Context;
using UnityEngine;

namespace HeavyMetalMachines.Combat.GadgetScript.Block.Parameter.Filter
{
	public class CheckGameState : ScriptableObject, IParameterComparison
	{
		public bool Compare(object context)
		{
			return ((IHMMGadgetContext)context).ScoreBoard.RoundState == (int)this._state;
		}

		[SerializeField]
		private BombScoreboardState _state;
	}
}
