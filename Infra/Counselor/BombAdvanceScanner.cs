using System;
using Hoplon.SensorSystem;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Infra.Counselor
{
	public class BombAdvanceScanner : GameHubObject, IScanner
	{
		public BombAdvanceScanner(SensorController controller, string redDistanceToGoalParameterName, string blueDistanceToGoalParameterName, string redAdvanceToGoalParameterName, string blueAdvanceToGoalParameterName)
		{
			this._redDistanceToGoalId = controller.GetHash(redDistanceToGoalParameterName);
			this._blueDistanceToGoalId = controller.GetHash(blueDistanceToGoalParameterName);
			this._redAdvanceToGoalId = controller.GetHash(redAdvanceToGoalParameterName);
			this._blueAdvanceToGoalId = controller.GetHash(blueAdvanceToGoalParameterName);
			this._timedUpdater = new TimedUpdater(1000, true, false);
			this.lastDirection = 0;
		}

		public void UpdateContext(SensorController context)
		{
			if (this._timedUpdater.ShouldHalt())
			{
				return;
			}
			float num;
			context.GetParameter(this._redDistanceToGoalId, out num);
			float num2 = num - this.previousRedValue;
			this.previousRedValue = num;
			float num3;
			context.GetParameter(this._blueDistanceToGoalId, out num3);
			float num4 = num3 - this.previousBlueValue;
			this.previousBlueValue = num3;
			if (Mathf.Abs(num2 - num4) < GameHubObject.Hub.CounselorConfig.WrongWaySensibility)
			{
				context.SetParameter(this._redAdvanceToGoalId, 0f);
				context.SetParameter(this._blueAdvanceToGoalId, 0f);
			}
			else if (num2 > num4)
			{
				context.SetParameter(this._redAdvanceToGoalId, -1f);
				context.SetParameter(this._blueAdvanceToGoalId, 1f);
			}
			else
			{
				context.SetParameter(this._redAdvanceToGoalId, 1f);
				context.SetParameter(this._blueAdvanceToGoalId, -1f);
			}
		}

		public override string ToString()
		{
			return string.Format("BombAdvanceScanner: _teamDistanceToGoalId {0} _advanceToGoalId {1}", this._redDistanceToGoalId, this._redAdvanceToGoalId);
		}

		public void Reset()
		{
		}

		private int _redDistanceToGoalId;

		private int _blueDistanceToGoalId;

		private int _redAdvanceToGoalId;

		private int _blueAdvanceToGoalId;

		private float previousRedValue;

		private float previousBlueValue;

		private float lastTimeAdvancing;

		private int lastDirection;

		private TimedUpdater _timedUpdater;
	}
}
