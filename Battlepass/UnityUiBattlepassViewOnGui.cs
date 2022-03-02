using System;
using UnityEngine;

namespace HeavyMetalMachines.Battlepass
{
	public class UnityUiBattlepassViewOnGui : MonoBehaviour
	{
		public void Setup(UnityUiBattlepassView view)
		{
			this._battlepassView = view;
		}

		private UnityUiBattlepassView _battlepassView;
	}
}
