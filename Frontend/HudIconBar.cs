using System;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	public class HudIconBar : MonoBehaviour, IHudIconBar
	{
		public HudStackIcon BladesIcon
		{
			get
			{
				return this._blades;
			}
		}

		[SerializeField]
		private HudStackIcon _blades;
	}
}
