using System;
using UnityEngine;

namespace HeavyMetalMachines.Combat.Infra
{
	[CreateAssetMenu(menuName = "Modifier/Info Array")]
	public class ModifierInfoArrayParameter : ScriptableObject
	{
		public ModifierInfo[] Infos;
	}
}
