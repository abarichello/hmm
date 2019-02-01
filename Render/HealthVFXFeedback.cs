using System;

namespace HeavyMetalMachines.Render
{
	public class HealthVFXFeedback : BaseVFXFeedback
	{
		protected override void OnStart()
		{
		}

		protected override bool CompareValues(float target, float current)
		{
			return current < target && this.combatObject.IsAlive();
		}

		protected override void OnUpdate()
		{
			this.percent = this.combatObject.Data.HP / (float)this.combatObject.Data.HPMax;
		}

		protected override void OnFinished()
		{
		}
	}
}
