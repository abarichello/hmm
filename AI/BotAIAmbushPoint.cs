using System;
using System.Collections.Generic;
using HeavyMetalMachines.Match;
using UnityEngine;

namespace HeavyMetalMachines.AI
{
	internal class BotAIAmbushPoint : MonoBehaviour
	{
		public bool IsMidPoint
		{
			get
			{
				return this.radius <= 1f;
			}
		}

		private void OnEnable()
		{
			BotAIAmbushPoint.AIAmbushPoints.Add(this);
		}

		private void OnDisable()
		{
			BotAIAmbushPoint.AIAmbushPoints.Remove(this);
		}

		private void OnDrawGizmos()
		{
			BotAIAmbushPoint.Kind kind = this.ambushKind;
			if (kind != BotAIAmbushPoint.Kind.AmbushOptionalPoint)
			{
				if (kind != BotAIAmbushPoint.Kind.AmbushMidPoint)
				{
					if (kind == BotAIAmbushPoint.Kind.AmbushPoint)
					{
						Gizmos.DrawIcon(base.transform.position, "Apple", false);
					}
				}
				else
				{
					Gizmos.DrawIcon(base.transform.position, "Apple_midpoint", false);
				}
			}
			else
			{
				Gizmos.DrawIcon(base.transform.position, "Apple_optional", false);
			}
			if ((this.nextWaypoint != null && this.nextWaypoint.ambushKind == BotAIAmbushPoint.Kind.AmbushMidPoint) || this.ambushKind == BotAIAmbushPoint.Kind.AmbushMidPoint)
			{
				Gizmos.color = new Color(0.5f, 1f, 0.5f);
			}
			else
			{
				Gizmos.color = new Color(1f, 0.5f, 0.5f);
			}
			Gizmos.DrawWireSphere(base.transform.position, this.radius);
			if (this.nextWaypoint != null)
			{
				Gizmos.DrawLine(base.transform.position, this.nextWaypoint.transform.position);
			}
			if (this.optionalPoint != null)
			{
				Gizmos.color = new Color(0.9f, 0.6f, 0.2f);
				Gizmos.DrawLine(base.transform.position, this.optionalPoint.transform.position);
			}
		}

		public float radius;

		public BotAIAmbushPoint nextWaypoint;

		public BotAIAmbushPoint optionalPoint;

		[Tooltip("In wich player it must be triggered")]
		public TeamKind ambushPointTeam;

		public BotAIAmbushPoint.Kind ambushKind;

		public static List<BotAIAmbushPoint> AIAmbushPoints = new List<BotAIAmbushPoint>();

		public enum Kind
		{
			AmbushPoint,
			AmbushMidPoint,
			AmbushOptionalPoint
		}
	}
}
