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
				Vector3 vector = springConstraint.currentSpringPosition;
				springConstraint.currentSpringPosition += this.gravity * this.deltaTime;
				if (!springConstraint.isAnchor)
				{
					SpringConstraint springConstraint2 = springConstraint.neighbor;
					if (springConstraint2 != null)
					{
						Vector3 vector2 = springConstraint.currentSpringPosition - springConstraint2.currentSpringPosition;
						float magnitude = vector2.magnitude;
						Vector3 vector3 = vector2 / magnitude;
						if (magnitude > 0.001f)
						{
							float num = (magnitude - springConstraint.restLength) / springConstraint.restLength;
							Vector3 vector4 = vector3 * num;
							vector = springConstraint.currentSpringPosition - vector4;
						}
					}
					springConstraint2 = springConstraint.previousNeighbor;
					if (springConstraint2 != null)
					{
						Vector3 vector5 = springConstraint.currentSpringPosition - springConstraint2.currentSpringPosition;
						float magnitude2 = vector5.magnitude;
						Vector3 vector6 = vector5 / magnitude2;
						if (magnitude2 > 0.001f)
						{
							float num2 = (magnitude2 - springConstraint.restLength) / springConstraint.restLength;
							Vector3 vector7 = vector6 * num2;
							vector = (vector + (springConstraint.currentSpringPosition - vector7)) * 0.5f;
						}
					}
				}
				springConstraint.targetSpringPosition = vector;
			}
			for (SpringConstraint springConstraint = this; springConstraint != null; springConstraint = springConstraint.neighbor)
			{
				Vector3 vector8 = springConstraint.targetSpringPosition - springConstraint.currentSpringPosition;
				if (vector8.magnitude > 0.01f)
				{
					springConstraint.velocity += vector8;
				}
				springConstraint.velocity = Vector3.ClampMagnitude(springConstraint.velocity, 50f);
				float magnitude3 = springConstraint.velocity.magnitude;
				Vector3 vector9 = springConstraint.velocity / magnitude3;
				if (magnitude3 > 50f)
				{
					springConstraint.currentSpringPosition += vector9 * (magnitude3 - 50f);
					springConstraint.velocity = vector9 * 50f;
				}
				springConstraint.velocity = Vector3.Lerp(springConstraint.velocity, Vector3.zero, Mathf.Clamp01(this.deltaTime * (springConstraint.restLength / vector8.magnitude)));
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
					Vector3 vector = springConstraint.currentSpringPosition - springConstraint2.currentSpringPosition;
					float magnitude = vector.magnitude;
					Vector3 vector2 = vector / magnitude;
					if (magnitude > this.restLength)
					{
						if (!springConstraint2.isAnchor)
						{
							springConstraint2.currentSpringPosition += vector2 * (magnitude - this.restLength) * 0.5f;
							springConstraint2.velocity += vector2 * (magnitude - this.restLength) * 0.2f;
						}
						else
						{
							springConstraint2.velocity -= vector2 * (magnitude - this.restLength) * 0.2f;
						}
						if (!springConstraint.isAnchor)
						{
							springConstraint.currentSpringPosition -= vector2 * (magnitude - this.restLength) * 0.5f;
							springConstraint.velocity -= vector2 * (magnitude - this.restLength) * 0.2f;
						}
						else
						{
							springConstraint.velocity += vector2 * (magnitude - this.restLength) * 0.2f;
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
