using System;
using UnityEngine;

namespace HeavyMetalMachines.Combat.GadgetScript.Body.Filter
{
	[Serializable]
	public class BaseCombatFilter
	{
		public string ClassName
		{
			get
			{
				return this._className;
			}
		}

		public string SerializedObject
		{
			get
			{
				return this._serializedObject;
			}
		}

		[SerializeField]
		private string _className;

		[SerializeField]
		private string _serializedObject;
	}
}
