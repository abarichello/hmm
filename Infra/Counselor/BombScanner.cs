using System;
using HeavyMetalMachines.Combat;
using HeavyMetalMachines.Infra.Context;
using HeavyMetalMachines.Match;
using HeavyMetalMachines.Utils.Bezier;
using Hoplon.SensorSystem;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Infra.Counselor
{
	public class BombScanner : GameHubObject, IScanner
	{
		public BombScanner(SensorController controller, string PointInGoalPathParameterName, string RedTeamDistanceToGoalParameterName, string BlueTeamDistanceToGoalParameterName, string CollisionWithBlockerParameterName, string isOvertimeParameterName, ApproximatedPathDistance approximatedPathDistance, BombTargetTrigger[] targets)
		{
			this._pointInGoalPathId = controller.GetHash(PointInGoalPathParameterName);
			this._redTeamDistanceId = controller.GetHash(RedTeamDistanceToGoalParameterName);
			this._blueTeamDistanceId = controller.GetHash(BlueTeamDistanceToGoalParameterName);
			this._isOvertimeId = controller.GetHash(isOvertimeParameterName);
			this._collidedBlockerStayId = controller.GetHash(CollisionWithBlockerParameterName);
			this._approximatedPathDistance = approximatedPathDistance;
			foreach (BombTargetTrigger bombTargetTrigger in targets)
			{
				if (bombTargetTrigger.TeamOwner == TeamKind.Red)
				{
					this.blueGoal = bombTargetTrigger;
				}
				else
				{
					this.redGoal = bombTargetTrigger;
				}
			}
			BombMovement bombMovement = GameHubObject.Hub.BombManager.BombMovement;
			bombMovement.OnCollisionWithBombBlocker = (Action)Delegate.Combine(bombMovement.OnCollisionWithBombBlocker, new Action(this.OnCollisionWithBombBlocker));
			BombScanner.Log.DebugFormat("Constructor _approximatedPathDistance {0} blueGoal {1} redGoal {2}", new object[]
			{
				this._approximatedPathDistance != null,
				this.blueGoal != null,
				this.redGoal != null
			});
		}

		~BombScanner()
		{
			this._approximatedPathDistance = null;
		}

		public void UpdateContext(SensorController context)
		{
			float num;
			context.GetParameter(context.MainClockId, ref num);
			float num2;
			context.GetParameter(context.DeltaTimeId, ref num2);
			context.SetParameter(this._collidedBlockerStayId, this._collidedBlockerStay);
			this._collidedBlockerStay -= num2;
			bool flag = GameHubObject.Hub.BombManager.ScoreBoard.CurrentState == BombScoreboardState.BombDelivery && GameHubObject.Hub.BombManager.ScoreBoard.IsInOvertime;
			context.SetParameter(this._isOvertimeId, (float)((!flag) ? 0 : 1));
			BombMovement bombMovement = GameHubObject.Hub.BombManager.BombMovement;
			if (bombMovement != null)
			{
				this.SetAbsoluteDistance(context, bombMovement);
			}
			else
			{
				context.SetParameter(this._pointInGoalPathId, float.MaxValue);
				context.SetParameter(this._redTeamDistanceId, float.MaxValue);
				context.SetParameter(this._blueTeamDistanceId, float.MaxValue);
			}
		}

		private void SetAbsoluteDistance(SensorController context, BombMovement bombMovement)
		{
			this._approximatedPathDistance.DistanceToGoal(bombMovement.transform.position, ref this.segmentInfo);
			float num;
			float num2;
			if (GameHubObject.Hub.BombManager.ScoreBoard.IsInOvertime)
			{
				this._approximatedPathDistance.DistanceToGoal(this.redGoal.transform.position, ref this.redSegmentInfo);
				this._approximatedPathDistance.DistanceToGoal(this.blueGoal.transform.position, ref this.blueSegmentInfo);
				num = this.segmentInfo.sqrDistance - this.redSegmentInfo.sqrDistance;
				num2 = this._approximatedPathDistance.SqrLength - this.redSegmentInfo.sqrDistance - this.segmentInfo.sqrDistance;
			}
			else
			{
				num = this.segmentInfo.sqrDistance;
				num2 = this._approximatedPathDistance.SqrLength - num;
			}
			context.SetParameter(this._pointInGoalPathId, Mathf.Min(num, num2));
			context.SetParameter(this._redTeamDistanceId, num);
			context.SetParameter(this._blueTeamDistanceId, num2);
		}

		private void OnCollisionWithBombBlocker()
		{
			this._collidedBlockerStay = 1f;
		}

		public override string ToString()
		{
			return string.Format("BombScanner: _pointInGoalPathId {0} _redTeamDistanceId {1} _blueTeamDistanceId {2}", this._pointInGoalPathId, this._redTeamDistanceId, this._blueTeamDistanceId);
		}

		public void Reset()
		{
			this._collidedBlockerStay = 0f;
		}

		private static readonly BitLogger Log = new BitLogger(typeof(BombScanner));

		private int _pointInGoalPathId;

		private int _redTeamDistanceId;

		private int _blueTeamDistanceId;

		private int _isOvertimeId;

		private int _collidedBlockerStayId;

		private float _collidedBlockerStay;

		private ApproximatedPathDistance _approximatedPathDistance;

		private ApproximatedPathDistance.SegmentInfo segmentInfo;

		private ApproximatedPathDistance.SegmentInfo redSegmentInfo;

		private ApproximatedPathDistance.SegmentInfo blueSegmentInfo;

		private BombTargetTrigger blueGoal;

		private BombTargetTrigger redGoal;
	}
}
