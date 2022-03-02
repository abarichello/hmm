using System;
using System.Linq;
using UnityEngine;

namespace HeavyMetalMachines.AI.Steering
{
	public class SteeringContextParametersTest : MonoBehaviour
	{
		private void Update()
		{
			Vector3 normalized = base.transform.position.normalized;
			this.MirroAxis = this.Parameters.RoundToIndex(normalized);
		}

		private void OnDrawGizmos()
		{
			Gizmos.color = Color.red;
			Gizmos.DrawLine(Vector3.zero, base.transform.position);
			if (this.DrawMirrorsTest)
			{
				Gizmos.color = Color.blue;
				for (int i = 0; i < this.Parameters.DirectionCount; i++)
				{
					Vector3 direction = this.Parameters.GetDirection(i);
					int mirroredDirection = this.Parameters.GetMirroredDirection(i, this.MirroAxis);
					Vector3 direction2 = this.Parameters.GetDirection(mirroredDirection);
					Gizmos.DrawLine(direction * 10f + Vector3.up * (float)i, direction2 * 10f + Vector3.up * (float)i);
				}
				return;
			}
			if (this.DrawAllTest)
			{
				float num = -1f;
				bool flag = false;
				for (int j = 0; j < 1440; j++)
				{
					Vector3 vector = Quaternion.AngleAxis((float)j / 4f, Vector3.up) * Vector3.forward;
					float num2 = this.Parameters.RoundToIndex(vector);
					if (num2 != num)
					{
						num = num2;
						flag = !flag;
					}
					Gizmos.color = ((!flag) ? Color.grey : Color.white);
					Gizmos.DrawLine(Vector3.zero, vector * 10f);
				}
				return;
			}
			for (int k = 0; k < this.Parameters.DirectionCount; k++)
			{
				if (this.DrawDirections.Length <= 0 || this.DrawDirections.Contains(k))
				{
					Vector3 direction3 = this.Parameters.GetDirection(k);
					float num3 = (float)k / (float)this.Parameters.DirectionCount;
					Gizmos.color = new Color(num3, 1f - num3, num3 * num3);
					Gizmos.DrawLine(Vector3.zero, direction3 * 10f);
				}
			}
		}

		public SteeringContextParameters Parameters;

		public float MirroAxis;

		public int[] DrawDirections = new int[0];

		public bool DrawAllTest;

		public bool DrawMirrorsTest;
	}
}
