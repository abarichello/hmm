using System;
using UnityEngine;

namespace HeavyMetalMachines.MatchMakingQueue.Infra
{
	[CreateAssetMenu(menuName = "Scriptable Object/NoviceTrialsAmountProvider")]
	public class NoviceTrialsAmountConfiguration : ScriptableObject
	{
		public int NoviceTrialsAmount
		{
			get
			{
				return this._noviceTrialsAmount;
			}
		}

		[SerializeField]
		private int _noviceTrialsAmount = 4;
	}
}
