using System;
using UnityEngine;

namespace HeavyMetalMachines.Tutorial.Behaviours
{
	public class ControlTypeWindow : MonoBehaviour
	{
		private void Awake()
		{
			this.TestButton.onClick.Clear();
			this.TestButton.onClick.Add(new EventDelegate(new EventDelegate.Callback(this.OnTestButtonClick)));
		}

		private void OnTestButtonClick()
		{
			if (this.OnTest != null)
			{
				this.OnTest(this);
			}
		}

		public Action<ControlTypeWindow> OnTest;

		public UIButton TestButton;
	}
}
