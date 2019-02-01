using System;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	public class ShopDriverPageToggle : MonoBehaviour
	{
		public UILabel PageNumber;

		public UIToggle Toggle;

		public UIButton Button;

		public GUIEventListener Eventlistener;

		[HideInInspector]
		public bool Visible;
	}
}
