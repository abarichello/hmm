using System;
using UnityEngine;

namespace HeavyMetalMachines.AI
{
	[CreateAssetMenu(menuName = "AI/AI Element Kind")]
	public class UnityAIElementKind : ScriptableObject, IAIElementKind
	{
		public int ID
		{
			get
			{
				return this._id;
			}
		}

		[SerializeField]
		[ReadOnly]
		private int _id;
	}
}
