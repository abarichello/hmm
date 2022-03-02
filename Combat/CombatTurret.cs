using System;
using Pocketverse;
using UnityEngine;

namespace HeavyMetalMachines.Combat
{
	public class CombatTurret : GameHubBehaviour, ICachedObject
	{
		private void Start()
		{
			if (!GameHubBehaviour.Hub.Net.IsClient())
			{
				base.enabled = false;
				return;
			}
			this._transform = base.transform;
			this.SearchCombatObject();
		}

		private void SearchCombatObject()
		{
			this._combatObject = CombatTurret.GetComponent<CombatObject>(this._transform);
		}

		public static T GetComponent<T>(Transform trans) where T : Component
		{
			T t = (T)((object)null);
			while (trans != null && t == null)
			{
				t = trans.GetComponent<T>();
				trans = trans.parent;
			}
			return t;
		}

		public void OnSendToCache()
		{
			this._combatObject = null;
			this._rotateDirection = 1;
		}

		public void OnGetFromCache()
		{
		}

		private void Update()
		{
			if (!this._combatObject)
			{
				this.SearchCombatObject();
			}
			if (!this._combatObject)
			{
				return;
			}
			if (!this._combatObject.CustomGadget0)
			{
				return;
			}
			Vector3 target = this._combatObject.CustomGadget0.Target;
			target.y = base.transform.position.y;
			Quaternion rotation = base.transform.rotation;
			Quaternion quaternion = Quaternion.LookRotation(target - base.transform.position, base.transform.up);
			if (Mathf.Abs(Quaternion.Angle(rotation, quaternion)) < this.minimunAngle)
			{
				return;
			}
			base.transform.LookAt(target);
			float num = Mathf.Abs(base.transform.localRotation.eulerAngles.y - this.defaultAngle);
			base.transform.rotation = Quaternion.Lerp(rotation, quaternion, this.lerpTime);
			if (num <= 180f)
			{
				if (num > this.turnAngleMax)
				{
					base.transform.rotation = rotation;
					this.AutoRotate();
					return;
				}
			}
			else if (num < 360f - this.turnAngleMax)
			{
				base.transform.rotation = rotation;
				this.AutoRotate();
				return;
			}
		}

		private void AutoRotate()
		{
			float num;
			if (Quaternion.Euler(0f, base.transform.localRotation.eulerAngles.y + (float)this._rotateDirection * this.turnAngleMax * Time.deltaTime / this.fullSpinTime - this.defaultAngle, 0f).eulerAngles.y <= 180f)
			{
				num = Mathf.Min(Quaternion.Euler(0f, base.transform.localRotation.eulerAngles.y + (float)this._rotateDirection * this.turnAngleMax * Time.deltaTime / this.fullSpinTime - this.defaultAngle, 0f).eulerAngles.y, this.turnAngleMax);
			}
			else
			{
				num = Mathf.Max(Quaternion.Euler(0f, base.transform.localRotation.eulerAngles.y + (float)this._rotateDirection * this.turnAngleMax * Time.deltaTime / this.fullSpinTime - this.defaultAngle, 0f).eulerAngles.y, 360f - this.turnAngleMax);
			}
			if (num == this.turnAngleMax || num == 360f - this.turnAngleMax)
			{
				this._rotateDirection *= -1;
			}
			num += this.defaultAngle;
			base.transform.localRotation = Quaternion.Euler(0f, num, 0f);
		}

		public float turnAngleMax = 180f;

		public float defaultAngle;

		public float height = 1f;

		public float fullSpinTime = 1f;

		public float lerpTime = 0.25f;

		public float minimunAngle = 5f;

		private int _rotateDirection = 1;

		private Transform _transform;

		private CombatObject _combatObject;
	}
}
