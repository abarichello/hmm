using System;
using UnityEngine;

namespace HeavyMetalMachines.Render
{
	public interface IAnimatorGadgetFeedback
	{
		void Activate();

		string TriggerName { get; set; }

		bool TriggerBool { get; set; }

		int TriggerInteger { get; set; }

		AnimatorControllerParameterType TriggerType { get; }
	}
}
