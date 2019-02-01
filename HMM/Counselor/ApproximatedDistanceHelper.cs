using System;
using HeavyMetalMachines.Utils.Bezier;
using UnityEngine;

namespace HeavyMetalMachines.HMM.Counselor
{
	public class ApproximatedDistanceHelper : MonoBehaviour
	{
		private void OnDrawGizmos()
		{
			Gizmos.color = Color.yellow;
			Gizmos.DrawLine(base.transform.position, this.segmentInfo.nearest);
			Gizmos.color = Color.red;
			Gizmos.DrawLine(this.segmentInfo.node, this.segmentInfo.nearest);
			int num = this.lastDirection;
			if (num != 0)
			{
				if (num != 1)
				{
					if (num == -1)
					{
						Gizmos.color = Color.green;
					}
				}
				else
				{
					Gizmos.color = Color.red;
				}
			}
			else
			{
				Gizmos.color = Color.yellow;
			}
			Gizmos.DrawWireSphere(base.transform.position, 5f);
		}

		private void Update()
		{
			if (this._pathCalc == null)
			{
				return;
			}
			if (this._pathCalc.Segments.Count > 0)
			{
				this._pathCalc.DistanceToGoal(base.transform.position, ref this.segmentInfo);
			}
			this.currentTime = Time.frameCount;
			this.currentValue = this.segmentInfo.sqrDistance;
			int num = this.currentValue.CompareTo(this.previousValue);
			this.previousValue = this.currentValue;
			if (this.lastDirection != num)
			{
				this.counter++;
				if (this.counter > this.maxcounter)
				{
					this.lastDirection = num;
				}
			}
			else
			{
				this.counter = 0;
			}
		}

		public ApproximatedPathDistance _pathCalc;

		public ApproximatedPathDistance.SegmentInfo segmentInfo;

		public int currentTime;

		public int lastTimeAdvancing;

		public float currentValue;

		public float previousValue;

		public int lastDirection;

		public int advanceDirection;

		public int counter;

		public int maxcounter = 10;
	}
}
