using System;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines
{
	public class GUIPlayerMatchPerformanceRules : GameHubScriptableObject
	{
		[Header("Damage Done Rule")]
		public float DamageDoneRule;

		[Header("Damage Done Level")]
		public float DamageDonePerfectLevel;

		public float DamageDoneGreatLevel;

		public float DamageDoneGoodLevel;

		public float DamageDoneAverageLevel;

		[Space(10f)]
		[Header("Repair Done Rule")]
		[Tooltip("To be multiplied with total seconds of the match")]
		public float RepairDoneRule;

		[Header("Repair Done Level")]
		public float RepairDonePerfectLevel;

		public float RepairDoneGreatLevel;

		public float RepairDoneGoodLevel;

		public float RepairDoneAverageLevel;

		[Space(10f)]
		[Header("BomTime Rule")]
		[Tooltip("Total seconds of the match will be divided by BombTimeRule")]
		public float BombTimeRule;

		[Header("BomTime Level")]
		public float BombTimePerfectLevel;

		public float BombTimeGreatLevel;

		public float BombTimeGoodLevel;

		public float BombTimeAverageLevel;

		[Space(10f)]
		[Header("DebuffTime Rule")]
		[Tooltip("Total seconds of the match will be divided by DebuffTimeRule")]
		public float DebuffTimeRule;

		[Header("DebuffTime Level")]
		public float DebuffTimePerfectLevel;

		public float DebuffTimeGreatLevel;

		public float DebuffTimeGoodLevel;

		public float DebuffTimeAverageLevel;
	}
}
