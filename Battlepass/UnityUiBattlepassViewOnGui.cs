using System;
using UnityEngine;

namespace HeavyMetalMachines.Battlepass
{
	public class UnityUiBattlepassViewOnGui : MonoBehaviour
	{
		public void Setup(BattlepassComponent battlepassComponent, UnityUiBattlepassView view)
		{
			this._battlepassComponent = battlepassComponent;
			this._battlepassView = view;
		}

		private BattlepassComponent _battlepassComponent;

		private UnityUiBattlepassView _battlepassView;
	}
}
