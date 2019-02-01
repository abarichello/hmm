using System;

namespace HeavyMetalMachines.Render
{
	public class RageVFXFeedback : BaseVFXFeedback
	{
		protected override void OnStart()
		{
		}

		protected override bool CompareValues(float target, float current)
		{
			return current > target;
		}

		protected override void OnUpdate()
		{
			float value = this.combatObject.GadgetStates.GadgetJokeBarState.Value;
			float maxValue = this.combatObject.GadgetStates.GadgetJokeBarState.MaxValue;
			this.percent = value / maxValue;
		}

		protected override void OnFinished()
		{
		}
	}
}
