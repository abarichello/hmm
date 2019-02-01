using System;
using System.Collections.Generic;
using UnityEngine;

namespace HeavyMetalMachines.Render
{
	public class SpringConstraint
	{
		public SpringConstraint(Vector3 startPosition, SpringConstraint neighbor, bool isAnchor)
		{
			this.currentSpringPosition = startPosition;
			if (neighbor != null)
			{
				this.neighbor = neighbor;
				neighbor.previousNeighbor = this;
				this.restLength = 0.25f;
			}
			this.isAnchor = isAnchor;
		}

		public Vector3 Position
		{
			get
			{
				return this.currentSpringPosition;
			}
			set
			{
				this.velocity = Vector3.zero;
				this.targetSpringPosition = value;
				this.currentSpringPosition = value;
			}
		}

		public Vector3 Velocity
		{
			get
			{
				return this.velocity;
			}
			set
			{
				this.velocity = value;
			}
		}

		private void SimulateSoftSpring()
		{
			for (SpringConstraint springConstraint = this; springConstraint != null; springConstraint = springConstraint.neighbor)
			{
				Vector3 a = springConstraint.currentSpringPosition;
				springConstraint.currentSpringPosition += this.gravity * this.deltaTime;
				if (!springConstraint.isAnchor)
				{
					SpringConstraint springConstraint2 = springConstraint.neighbor;
					if (springConstraint2 != null)
					{
						Vector3 a2 = springConstraint.currentSpringPosition - springConstraint2.currentSpringPosition;
						float magnitude = a2.magnitude;
						Vector3 a3 = a2 / magnitude;
						if (magnitude > 0.001f)
						{
							float d = (magnitude - springConstraint.restLength) / springConstraint.restLength;
							Vector3 b = a3 * d;
							a = springConstraint.currentSpringPosition - b;
						}
					}
					springConstraint2 = springConstraint.previousNeighbor;
					if (springConstraint2 != null)
					{
						Vector3 a4 = springConstraint.currentSpringPosition - springConstraint2.currentSpringPosition;
						float magnitude2 = a4.magnitude;
						Vector3 a5 = a4 / magnitude2;
						if (magnitude2 > 0.001f)
						{
							float d2 = (magnitude2 - springConstraint.restLength) / springConstraint.restLength;
							Vector3 b2 = a5 * d2;
							a = (a + (springConstraint.currentSpringPosition - b2)) * 0.5f;
						}
					}
				}
				springConstraint.targetSpringPosition = a;
			}
			for (SpringConstraint springConstraint = this; springConstraint != null; springConstraint = springConstraint.neighbor)
			{
				Vector3 b3 = springConstraint.targetSpringPosition - springConstraint.currentSpringPosition;
				if (b3.magnitude > 0.01f)
				{
					springConstraint.velocity += b3;
				}
				springConstraint.velocity = Vector3.ClampMagnitude(springConstraint.velocity, 50f);
				float magnitude3 = springConstraint.velocity.magnitude;
				Vector3 a6 = springConstraint.velocity / magnitude3;
				if (magnitude3 > 50f)
				{
					springConstraint.currentSpringPosition += a6 * (magnitude3 - 50f);
					springConstraint.velocity = a6 * 50f;
				}
				springConstraint.velocity = Vector3.Lerp(springConstraint.velocity, Vector3.zero, Mathf.Clamp01(this.deltaTime * (springConstraint.restLength / b3.magnitude)));
				springConstraint.currentSpringPosition += springConstraint.velocity * this.deltaTime;
			}
		}

		private void SimulateHardSpring()
		{
			for (SpringConstraint springConstraint = this; springConstraint != null; springConstraint = springConstraint.neighbor)
			{
				springConstraint.velocity = Vector3.ClampMagnitude(springConstraint.velocity, 50f);
				springConstraint.currentSpringPosition += springConstraint.velocity * this.deltaTime;
			}
			for (SpringConstraint springConstraint = this; springConstraint != null; springConstraint = springConstraint.neighbor)
			{
				springConstraint.velocity += this.gravity * this.deltaTime;
				SpringConstraint springConstraint2 = springConstraint.neighbor;
				if (springConstraint2 != null)
				{
					Vector3 a = springConstraint.currentSpringPosition - springConstraint2.currentSpringPosition;
					float magnitude = a.magnitude;
					Vector3 a2 = a / magnitude;
					if (magnitude > this.restLength)
					{
						if (!springConstraint2.isAnchor)
						{
							springConstraint2.currentSpringPosition += a2 * (magnitude - this.restLength) * 0.5f;
							springConstraint2.velocity += a2 * (magnitude - this.restLength) * 0.2f;
						}
						else
						{
							springConstraint2.velocity -= a2 * (magnitude - this.restLength) * 0.2f;
						}
						if (!springConstraint.isAnchor)
						{
							springConstraint.currentSpringPosition -= a2 * (magnitude - this.restLength) * 0.5f;
							springConstraint.velocity -= a2 * (magnitude - this.restLength) * 0.2f;
						}
						else
						{
							springConstraint.velocity += a2 * (magnitude - this.restLength) * 0.2f;
						}
					}
				}
			}
		}

		public void SimulateSystem()
		{
			SpringConstraint.SystemType systemType = this.type;
			if (systemType != SpringConstraint.SystemType.Soft)
			{
				if (systemType == SpringConstraint.SystemType.Hard)
				{
					this.SimulateHardSpring();
				}
			}
			else
			{
				this.SimulateSoftSpring();
			}
		}

		public void SimulateSystem(float deltaTime)
		{
			this.deltaTime = deltaTime;
			this.SimulateSystem();
		}

		public Vector3 gravity = new Vector3(0f, -0.2f, 0f);

		private Vector3 currentSpringPosition;

		public float restLength;

		private SpringConstraint neighbor;

		private SpringConstraint previousNeighbor;

		public bool isAnchor;

		public SpringConstraint.SystemType type;

		private Vector3 targetSpringPosition;

		private Vector3 velocity;

		private float deltaTime;

		public enum SystemType
		{
			Soft,
			Hard
		}

		private class SpringSystem
		{
			public void ClearSystem()
			{
				this.springList.Clear();
			}

			public void AddSpring(SpringConstraint spring)
			{
				this.springList.Add(spring);
			}

			private List<SpringConstraint> springList;
		}
	}
}
