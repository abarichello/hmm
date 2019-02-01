using System;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Frontend
{
	public class ProfileGui : StateGuiController
	{
		private void Awake()
		{
			if (ProfileGui.myXxx == null)
			{
				ProfileGui.myXxx = new ProfileGui.XXX();
				ProfileGui.myXxx.a = 3;
			}
		}

		private void OnEnable()
		{
		}

		private void OnDisable()
		{
		}

		private static readonly BitLogger Log = new BitLogger(typeof(ProfileGui));

		private Rect _rect = new Rect(500f, 0f, 150f, 30f);

		private ProfileHelper profileHelper;

		private static ProfileGui.XXX myXxx = null;

		public class XXX
		{
			public int a;
		}
	}
}
