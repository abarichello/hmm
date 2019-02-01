using System;
using System.Collections.Generic;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat
{
	[Serializable]
	public class CombatLink
	{
		public CombatLink(CombatLink.LinkHook point1, CombatLink.LinkHook point2, float range, float compresssion, float tension, float tensionBreakForce, bool clampIn, bool clampOut, bool clampOnCorners, string tag)
		{
			this.Point1 = point1;
			this.Point2 = point2;
			this.Tag = tag;
			this.Compression = compresssion;
			this.Tension = tension;
			this.TensionBreakForce = tensionBreakForce;
			this.ClampOut = clampOut;
			this.ClampIn = clampIn;
			this.Range = range;
			this._clampOnCorners = clampOnCorners;
		}

		public bool IsBroken { get; private set; }

		public string Tag { get; private set; }

		public CombatLink.LinkHook Point1 { get; private set; }

		public CombatLink.LinkHook Point2 { get; private set; }

		public float Compression { get; set; }

		public float Tension { get; set; }

		public float TensionBreakForce { get; set; }

		public bool ClampOut { get; set; }

		public bool ClampIn { get; set; }

		public bool IsFixedPivot
		{
			get
			{
				return this._fixedPivot;
			}
		}

		public float Range { get; set; }

		public Vector3 PositionDiff
		{
			get
			{
				return this._posDiff;
			}
		}

		public void Break()
		{
			this.IsBroken = true;
		}

		public void SetLengthOffset(float offset)
		{
			this._offset = offset;
		}

		private void UpdateHooks()
		{
			this.Point1.UpdateVelocity();
			this.Point2.UpdateVelocity();
			this._posDiff = this.Point2.Position - this.Point1.Position;
			Vector3 pivot = this.Point1.Position + this._posDiff * 0.5f;
			RaycastHit raycastHit;
			bool flag = Physics.Raycast(this.Point1.Position, this._posDiff, out raycastHit, this._posDiff.magnitude, 33554432);
			if (flag && !this._fixedPivot)
			{
				this._fixedPivot = true;
				this._pivot = raycastHit.collider.transform.position;
			}
			else if (!this._fixedPivot || !flag)
			{
				this._fixedPivot = false;
				this._pivot = pivot;
			}
			this.Point1.UpdatePivot(this._pivot);
			this.Point2.UpdatePivot(this._pivot);
		}

		public void Update(CombatMovement updater)
		{
			this._updatedPoints.Add(updater);
			if (this._updatedPoints.Count < 2 || this.IsBroken)
			{
				return;
			}
			this._updatedPoints.Clear();
			this.UpdateHooks();
			this._curLen = this.Point1.Distance + this.Point2.Distance;
			float num = this._curLen - (this.Range + this._offset);
			float num2 = Vector3.Dot(this.Point1.Velocity, this.Point1.Normal);
			float num3 = -Vector3.Dot(this.Point2.Velocity, this.Point2.Normal);
			if (this._curLen >= this.Range + this._offset)
			{
				if (this.ClampOut && (!this._fixedPivot || this._clampOnCorners))
				{
					this.Clamp(num);
					num = 0f;
				}
				if (this.Tension > 0f && num > 0f)
				{
					float num4 = this.Tension * num;
					if (num4 >= this.TensionBreakForce)
					{
						this.Break();
						return;
					}
					this.Point1.Movement.Push(this.Point1.Normal, false, Time.deltaTime * (num4 / this.GetPointMass(this.Point1, this.Point1.Normal)), true);
					this.Point2.Movement.Push(this.Point2.Normal, false, Time.deltaTime * (num4 / this.GetPointMass(this.Point2, this.Point2.Normal)), true);
				}
				if ((num2 <= 0f || num3 > 0f) && num2 - num3 <= 0f)
				{
					float pointMass = this.GetPointMass(this.Point1, this.Point1.Normal);
					float pointMass2 = this.GetPointMass(this.Point2, this.Point2.Normal);
					float num5 = (pointMass * num2 + pointMass2 * num3) / (pointMass + pointMass2);
					this.Point1.Movement.Push(this.Point1.Normal, false, num5 - num2, true);
					this.Point2.Movement.Push(-this.Point2.Normal, false, num5 - num3, true);
				}
			}
			else if (num < 0f)
			{
				if (this.ClampIn && (this._clampOnCorners || !this._fixedPivot))
				{
					this.Clamp(num);
					num = 0f;
				}
				if (this.Compression > 0f && num2 - num3 > 0f)
				{
					this.Point1.Movement.Push(this.Point1.Normal, false, Time.deltaTime * (num * this.Compression / this.GetPointMass(this.Point1, this.Point1.Normal)), true);
					this.Point2.Movement.Push(this.Point2.Normal, false, Time.deltaTime * (num * this.Compression / this.GetPointMass(this.Point2, this.Point2.Normal)), true);
				}
			}
			this.Point1.Movement.IncrementUpdatedLinkCount();
			this.Point2.Movement.IncrementUpdatedLinkCount();
		}

		private void Clamp(float stretch)
		{
			this.Point1.Clamp(stretch * 0.5f);
			this.Point2.Clamp(stretch * 0.5f);
		}

		public void FreezeVelocity(CombatMovement hookMovement)
		{
			CombatLink.LinkHook linkHook = (!(this.Point1.Movement == hookMovement)) ? this.Point2 : this.Point1;
			linkHook.FreezeVelocity();
		}

		public bool HasSameHooks(CombatLink other)
		{
			return (other.Point1.Movement == this.Point1.Movement || other.Point1.Movement == this.Point2.Movement) && (other.Point2.Movement == this.Point1.Movement || other.Point2.Movement == this.Point2.Movement);
		}

		public float GetOtherPointMass(CombatLink.LinkHook point, Vector3 velocity)
		{
			float mass;
			Vector3 position;
			if (point == this.Point1)
			{
				mass = this.Point2.Mass;
				position = this.Point2.Position;
			}
			else
			{
				mass = this.Point1.Mass;
				position = this.Point1.Position;
			}
			if (this._curLen > this.Range && Vector3.Dot(velocity, position - this._pivot) < 0f)
			{
				return mass;
			}
			return 0f;
		}

		public float GetPointMass(CombatLink.LinkHook point, Vector3 velocity)
		{
			float result;
			if (point == this.Point1)
			{
				if (this.Point2.Movement.Links.Count > 1)
				{
					result = this.Point1.MassStruggling;
				}
				else
				{
					result = this.Point1.Mass;
				}
			}
			else if (point == this.Point2)
			{
				if (this.Point1.Movement.Links.Count > 1)
				{
					result = this.Point2.MassStruggling;
				}
				else
				{
					result = this.Point2.Mass;
				}
			}
			else
			{
				result = 1f;
				CombatLink.Log.ErrorFormat("invalid point at {0}", new object[]
				{
					point
				});
			}
			return result;
		}

		public static readonly BitLogger Log = new BitLogger(typeof(CombatLink));

		private float _curLen;

		private float _offset;

		private Vector3 _posDiff;

		private Vector3 _pivot;

		private bool _fixedPivot;

		private bool _clampOnCorners;

		private HashSet<CombatMovement> _updatedPoints = new HashSet<CombatMovement>();

		public class LinkHook
		{
			public LinkHook(Transform point, CombatMovement hookMovement, float mass, float massStruggling)
			{
				this._transform = point;
				this.Movement = hookMovement;
				this.Mass = mass;
				this.Position = this._transform.position;
				this.MassStruggling = massStruggling;
			}

			public Vector3 Velocity { get; private set; }

			public Vector3 Position { get; private set; }

			public CombatMovement Movement { get; private set; }

			public float Mass { get; private set; }

			public float MassStruggling { get; private set; }

			public Vector3 Normal { get; private set; }

			public float Distance { get; private set; }

			public Vector3 Pivot { get; private set; }

			public CombatLink Link { get; private set; }

			public float PerpendicularSpeed
			{
				get
				{
					return Vector3.Dot(Vector3.Cross(this.Normal, this._transform.up), this.Velocity);
				}
			}

			public Vector3 CombatPositionDiff
			{
				get
				{
					return this.Position - this.Movement.Combat.Transform.position;
				}
			}

			public void UpdateVelocity()
			{
				Vector3 velocity = this.Movement.LastVelocity - Vector3.Cross(this.CombatPositionDiff, this.Movement.transform.up * this.Movement.LastAngularVelocity);
				velocity.y = 0f;
				Vector3 position = this._transform.position;
				position.y = 0f;
				if (this._velocityFrozen)
				{
					this._velocityFrozen = false;
					this.Position = position;
					return;
				}
				this.Position = position;
				this.Velocity = velocity;
			}

			public void UpdatePivot(Vector3 pivot)
			{
				Vector3 rhs = pivot - this.Position;
				rhs.y = 0f;
				this.Normal = rhs.normalized;
				this.Distance = Mathf.Abs(Vector3.Dot(this.Normal, rhs));
				this.Pivot = pivot;
			}

			public void Clamp(float distanceFromPivot)
			{
				Vector3 a = this.Pivot - this.Normal * (this.Distance - distanceFromPivot);
				this.Movement.ForcePosition(a - this.CombatPositionDiff, false);
			}

			public void FreezeVelocity()
			{
				this._velocityFrozen = true;
			}

			private bool _velocityFrozen;

			private readonly Transform _transform;
		}
	}
}
